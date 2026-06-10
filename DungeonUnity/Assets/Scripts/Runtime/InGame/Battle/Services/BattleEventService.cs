using Game.MasterData.Generated;
using TFramework.Debug;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// イベント選択肢の効果を状態へ適用するクラス
    /// </summary>
    public sealed class BattleEventService : IBattleEventService
    {
        /// <summary>
        /// 選択肢のeffectTypeに応じてプレイヤー状態を更新する
        /// </summary>
        public void ApplyEventChoice(Model.BattleSceneState state, Model.RuntimeEvent evt, int choiceId)
        {
            if (state == null || evt == null)
            {
                return;
            }

            Model.RuntimeEventChoice choice = FindChoice(evt, choiceId);
            if (choice == null)
            {
                TLogger.Warning($"EventChoice not found. eventId={evt.Id} choiceId={choiceId}", "Battle");
                return;
            }

            ApplyEffect(state, choice.EffectType, choice.EffectValue);
        }

        private static Model.RuntimeEventChoice FindChoice(Model.RuntimeEvent evt, int choiceId)
        {
            for (int i = 0; i < evt.Choices.Count; i++)
            {
                if (evt.Choices[i].ChoiceId == choiceId)
                {
                    return evt.Choices[i];
                }
            }

            return null;
        }

        private static void ApplyEffect(Model.BattleSceneState state, EffectType effectType, int value)
        {
            switch (effectType)
            {
                case EffectType.LoseHp:
                    state.PlayerHp = System.Math.Max(1, state.PlayerHp - value);
                    break;
                case EffectType.GainMaxHp:
                    state.PlayerMaxHp += value;
                    state.PlayerHp += value;
                    break;
                case EffectType.GainGold:
                    state.Gold += value;
                    break;
                case EffectType.DealDamage:
                    // イベント自傷はLoseHpと同等扱い（最低1残す）
                    state.PlayerHp = System.Math.Max(1, state.PlayerHp - value);
                    break;
                default:
                    TLogger.Warning($"EventChoice effectType not handled. type={effectType} value={value}", "Battle");
                    break;
            }
        }
    }
}
