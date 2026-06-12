using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// 戦闘内イベント通知クラス
    /// </summary>
    public sealed class BattleCombatEventService : IBattleCombatEventService
    {
        public void OnCombatStart(BattleSceneState state)
        {
        }

        public void OnPlayerTurnStart(BattleSceneState state)
        {
        }

        public void OnPlayerTurnEnd(BattleSceneState state)
        {
        }

        public void OnCardPlayed(BattleSceneState state, RuntimeCard card, BattleCardResolutionResult result)
        {
        }

        public void OnPlayerDamaged(BattleSceneState state, int damage)
        {
        }
    }
}
