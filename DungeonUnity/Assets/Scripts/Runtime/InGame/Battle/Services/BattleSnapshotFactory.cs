using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// UI表示用スナップショット構築クラス
    /// </summary>
    public sealed class BattleSnapshotFactory : IBattleSnapshotFactory
    {
        private readonly IBattleDisplayTextService _displayTextService;

        public BattleSnapshotFactory(IBattleDisplayTextService displayTextService)
        {
            _displayTextService = displayTextService;
        }

        public BattleSceneSnapshot CreateSnapshot(BattleSceneState state)
        {
            if (state == null)
            {
                return null;
            }

            return new BattleSceneSnapshot(
                state.CurrentPage,
                state.Nodes,
                state.Hand,
                state.RewardChoices,
                state.CurrentNodeIndex,
                state.PlayerMaxHp,
                state.PlayerHp,
                state.PlayerEnergy,
                state.PlayerBlock,
                state.Gold,
                state.CurrentEnemy,
                state.EnemyHp,
                state.EnemyBlock,
                state.BattleFinished,
                state.SelectedCardIndex,
                state.IsRestShopContinueEnabled,
                state.MapMessage,
                state.BattleHintMessage,
                state.RestShopMessage,
                state.ResultMessage,
                BuildEnemyIntent(state),
                BuildStatusViews(state.PlayerStatuses),
                BuildStatusViews(state.EnemyStatuses),
                BuildBuffViews(state.PlayerBuffs),
                BuildBuffViews(state.EnemyBuffs),
                BuildEnemyViews(state),
                state.SelectedEnemyIndex,
                BuildAvailableNodeIndices(state));
        }

        private BattleIntentViewModel BuildEnemyIntent(BattleSceneState state)
        {
            BattleEnemyState enemyState = GetSelectedEnemy(state);
            RuntimeEnemyAction action = SelectEnemyActionPreview(state, enemyState);
            if (action == null)
            {
                return null;
            }

            return new BattleIntentViewModel(
                action.IntentType,
                _displayTextService.GetIntentName(action.IntentType),
                action.Damage,
                action.HitCount,
                action.Block,
                action.StatusType,
                _displayTextService.GetStatusName(action.StatusType),
                action.StatusValue,
                action.BuffType,
                _displayTextService.GetBuffName(action.BuffType),
                action.BuffValue);
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

            return new BattleIntentViewModel(
                action.IntentType,
                _displayTextService.GetIntentName(action.IntentType),
                action.Damage,
                action.HitCount,
                action.Block,
                action.StatusType,
                _displayTextService.GetStatusName(action.StatusType),
                action.StatusValue,
                action.BuffType,
                _displayTextService.GetBuffName(action.BuffType),
                action.BuffValue);
        }

        private BattleEnemyState GetSelectedEnemy(BattleSceneState state)
        {
            if (state.SelectedEnemyIndex >= 0 && state.SelectedEnemyIndex < state.Enemies.Count)
            {
                BattleEnemyState enemyState = state.Enemies[state.SelectedEnemyIndex];
                if (enemyState != null && !enemyState.IsDefeated)
                {
                    return enemyState;
                }
            }

            for (int i = 0; i < state.Enemies.Count; i++)
            {
                BattleEnemyState enemyState = state.Enemies[i];
                if (enemyState != null && !enemyState.IsDefeated)
                {
                    state.SelectedEnemyIndex = i;
                    return enemyState;
                }
            }

            return null;
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
                if (CanMoveToNode(state, i))
                {
                    indices.Add(i);
                }
            }

            return indices;
        }

        private bool CanMoveToNode(BattleSceneState state, int index)
        {
            if (state.CurrentNodeIndex < 0)
            {
                return index == 0;
            }

            RuntimeMapNode currentNode = state.Nodes[state.CurrentNodeIndex];
            if (currentNode.NextNodeIndices != null && currentNode.NextNodeIndices.Count > 0)
            {
                for (int i = 0; i < currentNode.NextNodeIndices.Count; i++)
                {
                    if (currentNode.NextNodeIndices[i] == index)
                    {
                        return true;
                    }
                }

                return false;
            }

            return index == state.CurrentNodeIndex + 1;
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
    }
}
