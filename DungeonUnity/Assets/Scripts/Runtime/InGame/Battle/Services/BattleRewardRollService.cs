using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;
using UnityEngine;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneの報酬抽選を扱うクラス
    /// </summary>
    public sealed class BattleRewardRollService : IBattleRewardRollService
    {
        /// <summary>
        /// カード報酬候補を選出する
        /// </summary>
        public IReadOnlyList<RuntimeRewardEntry> SelectCardRewardChoices(BattleSceneState state, RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider)
        {
            List<RuntimeRewardEntry> rewards = new List<RuntimeRewardEntry>();
            if (state == null)
            {
                return rewards;
            }

            List<RuntimeRewardEntry> candidates = BuildRewardCandidates(state, runDefinition);
            int maxCount = runDefinition != null && runDefinition.CardRewardChoiceCount > 0
                ? runDefinition.CardRewardChoiceCount
                : BattleSceneConstants.DefaultRewardChoiceCount;
            maxCount = Mathf.Min(maxCount, candidates.Count);

            for (int i = 0; i < maxCount; i++)
            {
                RuntimeRewardEntry selected = SelectWeightedEntry(candidates, randomProvider);
                if (selected == null)
                {
                    break;
                }

                rewards.Add(selected);
                candidates.Remove(selected);
            }

            if (rewards.Count > 0)
            {
                return rewards;
            }

            HashSet<int> pickedCardIds = new HashSet<int>();
            for (int i = 0; i < state.Deck.Count && rewards.Count < BattleSceneConstants.DefaultRewardChoiceCount; i++)
            {
                RuntimeCard card = state.Deck[i];
                if (card == null || !pickedCardIds.Add(card.Id))
                {
                    continue;
                }

                rewards.Add(new RuntimeRewardEntry(RewardType.Card, 1, card, null, null, 1, 1, 999));
            }

            return rewards;
        }

        /// <summary>
        /// ポーションドロップ有無を抽選する
        /// </summary>
        public bool RollPotionDrop(RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider)
        {
            if (runDefinition == null || runDefinition.PotionDropChance <= 0)
            {
                return false;
            }

            return randomProvider.Range(0, 100) < runDefinition.PotionDropChance;
        }

        /// <summary>
        /// レリックドロップ有無を抽選する
        /// </summary>
        public bool RollRelicDrop(RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider)
        {
            if (runDefinition == null || runDefinition.RelicDropChance <= 0)
            {
                return false;
            }

            return randomProvider.Range(0, 100) < runDefinition.RelicDropChance;
        }

        /// <summary>
        /// 報酬候補一覧を構築する
        /// </summary>
        private static List<RuntimeRewardEntry> BuildRewardCandidates(BattleSceneState state, RuntimeRunDefinition runDefinition)
        {
            List<RuntimeRewardEntry> candidates = new List<RuntimeRewardEntry>();
            if (runDefinition == null)
            {
                return candidates;
            }

            int currentFloor = GetCurrentFloor(state);
            for (int i = 0; i < runDefinition.RewardPool.Count; i++)
            {
                RuntimeRewardEntry entry = runDefinition.RewardPool[i];
                if (entry == null || entry.Card == null)
                {
                    continue;
                }

                if (currentFloor < entry.MinFloor || currentFloor > entry.MaxFloor)
                {
                    continue;
                }

                candidates.Add(entry);
            }

            return candidates;
        }

        /// <summary>
        /// 現在階層を取得する
        /// </summary>
        private static int GetCurrentFloor(BattleSceneState state)
        {
            if (state == null ||
                state.CurrentNodeIndex < 0 ||
                state.CurrentNodeIndex >= state.Nodes.Count)
            {
                return 1;
            }

            return state.Nodes[state.CurrentNodeIndex].Floor;
        }

        /// <summary>
        /// 重み付き報酬候補を選択する
        /// </summary>
        private static RuntimeRewardEntry SelectWeightedEntry(
            IReadOnlyList<RuntimeRewardEntry> entries,
            IBattleRandomProvider randomProvider)
        {
            if (entries == null || entries.Count == 0)
            {
                return null;
            }

            int totalWeight = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                totalWeight += Mathf.Max(0, entries[i].Weight);
            }

            if (totalWeight <= 0)
            {
                return entries[0];
            }

            int roll = randomProvider.Range(0, totalWeight);
            int currentWeight = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                currentWeight += Mathf.Max(0, entries[i].Weight);
                if (roll < currentWeight)
                {
                    return entries[i];
                }
            }

            return entries[entries.Count - 1];
        }
    }
}
