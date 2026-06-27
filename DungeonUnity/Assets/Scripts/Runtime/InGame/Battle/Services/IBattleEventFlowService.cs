using System;
using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneのイベントフローを扱うインターフェース
    /// </summary>
    public interface IBattleEventFlowService
    {
        /// <summary>
        /// イベント画面を開く
        /// </summary>
        void OpenEvent(
            BattleSceneState state,
            RuntimeRunDefinition runDefinition,
            Action<BattleScenePage> setCurrentPage,
            Action openMap);

        /// <summary>
        /// イベント選択肢を適用する
        /// </summary>
        void SelectEventChoice(BattleSceneState state, int choiceId, Action openMap);
    }
}
