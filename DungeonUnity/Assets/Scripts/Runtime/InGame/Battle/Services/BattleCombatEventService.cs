using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// 戦闘内イベント通知クラス
    /// </summary>
    public sealed class BattleCombatEventService : IBattleCombatEventService
    {
        private readonly IBattleRelicService _relicService;

        public BattleCombatEventService(IBattleRelicService relicService)
        {
            _relicService = relicService;
        }

        public void OnCombatStart(BattleSceneState state)
        {
            _relicService.ApplyEffects(state, RelicTriggerType.CombatStart);
        }

        public void OnPlayerTurnStart(BattleSceneState state)
        {
            _relicService.ApplyEffects(state, RelicTriggerType.PlayerTurnStart);
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
