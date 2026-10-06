using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;
using UnityEngine;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneの戦闘解算を扱うクラス
    /// </summary>
    public sealed class BattleCombatResolver : IBattleCombatResolver
    {
        private readonly IBattleDeckService _deckService;
        private readonly IBattleEnemyActionSelector _enemyActionSelector;

        public BattleCombatResolver(IBattleDeckService deckService, IBattleEnemyActionSelector enemyActionSelector)
        {
            _deckService = deckService;
            _enemyActionSelector = enemyActionSelector;
        }

        /// <summary>
        /// カードの使用可否を判定する
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
        /// カード使用結果を解決する
        /// </summary>
        public BattleCardResolutionResult PlayCard(BattleSceneState state, int handIndex,
            IBattleRandomProvider randomProvider)
        {
            if (state == null || handIndex < 0 || handIndex >= state.Hand.Count)
            {
                return default;
            }

            RuntimeCard card = state.Hand[handIndex];
            if (card == null)
            {
                return default;
            }

            _enemyActionSelector.NormalizeSelectedEnemyIndex(state);
            state.Hand.RemoveAt(handIndex);
            state.PlayerEnergy -= card.Cost;
            MovePlayedCardToDestinationPile(state, card);

            int totalDamage = 0;
            int totalBlock = 0;
            int totalDraw = 0;
            IReadOnlyList<RuntimeCardEffect> effects = card.Effects;
            for (int i = 0; i < effects.Count; i++)
            {
                RuntimeCardEffect effect = effects[i];
                switch (effect.EffectType)
                {
                    case EffectType.DealDamage:
                        totalDamage += ResolveCardDamage(state, effect);
                        break;
                    case EffectType.GainBlock:
                        state.PlayerBlock += effect.Value;
                        totalBlock += effect.Value;
                        break;
                    case EffectType.GainEnergy:
                        state.PlayerEnergy += effect.Value;
                        break;
                    case EffectType.ApplyStatus:
                        ApplyCardStatus(state, effect);
                        break;
                    case EffectType.DrawCards:
                        totalDraw += _deckService.DrawCards(state, randomProvider, effect.Value);
                        break;
                }
            }

            return new BattleCardResolutionResult(totalDamage, totalBlock, totalDraw);
        }

        /// <summary>
        /// 生存敵の次の行動を一度だけ確定する
        /// </summary>
        public void PrepareEnemyActions(BattleSceneState state, IBattleRandomProvider randomProvider)
        {
            if (state == null)
            {
                return;
            }

            List<BattleEnemyState> orderedEnemies = state.Enemies
                .Where(enemy => enemy != null && !enemy.IsDefeated && enemy.Enemy != null)
                .OrderBy(enemy => enemy.SlotIndex)
                .ToList();
            for (int i = 0; i < orderedEnemies.Count; i++)
            {
                BattleEnemyState enemyState = orderedEnemies[i];
                IReadOnlyList<RuntimeEnemyAction> actions = enemyState.Enemy.Actions;
                if (actions == null || actions.Count == 0)
                {
                    enemyState.PlannedAction = null;
                    continue;
                }

                if (enemyState.PlannedAction != null)
                {
                    continue;
                }

                RuntimeEnemyAction plannedAction = _enemyActionSelector.SelectEnemyAction(enemyState, randomProvider);
                if (plannedAction == null)
                {
                    throw new InvalidOperationException(
                        $"Enemy {enemyState.Enemy.Id} has actions but no action could be planned.");
                }

                enemyState.PlannedAction = plannedAction;
            }
        }

        /// <summary>
        /// 敵ターン結果を解決する
        /// </summary>
        public BattleEnemyTurnResult ResolveEnemyTurn(BattleSceneState state)
        {
            if (state == null || state.Enemies.Count == 0)
            {
                return default;
            }

            for (int i = 0; i < state.Enemies.Count; i++)
            {
                BattleEnemyState enemyState = state.Enemies[i];
                IReadOnlyList<RuntimeEnemyAction> actions = enemyState?.Enemy?.Actions;
                if (enemyState != null && !enemyState.IsDefeated && actions != null && actions.Count > 0 &&
                    enemyState.PlannedAction == null)
                {
                    throw new InvalidOperationException(
                        $"Enemy {enemyState.Enemy.Id} reached resolution without a planned action.");
                }
            }

            BattleCombatRules.TickExpiringStatuses(state.PlayerStatuses);

            RuntimeEnemyAction lastAction = null;
            int totalDamage = 0;
            List<BattleEnemyState> orderedEnemies = state.Enemies
                .Where(enemy => enemy != null && !enemy.IsDefeated)
                .OrderBy(enemy => enemy.SlotIndex)
                .ToList();
            for (int i = 0; i < orderedEnemies.Count; i++)
            {
                BattleEnemyState enemyState = orderedEnemies[i];
                enemyState.Block = 0;
                RuntimeEnemyAction action = enemyState.PlannedAction;
                if (action == null)
                {
                    continue;
                }

                enemyState.PlannedAction = null;
                if (action.Damage > 0)
                {
                    totalDamage += ResolveEnemyDamage(state, enemyState, action);
                }

                if (action.Block > 0)
                {
                    enemyState.Block += action.Block;
                }

                BattleCombatRules.ApplyStatus(state.PlayerStatuses, action.StatusType, action.StatusValue);
                BattleCombatRules.ApplyBuff(enemyState.Buffs, action.BuffType, action.BuffValue);

                enemyState.TurnCount++;
                BattleCombatRules.TickExpiringStatuses(enemyState.Statuses);
                lastAction = action;
            }

            state.PlayerBlock = 0;
            SyncPrimaryEnemyState(state);

            return new BattleEnemyTurnResult(lastAction, totalDamage);
        }

        /// <summary>
        /// カードのダメージ効果を解決する
        /// </summary>
        private int ResolveCardDamage(BattleSceneState state, RuntimeCardEffect effect)
        {
            int hitCount = Math.Max(1, effect.HitCount);
            int totalDamage = 0;
            foreach (BattleEnemyState enemyState in _enemyActionSelector.GetTargetEnemies(state, effect.TargetSide))
            {
                for (int i = 0; i < hitCount; i++)
                {
                    int damage =
                        BattleCombatRules.ApplyOutgoingModifiers(effect.Value, state.PlayerStatuses, state.PlayerBuffs);
                    damage = BattleCombatRules.ApplyIncomingModifiers(damage, enemyState.Statuses);
                    totalDamage += ApplyDamageToEnemy(state, enemyState, damage);
                }
            }

            SyncPrimaryEnemyState(state);
            return totalDamage;
        }

        /// <summary>
        /// カードの状態付与を解決する
        /// </summary>
        private void ApplyCardStatus(BattleSceneState state, RuntimeCardEffect effect)
        {
            if (effect.TargetSide == TargetSide.Self)
            {
                BattleCombatRules.ApplyStatus(state.PlayerStatuses, effect.StatusType, effect.StatusValue);
                return;
            }

            foreach (BattleEnemyState enemyState in _enemyActionSelector.GetTargetEnemies(state, effect.TargetSide))
            {
                BattleCombatRules.ApplyStatus(enemyState.Statuses, effect.StatusType, effect.StatusValue);
            }

            SyncPrimaryEnemyState(state);
        }

        /// <summary>
        /// 使用したカードを解決前に遷移先Pileへ移動する
        /// </summary>
        private static void MovePlayedCardToDestinationPile(BattleSceneState state, RuntimeCard card)
        {
            if (card.ExhaustsOnPlay)
            {
                MoveCardToExhaust(state, card);
                return;
            }

            MoveCardToDiscard(state, card);
        }

        /// <summary>
        /// カードをDiscardPileへ移動する
        /// </summary>
        private static void MoveCardToDiscard(BattleSceneState state, RuntimeCard card)
        {
            state.DiscardPile.Add(card);
        }

        /// <summary>
        /// カードをExhaustPileへ移動する
        /// </summary>
        private static void MoveCardToExhaust(BattleSceneState state, RuntimeCard card)
        {
            state.ExhaustPile.Add(card);
        }

        /// <summary>
        /// 敵のダメージ行動を解決する
        /// </summary>
        private static int ResolveEnemyDamage(BattleSceneState state, BattleEnemyState enemyState,
            RuntimeEnemyAction action)
        {
            if (action.Damage <= 0)
            {
                return 0;
            }

            int hitCount = Math.Max(1, action.HitCount);
            int totalDamage = 0;
            for (int i = 0; i < hitCount; i++)
            {
                int damage =
                    BattleCombatRules.ApplyOutgoingModifiers(action.Damage, enemyState.Statuses, enemyState.Buffs);
                damage = BattleCombatRules.ApplyIncomingModifiers(damage, state.PlayerStatuses);
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
        private int ApplyDamageToEnemy(BattleSceneState state, BattleEnemyState enemyState, int damage)
        {
            int remainingDamage = Mathf.Max(0, damage - enemyState.Block);
            enemyState.Block = Mathf.Max(0, enemyState.Block - damage);
            enemyState.Hp -= remainingDamage;
            if (enemyState.Hp <= 0)
            {
                enemyState.Hp = 0;
                enemyState.IsDefeated = true;
                enemyState.PlannedAction = null;
                _enemyActionSelector.NormalizeSelectedEnemyIndex(state);
            }

            return remainingDamage;
        }

        /// <summary>
        /// 単体敵表示互換用stateを同期する
        /// </summary>
        private void SyncPrimaryEnemyState(BattleSceneState state)
        {
            BattleEnemyState selectedEnemy = _enemyActionSelector.GetSelectedEnemy(state);
            if (selectedEnemy == null)
            {
                state.ClearSelectedEnemyDisplay();
                return;
            }

            state.SyncSelectedEnemyDisplay(selectedEnemy, state.SelectedEnemyIndex);
        }
    }

    /// <summary>
    /// 戦闘予測と実行で共有する計算規則
    /// </summary>
    public static class BattleCombatRules
    {
        public static int ApplyOutgoingModifiers(
            int baseDamage,
            IReadOnlyDictionary<StatusType, int> statuses,
            IReadOnlyDictionary<BuffType, int> buffs)
        {
            int damage = Mathf.Max(0, baseDamage);
            if (TryGetStatusValue(statuses, StatusType.Weak, out int weakValue) && weakValue > 0)
            {
                damage = Mathf.FloorToInt(damage * 0.75f);
            }

            damage += GetBuffValue(buffs);
            return Mathf.Max(0, damage);
        }

        public static int ApplyIncomingModifiers(int damage, IReadOnlyDictionary<StatusType, int> statuses)
        {
            int result = Mathf.Max(0, damage);
            if (TryGetStatusValue(statuses, StatusType.Vulnerable, out int vulnerableValue) && vulnerableValue > 0)
            {
                result = Mathf.CeilToInt(result * 1.5f);
            }

            return Mathf.Max(0, result);
        }

        public static void ApplyStatus(IDictionary<StatusType, int> statuses, StatusType statusType, int value)
        {
            if (statuses == null || statusType == StatusType.None || value <= 0)
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

        public static void ApplyBuff(IDictionary<BuffType, int> buffs, BuffType buffType, int value)
        {
            if (buffs == null || buffType == BuffType.None || value <= 0)
            {
                return;
            }

            if (buffs.TryGetValue(buffType, out int currentValue))
            {
                buffs[buffType] = currentValue + value;
                return;
            }

            buffs[buffType] = value;
        }

        public static void TickExpiringStatuses(IDictionary<StatusType, int> statuses)
        {
            if (statuses == null || statuses.Count == 0)
            {
                return;
            }

            List<StatusType> keys = new List<StatusType>(statuses.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                StatusType statusType = keys[i];
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

        private static bool ShouldExpire(StatusType statusType)
        {
            return statusType == StatusType.Weak ||
                   statusType == StatusType.Vulnerable ||
                   statusType == StatusType.Slimed;
        }

        private static int GetBuffValue(IReadOnlyDictionary<BuffType, int> buffs)
        {
            int total = 0;
            if (TryGetBuffValue(buffs, BuffType.Strength, out int strengthValue))
            {
                total += strengthValue;
            }

            if (TryGetBuffValue(buffs, BuffType.Ritual, out int ritualValue))
            {
                total += ritualValue;
            }

            if (TryGetBuffValue(buffs, BuffType.Enrage, out int enrageValue))
            {
                total += enrageValue;
            }

            return total;
        }

        private static bool TryGetStatusValue(
            IReadOnlyDictionary<StatusType, int> statuses,
            StatusType statusType,
            out int value)
        {
            value = 0;
            return statuses != null && statuses.TryGetValue(statusType, out value);
        }

        private static bool TryGetBuffValue(IReadOnlyDictionary<BuffType, int> buffs, BuffType buffType, out int value)
        {
            value = 0;
            return buffs != null && buffs.TryGetValue(buffType, out value);
        }
    }
}
