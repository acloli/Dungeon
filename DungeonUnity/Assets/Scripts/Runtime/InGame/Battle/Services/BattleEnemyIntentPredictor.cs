using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// 次の敵ターンにおける各敵の攻撃値を予測するクラス
    /// </summary>
    public static class BattleEnemyIntentPredictor
    {
        public static Dictionary<BattleEnemyState, int> PredictDamagePerHit(BattleSceneState state)
        {
            Dictionary<BattleEnemyState, int> predictions = new Dictionary<BattleEnemyState, int>();
            if (state == null || state.CurrentPage != BattleScenePage.Battle)
            {
                return predictions;
            }

            Dictionary<StatusType, int> playerStatuses = new Dictionary<StatusType, int>(state.PlayerStatuses);
            BattleCombatRules.TickExpiringStatuses(playerStatuses);

            List<BattleEnemyState> orderedEnemies = state.Enemies
                .Where(enemy => enemy != null && !enemy.IsDefeated && enemy.Enemy != null)
                .OrderBy(enemy => enemy.SlotIndex)
                .ToList();
            for (int i = 0; i < orderedEnemies.Count; i++)
            {
                BattleEnemyState enemyState = orderedEnemies[i];
                IReadOnlyList<RuntimeEnemyAction> actions = enemyState.Enemy.Actions;
                RuntimeEnemyAction action = enemyState.PlannedAction;
                if (action == null)
                {
                    if (actions != null && actions.Count > 0)
                    {
                        throw new InvalidOperationException(
                            $"Enemy {enemyState.Enemy.Id} reached intent prediction without a planned action.");
                    }

                    continue;
                }

                Dictionary<StatusType, int> enemyStatuses = new Dictionary<StatusType, int>(enemyState.Statuses);
                Dictionary<BuffType, int> enemyBuffs = new Dictionary<BuffType, int>(enemyState.Buffs);
                int damagePerHit = 0;
                if (action.Damage > 0)
                {
                    damagePerHit =
                        BattleCombatRules.ApplyOutgoingModifiers(action.Damage, enemyStatuses, enemyBuffs);
                    damagePerHit = BattleCombatRules.ApplyIncomingModifiers(damagePerHit, playerStatuses);
                }

                predictions[enemyState] = damagePerHit;

                BattleCombatRules.ApplyStatus(playerStatuses, action.StatusType, action.StatusValue);
                BattleCombatRules.ApplyBuff(enemyBuffs, action.BuffType, action.BuffValue);
                BattleCombatRules.TickExpiringStatuses(enemyStatuses);
            }

            return predictions;
        }
    }
}
