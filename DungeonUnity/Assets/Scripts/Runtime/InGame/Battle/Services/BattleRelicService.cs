using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// レリック効果適用クラス
    /// </summary>
    public sealed class BattleRelicService : IBattleRelicService
    {
        public void RestoreOwnedRelics(BattleSceneState state, RuntimeRunDefinition runDefinition, IReadOnlyList<int> ownedRelicIds)
        {
            if (state == null || runDefinition?.RelicCatalog == null)
            {
                return;
            }

            state.OwnedRelics.Clear();
            if (ownedRelicIds == null)
            {
                return;
            }

            for (int i = 0; i < ownedRelicIds.Count; i++)
            {
                int relicId = ownedRelicIds[i];
                if (runDefinition.RelicCatalog.TryGetValue(relicId, out RuntimeRelic relic))
                {
                    AddOwnedRelic(state, relic);
                }
            }
        }

        public bool AddOwnedRelic(BattleSceneState state, RuntimeRelic relic)
        {
            if (state == null || relic == null)
            {
                return false;
            }

            for (int i = 0; i < state.OwnedRelics.Count; i++)
            {
                RuntimeRelic ownedRelic = state.OwnedRelics[i];
                if (ownedRelic != null && ownedRelic.Id == relic.Id)
                {
                    return false;
                }
            }

            state.OwnedRelics.Add(relic);
            return true;
        }

        public RuntimeRelic RollBattleRewardRelic(BattleSceneState state, RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider)
        {
            if (state == null || runDefinition?.RelicCatalog == null || randomProvider == null)
            {
                return null;
            }

            List<RuntimeRelic> candidates = new List<RuntimeRelic>();
            foreach (KeyValuePair<int, RuntimeRelic> entry in runDefinition.RelicCatalog)
            {
                if (!HasOwnedRelic(state, entry.Key) && entry.Value != null)
                {
                    candidates.Add(entry.Value);
                }
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            return candidates[randomProvider.Range(0, candidates.Count)];
        }

        public void ApplyEffects(BattleSceneState state, RelicTriggerType triggerType)
        {
            if (state == null)
            {
                return;
            }

            for (int i = 0; i < state.OwnedRelics.Count; i++)
            {
                RuntimeRelic relic = state.OwnedRelics[i];
                if (relic?.Effects == null)
                {
                    continue;
                }

                for (int effectIndex = 0; effectIndex < relic.Effects.Count; effectIndex++)
                {
                    RuntimeRelicEffect effect = relic.Effects[effectIndex];
                    if (effect == null || effect.TriggerType != triggerType)
                    {
                        continue;
                    }

                    ApplyEffect(state, effect);
                }
            }
        }

        private static bool HasOwnedRelic(BattleSceneState state, int relicId)
        {
            for (int i = 0; i < state.OwnedRelics.Count; i++)
            {
                RuntimeRelic relic = state.OwnedRelics[i];
                if (relic != null && relic.Id == relicId)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplyEffect(BattleSceneState state, RuntimeRelicEffect effect)
        {
            switch (effect.EffectType)
            {
                case EffectType.GainBlock:
                    state.PlayerBlock += effect.Value;
                    break;
                case EffectType.GainEnergy:
                    state.PlayerEnergy += effect.Value;
                    break;
            }
        }
    }
}
