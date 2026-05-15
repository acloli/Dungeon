using System.Collections.Generic;
using System.Reflection;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Domain;
using NUnit.Framework;
using UnityEngine;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleSceneFlowServiceの編集モード試験クラス
    /// </summary>
    public sealed class BattleSceneFlowServiceTests
    {
        [Test]
        public void Initialize_OpensMapWithRunDefaults()
        {
            BattleSceneFlowService service = CreateService(0);
            RunStartConfig config = CreateRunStartConfig(
                playerMaxHp: 50,
                startingGold: 120,
                mapNodes: new[]
                {
                    CreateNode(InGameNodeType.Battle, "B1"),
                    CreateNode(InGameNodeType.Boss, "Boss")
                },
                starterDeck: new[] { CreateCard("strike", "Strike", 1, 6) },
                rewardPool: new[] { CreateCard("burst", "Burst", 2, 12) },
                normalEnemy: CreateEnemy("slime", "Slime", 18, 4),
                eliteEnemy: CreateEnemy("guard", "Guard", 24, 6),
                bossEnemy: CreateEnemy("boss", "Boss", 40, 8));

            service.Initialize(config);
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Map));
            Assert.That(snapshot.PlayerMaxHp, Is.EqualTo(50));
            Assert.That(snapshot.PlayerHp, Is.EqualTo(50));
            Assert.That(snapshot.Gold, Is.EqualTo(120));
            Assert.That(snapshot.Nodes.Count, Is.EqualTo(2));
            Assert.That(snapshot.MapMessage, Does.Contain("Next 1/2"));
        }

        [Test]
        public void SelectMapNode_BattleNode_OpensBattleAndDrawsHand()
        {
            BattleSceneFlowService service = CreateService(0, 0, 0, 0, 0);
            CardDefinition strike = CreateCard("strike", "Strike", 1, 6);
            RunStartConfig config = CreateRunStartConfig(
                playerMaxHp: 40,
                startingGold: 100,
                mapNodes: new[]
                {
                    CreateNode(InGameNodeType.Battle, "B1")
                },
                starterDeck: new[] { strike, strike, strike },
                rewardPool: new[] { CreateCard("burst", "Burst", 2, 12) },
                normalEnemy: CreateEnemy("slime", "Slime", 18, 4),
                eliteEnemy: null,
                bossEnemy: null);

            service.Initialize(config);
            service.SelectMapNode(0);
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Battle));
            Assert.That(snapshot.Hand.Count, Is.EqualTo(3));
            Assert.That(snapshot.CurrentEnemy.DisplayName, Is.EqualTo("Slime"));
            Assert.That(snapshot.BattleHintMessage, Is.EqualTo("Select a card, then click enemy target."));
        }

        [Test]
        public void TryPlaySelectedCard_KillEnemy_OpensRewardAndAddsGold()
        {
            BattleSceneFlowService service = CreateService(0, 0, 0, 0, 0);
            CardDefinition finisher = CreateCard("finisher", "Finisher", 1, 99);
            CardDefinition reward = CreateCard("reward", "Reward", 1, 5);
            RunStartConfig config = CreateRunStartConfig(
                playerMaxHp: 40,
                startingGold: 100,
                mapNodes: new[]
                {
                    CreateNode(InGameNodeType.Battle, "B1")
                },
                starterDeck: new[] { finisher },
                rewardPool: new[] { reward },
                normalEnemy: CreateEnemy("slime", "Slime", 18, 4),
                eliteEnemy: null,
                bossEnemy: null);

            service.Initialize(config);
            service.SelectMapNode(0);
            service.SelectHandCard(0);
            service.TryPlaySelectedCard();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Reward));
            Assert.That(snapshot.Gold, Is.EqualTo(130));
            Assert.That(snapshot.RewardChoices.Count, Is.EqualTo(1));
            Assert.That(snapshot.RewardChoices[0].DisplayName, Is.EqualTo("Reward"));
        }

        [Test]
        public void EndTurn_WhenPlayerDies_OpensResult()
        {
            BattleSceneFlowService service = CreateService(0, 0, 0, 0, 0);
            RunStartConfig config = CreateRunStartConfig(
                playerMaxHp: 3,
                startingGold: 100,
                mapNodes: new[]
                {
                    CreateNode(InGameNodeType.Battle, "B1")
                },
                starterDeck: new[] { CreateCard("strike", "Strike", 1, 1) },
                rewardPool: new[] { CreateCard("reward", "Reward", 1, 5) },
                normalEnemy: CreateEnemy("slime", "Slime", 18, 5),
                eliteEnemy: null,
                bossEnemy: null);

            service.Initialize(config);
            service.SelectMapNode(0);
            service.EndTurn();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Result));
            Assert.That(snapshot.ResultMessage, Is.EqualTo("Run Failed"));
        }

        [Test]
        public void RestShopFlow_RestAndContinue_ReturnsToMap()
        {
            BattleSceneFlowService service = CreateService(0, 0, 0, 0, 0);
            RunStartConfig config = CreateRunStartConfig(
                playerMaxHp: 40,
                startingGold: 100,
                mapNodes: new[]
                {
                    CreateNode(InGameNodeType.RestShop, "Rest")
                },
                starterDeck: new[] { CreateCard("strike", "Strike", 1, 6) },
                rewardPool: new[] { CreateCard("reward", "Reward", 1, 5) },
                normalEnemy: CreateEnemy("slime", "Slime", 18, 4),
                eliteEnemy: null,
                bossEnemy: null);

            service.Initialize(config);
            service.SelectMapNode(0);
            service.ApplyRest();
            BattleSceneSnapshot restSnapshot = service.CreateSnapshot();

            Assert.That(restSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.RestShop));
            Assert.That(restSnapshot.IsRestShopContinueEnabled, Is.True);
            Assert.That(restSnapshot.RestShopMessage, Does.Contain("Rest done."));

            service.ContinueFromRestShop();
            BattleSceneSnapshot mapSnapshot = service.CreateSnapshot();

            Assert.That(mapSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.Map));
        }

        private static BattleSceneFlowService CreateService(params int[] values)
        {
            return new BattleSceneFlowService(new BattleSceneRules(), new SequenceRandomProvider(values));
        }

        private static RunStartConfig CreateRunStartConfig(
            int playerMaxHp,
            int startingGold,
            IReadOnlyList<MapTemplate.Node> mapNodes,
            IReadOnlyList<CardDefinition> starterDeck,
            IReadOnlyList<CardDefinition> rewardPool,
            EnemyDefinition normalEnemy,
            EnemyDefinition eliteEnemy,
            EnemyDefinition bossEnemy)
        {
            RunStartConfig config = ScriptableObject.CreateInstance<RunStartConfig>();
            MapTemplate mapTemplate = ScriptableObject.CreateInstance<MapTemplate>();

            SetField(config, "_playerMaxHp", playerMaxHp);
            SetField(config, "_startingGold", startingGold);
            SetField(config, "_mapTemplate", mapTemplate);
            SetField(config, "_starterDeck", new List<CardDefinition>(starterDeck));
            SetField(config, "_rewardPool", new List<CardDefinition>(rewardPool));
            SetField(config, "_normalEnemy", normalEnemy);
            SetField(config, "_eliteEnemy", eliteEnemy);
            SetField(config, "_bossEnemy", bossEnemy);
            SetField(mapTemplate, "_nodes", new List<MapTemplate.Node>(mapNodes));

            return config;
        }

        private static CardDefinition CreateCard(string cardId, string displayName, int cost, int damage)
        {
            CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>();
            SetField(card, "_cardId", cardId);
            SetField(card, "_displayName", displayName);
            SetField(card, "_cost", cost);
            SetField(card, "_damage", damage);
            return card;
        }

        private static EnemyDefinition CreateEnemy(string enemyId, string displayName, int maxHp, int intentDamage)
        {
            EnemyDefinition enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            SetField(enemy, "_enemyId", enemyId);
            SetField(enemy, "_displayName", displayName);
            SetField(enemy, "_maxHp", maxHp);
            SetField(enemy, "_intentDamage", intentDamage);
            return enemy;
        }

        private static MapTemplate.Node CreateNode(InGameNodeType nodeType, string label)
        {
            return new MapTemplate.Node
            {
                NodeType = nodeType,
                Label = label
            };
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(target, value);
        }

        /// <summary>
        /// 固定乱数提供クラス
        /// </summary>
        private sealed class SequenceRandomProvider : IBattleRandomProvider
        {
            private readonly Queue<int> _values;

            public SequenceRandomProvider(IEnumerable<int> values)
            {
                _values = new Queue<int>(values);
            }

            public int Range(int minInclusive, int maxExclusive)
            {
                if (_values.Count == 0)
                {
                    return minInclusive;
                }

                int value = _values.Dequeue();
                if (value < minInclusive)
                {
                    return minInclusive;
                }

                if (value >= maxExclusive)
                {
                    return maxExclusive - 1;
                }

                return value;
            }
        }
    }
}
