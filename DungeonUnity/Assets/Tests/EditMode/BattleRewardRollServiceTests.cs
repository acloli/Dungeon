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
    /// BattleRewardRollServiceのEditorモードテストクラス
    /// </summary>
    public sealed class BattleRewardRollServiceTests
    {
        [Test]
        public void SelectCardRewardChoices_FiltersByCurrentFloor()
        {
            RuntimeRewardEntry floorOne = CreateRewardEntry(1001, "Floor1", 10, 1, 1);
            RuntimeRewardEntry floorTwo = CreateRewardEntry(1002, "Floor2", 10, 2, 2);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(new[] { floorOne, floorTwo });
            BattleSceneState state = new BattleSceneState
            {
                CurrentNodeIndex = 0
            };
            state.Nodes.Add(new RuntimeMapNode(5301, "node_1", 2, InGameNodeType.Battle, "Battle", string.Empty, Array.Empty<int>()));
            BattleRewardRollService service = new BattleRewardRollService();

            IReadOnlyList<RuntimeRewardEntry> rewards = service.SelectCardRewardChoices(
                state,
                runDefinition,
                new FixedRandomProvider(0));

            Assert.That(rewards.Count, Is.EqualTo(1));
            Assert.That(rewards[0].Card.DisplayName, Is.EqualTo("Floor2"));
        }

        [Test]
        public void SelectCardRewardChoices_WithoutRewardPool_FallsBackToDeckCards()
        {
            BattleSceneState state = new BattleSceneState();
            state.Deck.Add(CreateCard(1001, "Strike"));
            state.Deck.Add(CreateCard(1001, "Strike"));
            state.Deck.Add(CreateCard(1002, "Guard"));
            BattleRewardRollService service = new BattleRewardRollService();

            IReadOnlyList<RuntimeRewardEntry> rewards = service.SelectCardRewardChoices(
                state,
                CreateRunDefinition(Array.Empty<RuntimeRewardEntry>()),
                new FixedRandomProvider(0));

            Assert.That(rewards.Count, Is.EqualTo(2));
            Assert.That(rewards[0].Card.Id, Is.EqualTo(1001));
            Assert.That(rewards[1].Card.Id, Is.EqualTo(1002));
        }

        [Test]
        public void RollPotionDrop_UsesDropChance()
        {
            RuntimeRunDefinitionBuilder builder = BattleTestData.RunDefinition();
            builder.RunProfileId = 5501;
            builder.Key = "reward_roll";
            builder.PotionDropChance = 30;
            RuntimeRunDefinition runDefinition = builder.Build();
            BattleRewardRollService service = new BattleRewardRollService();

            Assert.That(service.RollPotionDrop(runDefinition, new FixedRandomProvider(29)), Is.True);
            Assert.That(service.RollPotionDrop(runDefinition, new FixedRandomProvider(30)), Is.False);
        }

        private static RuntimeRunDefinition CreateRunDefinition(IReadOnlyList<RuntimeRewardEntry> rewardPool)
        {
            RuntimeRunDefinitionBuilder builder = BattleTestData.RunDefinition();
            builder.RunProfileId = 5501;
            builder.Key = "reward_roll";
            builder.StarterDeck = Array.Empty<RuntimeCard>();
            builder.CardCatalog = new Dictionary<int, RuntimeCard>();
            builder.RewardPool = rewardPool;
            builder.Nodes = Array.Empty<RuntimeMapNode>();
            builder.EncountersByNodeType = new Dictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>>();
            builder.PossibleEvents = Array.Empty<RuntimeEvent>();
            builder.RelicCatalog = new Dictionary<int, RuntimeRelic>();
            builder.PotionCatalog = new Dictionary<int, RuntimePotion>();
            builder.ItemPriceRules = Array.Empty<RuntimeItemPriceRule>();
            builder.CardRewardChoiceCount = 3;
            return builder.Build();
        }

        private static RuntimeRewardEntry CreateRewardEntry(int cardId, string displayName, int weight, int minFloor, int maxFloor)
        {
            RuntimeRewardEntryBuilder builder = BattleTestData.RewardEntry();
            builder.Card = CreateCard(cardId, displayName);
            builder.Weight = weight;
            builder.MinFloor = minFloor;
            builder.MaxFloor = maxFloor;
            return builder.Build();
        }

        private static RuntimeCard CreateCard(int id, string displayName)
        {
            var builder = BattleTestData.Card(id);
            builder.DisplayName = displayName;
            builder.Cost = 1;
            builder.Effects = new[]
            {
                new RuntimeCardEffect(1, EffectType.DealDamage, 6, 1, StatusType.None, 0, TargetSide.Enemy)
            };
            return builder.Build();
        }

        /// <summary>
        /// 固定値を返す乱数提供クラス
        /// </summary>
        private sealed class FixedRandomProvider : IBattleRandomProvider
        {
            private readonly int _value;

            public FixedRandomProvider(int value)
            {
                _value = value;
            }

            public int Range(int minInclusive, int maxExclusive)
            {
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
