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
            ApplyRelicTrigger(state, RelicTriggerType.CombatStart);
        }

        public void OnPlayerTurnStart(BattleSceneState state)
        {
            // 新しいプレイヤーターンを通知する前にターン単位の発火状態をリセットする
            state?.ClearTurnRelicEffectActivations();
            ApplyRelicTrigger(state, RelicTriggerType.PlayerTurnStart);
        }

        public void OnPlayerTurnEnd(BattleSceneState state)
        {
            ApplyRelicTrigger(state, RelicTriggerType.PlayerTurnEnd);
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
            ApplyRelicTrigger(state, RelicTriggerType.PlayerDamaged);
        }

        public void OnShuffle(BattleSceneState state)
        {
            ApplyRelicTrigger(state, RelicTriggerType.Shuffle);
        }

        public void OnCardExhausted(BattleSceneState state, RuntimeCard card)
        {
            ApplyRelicTrigger(state, RelicTriggerType.CardExhausted);
        }

        public void OnLoseHp(BattleSceneState state, int amount)
        {
            ApplyRelicTrigger(state, RelicTriggerType.LoseHp);
        }

        private void ApplyRelicTrigger(BattleSceneState state, RelicTriggerType triggerType)
        {
            RelicTriggerContext context = new RelicTriggerContext(triggerType);
            _relicService.ApplyEffects(state, context);
        }
    }
}
