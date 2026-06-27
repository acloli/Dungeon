using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneの戦闘解算を扱うインターフェース
    /// </summary>
    public interface IBattleCombatResolver
    {
        /// <summary>
        /// カードの使用可否を判定する
        /// </summary>
        bool CanPlayCard(BattleSceneState state, RuntimeCard card);

        /// <summary>
        /// カード使用結果を解決する
        /// </summary>
        BattleCardResolutionResult PlayCard(BattleSceneState state, int handIndex, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 敵ターン結果を解決する
        /// </summary>
        BattleEnemyTurnResult ResolveEnemyTurn(BattleSceneState state, IBattleRandomProvider randomProvider);
    }
}
