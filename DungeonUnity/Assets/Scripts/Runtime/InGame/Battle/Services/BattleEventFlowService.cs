using System;
using Dungeon.Runtime.InGame.Battle.Model;
using TFramework.Debug;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneのイベント子フローを扱うクラス
    /// </summary>
    public sealed class BattleEventFlowService : IBattleEventFlowService
    {
        private readonly IBattleRandomProvider _randomProvider;
        private readonly IBattleEventService _eventService;

        public BattleEventFlowService(IBattleRandomProvider randomProvider, IBattleEventService eventService)
        {
            _randomProvider = randomProvider;
            _eventService = eventService;
        }

        /// <summary>
        /// イベント画面を開く
        /// </summary>
        public void OpenEvent(
            BattleSceneState state,
            RuntimeRunDefinition runDefinition,
            Action<BattleScenePage> setCurrentPage,
            Action openMap)
        {
            if (runDefinition == null || runDefinition.PossibleEvents == null || runDefinition.PossibleEvents.Count == 0)
            {
                TLogger.Warning(BattleSceneConstants.NoEventAvailable, "Battle");
                openMap();
                return;
            }

            int index = _randomProvider.Range(0, runDefinition.PossibleEvents.Count);
            state.CurrentEvent = runDefinition.PossibleEvents[index];
            setCurrentPage(BattleScenePage.Event);
            state.EventMessage = string.Format(
                BattleSceneConstants.EventStateFormat,
                state.PlayerHp,
                state.PlayerMaxHp,
                state.Gold);
        }

        /// <summary>
        /// イベント選択肢を適用する
        /// </summary>
        public void SelectEventChoice(BattleSceneState state, int choiceId, Action openMap)
        {
            if (state.CurrentEvent != null)
            {
                _eventService.ApplyEventChoice(state, state.CurrentEvent, choiceId);
            }

            state.CurrentEvent = null;
            state.EventMessage = string.Empty;
            openMap();
        }
    }
}
