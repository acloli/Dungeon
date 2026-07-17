using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// レリック効果条件を評価するクラス
    /// </summary>
    public sealed class RelicConditionEvaluator
    {
        /// <summary>
        /// すべての条件を満たすか評価する
        /// </summary>
        public bool EvaluateAll(
            BattleSceneState state,
            RelicTriggerContext context,
            IReadOnlyList<RuntimeRelicCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0)
            {
                return true;
            }

            if (state == null || context == null)
            {
                return false;
            }

            for (int i = 0; i < conditions.Count; i++)
            {
                if (!EvaluateCondition(state, context, conditions[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 単一条件を評価する
        /// </summary>
        private static bool EvaluateCondition(
            BattleSceneState state,
            RelicTriggerContext context,
            RuntimeRelicCondition condition)
        {
            if (condition == null)
            {
                return false;
            }

            switch (condition.ConditionType)
            {
                case RelicConditionType.PlayedCardCostEquals:
                    return context.PlayedCard != null && context.PlayedCard.Cost == condition.CardCost;

                case RelicConditionType.PlayerHpPercentAtMost:
                    return state.PlayerMaxHp > 0
                           && (long)state.PlayerHp * 100 <= (long)state.PlayerMaxHp * condition.HpPercent;

                case RelicConditionType.NodeTypeEquals:
                    return context.NodeType.HasValue
                           && condition.NodeType.HasValue
                           && context.NodeType.Value == condition.NodeType.Value;

                default:
                    return false;
            }
        }
    }
}
