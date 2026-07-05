using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Domain;
using Dungeon.Tests.EditMode.Support;
using Game.MasterData.Generated;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleEncounterSelectorのEditorモードテストクラス
    /// </summary>
    public sealed class BattleEncounterSelectorTests
    {
        [Test]
        public void SelectEncounterFormation_UsesWeightedEntry()
        {
            RuntimeEncounterFormation first = CreateFormation("First");
            RuntimeEncounterFormation second = CreateFormation("Second");
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                new[]
                {
                    CreateEncounterEntry(first, 10),
                    CreateEncounterEntry(second, 20)
                });
            BattleEncounterSelector service = new BattleEncounterSelector();

            RuntimeEncounterFormation selected = service.SelectEncounterFormation(
                runDefinition,
                InGameNodeType.Battle,
                new FixedRandomProvider(15));

            Assert.That(selected.DisplayName, Is.EqualTo("Second"));
        }

        [Test]
        public void SelectEncounterFormation_WithoutEntries_ReturnsFallbackFormation()
        {
            BattleEncounterSelector service = new BattleEncounterSelector();

            RuntimeEncounterFormation selected = service.SelectEncounterFormation(
                runDefinition: null,
                InGameNodeType.EliteBattle,
                new FixedRandomProvider(0));

            Assert.That(selected, Is.Not.Null);
            Assert.That(selected.Enemies.Count, Is.EqualTo(1));
            Assert.That(selected.Enemies[0].Enemy.EnemyTier, Is.EqualTo(EnemyTier.Elite));
        }

        [Test]
        public void RollEnemyHp_UsesRangeWithinBounds()
        {
            var enemyBuilder = BattleTestData.Enemy(3001);
            enemyBuilder.DisplayName = "Slime";
            enemyBuilder.HpMin = 10;
            enemyBuilder.HpMax = 15;
            RuntimeEnemy enemy = enemyBuilder.Build();
            BattleEncounterSelector service = new BattleEncounterSelector();

            int rolledHp = service.RollEnemyHp(enemy, new FixedRandomProvider(12));

            Assert.That(rolledHp, Is.EqualTo(12));
        }

        private static RuntimeRunDefinition CreateRunDefinition(IReadOnlyList<RuntimeEncounterEntry> battleEncounters)
        {
            RuntimeRunDefinitionBuilder builder = BattleTestData.RunDefinition();
            builder.RunProfileId = 5501;
            builder.Key = "encounter_test";
            builder.EncountersByNodeType = new Dictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>>
            {
                { InGameNodeType.Battle, battleEncounters }
            };
            builder.StarterDeck = Array.Empty<RuntimeCard>();
            builder.CardCatalog = new Dictionary<int, RuntimeCard>();
            builder.RewardPool = Array.Empty<RuntimeRewardEntry>();
            builder.Nodes = Array.Empty<RuntimeMapNode>();
            builder.PossibleEvents = Array.Empty<RuntimeEvent>();
            builder.RelicCatalog = new Dictionary<int, RuntimeRelic>();
            builder.PotionCatalog = new Dictionary<int, RuntimePotion>();
            builder.ItemPriceRules = Array.Empty<RuntimeItemPriceRule>();
            return builder.Build();
        }

        private static RuntimeEncounterEntry CreateEncounterEntry(RuntimeEncounterFormation formation, int weight)
        {
            return new RuntimeEncounterEntry(formation, weight);
        }

        private static RuntimeEncounterFormation CreateFormation(string name)
        {
            RuntimeEnemy enemy = new RuntimeEnemy(
                3001,
                "enemy_key",
                name,
                string.Empty,
                EnemyTier.Normal,
                10,
                10,
                10,
                Array.Empty<RuntimeEnemyAction>());
            return new RuntimeEncounterFormation(
                7001,
                "formation_key",
                name,
                new[] { new RuntimeEncounterEnemyEntry(enemy, 0) });
        }

        /// <summary>
        /// 固定値を返す乱数提供クラス
        /// </summary>
        private sealed class FixedRandomProvider : IBattleRandomProvider
        {
            private readonly int _value;

            public int Seed { get; private set; }

            public int Counter { get; private set; }

            public FixedRandomProvider(int value)
            {
                _value = value;
            }

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
                if (_value < minInclusive)
                {
                    return minInclusive;
                }

                if (_value >= maxExclusive)
                {
                    return maxExclusive - 1;
                }

                return _value;
            }
        }
    }
}
