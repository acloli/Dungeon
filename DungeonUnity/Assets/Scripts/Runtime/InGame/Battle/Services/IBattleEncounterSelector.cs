using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneの遭遇選択を扱うインターフェース
    /// </summary>
    public interface IBattleEncounterSelector
    {
        /// <summary>
        /// ノード種別に応じた遭遇編成を選択する
        /// </summary>
        RuntimeEncounterFormation SelectEncounterFormation(RuntimeRunDefinition runDefinition, InGameNodeType nodeType, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 敵の初期HPを決定する
        /// </summary>
        int RollEnemyHp(RuntimeEnemy enemy, IBattleRandomProvider randomProvider);
    }
}
