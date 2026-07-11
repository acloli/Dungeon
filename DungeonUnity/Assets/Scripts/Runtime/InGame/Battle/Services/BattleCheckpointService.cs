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
        public void RestoreFromSave(BattleSceneState state, RuntimeRunDefinition runDefinition, RunSaveData saveData, IReadOnlyDictionary<int, RuntimeCard> cardCatalog, IBattleRelicService relicService, IBattlePotionService potionService)
        {
            state.PlayerMaxHp = saveData.PlayerMaxHp;
            state.PlayerHp = saveData.PlayerHp;
            state.PlayerEnergy = saveData.PlayerEnergy;
            state.MaxPotionCount = saveData.MaxPotionCount > 0 ? saveData.MaxPotionCount : BattleSceneConstants.DefaultMaxPotionCount;
            state.Gold = saveData.Gold;
            state.CurrentNodeIndex = saveData.CurrentNodeIndex;
            state.CurrentPage = (BattleScenePage)saveData.CurrentPage;
            RestoreMapRoute(state, saveData);

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
            state.ClearOwnedInspections();
            state.ClearPendingRewards();
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
        public RunSaveData BuildSaveData(BattleSceneState state, RuntimeRunDefinition runDefinition, int masterSeed, int mapSeed, int mapLayoutVersion, int randomCounter)
        {
            RunSaveData data = new RunSaveData
            {
                RunProfileId = runDefinition.RunProfileId,
                PlayerMaxHp = state.PlayerMaxHp,
                PlayerHp = state.PlayerHp,
                PlayerEnergy = state.PlayerEnergy,
                MaxPotionCount = state.MaxPotionCount > 0 ? state.MaxPotionCount : BattleSceneConstants.DefaultMaxPotionCount,
                Gold = state.Gold,
                CurrentNodeIndex = state.CurrentNodeIndex,
                CurrentPage = (int)ResolveCheckpointPage(state.CurrentPage),
                DeckCardIds = new List<int>(),
                MapRouteNodeIndices = new List<int>(),
                OwnedRelicIds = new List<int>(),
                OwnedPotionIds = new List<int>(),
                ShopItems = new List<SaveShopItem>(),
                IsCardRemovalSoldOut = state.IsCardRemovalSoldOut,
                CardRemovalCount = state.CardRemovalCount,
                MasterSeed = masterSeed,
                MapSeed = mapSeed,
                MapLayoutVersion = mapLayoutVersion,
                RandomCounter = randomCounter
            };

            for (int i = 0; i < state.Deck.Count; i++)
            {
                if (state.Deck[i] != null)
                {
                    data.DeckCardIds.Add(state.Deck[i].Id);
                }
            }

            for (int i = 0; i < state.MapRouteNodeIndices.Count; i++)
            {
                data.MapRouteNodeIndices.Add(state.MapRouteNodeIndices[i]);
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
        /// 保存済みの経路履歴を復元する
        /// </summary>
        private static void RestoreMapRoute(BattleSceneState state, RunSaveData saveData)
        {
            state.MapRouteNodeIndices.Clear();
            IReadOnlyList<int> route = ResolveMapRoute(saveData.MapRouteNodeIndices, state.Nodes, state.CurrentNodeIndex);
            for (int i = 0; i < route.Count; i++)
            {
                state.MapRouteNodeIndices.Add(route[i]);
            }
        }

        /// <summary>
        /// 保存済み経路、または旧セーブ向けの復元経路を返す
        /// </summary>
        private static IReadOnlyList<int> ResolveMapRoute(
            IReadOnlyList<int> savedRoute,
            IReadOnlyList<RuntimeMapNode> nodes,
            int currentNodeIndex)
        {
            if (IsValidRoute(savedRoute, nodes, currentNodeIndex))
            {
                return savedRoute;
            }

            if (TryBuildRoute(nodes, currentNodeIndex, out List<int> restoredRoute))
            {
                return restoredRoute;
            }

            List<int> fallback = new List<int>();
            if (IsValidNodeIndex(nodes, currentNodeIndex))
            {
                fallback.Add(currentNodeIndex);
            }

            return fallback;
        }

        /// <summary>
        /// 経路履歴が現在ノードまでの正しい接続列か判定する
        /// </summary>
        private static bool IsValidRoute(
            IReadOnlyList<int> route,
            IReadOnlyList<RuntimeMapNode> nodes,
            int currentNodeIndex)
        {
            if (currentNodeIndex < 0)
            {
                return route != null && route.Count == 0;
            }

            if (route == null ||
                route.Count == 0 ||
                route[route.Count - 1] != currentNodeIndex ||
                route[0] != 0)
            {
                return false;
            }

            for (int i = 0; i < route.Count; i++)
            {
                if (!IsValidNodeIndex(nodes, route[i]))
                {
                    return false;
                }

                if (i > 0 && !HasConnection(nodes, route[i - 1], route[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 起点から現在ノードまでの決定的な経路を復元する
        /// </summary>
        private static bool TryBuildRoute(
            IReadOnlyList<RuntimeMapNode> nodes,
            int currentNodeIndex,
            out List<int> route)
        {
            route = new List<int>();
            if (!IsValidNodeIndex(nodes, currentNodeIndex))
            {
                return false;
            }

            bool[] visited = new bool[nodes.Count];
            if (TryBuildRouteRecursive(nodes, 0, currentNodeIndex, route, visited))
            {
                return true;
            }

            route.Clear();
            return false;
        }

        /// <summary>
        /// node index 昇順で経路を探索する
        /// </summary>
        private static bool TryBuildRouteRecursive(
            IReadOnlyList<RuntimeMapNode> nodes,
            int nodeIndex,
            int targetNodeIndex,
            List<int> route,
            bool[] visited)
        {
            if (!IsValidNodeIndex(nodes, nodeIndex) || visited[nodeIndex])
            {
                return false;
            }

            visited[nodeIndex] = true;
            route.Add(nodeIndex);
            if (nodeIndex == targetNodeIndex)
            {
                return true;
            }

            IReadOnlyList<int> sourceNextNodeIndices = nodes[nodeIndex].NextNodeIndices;
            if (sourceNextNodeIndices == null)
            {
                route.RemoveAt(route.Count - 1);
                visited[nodeIndex] = false;
                return false;
            }

            List<int> nextNodeIndices = new List<int>(sourceNextNodeIndices);
            nextNodeIndices.Sort();
            for (int i = 0; i < nextNodeIndices.Count; i++)
            {
                if (TryBuildRouteRecursive(nodes, nextNodeIndices[i], targetNodeIndex, route, visited))
                {
                    return true;
                }
            }

            route.RemoveAt(route.Count - 1);
            visited[nodeIndex] = false;
            return false;
        }

        /// <summary>
        /// ノードが存在するか判定する
        /// </summary>
        private static bool IsValidNodeIndex(IReadOnlyList<RuntimeMapNode> nodes, int index)
        {
            return nodes != null && index >= 0 && index < nodes.Count && nodes[index] != null;
        }

        /// <summary>
        /// 2ノード間に接続があるか判定する
        /// </summary>
        private static bool HasConnection(IReadOnlyList<RuntimeMapNode> nodes, int sourceIndex, int targetIndex)
        {
            if (!IsValidNodeIndex(nodes, sourceIndex) || !IsValidNodeIndex(nodes, targetIndex))
            {
                return false;
            }

            IReadOnlyList<int> nextNodeIndices = nodes[sourceIndex].NextNodeIndices;
            if (nextNodeIndices == null)
            {
                return false;
            }

            for (int i = 0; i < nextNodeIndices.Count; i++)
            {
                if (nextNodeIndices[i] == targetIndex)
                {
                    return true;
                }
            }

            return false;
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
