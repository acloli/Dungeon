using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneの敵行動選択と対象補正を扱うインターフェース
    /// </summary>
    public interface IBattleEnemyActionSelector
    {
        /// <summary>
        /// 現在ターンの敵行動を選出する
        /// </summary>
        RuntimeEnemyAction SelectEnemyAction(BattleEnemyState enemyState, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 効果対象の敵一覧を取得する
        /// </summary>
        IEnumerable<BattleEnemyState> GetTargetEnemies(BattleSceneState state, TargetSide targetSide);

        /// <summary>
        /// 選択中の生存敵を取得する
        /// </summary>
        BattleEnemyState GetSelectedEnemy(BattleSceneState state);

        /// <summary>
        /// 敵選択を生存敵へ補正する
        /// </summary>
        void NormalizeSelectedEnemyIndex(BattleSceneState state);
    }
}
