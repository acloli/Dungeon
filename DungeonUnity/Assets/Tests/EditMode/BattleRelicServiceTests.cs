using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Game.MasterData.Generated;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleRelicServiceのEditorモードテストクラス
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
        public void ApplyEffects_DealDamageEnemy_DamagesFirstAliveEnemy()
        {
            BattleSceneState state = new BattleSceneState();
            state.Enemies.Add(CreateEnemyState(3001, 12));
            state.Enemies.Add(CreateEnemyState(3002, 14));
            BattleRelicService service = new BattleRelicService();
            service.AddOwnedRelic(state, CreateRelic(3, new[]
            {
                CreateEffect(RelicTriggerType.CombatStart, EffectType.DealDamage, 5, targetSide: TargetSide.Enemy)
            }));

            service.ApplyEffects(state, RelicTriggerType.CombatStart);

            Assert.That(state.Enemies[0].Hp, Is.EqualTo(7));
            Assert.That(state.Enemies[1].Hp, Is.EqualTo(14));
        }

        [Test]
        public void ApplyEffects_DealDamageAllEnemies_DamagesAliveEnemies()
        {
            BattleSceneState state = new BattleSceneState();
            state.Enemies.Add(CreateEnemyState(3001, 12));
            state.Enemies.Add(CreateEnemyState(3002, 0, isDefeated: true));
            state.Enemies.Add(CreateEnemyState(3003, 14));
            BattleRelicService service = new BattleRelicService();
            service.AddOwnedRelic(state, CreateRelic(4, new[]
            {
                CreateEffect(RelicTriggerType.CombatStart, EffectType.DealDamage, 4, targetSide: TargetSide.AllEnemies)
            }));

            service.ApplyEffects(state, RelicTriggerType.CombatStart);

            Assert.That(state.Enemies[0].Hp, Is.EqualTo(8));
            Assert.That(state.Enemies[1].Hp, Is.EqualTo(0));
            Assert.That(state.Enemies[2].Hp, Is.EqualTo(10));
        }

        [Test]
        public void ApplyEffects_ApplyStatusEnemy_AddsStatusToFirstAliveEnemy()
        {
            BattleSceneState state = new BattleSceneState();
            state.Enemies.Add(CreateEnemyState(3001, 12));
            state.Enemies.Add(CreateEnemyState(3002, 14));
            BattleRelicService service = new BattleRelicService();
            service.AddOwnedRelic(state, CreateRelic(5, new[]
            {
                CreateEffect(RelicTriggerType.CardPlayed, EffectType.ApplyStatus, 0, StatusType.Weak, 2, TargetSide.Enemy)
            }));

            service.ApplyEffects(state, RelicTriggerType.CardPlayed);

            Assert.That(state.Enemies[0].Statuses[StatusType.Weak], Is.EqualTo(2));
            Assert.That(state.Enemies[1].Statuses, Does.Not.ContainKey(StatusType.Weak));
        }

        [Test]
        public void ApplyEffects_ApplyStatusPlayer_AddsStatusToPlayer()
        {
            BattleSceneState state = new BattleSceneState();
            BattleRelicService service = new BattleRelicService();
            service.AddOwnedRelic(state, CreateRelic(6, new[]
            {
                CreateEffect(RelicTriggerType.PlayerDamaged, EffectType.ApplyStatus, 0, StatusType.Vulnerable, 3, TargetSide.Self)
            }));

            service.ApplyEffects(state, RelicTriggerType.PlayerDamaged);

            Assert.That(state.PlayerStatuses[StatusType.Vulnerable], Is.EqualTo(3));
        }

        [Test]
        public void ApplyEffects_DrawCards_AddsCardsToHand()
        {
            BattleSceneState state = new BattleSceneState();
            state.DrawPile.Add(CreateCard(1001, "Strike"));
            state.DrawPile.Add(CreateCard(1002, "Guard"));
            BattleRelicService service = CreateServiceWithRules();
            service.AddOwnedRelic(state, CreateRelic(7, new[]
            {
                CreateEffect(RelicTriggerType.PlayerTurnStart, EffectType.DrawCards, 2)
            }));

            service.ApplyEffects(state, RelicTriggerType.PlayerTurnStart);

            Assert.That(state.Hand.Count, Is.EqualTo(2));
            Assert.That(state.DrawPile, Is.Empty);
        }

        [Test]
        public void ApplyEffects_GainGold_IncreasesGold()
        {
            BattleSceneState state = new BattleSceneState
            {
                Gold = 20
            };
            BattleRelicService service = new BattleRelicService();
            service.AddOwnedRelic(state, CreateRelic(8, new[]
            {
                CreateEffect(RelicTriggerType.CombatStart, EffectType.GainGold, 15)
            }));

            service.ApplyEffects(state, RelicTriggerType.CombatStart);

            Assert.That(state.Gold, Is.EqualTo(35));
        }

        [Test]
        public void ApplyEffects_GainMaxHp_IncreasesMaxHpAndCurrentHp()
        {
            BattleSceneState state = new BattleSceneState
            {
                PlayerMaxHp = 40,
                PlayerHp = 24
            };
            BattleRelicService service = new BattleRelicService();
            service.AddOwnedRelic(state, CreateRelic(9, new[]
            {
                CreateEffect(RelicTriggerType.CombatStart, EffectType.GainMaxHp, 6)
            }));

            service.ApplyEffects(state, RelicTriggerType.CombatStart);

            Assert.That(state.PlayerMaxHp, Is.EqualTo(46));
            Assert.That(state.PlayerHp, Is.EqualTo(30));
        }

        [Test]
        public void ApplyEffects_LoseHp_DecreasesCurrentHp()
        {
            BattleSceneState state = new BattleSceneState
            {
                PlayerMaxHp = 40,
                PlayerHp = 24
            };
            BattleRelicService service = new BattleRelicService();
            service.AddOwnedRelic(state, CreateRelic(10, new[]
            {
                CreateEffect(RelicTriggerType.LoseHp, EffectType.LoseHp, 7)
            }));

            service.ApplyEffects(state, RelicTriggerType.LoseHp);

            Assert.That(state.PlayerHp, Is.EqualTo(17));
        }

        [Test]
        public void ApplyEffects_PotionCapacityDelta_ChangesMaxPotionCount()
        {
            BattleSceneState state = new BattleSceneState
            {
                MaxPotionCount = 3
            };
            BattleRelicService service = new BattleRelicService();
            service.AddOwnedRelic(state, CreateRelic(11, new[]
            {
                CreateEffect(RelicTriggerType.CombatStart, EffectType.GainBlock, 0, potionCapacityDelta: 2)
            }));

            service.ApplyEffects(state, RelicTriggerType.CombatStart);

            Assert.That(state.MaxPotionCount, Is.EqualTo(5));
        }

        [Test]
        public void ApplyEffects_WhenTriggerMismatch_DoesNotApplyEffect()
        {
            BattleSceneState state = new BattleSceneState
            {
                PlayerBlock = 1,
                MaxPotionCount = 3
            };
            BattleRelicService service = new BattleRelicService();
            service.AddOwnedRelic(state, CreateRelic(12, new[]
            {
                CreateEffect(RelicTriggerType.PlayerTurnStart, EffectType.GainBlock, 6, potionCapacityDelta: 1)
            }));

            service.ApplyEffects(state, RelicTriggerType.CombatStart);

            Assert.That(state.PlayerBlock, Is.EqualTo(1));
            Assert.That(state.MaxPotionCount, Is.EqualTo(3));
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
                6301,
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

        private static BattleRelicService CreateServiceWithRules()
        {
            BattleSceneRules rules = new BattleSceneRules(
                new BattleDeckService(),
                null,
                null,
                null);
            return new BattleRelicService(rules, new FixedRandomProvider());
        }

        private static BattleEnemyState CreateEnemyState(int id, int hp, bool isDefeated = false)
        {
            RuntimeEnemy enemy = new RuntimeEnemy(
                id,
                $"enemy_{id}",
                $"Enemy{id}",
                string.Empty,
                EnemyTier.Normal,
                hp,
                hp,
                1,
                new List<RuntimeEnemyAction>());
            return new BattleEnemyState(enemy, id - 3001, hp)
            {
                IsDefeated = isDefeated
            };
        }

        private static RuntimeCard CreateCard(int id, string name)
        {
            var builder = Support.BattleTestData.Card(id);
            builder.DisplayName = name;
            builder.Cost = 1;
            builder.Effects = new List<RuntimeCardEffect>();
            return builder.Build();
        }

        private static RuntimeRelic CreateRelic(int id, IReadOnlyList<RuntimeRelicEffect> effects)
        {
            return new RuntimeRelic(id, $"relic_{id}", $"Relic{id}", string.Empty, string.Empty, string.Empty, string.Empty, CardRarity.Uncommon, effects);
        }

        private static RuntimeRelicEffect CreateEffect(
            RelicTriggerType triggerType,
            EffectType effectType,
            int value,
            StatusType statusType = StatusType.None,
            int statusValue = 0,
            TargetSide targetSide = TargetSide.Self,
            int hitCount = 1,
            int potionCapacityDelta = 0)
        {
            return new RuntimeRelicEffect(1, triggerType, effectType, value, hitCount, statusType, statusValue, targetSide, potionCapacityDelta);
        }

        private sealed class FixedRandomProvider : IBattleRandomProvider
        {
            public int Seed { get; private set; }

            public int Counter { get; private set; }

            public void Initialize(int seed)
            {
                Seed = seed;
                Counter = 0;
            }

            public void Restore(int seed, int counter)
            {
                Seed = seed;
                Counter = counter;
            }

            public int Range(int minInclusive, int maxExclusive)
            {
                Counter++;
                return minInclusive;
            }
        }
    }
}
