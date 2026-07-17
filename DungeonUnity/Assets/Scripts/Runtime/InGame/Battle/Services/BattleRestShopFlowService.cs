using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneの休憩所・ショップフローを扱うクラス
    /// </summary>
    public sealed class BattleRestShopFlowService : IBattleRestShopFlowService
    {
        private readonly IBattleSceneRules _rules;
        private readonly IBattleRandomProvider _randomProvider;
        private readonly IBattleShopService _shopService;
        private readonly IBattlePotionService _potionService;
        private readonly IBattleRelicService _relicService;

        public BattleRestShopFlowService(
            IBattleSceneRules rules,
            IBattleRandomProvider randomProvider,
            IBattleShopService shopService,
            IBattlePotionService potionService,
            IBattleRelicService relicService)
        {
            _rules = rules;
            _randomProvider = randomProvider;
            _shopService = shopService;
            _potionService = potionService;
            _relicService = relicService;
        }

        /// <summary>
        /// 休憩所画面を開く
        /// </summary>
        public void OpenRestShop(BattleSceneState state, RuntimeRunDefinition runDefinition, Action<BattleScenePage> setCurrentPage)
        {
            setCurrentPage(BattleScenePage.RestShop);
            state.IsRestShopContinueEnabled = false;
            state.CardSelectMode = CardSelectMode.CardRemoval;

            if (state.ShopItems == null || state.ShopItems.Count == 0)
            {
                _shopService.InitializeShop(state, runDefinition, _randomProvider);
            }

            state.RestShopMessage = string.Format(
                BattleSceneConstants.RestShopStateFormat,
                state.PlayerHp,
                state.PlayerMaxHp,
                state.Gold);
        }

        /// <summary>
        /// 休憩を適用する
        /// </summary>
        public void ApplyRest(BattleSceneState state)
        {
            _rules.ApplyRest(state);
            state.RestShopMessage = string.Format(
                BattleSceneConstants.RestDoneFormat,
                state.PlayerHp,
                state.PlayerMaxHp);
            state.IsRestShopContinueEnabled = true;
        }

        /// <summary>
        /// 強化候補選択を開く
        /// </summary>
        public void ApplyUpgrade(BattleSceneState state, RuntimeRunDefinition runDefinition, Action<BattleScenePage> setCurrentPage)
        {
            if (!HasUpgradeableCards(state, runDefinition))
            {
                state.RestShopMessage = BattleSceneConstants.NoUpgradeableCards;
                return;
            }

            state.CardSelectMode = CardSelectMode.Upgrade;
            state.CardSelectMessage = string.Empty;
            setCurrentPage(BattleScenePage.CardSelect);
        }

        /// <summary>
        /// 現在のカード選択候補取得
        /// </summary>
        public IReadOnlyList<RuntimeCard> GetCardSelectCards(BattleSceneState state, RuntimeRunDefinition runDefinition)
        {
            if (state.CardSelectMode != CardSelectMode.Upgrade)
            {
                return state.Deck;
            }

            List<RuntimeCard> upgradeableCards = new List<RuntimeCard>();
            for (int i = 0; i < state.Deck.Count; i++)
            {
                RuntimeCard card = state.Deck[i];
                if (CanUpgradeCard(runDefinition, card))
                {
                    upgradeableCards.Add(card);
                }
            }

            return upgradeableCards;
        }

        /// <summary>
        /// 現在のカード選択価格取得
        /// </summary>
        public IReadOnlyDictionary<int, int> GetCardSelectPrices(
            BattleSceneState state,
            RuntimeRunDefinition runDefinition,
            bool isFreeUpgradeAvailable)
        {
            Dictionary<int, int> prices = new Dictionary<int, int>();
            if (state.CardSelectMode != CardSelectMode.Upgrade)
            {
                return prices;
            }

            IReadOnlyList<RuntimeCard> cards = GetCardSelectCards(state, runDefinition);
            for (int i = 0; i < cards.Count; i++)
            {
                RuntimeCard card = cards[i];
                if (card != null)
                {
                    prices[card.Id] = isFreeUpgradeAvailable
                        ? 0
                        : _shopService.GetCardUpgradePrice(runDefinition, card);
                }
            }

            return prices;
        }

        /// <summary>
        /// 現在のカード選択強化後カード取得
        /// </summary>
        public IReadOnlyDictionary<int, RuntimeCard> GetCardSelectUpgradedCards(
            BattleSceneState state,
            RuntimeRunDefinition runDefinition)
        {
            Dictionary<int, RuntimeCard> upgradedCards = new Dictionary<int, RuntimeCard>();
            if (state.CardSelectMode != CardSelectMode.Upgrade || runDefinition == null)
            {
                return upgradedCards;
            }

            IReadOnlyList<RuntimeCard> cards = GetCardSelectCards(state, runDefinition);
            for (int i = 0; i < cards.Count; i++)
            {
                RuntimeCard card = cards[i];
                if (card != null && runDefinition.CardCatalog.TryGetValue(card.UpgradeCardId, out RuntimeCard upgradedCard))
                {
                    upgradedCards[card.Id] = upgradedCard;
                }
            }

            return upgradedCards;
        }

        /// <summary>
        /// ショップ画面を開く
        /// </summary>
        public void OpenShop(BattleSceneState state, Action<BattleScenePage> setCurrentPage)
        {
            state.CardSelectMode = CardSelectMode.CardRemoval;
            setCurrentPage(BattleScenePage.Shop);
        }

        /// <summary>
        /// ショップ商品を購入する
        /// </summary>
        public bool PurchaseShopItem(BattleSceneState state, int slotIndex)
        {
            BattleShopItemState item = FindShopItem(state, slotIndex);
            if (item == null)
            {
                return false;
            }

            if (item.RewardType == RewardType.Potion && item.Potion != null)
            {
                if (state.Gold < item.Price || item.IsSoldOut)
                {
                    return false;
                }

                if (_potionService.HasCapacity(state))
                {
                    if (_shopService.PurchaseShopItem(state, slotIndex) && _potionService.AddOwnedPotion(state, item.Potion))
                    {
                        state.ClearOwnedPotionInspection();
                        return true;
                    }

                    return false;
                }

                state.PendingPotionOffer = _potionService.CreateOffer(item.Potion, PotionOfferSource.Shop, slotIndex);
                return false;
            }

            if (!_shopService.PurchaseShopItem(state, slotIndex))
            {
                return false;
            }

            GrantPurchasedRelic(state, slotIndex);
            state.ClearOwnedRelicInspection();
            return true;
        }

        /// <summary>
        /// カード削除選択を開く
        /// </summary>
        public void OpenCardRemoval(BattleSceneState state, Action<BattleScenePage> setCurrentPage)
        {
            state.CardSelectMode = CardSelectMode.CardRemoval;
            state.CardSelectMessage = string.Empty;
            setCurrentPage(BattleScenePage.CardSelect);
        }

        /// <summary>
        /// カード削除を購入する
        /// </summary>
        public bool PurchaseCardRemoval(BattleSceneState state, RuntimeCard card, Action<BattleScenePage> setCurrentPage)
        {
            bool saveNeeded = _shopService.PurchaseCardRemoval(state, card);
            setCurrentPage(BattleScenePage.Shop);
            return saveNeeded;
        }

        /// <summary>
        /// カード選択をキャンセルする
        /// </summary>
        public void CancelCardSelect(BattleSceneState state, Action<BattleScenePage> setCurrentPage, Action reopenRestShop)
        {
            if (state.CardSelectMode == CardSelectMode.Upgrade)
            {
                bool canContinue = state.IsRestShopContinueEnabled;
                string cardSelectMessage = state.CardSelectMessage;
                reopenRestShop();
                if (canContinue)
                {
                    state.RestShopMessage = string.IsNullOrEmpty(cardSelectMessage)
                        ? state.RestShopMessage
                        : cardSelectMessage;
                    state.IsRestShopContinueEnabled = true;
                }

                return;
            }

            setCurrentPage(BattleScenePage.Shop);
        }

        /// <summary>
        /// カード選択を確定する
        /// </summary>
        public bool ConfirmCardSelect(
            BattleSceneState state,
            RuntimeRunDefinition runDefinition,
            RuntimeCard card,
            Action<BattleScenePage> setCurrentPage)
        {
            if (state.CardSelectMode == CardSelectMode.Upgrade)
            {
                return ApplyCardUpgrade(state, runDefinition, card);
            }

            return PurchaseCardRemoval(state, card, setCurrentPage);
        }

        /// <summary>
        /// ショップから退出する
        /// </summary>
        public void LeaveShop(BattleSceneState state, Action<BattleScenePage> setCurrentPage)
        {
            setCurrentPage(BattleScenePage.RestShop);
            state.RestShopMessage = string.Format(
                BattleSceneConstants.RestShopStateFormat,
                state.PlayerHp,
                state.PlayerMaxHp,
                state.Gold);
            state.IsRestShopContinueEnabled = true;
        }

        /// <summary>
        /// 休憩所から継続する
        /// </summary>
        public void ContinueFromRestShop(BattleSceneState state, Action openMap)
        {
            openMap();
        }

        private static bool HasUpgradeableCards(BattleSceneState state, RuntimeRunDefinition runDefinition)
        {
            for (int i = 0; i < state.Deck.Count; i++)
            {
                if (CanUpgradeCard(runDefinition, state.Deck[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CanUpgradeCard(RuntimeRunDefinition runDefinition, RuntimeCard card)
        {
            return card != null
                   && card.UpgradeCardId > 0
                   && runDefinition != null
                   && runDefinition.CardCatalog.ContainsKey(card.UpgradeCardId);
        }

        private bool ApplyCardUpgrade(BattleSceneState state, RuntimeRunDefinition runDefinition, RuntimeCard card)
        {
            if (!CanUpgradeCard(runDefinition, card))
            {
                state.CardSelectMessage = BattleSceneConstants.NoUpgradeableCards;
                return false;
            }

            int deckIndex = FindDeckCardIndex(state, card);
            if (deckIndex < 0 || !runDefinition.CardCatalog.TryGetValue(card.UpgradeCardId, out RuntimeCard upgradedCard))
            {
                state.CardSelectMessage = BattleSceneConstants.NoUpgradeableCards;
                return false;
            }

            int upgradePrice = _shopService.GetCardUpgradePrice(runDefinition, card);
            if (state.Gold < upgradePrice)
            {
                state.CardSelectMessage = BattleSceneConstants.NotEnoughGold;
                return false;
            }

            state.Deck[deckIndex] = upgradedCard;
            state.Gold -= upgradePrice;
            state.CardSelectMessage = string.Format(
                BattleSceneConstants.UpgradeDoneFormat,
                card.DisplayName,
                upgradedCard.DisplayName,
                upgradePrice,
                state.Gold);
            state.IsRestShopContinueEnabled = true;
            return true;
        }

        private static int FindDeckCardIndex(BattleSceneState state, RuntimeCard card)
        {
            for (int i = 0; i < state.Deck.Count; i++)
            {
                RuntimeCard deckCard = state.Deck[i];
                if (ReferenceEquals(deckCard, card) || deckCard?.Id == card?.Id)
                {
                    return i;
                }
            }

            return -1;
        }

        private void GrantPurchasedRelic(BattleSceneState state, int slotIndex)
        {
            for (int i = 0; i < state.ShopItems.Count; i++)
            {
                BattleShopItemState item = state.ShopItems[i];
                if (item == null || item.SlotIndex != slotIndex || item.RewardType != RewardType.Relic || item.Relic == null)
                {
                    continue;
                }

                _relicService.AddOwnedRelic(state, item.Relic);
                return;
            }
        }

        private static BattleShopItemState FindShopItem(BattleSceneState state, int slotIndex)
        {
            for (int i = 0; i < state.ShopItems.Count; i++)
            {
                BattleShopItemState item = state.ShopItems[i];
                if (item != null && item.SlotIndex == slotIndex)
                {
                    return item;
                }
            }

            return null;
        }

    }
}
