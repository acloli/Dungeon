using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Game.MasterData.Generated;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleRelicServiceのEditモードテストクラス
    /// </summary>
    public sealed class BattleRelicServiceTests
    {
        [Test]
        public void AddOwnedRelic_DeduplicatesById()
        {
            BattleSceneState state = new BattleSceneState();
            BattleRelicService service = new BattleRelicService();
            RuntimeRelic relic = CreateRelic(1, new[]
            {
                CreateEffect(RelicTriggerType.CombatStart, EffectType.GainBlock, 6)
            });

            bool first = service.AddOwnedRelic(state, relic);
            bool second = service.AddOwnedRelic(state, relic);

            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
            Assert.That(state.OwnedRelics.Count, Is.EqualTo(1));
        }

        [Test]
        public void ApplyEffects_CombatStartGainBlock_IncreasesPlayerBlock()
        {
            BattleSceneState state = new BattleSceneState();
            BattleRelicService service = new BattleRelicService();
            service.AddOwnedRelic(state, CreateRelic(1, new[]
            {
                CreateEffect(RelicTriggerType.CombatStart, EffectType.GainBlock, 6)
            }));

            service.ApplyEffects(state, RelicTriggerType.CombatStart);

            Assert.That(state.PlayerBlock, Is.EqualTo(6));
        }

        [Test]
        public void ApplyEffects_PlayerTurnStartGainEnergy_IncreasesPlayerEnergy()
        {
            BattleSceneState state = new BattleSceneState
            {
                PlayerEnergy = 3
            };
            BattleRelicService service = new BattleRelicService();
            service.AddOwnedRelic(state, CreateRelic(2, new[]
            {
                CreateEffect(RelicTriggerType.PlayerTurnStart, EffectType.GainEnergy, 1)
            }));

            service.ApplyEffects(state, RelicTriggerType.PlayerTurnStart);

            Assert.That(state.PlayerEnergy, Is.EqualTo(4));
        }

        [Test]
        public void RollBattleRewardRelic_SkipsOwnedRelics()
        {
            BattleSceneState state = new BattleSceneState();
            BattleRelicService service = new BattleRelicService();
            RuntimeRelic ownedRelic = CreateRelic(1, new[] { CreateEffect(RelicTriggerType.CombatStart, EffectType.GainBlock, 6) });
            RuntimeRelic newRelic = CreateRelic(2, new[] { CreateEffect(RelicTriggerType.PlayerTurnStart, EffectType.GainEnergy, 1) });
            service.AddOwnedRelic(state, ownedRelic);
            RuntimeRunDefinition runDefinition = new RuntimeRunDefinition(
                5501,
                "run_test",
                CharacterArchetype.CrimsonExile,
                50,
                120,
                3,
                0,
                100,
                new List<RuntimeCard>(),
                new Dictionary<int, RuntimeCard>(),
                new List<RuntimeRewardEntry>(),
                new List<RuntimeMapNode>(),
                new Dictionary<Dungeon.Runtime.InGame.Domain.InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>>(),
                new List<RuntimeEvent>(),
                new Dictionary<int, RuntimeRelic>
                {
                    { ownedRelic.Id, ownedRelic },
                    { newRelic.Id, newRelic }
                },
                new Dictionary<int, RuntimePotion>(),
                null,
                null,
                null);

            RuntimeRelic rolledRelic = service.RollBattleRewardRelic(state, runDefinition, new FixedRandomProvider());

            Assert.That(rolledRelic.Id, Is.EqualTo(2));
        }

        private static RuntimeRelic CreateRelic(int id, IReadOnlyList<RuntimeRelicEffect> effects)
        {
            return new RuntimeRelic(id, $"relic_{id}", $"Relic{id}", string.Empty, string.Empty, string.Empty, string.Empty, CardRarity.Uncommon, effects);
        }

        private static RuntimeRelicEffect CreateEffect(RelicTriggerType triggerType, EffectType effectType, int value)
        {
            return new RuntimeRelicEffect(1, triggerType, effectType, value, 1, StatusType.None, 0, TargetSide.Self);
        }

        private sealed class FixedRandomProvider : IBattleRandomProvider
        {
            public int Range(int minInclusive, int maxExclusive)
            {
                return minInclusive;
            }
        }
    }
}
