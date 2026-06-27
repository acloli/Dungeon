using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Save.Model;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneのcheckpoint保存と復元を扱うクラス
    /// </summary>
    public sealed class BattleCheckpointService : IBattleCheckpointService
    {
        /// <summary>
        /// セーブデータからBattleScene状態を復元する
        /// </summary>
        public void RestoreFromSave(
            BattleSceneState state,
            RuntimeRunDefinition runDefinition,
            RunSaveData saveData,
            IReadOnlyDictionary<int, RuntimeCard> cardCatalog,
            IBattleRelicService relicService,
            IBattlePotionService potionService)
        {
            state.PlayerMaxHp = saveData.PlayerMaxHp;
            state.PlayerHp = saveData.PlayerHp;
            state.PlayerEnergy = saveData.PlayerEnergy;
            state.Gold = saveData.Gold;
            state.CurrentNodeIndex = saveData.CurrentNodeIndex;
            state.CurrentPage = (BattleScenePage)saveData.CurrentPage;

            state.Deck.Clear();
            if (saveData.DeckCardIds != null)
            {
                for (int i = 0; i < saveData.DeckCardIds.Count; i++)
                {
                    int cardId = saveData.DeckCardIds[i];
                    if (cardCatalog.TryGetValue(cardId, out RuntimeCard card))
                    {
                        state.Deck.Add(card);
                    }
                }
            }

            state.SelectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
            state.SelectedOwnedRelicIndex = BattleSceneConstants.UnselectedCardIndex;
            state.OwnedRelicHintMessage = string.Empty;
            state.SelectedOwnedPotionIndex = BattleSceneConstants.UnselectedCardIndex;
            state.OwnedPotionHintMessage = string.Empty;
            state.PendingRelicReward = null;
            state.PendingPotionReward = null;
            state.PendingPotionOffer = null;
            relicService.RestoreOwnedRelics(state, runDefinition, saveData.OwnedRelicIds);
            potionService.RestoreOwnedPotions(state, runDefinition, saveData.OwnedPotionIds);

            state.ShopItems.Clear();
            state.IsCardRemovalSoldOut = saveData.IsCardRemovalSoldOut;
            state.CardRemovalCount = saveData.CardRemovalCount;
            if (saveData.ShopItems == null)
            {
                return;
            }

            for (int i = 0; i < saveData.ShopItems.Count; i++)
            {
                SaveShopItem savedItem = saveData.ShopItems[i];
                RuntimeCard card = null;
                RuntimeRelic relic = null;
                RuntimePotion potion = null;
                if (savedItem.RewardType == (int)RewardType.Card && savedItem.CardId > 0)
                {
                    cardCatalog.TryGetValue(savedItem.CardId, out card);
                }
                else if (savedItem.RewardType == (int)RewardType.Relic && savedItem.ItemId > 0)
                {
                    runDefinition.RelicCatalog.TryGetValue(savedItem.ItemId, out relic);
                }
                else if (savedItem.RewardType == (int)RewardType.Potion && savedItem.ItemId > 0)
                {
                    runDefinition.PotionCatalog.TryGetValue(savedItem.ItemId, out potion);
                }

                state.ShopItems.Add(new BattleShopItemState(
                    savedItem.SlotIndex,
                    (RewardType)savedItem.RewardType,
                    card,
                    relic,
                    potion,
                    savedItem.ItemId,
                    savedItem.Price,
                    savedItem.IsSoldOut));
            }
        }

        /// <summary>
        /// 現在状態からcheckpoint保存データを構築する
        /// </summary>
        public RunSaveData BuildSaveData(BattleSceneState state, RuntimeRunDefinition runDefinition)
        {
            RunSaveData data = new RunSaveData
            {
                RunProfileId = runDefinition.RunProfileId,
                PlayerMaxHp = state.PlayerMaxHp,
                PlayerHp = state.PlayerHp,
                PlayerEnergy = state.PlayerEnergy,
                Gold = state.Gold,
                CurrentNodeIndex = state.CurrentNodeIndex,
                CurrentPage = (int)ResolveCheckpointPage(state.CurrentPage),
                DeckCardIds = new List<int>(),
                OwnedRelicIds = new List<int>(),
                OwnedPotionIds = new List<int>(),
                ShopItems = new List<SaveShopItem>(),
                IsCardRemovalSoldOut = state.IsCardRemovalSoldOut,
                CardRemovalCount = state.CardRemovalCount
            };

            for (int i = 0; i < state.Deck.Count; i++)
            {
                if (state.Deck[i] != null)
                {
                    data.DeckCardIds.Add(state.Deck[i].Id);
                }
            }

            for (int i = 0; i < state.OwnedRelics.Count; i++)
            {
                if (state.OwnedRelics[i] != null)
                {
                    data.OwnedRelicIds.Add(state.OwnedRelics[i].Id);
                }
            }

            for (int i = 0; i < state.OwnedPotions.Count; i++)
            {
                if (state.OwnedPotions[i] != null)
                {
                    data.OwnedPotionIds.Add(state.OwnedPotions[i].Id);
                }
            }

            for (int i = 0; i < state.ShopItems.Count; i++)
            {
                BattleShopItemState item = state.ShopItems[i];
                if (item == null)
                {
                    continue;
                }

                data.ShopItems.Add(new SaveShopItem
                {
                    SlotIndex = item.SlotIndex,
                    RewardType = (int)item.RewardType,
                    CardId = item.Card != null ? item.Card.Id : 0,
                    ItemId = item.ItemId,
                    Price = item.Price,
                    IsSoldOut = item.IsSoldOut
                });
            }

            return data;
        }

        /// <summary>
        /// checkpoint保存用ページ正規化
        /// </summary>
        private static BattleScenePage ResolveCheckpointPage(BattleScenePage currentPage)
        {
            return currentPage switch
            {
                BattleScenePage.Shop => BattleScenePage.RestShop,
                BattleScenePage.CardSelect => BattleScenePage.RestShop,
                _ => currentPage
            };
        }
    }
}
