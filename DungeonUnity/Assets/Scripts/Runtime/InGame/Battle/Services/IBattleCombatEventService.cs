using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// 戦闘内イベント通知インターフェース
    /// </summary>
    public interface IBattleCombatEventService
    {
        /// <summary>
        /// 戦闘開始通知
        /// </summary>
        void OnCombatStart(BattleSceneState state);

        /// <summary>
        /// プレイヤーターン開始通知
        /// </summary>
        void OnPlayerTurnStart(BattleSceneState state);

        /// <summary>
        /// プレイヤーターン終了通知
        /// </summary>
        void OnPlayerTurnEnd(BattleSceneState state);

        /// <summary>
        /// カード使用後通知
        /// </summary>
        void OnCardPlayed(BattleSceneState state, RuntimeCard card, BattleCardResolutionResult result);

        /// <summary>
        /// プレイヤー被ダメージ通知
        /// </summary>
        void OnPlayerDamaged(BattleSceneState state, int damage);
    }
}
