using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using UnityEngine;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleScene の基本ルールクラス
    /// </summary>
    public sealed class BattleSceneRules : IBattleSceneRules
    {
        /// <summary>
        /// Run 状態初期化
        /// </summary>
        public void InitializeRun(BattleSceneState state, RuntimeRunDefinition runDefinition)
        {
            if (state == null)
            {
                return;
            }

            state.PlayerMaxHp = runDefinition != null ? runDefinition.PlayerMaxHp : BattleSceneConstants.DefaultPlayerMaxHp;
            state.PlayerHp = state.PlayerMaxHp;
            state.PlayerEnergy = BattleSceneConstants.DefaultPlayerEnergy;
            state.PlayerBlock = 0;
            state.Gold = runDefinition != null ? runDefinition.StartingGold : BattleSceneConstants.DefaultStartingGold;
            state.CurrentNodeIndex = BattleSceneConstants.DefaultNodeIndex;
            state.CurrentEnemy = null;
            state.EnemyHp = 0;
            state.EnemyBlock = 0;
            state.BattleFinished = false;
            state.EnemyTurnCount = 0;
            state.EnemyCycleIndex = 0;
            state.SelectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
            state.RewardChoices.Clear();
            state.Deck.Clear();
            state.Hand.Clear();
            state.Nodes.Clear();
            state.PlayerStatuses.Clear();
            state.EnemyStatuses.Clear();

            if (runDefinition != null)
            {
                for (int i = 0; i < runDefinition.StarterDeck.Count; i++)
                {
                    RuntimeCard card = runDefinition.StarterDeck[i];
                    if (card != null)
                    {
                        state.Deck.Add(card);
                    }
                }

                for (int i = 0; i < runDefinition.Nodes.Count; i++)
                {
                    state.Nodes.Add(runDefinition.Nodes[i]);
                }
            }

            if (state.Nodes.Count == 0)
            {
                AddDefaultNodes(state.Nodes);
            }
        }

        /// <summary>
        /// 手札補充
        /// </summary>
        public void DrawHand(BattleSceneState state, IBattleRandomProvider randomProvider)
        {
            if (state == null)
            {
                return;
            }

            state.Hand.Clear();
            DrawCards(state, randomProvider, BattleSceneConstants.DefaultHandSize);
        }

        /// <summary>
        /// 敵選出
        /// </summary>
        public RuntimeEnemy SelectEnemy(RuntimeRunDefinition runDefinition, InGameNodeType nodeType, IBattleRandomProvider randomProvider)
        {
            if (runDefinition == null ||
                !runDefinition.EncountersByNodeType.TryGetValue(nodeType, out IReadOnlyList<RuntimeEncounterEntry> encounters) ||
                encounters == null ||
                encounters.Count == 0)
            {
                return CreateFallbackEnemy(nodeType);
            }

            RuntimeEncounterEntry selected = SelectWeightedEntry(encounters, randomProvider);
            return selected != null ? selected.Enemy : CreateFallbackEnemy(nodeType);
        }

        /// <summary>
        /// 敵初期HP取得
        /// </summary>
        public int RollEnemyHp(RuntimeEnemy enemy, IBattleRandomProvider randomProvider)
        {
            if (enemy == null)
            {
                return BattleSceneConstants.DefaultEnemyHp;
            }

            int minHp = Mathf.Max(1, enemy.HpMin);
            int maxHp = Mathf.Max(minHp, enemy.HpMax);
            if (minHp == maxHp)
            {
                return maxHp;
            }

            return randomProvider.Range(minHp, maxHp + 1);
        }

        /// <summary>
        /// 報酬候補選出
        /// </summary>
        public IReadOnlyList<RuntimeCard> SelectRewardChoices(BattleSceneState state, RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider)
        {
            List<RuntimeCard> rewards = new List<RuntimeCard>();
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

                rewards.Add(selected.Card);
                candidates.Remove(selected);
            }

            if (rewards.Count > 0)
            {
                return rewards;
            }

            // 報酬定義が不足している場合は、現在デッキから重複を避けて補完する。
            HashSet<int> pickedCardIds = new HashSet<int>();
            for (int i = 0; i < state.Deck.Count && rewards.Count < BattleSceneConstants.DefaultRewardChoiceCount; i++)
            {
                RuntimeCard card = state.Deck[i];
                if (card == null || !pickedCardIds.Add(card.Id))
                {
                    continue;
                }

                rewards.Add(card);
            }

            return rewards;
        }

        /// <summary>
        /// 使用可否判定
        /// </summary>
        public bool CanPlayCard(BattleSceneState state, RuntimeCard card)
        {
            if (state == null || card == null)
            {
                return false;
            }

            return state.PlayerEnergy >= card.Cost;
        }

        /// <summary>
        /// カード適用
        /// </summary>
        public BattleCardResolutionResult PlayCard(BattleSceneState state, RuntimeCard card, IBattleRandomProvider randomProvider)
        {
            if (state == null || card == null)
            {
                return default;
            }

            state.PlayerEnergy -= card.Cost;

            int totalDamage = 0;
            int totalBlock = 0;
            int totalDraw = 0;
            IReadOnlyList<RuntimeCardEffect> effects = card.Effects;
            for (int i = 0; i < effects.Count; i++)
            {
                RuntimeCardEffect effect = effects[i];
                switch (effect.EffectType)
                {
                    case BattleEffectType.DealDamage:
                        totalDamage += ResolveCardDamage(state, effect);
                        break;
                    case BattleEffectType.GainBlock:
                        state.PlayerBlock += effect.Value;
                        totalBlock += effect.Value;
                        break;
                    case BattleEffectType.ApplyStatus:
                        ApplyCardStatus(state, effect);
                        break;
                    case BattleEffectType.DrawCards:
                        DrawCards(state, randomProvider, effect.Value);
                        totalDraw += effect.Value;
                        break;
                }
            }

            return new BattleCardResolutionResult(totalDamage, totalBlock, totalDraw);
        }

        /// <summary>
        /// 敵ターン解決
        /// </summary>
        public BattleEnemyTurnResult ResolveEnemyTurn(BattleSceneState state, IBattleRandomProvider randomProvider)
        {
            if (state == null || state.CurrentEnemy == null)
            {
                return default;
            }

            // プレイヤーターン終了時点で切れる状態を先に整理する。
            TickExpiringStatuses(state.PlayerStatuses);
            state.EnemyBlock = 0;

            RuntimeEnemyAction action = SelectEnemyAction(state, randomProvider);
            if (action == null)
            {
                state.PlayerBlock = 0;
                return default;
            }

            int damageDealt = ResolveEnemyDamage(state, action);
            if (action.Block > 0)
            {
                state.EnemyBlock += action.Block;
            }

            ApplyStatus(state.PlayerStatuses, action.StatusType, action.StatusValue);
            ApplyStatus(state.EnemyStatuses, action.BuffType, action.BuffValue);

            state.EnemyTurnCount++;
            TickExpiringStatuses(state.EnemyStatuses);
            state.PlayerBlock = 0;

            return new BattleEnemyTurnResult(action, damageDealt);
        }

        /// <summary>
        /// 休憩適用
        /// </summary>
        public void ApplyRest(BattleSceneState state)
        {
            if (state == null)
            {
                return;
            }

            state.PlayerHp = Mathf.Min(state.PlayerMaxHp, state.PlayerHp + BattleSceneConstants.RestHealAmount);
        }

        /// <summary>
        /// 購入適用
        /// </summary>
        public bool ApplyShopPurchase(BattleSceneState state, IBattleRandomProvider randomProvider)
        {
            if (state == null || state.Deck.Count == 0)
            {
                return false;
            }

            if (state.Gold < BattleSceneConstants.ShopPurchaseCost)
            {
                return false;
            }

            state.Gold -= BattleSceneConstants.ShopPurchaseCost;
            int index = randomProvider.Range(0, state.Deck.Count);
            state.Deck.Add(state.Deck[index]);
            return true;
        }

        /// <summary>
        /// カードのダメージ効果を解決する
        /// </summary>
        private static int ResolveCardDamage(BattleSceneState state, RuntimeCardEffect effect)
        {
            int hitCount = Math.Max(1, effect.HitCount);
            int totalDamage = 0;
            for (int i = 0; i < hitCount; i++)
            {
                int damage = ApplyOutgoingModifiers(effect.Value, state.PlayerStatuses);
                damage = ApplyIncomingModifiers(damage, state.EnemyStatuses);
                totalDamage += ApplyDamageToEnemy(state, damage);
            }

            return totalDamage;
        }

        /// <summary>
        /// カードの状態付与を解決する
        /// </summary>
        private static void ApplyCardStatus(BattleSceneState state, RuntimeCardEffect effect)
        {
            if (effect.TargetSide == BattleTargetSide.Self)
            {
                ApplyStatus(state.PlayerStatuses, effect.StatusType, effect.StatusValue);
                return;
            }

            if (effect.TargetSide == BattleTargetSide.Enemy || effect.TargetSide == BattleTargetSide.AllEnemies)
            {
                ApplyStatus(state.EnemyStatuses, effect.StatusType, effect.StatusValue);
            }
        }

        /// <summary>
        /// 敵のダメージ行動を解決する
        /// </summary>
        private static int ResolveEnemyDamage(BattleSceneState state, RuntimeEnemyAction action)
        {
            int hitCount = Math.Max(1, action.HitCount);
            int totalDamage = 0;
            for (int i = 0; i < hitCount; i++)
            {
                int damage = ApplyOutgoingModifiers(action.Damage, state.EnemyStatuses);
                damage = ApplyIncomingModifiers(damage, state.PlayerStatuses);
                totalDamage += ApplyDamageToPlayer(state, damage);
            }

            return totalDamage;
        }

        /// <summary>
        /// プレイヤーへのダメージをBlock込みで適用する
        /// </summary>
        private static int ApplyDamageToPlayer(BattleSceneState state, int damage)
        {
            int remainingDamage = Mathf.Max(0, damage - state.PlayerBlock);
            state.PlayerBlock = Mathf.Max(0, state.PlayerBlock - damage);
            state.PlayerHp -= remainingDamage;
            return remainingDamage;
        }

        /// <summary>
        /// 敵へのダメージをBlock込みで適用する
        /// </summary>
        private static int ApplyDamageToEnemy(BattleSceneState state, int damage)
        {
            int remainingDamage = Mathf.Max(0, damage - state.EnemyBlock);
            state.EnemyBlock = Mathf.Max(0, state.EnemyBlock - damage);
            state.EnemyHp -= remainingDamage;
            return remainingDamage;
        }

        /// <summary>
        /// 与ダメージ側補正
        /// </summary>
        private static int ApplyOutgoingModifiers(int baseDamage, IReadOnlyDictionary<BattleStatusType, int> statuses)
        {
            int damage = Mathf.Max(0, baseDamage);
            if (TryGetStatusValue(statuses, BattleStatusType.Weak, out int weakValue) && weakValue > 0)
            {
                damage = Mathf.FloorToInt(damage * 0.75f);
            }

            damage += GetBuffValue(statuses);
            return Mathf.Max(0, damage);
        }

        /// <summary>
        /// 被ダメージ側補正
        /// </summary>
        private static int ApplyIncomingModifiers(int damage, IReadOnlyDictionary<BattleStatusType, int> statuses)
        {
            int result = Mathf.Max(0, damage);
            if (TryGetStatusValue(statuses, BattleStatusType.Vulnerable, out int vulnerableValue) && vulnerableValue > 0)
            {
                result = Mathf.CeilToInt(result * 1.5f);
            }

            return Mathf.Max(0, result);
        }

        /// <summary>
        /// ステータス付与
        /// </summary>
        private static void ApplyStatus(IDictionary<BattleStatusType, int> statuses, BattleStatusType statusType, int value)
        {
            if (statuses == null || statusType == BattleStatusType.None || value <= 0)
            {
                return;
            }

            if (statuses.TryGetValue(statusType, out int currentValue))
            {
                statuses[statusType] = currentValue + value;
                return;
            }

            statuses[statusType] = value;
        }

        /// <summary>
        /// ターンで自然減衰する状態を更新する
        /// </summary>
        private static void TickExpiringStatuses(IDictionary<BattleStatusType, int> statuses)
        {
            if (statuses == null || statuses.Count == 0)
            {
                return;
            }

            List<BattleStatusType> keys = new List<BattleStatusType>(statuses.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                BattleStatusType statusType = keys[i];
                if (!ShouldExpire(statusType))
                {
                    continue;
                }

                int nextValue = statuses[statusType] - BattleSceneConstants.DefaultStatusDuration;
                if (nextValue > 0)
                {
                    statuses[statusType] = nextValue;
                    continue;
                }

                statuses.Remove(statusType);
            }
        }

        /// <summary>
        /// 自然減衰対象判定
        /// </summary>
        private static bool ShouldExpire(BattleStatusType statusType)
        {
            return statusType == BattleStatusType.Weak ||
                   statusType == BattleStatusType.Vulnerable ||
                   statusType == BattleStatusType.Slimed;
        }

        /// <summary>
        /// バフ値合算
        /// </summary>
        private static int GetBuffValue(IReadOnlyDictionary<BattleStatusType, int> statuses)
        {
            int total = 0;
            if (TryGetStatusValue(statuses, BattleStatusType.Strength, out int strengthValue))
            {
                total += strengthValue;
            }
            if (TryGetStatusValue(statuses, BattleStatusType.Ritual, out int ritualValue))
            {
                total += ritualValue;
            }
            if (TryGetStatusValue(statuses, BattleStatusType.Enrage, out int enrageValue))
            {
                total += enrageValue;
            }

            return total;
        }

        /// <summary>
        /// ステータス値取得
        /// </summary>
        private static bool TryGetStatusValue(IReadOnlyDictionary<BattleStatusType, int> statuses, BattleStatusType statusType, out int value)
        {
            value = 0;
            return statuses != null && statuses.TryGetValue(statusType, out value);
        }

        /// <summary>
        /// 指定枚数だけ手札へ追加する
        /// </summary>
        private static void DrawCards(BattleSceneState state, IBattleRandomProvider randomProvider, int drawCount)
        {
            if (state == null || state.Deck.Count == 0 || drawCount <= 0)
            {
                return;
            }

            int actualDrawCount = Mathf.Min(drawCount, state.Deck.Count);
            for (int i = 0; i < actualDrawCount; i++)
            {
                int index = randomProvider.Range(0, state.Deck.Count);
                RuntimeCard card = state.Deck[index];
                if (card != null)
                {
                    state.Hand.Add(card);
                }
            }
        }

        /// <summary>
        /// 現在の敵行動を選出する
        /// </summary>
        private static RuntimeEnemyAction SelectEnemyAction(BattleSceneState state, IBattleRandomProvider randomProvider)
        {
            IReadOnlyList<RuntimeEnemyAction> actions = state.CurrentEnemy.Actions;
            if (actions == null || actions.Count == 0)
            {
                return null;
            }

            List<RuntimeEnemyAction> openingActions = FilterActions(actions, BattleEnemyRepeatRule.OpeningOnly);
            if (state.EnemyTurnCount == 0 && openingActions.Count > 0)
            {
                return openingActions[0];
            }

            List<RuntimeEnemyAction> repeatActions = FilterActions(actions, BattleEnemyRepeatRule.RepeatAfterOpening);
            if (state.EnemyTurnCount > 0 && repeatActions.Count > 0)
            {
                return repeatActions[0];
            }

            List<RuntimeEnemyAction> afterOpeningRandomActions = FilterActions(actions, BattleEnemyRepeatRule.AfterOpeningRandom);
            if (state.EnemyTurnCount > 0 && afterOpeningRandomActions.Count > 0)
            {
                int index = randomProvider.Range(0, afterOpeningRandomActions.Count);
                return afterOpeningRandomActions[index];
            }

            List<RuntimeEnemyAction> randomActions = FilterActions(actions, BattleEnemyRepeatRule.Random);
            if (randomActions.Count > 0)
            {
                int index = randomProvider.Range(0, randomActions.Count);
                return randomActions[index];
            }

            List<RuntimeEnemyAction> cycleActions = FilterActions(actions, BattleEnemyRepeatRule.Cycle);
            if (cycleActions.Count > 0)
            {
                RuntimeEnemyAction selected = cycleActions[state.EnemyCycleIndex % cycleActions.Count];
                state.EnemyCycleIndex++;
                return selected;
            }

            return actions[0];
        }

        /// <summary>
        /// 反復規則ごとに行動を抽出する
        /// </summary>
        private static List<RuntimeEnemyAction> FilterActions(IReadOnlyList<RuntimeEnemyAction> actions, BattleEnemyRepeatRule repeatRule)
        {
            List<RuntimeEnemyAction> filtered = new List<RuntimeEnemyAction>();
            for (int i = 0; i < actions.Count; i++)
            {
                RuntimeEnemyAction action = actions[i];
                if (action.RepeatRule == repeatRule)
                {
                    filtered.Add(action);
                }
            }

            return filtered;
        }

        /// <summary>
        /// 報酬候補一覧構築
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
        /// 現在階層取得
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
        /// 重み付き候補選択
        /// </summary>
        private static T SelectWeightedEntry<T>(IReadOnlyList<T> entries, IBattleRandomProvider randomProvider)
            where T : class
        {
            if (entries == null || entries.Count == 0)
            {
                return null;
            }

            int totalWeight = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                switch (entries[i])
                {
                    case RuntimeEncounterEntry encounter:
                        totalWeight += Mathf.Max(0, encounter.Weight);
                        break;
                    case RuntimeRewardEntry reward:
                        totalWeight += Mathf.Max(0, reward.Weight);
                        break;
                }
            }

            if (totalWeight <= 0)
            {
                return entries[0];
            }

            int roll = randomProvider.Range(0, totalWeight);
            int currentWeight = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                int weight = entries[i] switch
                {
                    RuntimeEncounterEntry encounter => Mathf.Max(0, encounter.Weight),
                    RuntimeRewardEntry reward => Mathf.Max(0, reward.Weight),
                    _ => 0
                };

                currentWeight += weight;
                if (roll < currentWeight)
                {
                    return entries[i];
                }
            }

            return entries[entries.Count - 1];
        }

        /// <summary>
        /// 既定ノード補完
        /// </summary>
        private static void AddDefaultNodes(List<RuntimeMapNode> nodes)
        {
            nodes.Add(new RuntimeMapNode(1, "default_01", 1, InGameNodeType.Battle, BattleSceneConstants.DefaultBattleNodeLabel, string.Empty, new[] { 1 }));
            nodes.Add(new RuntimeMapNode(2, "default_02", 2, InGameNodeType.RestShop, BattleSceneConstants.DefaultRestNodeLabel, string.Empty, new[] { 2 }));
            nodes.Add(new RuntimeMapNode(3, "default_03", 3, InGameNodeType.Battle, BattleSceneConstants.DefaultBattleNodeTwoLabel, string.Empty, new[] { 3 }));
            nodes.Add(new RuntimeMapNode(4, "default_04", 4, InGameNodeType.EliteBattle, BattleSceneConstants.DefaultEliteNodeLabel, string.Empty, new[] { 4 }));
            nodes.Add(new RuntimeMapNode(5, "default_05", 5, InGameNodeType.RestShop, BattleSceneConstants.DefaultShopNodeLabel, string.Empty, new[] { 5 }));
            nodes.Add(new RuntimeMapNode(6, "default_06", 6, InGameNodeType.Boss, BattleSceneConstants.DefaultBossNodeLabel, string.Empty, Array.Empty<int>()));
        }

        /// <summary>
        /// データ不足時のフォールバック敵生成
        /// </summary>
        private static RuntimeEnemy CreateFallbackEnemy(InGameNodeType nodeType)
        {
            int baseHp = nodeType == InGameNodeType.Boss ? 60 : BattleSceneConstants.DefaultEnemyHp;
            int damage = nodeType == InGameNodeType.EliteBattle ? 8 : 4;
            RuntimeEnemyAction action = new RuntimeEnemyAction(
                1,
                "Attack",
                damage,
                1,
                0,
                BattleStatusType.None,
                0,
                BattleStatusType.None,
                0,
                BattleEnemyRepeatRule.RepeatAfterOpening);

            return new RuntimeEnemy(
                0,
                "fallback_enemy",
                BattleSceneConstants.UnknownEnemyName,
                string.Empty,
                nodeType.ToString(),
                baseHp,
                baseHp,
                20,
                new[] { action });
        }
    }
}
