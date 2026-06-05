using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Domain;
using NUnit.Framework;

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
            BattleSceneFlowService service = CreateService(CreateRunDefinition(), 0);

            service.Initialize(5501);
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
            RuntimeCard strike = CreateCard(1001, "Strike", 1, 6);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { strike, strike, strike },
                rewardCards: new[] { CreateRewardEntry(CreateCard(2001, "Burst", 2, 12), 10, 1, 99) },
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 14, CreateAction(1, 4, BattleEnemyRepeatRule.RepeatAfterOpening)), 10) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0);

            service.Initialize(5501);
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
            RuntimeCard finisher = CreateCard(1001, "Finisher", 1, 99);
            RuntimeCard reward = CreateCard(1002, "Reward", 1, 5);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { finisher },
                rewardCards: new[] { CreateRewardEntry(reward, 10, 1, 99) },
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 30, CreateAction(1, 4, BattleEnemyRepeatRule.RepeatAfterOpening)), 10) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectHandCard(0);
            service.TryPlaySelectedCard();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Reward));
            Assert.That(snapshot.Gold, Is.EqualTo(150));
            Assert.That(snapshot.RewardChoices.Count, Is.EqualTo(1));
            Assert.That(snapshot.RewardChoices[0].DisplayName, Is.EqualTo("Reward"));
        }

        [Test]
        public void EndTurn_WhenPlayerDies_OpensResult()
        {
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                playerMaxHp: 3,
                starterDeck: new[] { CreateCard(1001, "Strike", 1, 1) },
                rewardCards: new[] { CreateRewardEntry(CreateCard(1002, "Reward", 1, 5), 10, 1, 99) },
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 30, CreateAction(1, 5, BattleEnemyRepeatRule.RepeatAfterOpening)), 10) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.EndTurn();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Result));
            Assert.That(snapshot.ResultMessage, Is.EqualTo("Run Failed"));
        }

        [Test]
        public void RestShopFlow_RestAndContinue_ReturnsToMap()
        {
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.RestShop, "Rest", new[] { 1 }) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0);

            service.Initialize(5501);
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

        [Test]
        public void TryPlaySelectedCard_GainBlock_ReducesIncomingDamage()
        {
            RuntimeCard guard = CreateCard(1001, "Guard", 1, 0, new[]
            {
                new RuntimeCardEffect(1, BattleEffectType.GainBlock, 5, 1, BattleStatusType.None, 0, BattleTargetSide.Self)
            });
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { guard },
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 10, CreateAction(1, 7, BattleEnemyRepeatRule.RepeatAfterOpening)), 10) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectHandCard(0);
            service.TryPlaySelectedCard();
            service.EndTurn();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.PlayerHp, Is.EqualTo(48));
        }

        private static BattleSceneFlowService CreateService(RuntimeRunDefinition runDefinition, params int[] values)
        {
            return new BattleSceneFlowService(
                new BattleSceneRules(),
                new SequenceRandomProvider(values),
                new FakeBattleMasterDataFacade(runDefinition));
        }

        private static RuntimeRunDefinition CreateRunDefinition(
            int playerMaxHp = 50,
            int startingGold = 120,
            IReadOnlyList<RuntimeMapNode> nodes = null,
            IReadOnlyList<RuntimeCard> starterDeck = null,
            IReadOnlyList<RuntimeRewardEntry> rewardCards = null,
            IReadOnlyList<RuntimeEncounterEntry> battleEncounters = null,
            IReadOnlyList<RuntimeEncounterEntry> eliteEncounters = null,
            IReadOnlyList<RuntimeEncounterEntry> bossEncounters = null)
        {
            Dictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>> encounters =
                new Dictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>>
                {
                    { InGameNodeType.Battle, battleEncounters ?? new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 14, CreateAction(1, 4, BattleEnemyRepeatRule.RepeatAfterOpening)), 10) } },
                    { InGameNodeType.EliteBattle, eliteEncounters ?? new[] { CreateEncounter(CreateEnemy(3002, "Guard", 24, 24, 30, CreateAction(1, 6, BattleEnemyRepeatRule.RepeatAfterOpening)), 10) } },
                    { InGameNodeType.Boss, bossEncounters ?? new[] { CreateEncounter(CreateEnemy(3003, "Boss", 40, 40, 100, CreateAction(1, 8, BattleEnemyRepeatRule.RepeatAfterOpening)), 10) } }
                };

            return new RuntimeRunDefinition(
                5501,
                "run_test",
                "CrimsonExile",
                playerMaxHp,
                startingGold,
                3,
                starterDeck ?? new[] { CreateCard(1001, "Strike", 1, 6) },
                rewardCards ?? new[] { CreateRewardEntry(CreateCard(1002, "Reward", 1, 5), 10, 1, 99) },
                nodes ?? new[]
                {
                    CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }),
                    CreateNode(5302, 2, InGameNodeType.Boss, "Boss", new int[0])
                },
                encounters);
        }

        private static RuntimeCard CreateCard(int id, string displayName, int cost, int damage, IReadOnlyList<RuntimeCardEffect> effects = null)
        {
            return new RuntimeCard(
                id,
                $"card_{id}",
                displayName,
                string.Empty,
                cost,
                "Attack",
                "Common",
                "CrimsonExile",
                effects ?? new[]
                {
                    new RuntimeCardEffect(1, BattleEffectType.DealDamage, damage, 1, BattleStatusType.None, 0, BattleTargetSide.Enemy)
                });
        }

        private static RuntimeEnemy CreateEnemy(int id, string displayName, int hpMin, int hpMax, int goldReward, params RuntimeEnemyAction[] actions)
        {
            return new RuntimeEnemy(
                id,
                $"enemy_{id}",
                displayName,
                string.Empty,
                "Normal",
                hpMin,
                hpMax,
                goldReward,
                actions);
        }

        private static RuntimeEnemyAction CreateAction(int order, int damage, BattleEnemyRepeatRule repeatRule)
        {
            return new RuntimeEnemyAction(
                order,
                "Attack",
                damage,
                1,
                0,
                BattleStatusType.None,
                0,
                BattleStatusType.None,
                0,
                repeatRule);
        }

        private static RuntimeMapNode CreateNode(int id, int floor, InGameNodeType nodeType, string displayName, IReadOnlyList<int> nextNodeIndices)
        {
            return new RuntimeMapNode(id, $"node_{id}", floor, nodeType, displayName, string.Empty, nextNodeIndices);
        }

        private static RuntimeEncounterEntry CreateEncounter(RuntimeEnemy enemy, int weight)
        {
            return new RuntimeEncounterEntry(enemy, weight);
        }

        private static RuntimeRewardEntry CreateRewardEntry(RuntimeCard card, int weight, int minFloor, int maxFloor)
        {
            return new RuntimeRewardEntry(card, weight, minFloor, maxFloor);
        }

        /// <summary>
        /// テスト用MasterDataFacade
        /// </summary>
        private sealed class FakeBattleMasterDataFacade : IBattleMasterDataFacade
        {
            private readonly RuntimeRunDefinition _runDefinition;

            public FakeBattleMasterDataFacade(RuntimeRunDefinition runDefinition)
            {
                _runDefinition = runDefinition;
            }

            public RuntimeRunDefinition BuildRunDefinition(int runProfileId)
            {
                return _runDefinition;
            }
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
