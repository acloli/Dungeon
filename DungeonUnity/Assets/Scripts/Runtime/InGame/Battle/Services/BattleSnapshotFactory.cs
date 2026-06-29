using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// UI表示用スナップショット構築クラス
    /// </summary>
    public sealed class BattleSnapshotFactory : IBattleSnapshotFactory
    {
        private readonly IBattleDisplayTextService _displayTextService;
        private readonly IBattleShopService _shopService;
        private readonly IBattleEnemyActionSelector _enemyActionSelector;
        private readonly IBattlePileOrderService _pileOrderService;

        public BattleSnapshotFactory(IBattleDisplayTextService displayTextService, IBattleShopService shopService, IBattleEnemyActionSelector enemyActionSelector, IBattlePileOrderService pileOrderService)
        {
            _displayTextService = displayTextService;
            _shopService = shopService;
            _enemyActionSelector = enemyActionSelector;
            _pileOrderService = pileOrderService;
        }

        public BattleSceneSnapshot CreateSnapshot(BattleSceneState state)
        {
            if (state == null)
            {
                return null;
            }

            return new BattleSceneSnapshotBuilder(state.CurrentPage)
            {
                HostChrome = BuildHostChromeSnapshot(state),
                Map = BuildMapSnapshot(state),
                Combat = BuildCombatSnapshot(state),
                Reward = BuildRewardSnapshot(state),
                RestShop = BuildRestShopSnapshot(state),
                Shop = BuildShopSnapshot(state),
                Event = BuildEventSnapshot(state),
                Result = BuildResultSnapshot(state),
                PotionReplace = BuildPotionReplaceSnapshot(state),
                PileInspect = BuildPileInspectSnapshot(state)
            }.Build();
        }

        private BattleHostChromeSnapshot BuildHostChromeSnapshot(BattleSceneState state)
        {
            return new BattleHostChromeSnapshot(
                BuildOwnedRelicViews(state),
                BuildOwnedPotionViews(state),
                state.SelectedOwnedRelicIndex,
                state.SelectedOwnedPotionIndex,
                state.OwnedRelicHintMessage,
                state.OwnedPotionHintMessage,
                CanUseSelectedPotion(state));
        }

        private BattleMapSnapshot BuildMapSnapshot(BattleSceneState state)
        {
            return new BattleMapSnapshot(
                state.Nodes,
                BuildAvailableNodeIndices(state),
                state.MapMessage);
        }

        private BattleCombatSnapshot BuildCombatSnapshot(BattleSceneState state)
        {
            return new BattleCombatSnapshot(
                state.PlayerMaxHp,
                state.PlayerHp,
                state.PlayerEnergy,
                state.PlayerBlock,
                state.Gold,
                state.BattleHintMessage,
                BuildHandCardViews(state),
                BuildEnemyViews(state),
                state.SelectedEnemyIndex,
                BuildEnemyIntent(state),
                BuildStatusViews(state.PlayerStatuses),
                BuildStatusViews(state.EnemyStatuses),
                BuildBuffViews(state.PlayerBuffs),
                BuildBuffViews(state.EnemyBuffs),
                state.DrawPile.Count,
                state.DiscardPile.Count,
                state.ExhaustPile.Count,
                state.Hand.Count,
                BattleSceneConstants.MaxHandSize,
                state.CurrentEnemy,
                state.EnemyHp,
                state.EnemyBlock);
        }

        private BattleRewardSnapshot BuildRewardSnapshot(BattleSceneState state)
        {
            return new BattleRewardSnapshot(
                state.RewardChoices,
                state.GoldClaimed,
                state.PotionClaimed,
                state.RelicClaimed,
                state.CardRewardPicked,
                state.BattleGoldReward,
                state.PotionDropped,
                state.PendingRelicReward != null,
                state.PendingPotionReward,
                state.PendingRelicReward);
        }

        private BattleRestShopSnapshot BuildRestShopSnapshot(BattleSceneState state)
        {
            return new BattleRestShopSnapshot(
                state.RestShopMessage,
                state.IsRestShopContinueEnabled);
        }

        private BattleShopSnapshot BuildShopSnapshot(BattleSceneState state)
        {
            return new BattleShopSnapshot(
                state.Gold,
                BuildShopItemViews(state),
                state.IsCardRemovalSoldOut,
                _shopService.GetCardRemovalPrice(state));
        }

        private static BattleEventSnapshot BuildEventSnapshot(BattleSceneState state)
        {
            return new BattleEventSnapshot(state.CurrentEvent);
        }

        private static BattleResultSnapshot BuildResultSnapshot(BattleSceneState state)
        {
            return new BattleResultSnapshot(state.ResultMessage);
        }

        private BattlePotionReplaceSnapshot BuildPotionReplaceSnapshot(BattleSceneState state)
        {
            return new BattlePotionReplaceSnapshot(
                BuildOwnedPotionViews(state),
                state.PendingPotionOffer);
        }

        private BattleIntentViewModel BuildEnemyIntent(BattleSceneState state)
        {
            BattleEnemyState enemyState = _enemyActionSelector.GetSelectedEnemy(state);
            RuntimeEnemyAction action = SelectEnemyActionPreview(state, enemyState);
            if (action == null)
            {
                return null;
            }

            return BattleIntentViewModel.FromAction(
                action,
                _displayTextService.GetIntentName(action.IntentType),
                _displayTextService.GetStatusName(action.StatusType),
                _displayTextService.GetBuffName(action.BuffType));
        }

        private RuntimeEnemyAction SelectEnemyActionPreview(BattleSceneState state, BattleEnemyState enemyState)
        {
            if (state.CurrentPage != BattleScenePage.Battle ||
                enemyState == null ||
                enemyState.Enemy == null ||
                enemyState.Enemy.Actions == null ||
                enemyState.Enemy.Actions.Count == 0 ||
                enemyState.IsDefeated)
            {
                return null;
            }

            RuntimeEnemyAction openingAction = FindFirstAction(enemyState, RepeatRule.OpeningOnly);
            if (enemyState.TurnCount == 0 && openingAction != null)
            {
                return openingAction;
            }

            RuntimeEnemyAction repeatAction = FindFirstAction(enemyState, RepeatRule.RepeatAfterOpening);
            if (enemyState.TurnCount > 0 && repeatAction != null)
            {
                return repeatAction;
            }

            RuntimeEnemyAction afterOpeningRandomAction = FindFirstAction(enemyState, RepeatRule.AfterOpeningRandom);
            if (enemyState.TurnCount > 0 && afterOpeningRandomAction != null)
            {
                return afterOpeningRandomAction;
            }

            RuntimeEnemyAction randomAction = FindFirstAction(enemyState, RepeatRule.Random);
            if (randomAction != null)
            {
                return randomAction;
            }

            RuntimeEnemyAction cycleAction = FindCycleActionPreview(enemyState);
            return cycleAction ?? enemyState.Enemy.Actions[0];
        }

        private RuntimeEnemyAction FindFirstAction(BattleEnemyState enemyState, RepeatRule repeatRule)
        {
            IReadOnlyList<RuntimeEnemyAction> actions = enemyState.Enemy.Actions;
            for (int i = 0; i < actions.Count; i++)
            {
                RuntimeEnemyAction action = actions[i];
                if (action.RepeatRule == repeatRule)
                {
                    return action;
                }
            }

            return null;
        }

        private RuntimeEnemyAction FindCycleActionPreview(BattleEnemyState enemyState)
        {
            List<RuntimeEnemyAction> cycleActions = new List<RuntimeEnemyAction>();
            IReadOnlyList<RuntimeEnemyAction> actions = enemyState.Enemy.Actions;
            for (int i = 0; i < actions.Count; i++)
            {
                RuntimeEnemyAction action = actions[i];
                if (action.RepeatRule == RepeatRule.Cycle)
                {
                    cycleActions.Add(action);
                }
            }

            if (cycleActions.Count == 0)
            {
                return null;
            }

            return cycleActions[enemyState.CycleIndex % cycleActions.Count];
        }

        private IReadOnlyList<BattleEnemyViewModel> BuildEnemyViews(BattleSceneState state)
        {
            List<BattleEnemyViewModel> views = new List<BattleEnemyViewModel>();
            for (int i = 0; i < state.Enemies.Count; i++)
            {
                BattleEnemyState enemyState = state.Enemies[i];
                if (enemyState == null || enemyState.Enemy == null)
                {
                    continue;
                }

                views.Add(new BattleEnemyViewModel(
                    enemyState.SlotIndex,
                    enemyState.Enemy.DisplayName,
                    enemyState.Hp,
                    enemyState.Block,
                    enemyState.IsDefeated,
                    BuildIntentView(state, enemyState),
                    BuildStatusViews(enemyState.Statuses),
                    BuildBuffViews(enemyState.Buffs)));
            }

            return views;
        }

        private BattleIntentViewModel BuildIntentView(BattleSceneState state, BattleEnemyState enemyState)
        {
            RuntimeEnemyAction action = SelectEnemyActionPreview(state, enemyState);
            if (action == null)
            {
                return null;
            }

            return BattleIntentViewModel.FromAction(
                action,
                _displayTextService.GetIntentName(action.IntentType),
                _displayTextService.GetStatusName(action.StatusType),
                _displayTextService.GetBuffName(action.BuffType));
        }

        private IReadOnlyList<int> BuildAvailableNodeIndices(BattleSceneState state)
        {
            List<int> indices = new List<int>();
            if (state.CurrentPage != BattleScenePage.Map || state.Nodes == null)
            {
                return indices;
            }

            for (int i = 0; i < state.Nodes.Count; i++)
            {
                if (state.CanMoveToNode(i))
                {
                    indices.Add(i);
                }
            }

            return indices;
        }

        private static IReadOnlyList<BattleMultiIconViewModel> BuildOwnedRelicViews(BattleSceneState state)
        {
            List<BattleMultiIconViewModel> views = new List<BattleMultiIconViewModel>();
            for (int i = 0; i < state.OwnedRelics.Count; i++)
            {
                RuntimeRelic relic = state.OwnedRelics[i];
                if (relic == null)
                {
                    continue;
                }

                views.Add(BattleMultiIconViewModel.CreateRelic(
                    relic,
                    isSelected: i == state.SelectedOwnedRelicIndex));
            }

            return views;
        }

        private static IReadOnlyList<BattleMultiIconViewModel> BuildOwnedPotionViews(BattleSceneState state)
        {
            List<BattleMultiIconViewModel> views = new List<BattleMultiIconViewModel>();
            for (int i = 0; i < state.OwnedPotions.Count; i++)
            {
                RuntimePotion potion = state.OwnedPotions[i];
                if (potion == null)
                {
                    continue;
                }

                views.Add(BattleMultiIconViewModel.CreatePotion(
                    potion,
                    isSelected: i == state.SelectedOwnedPotionIndex));
            }

            return views;
        }

        private static bool CanUseSelectedPotion(BattleSceneState state)
        {
            if (state == null || state.SelectedOwnedPotionIndex < 0 || state.SelectedOwnedPotionIndex >= state.OwnedPotions.Count)
            {
                return false;
            }

            RuntimePotion potion = state.OwnedPotions[state.SelectedOwnedPotionIndex];
            if (potion == null)
            {
                return false;
            }

            return state.CurrentPage switch
            {
                BattleScenePage.Battle => potion.UseContext == PotionUseContext.BattleOnly || potion.UseContext == PotionUseContext.Both,
                _ => potion.UseContext == PotionUseContext.OutOfBattleOnly || potion.UseContext == PotionUseContext.Both
            };
        }

        private IReadOnlyList<BattleHandCardViewModel> BuildHandCardViews(BattleSceneState state)
        {
            List<BattleHandCardViewModel> views = new List<BattleHandCardViewModel>();
            if (state.Hand == null)
            {
                return views;
            }

            for (int i = 0; i < state.Hand.Count; i++)
            {
                RuntimeCard card = state.Hand[i];
                if (card == null)
                {
                    continue;
                }

                views.Add(new BattleHandCardViewModel(
                    card,
                    BuildCardIconViewModel(
                        card,
                        state.PlayerEnergy >= card.Cost,
                        i == state.SelectedCardIndex,
                        true)));
            }

            return views;
        }

        private IReadOnlyList<BattleStatusViewModel> BuildStatusViews(IReadOnlyDictionary<StatusType, int> statuses)
        {
            List<BattleStatusViewModel> views = new List<BattleStatusViewModel>();
            if (statuses == null)
            {
                return views;
            }

            foreach (KeyValuePair<StatusType, int> status in statuses)
            {
                if (status.Key == StatusType.None || status.Value <= 0)
                {
                    continue;
                }

                views.Add(new BattleStatusViewModel(_displayTextService.GetStatusName(status.Key), status.Value, false));
            }

            return views;
        }

        private IReadOnlyList<BattleStatusViewModel> BuildBuffViews(IReadOnlyDictionary<BuffType, int> buffs)
        {
            List<BattleStatusViewModel> views = new List<BattleStatusViewModel>();
            if (buffs == null)
            {
                return views;
            }

            foreach (KeyValuePair<BuffType, int> buff in buffs)
            {
                if (buff.Key == BuffType.None || buff.Value <= 0)
                {
                    continue;
                }

                views.Add(new BattleStatusViewModel(_displayTextService.GetBuffName(buff.Key), buff.Value, true));
            }

            return views;
        }

        private IReadOnlyList<BattleShopItemViewModel> BuildShopItemViews(BattleSceneState state)
        {
            List<BattleShopItemViewModel> views = new List<BattleShopItemViewModel>();
            if (state.ShopItems == null)
            {
                return views;
            }

            for (int i = 0; i < state.ShopItems.Count; i++)
            {
                BattleShopItemState item = state.ShopItems[i];
                if (item == null)
                {
                    continue;
                }

                views.Add(new BattleShopItemViewModel(
                    item.SlotIndex,
                    item.RewardType,
                    BuildShopItemDisplayName(item),
                    item.Price,
                    item.IsSoldOut,
                    item.Card,
                    item.Relic,
                    item.Potion,
                    item.ItemId,
                    BuildShopItemIconViewModel(item, state.Gold)));
            }

            return views;
        }

        private string BuildShopItemDisplayName(BattleShopItemState item)
        {
            if (item.RewardType == RewardType.Card)
            {
                return item.Card != null ? item.Card.DisplayName : "Card";
            }
            if (item.RewardType == RewardType.Potion)
            {
                return item.Potion != null ? item.Potion.DisplayName : $"Potion {item.ItemId}";
            }
            if (item.RewardType == RewardType.Relic)
            {
                return item.Relic != null ? item.Relic.DisplayName : $"Relic {item.ItemId}";
            }
            return item.RewardType.ToString();
        }

        private BattleMultiIconViewModel BuildShopItemIconViewModel(BattleShopItemState item, int currentGold)
        {
            bool isAffordable = !item.IsSoldOut && currentGold >= item.Price;
            bool isInteractable = !item.IsSoldOut && isAffordable;

            if (item.RewardType == RewardType.Card && item.Card != null)
            {
                return BuildCardIconViewModel(item.Card, isAffordable, false, isInteractable);
            }

            if (item.RewardType == RewardType.Relic && item.Relic != null)
            {
                return BattleMultiIconViewModel.CreateRelic(
                    item.Relic,
                    isInteractable: isInteractable,
                    isAffordable: isAffordable);
            }

            if (item.RewardType == RewardType.Potion && item.Potion != null)
            {
                return BattleMultiIconViewModel.CreatePotion(
                    item.Potion,
                    isInteractable: isInteractable,
                    isAffordable: isAffordable);
            }

            return BattleMultiIconViewModel.CreatePlaceholder(
                BattleIconKind.None,
                BuildShopItemDisplayName(item),
                CardRarity.Common,
                isInteractable,
                isAffordable);
        }

        private static BattleMultiIconViewModel BuildCardIconViewModel(RuntimeCard card, bool isAffordable, bool isSelected, bool isInteractable)
        {
            return BattleMultiIconViewModel.CreateCard(card, isAffordable, isInteractable, isSelected);
        }

        /// <summary>
        /// パイル確認スナップショットを構築する
        /// </summary>
        private BattlePileInspectSnapshot BuildPileInspectSnapshot(BattleSceneState state)
        {
            BattlePileType? openedPileType = state.OpenedPileType;
            if (openedPileType == null)
            {
                return new BattlePileInspectSnapshot(isOpen: false);
            }

            IReadOnlyList<RuntimeCard> sourceCards = GetPileCards(state, openedPileType.Value);
            IReadOnlyList<RuntimeCard> orderedCards = _pileOrderService.Order(openedPileType.Value, sourceCards, state);

            List<BattleMultiIconViewModel> cardViews = new List<BattleMultiIconViewModel>(orderedCards.Count);
            for (int i = 0; i < orderedCards.Count; i++)
            {
                RuntimeCard card = orderedCards[i];
                if (card == null)
                {
                    continue;
                }

                cardViews.Add(BattleMultiIconViewModel.CreateCard(card, isInteractable: false));
            }

            string title = GetPileTitle(openedPileType.Value);

            return new BattlePileInspectSnapshot(
                openedPileType.Value,
                title,
                cardViews,
                isOpen: true);
        }

        /// <summary>
        /// パイル種別に対応するカードリストを取得する
        /// </summary>
        private static IReadOnlyList<RuntimeCard> GetPileCards(BattleSceneState state, BattlePileType pileType)
        {
            switch (pileType)
            {
                case BattlePileType.Draw:
                    return state.DrawPile;
                case BattlePileType.Discard:
                    return state.DiscardPile;
                case BattlePileType.Exhaust:
                    return state.ExhaustPile;
                default:
                    return System.Array.Empty<RuntimeCard>();
            }
        }

        /// <summary>
        /// パイル種別に対応するタイトルを取得する
        /// </summary>
        private static string GetPileTitle(BattlePileType pileType)
        {
            switch (pileType)
            {
                case BattlePileType.Draw:
                    return "山札";
                case BattlePileType.Discard:
                    return "捨て札";
                case BattlePileType.Exhaust:
                    return "廃棄札";
                default:
                    return string.Empty;
            }
        }
    }
}
