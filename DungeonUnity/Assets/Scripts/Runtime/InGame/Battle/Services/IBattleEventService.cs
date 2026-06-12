using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// イベント選択肢適用サービスインターフェース
    /// </summary>
    public interface IBattleEventService
    {
        /// <summary>
        /// イベント選択肢の効果を状態へ適用する
        /// </summary>
        void ApplyEventChoice(BattleSceneState state, RuntimeEvent evt, int choiceId);
    }
}
