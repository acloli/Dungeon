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
            // 新しいプレイヤーターンを通知する前にターン単位の発火状態をリセットする
            state?.ClearTurnRelicEffectActivations();
            _relicService.ApplyEffects(state, RelicTriggerType.PlayerTurnStart);
        }

        public void OnPlayerTurnEnd(BattleSceneState state)
        {
            _relicService.ApplyEffects(state, RelicTriggerType.PlayerTurnEnd);
        }

        public void OnCardPlayed(BattleSceneState state, RuntimeCard card, BattleCardResolutionResult result)
        {
            RelicTriggerContext context = new RelicTriggerContext(
                RelicTriggerType.CardPlayed,
                playedCard: card);
            _relicService.ApplyEffects(state, context);
        }

        public void OnPlayerDamaged(BattleSceneState state, int damage)
        {
            _relicService.ApplyEffects(state, RelicTriggerType.PlayerDamaged);
        }

        public void OnShuffle(BattleSceneState state)
        {
            _relicService.ApplyEffects(state, RelicTriggerType.Shuffle);
        }

        public void OnCardExhausted(BattleSceneState state, RuntimeCard card)
        {
            _relicService.ApplyEffects(state, RelicTriggerType.CardExhausted);
        }

        public void OnLoseHp(BattleSceneState state, int amount)
        {
            _relicService.ApplyEffects(state, RelicTriggerType.LoseHp);
        }
    }
}
