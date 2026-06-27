using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneの敵行動選択と対象補正を扱うクラス
    /// </summary>
    public sealed class BattleEnemyActionSelector : IBattleEnemyActionSelector
    {
        /// <summary>
        /// 現在ターンの敵行動を選出する
        /// </summary>
        public RuntimeEnemyAction SelectEnemyAction(BattleEnemyState enemyState, IBattleRandomProvider randomProvider)
        {
            if (enemyState == null || enemyState.Enemy == null)
            {
                return null;
            }

            IReadOnlyList<RuntimeEnemyAction> actions = enemyState.Enemy.Actions;
            if (actions == null || actions.Count == 0)
            {
                return null;
            }

            List<RuntimeEnemyAction> openingActions = FilterActions(actions, RepeatRule.OpeningOnly);
            if (enemyState.TurnCount == 0 && openingActions.Count > 0)
            {
                return openingActions[0];
            }

            List<RuntimeEnemyAction> repeatActions = FilterActions(actions, RepeatRule.RepeatAfterOpening);
            if (enemyState.TurnCount > 0 && repeatActions.Count > 0)
            {
                return repeatActions[0];
            }

            List<RuntimeEnemyAction> afterOpeningRandomActions = FilterActions(actions, RepeatRule.AfterOpeningRandom);
            if (enemyState.TurnCount > 0 && afterOpeningRandomActions.Count > 0)
            {
                int index = randomProvider.Range(0, afterOpeningRandomActions.Count);
                return afterOpeningRandomActions[index];
            }

            List<RuntimeEnemyAction> randomActions = FilterActions(actions, RepeatRule.Random);
            if (randomActions.Count > 0)
            {
                int index = randomProvider.Range(0, randomActions.Count);
                return randomActions[index];
            }

            List<RuntimeEnemyAction> cycleActions = FilterActions(actions, RepeatRule.Cycle);
            if (cycleActions.Count > 0)
            {
                RuntimeEnemyAction selected = cycleActions[enemyState.CycleIndex % cycleActions.Count];
                enemyState.CycleIndex++;
                return selected;
            }

            return actions[0];
        }

        /// <summary>
        /// 効果対象の敵一覧を取得する
        /// </summary>
        public IEnumerable<BattleEnemyState> GetTargetEnemies(BattleSceneState state, TargetSide targetSide)
        {
            if (state == null)
            {
                yield break;
            }

            if (targetSide == TargetSide.AllEnemies)
            {
                for (int i = 0; i < state.Enemies.Count; i++)
                {
                    BattleEnemyState enemyState = state.Enemies[i];
                    if (enemyState != null && !enemyState.IsDefeated)
                    {
                        yield return enemyState;
                    }
                }

                yield break;
            }

            BattleEnemyState selectedEnemy = GetSelectedEnemy(state);
            if (selectedEnemy != null)
            {
                yield return selectedEnemy;
            }
        }

        /// <summary>
        /// 選択中の生存敵を取得する
        /// </summary>
        public BattleEnemyState GetSelectedEnemy(BattleSceneState state)
        {
            if (state == null)
            {
                return null;
            }

            NormalizeSelectedEnemyIndex(state);
            if (state.SelectedEnemyIndex < 0 || state.SelectedEnemyIndex >= state.Enemies.Count)
            {
                return null;
            }

            BattleEnemyState enemyState = state.Enemies[state.SelectedEnemyIndex];
            return enemyState != null && !enemyState.IsDefeated ? enemyState : null;
        }

        /// <summary>
        /// 敵選択を生存敵へ補正する
        /// </summary>
        public void NormalizeSelectedEnemyIndex(BattleSceneState state)
        {
            if (state == null || state.Enemies.Count == 0)
            {
                return;
            }

            if (state.SelectedEnemyIndex >= 0 &&
                state.SelectedEnemyIndex < state.Enemies.Count &&
                state.Enemies[state.SelectedEnemyIndex] != null &&
                !state.Enemies[state.SelectedEnemyIndex].IsDefeated)
            {
                return;
            }

            for (int i = 0; i < state.Enemies.Count; i++)
            {
                if (state.Enemies[i] != null && !state.Enemies[i].IsDefeated)
                {
                    state.SelectedEnemyIndex = i;
                    return;
                }
            }
        }

        /// <summary>
        /// 反復規則ごとに行動を抽出する
        /// </summary>
        private static List<RuntimeEnemyAction> FilterActions(IReadOnlyList<RuntimeEnemyAction> actions, RepeatRule repeatRule)
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
    }
}
