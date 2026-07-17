using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Domain;
using Dungeon.Tests.EditMode.Support;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleMapGeneratorのEditorモードテストクラス
    /// </summary>
    [TestFixture]
    public sealed class BattleMapGeneratorTests
    {
        private const int ExpectedFloorCount = 8;
        private const int MinNodesPerFloor = 1;
        private const int MaxNodesPerFloor = 3;

        [Test]
        public void Generate_SameSeed_ReturnsSameLayout()
        {
            BattleMapGenerator generator = new BattleMapGenerator();
            RuntimeRunDefinition runDefinition = CreateRunDefinition();

            IReadOnlyList<RuntimeMapNode> first = generator.Generate(runDefinition, 12345);
            IReadOnlyList<RuntimeMapNode> second = generator.Generate(runDefinition, 12345);

            Assert.That(BuildSignature(first), Is.EqualTo(BuildSignature(second)));
        }

        [Test]
        public void Generate_DifferentSeed_ReturnsDifferentLayout()
        {
            BattleMapGenerator generator = new BattleMapGenerator();
            RuntimeRunDefinition runDefinition = CreateRunDefinition();

            IReadOnlyList<RuntimeMapNode> first = generator.Generate(runDefinition, 12345);
            IReadOnlyList<RuntimeMapNode> second = generator.Generate(runDefinition, 67890);

            Assert.That(BuildSignature(first), Is.Not.EqualTo(BuildSignature(second)));
        }

        [Test]
        public void Generate_WithMultipleSeeds_ProducesDifferentFloorWidthSignatures()
        {
            BattleMapGenerator generator = new BattleMapGenerator();
            RuntimeRunDefinition runDefinition = CreateRunDefinition();
            HashSet<string> signatures = new HashSet<string>();

            for (int seed = 1000; seed < 1020; seed++)
            {
                IReadOnlyList<RuntimeMapNode> nodes = generator.Generate(runDefinition, seed);
                AssertMapStructure(nodes);
                Assert.That(nodes.Count(node => node.NodeType == InGameNodeType.Boss), Is.EqualTo(1), $"Seed {seed} should have one boss.");
                Assert.That(nodes.Single(node => node.NodeType == InGameNodeType.Boss).Floor, Is.EqualTo(ExpectedFloorCount), $"Seed {seed} boss should be final floor.");
                Assert.That(nodes.Count(node => node.NodeType == InGameNodeType.EliteBattle), Is.GreaterThanOrEqualTo(1), $"Seed {seed} should have an elite.");
                Assert.That(nodes.Count(node => node.NodeType == InGameNodeType.Event), Is.GreaterThanOrEqualTo(1), $"Seed {seed} should have an event.");
                Assert.That(nodes.Count(node => node.NodeType == InGameNodeType.RestShop), Is.GreaterThanOrEqualTo(1), $"Seed {seed} should have a rest shop.");
                Assert.That(nodes.Count(node => node.NodeType == InGameNodeType.Treasure), Is.GreaterThanOrEqualTo(1), $"Seed {seed} should have treasure.");
                Assert.That(
                    nodes.Count(node => node.Floor == ExpectedFloorCount - 1 && node.NodeType == InGameNodeType.RestShop),
                    Is.GreaterThanOrEqualTo(1),
                    $"Seed {seed} should have a rest shop before boss.");
                AssertEdgesAdvanceToNextFloor(nodes);
                AssertEveryFloorReachable(nodes);
                AssertTreasureNodesConnectAllNextFloor(nodes);
                signatures.Add(BuildFloorWidthSignature(nodes));
            }

            Assert.That(signatures.Count, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void Generate_SatisfiesExpectedConstraints()
        {
            BattleMapGenerator generator = new BattleMapGenerator();
            RuntimeRunDefinition runDefinition = CreateRunDefinition();

            IReadOnlyList<RuntimeMapNode> nodes = generator.Generate(runDefinition, 12345);

            AssertMapStructure(nodes);
            Assert.That(nodes.Count(node => node.NodeType == InGameNodeType.Boss), Is.EqualTo(1));
            Assert.That(nodes.Single(node => node.NodeType == InGameNodeType.Boss).Floor, Is.EqualTo(ExpectedFloorCount));
            Assert.That(nodes.Count(node => node.NodeType == InGameNodeType.EliteBattle), Is.GreaterThanOrEqualTo(1));
            Assert.That(nodes.Count(node => node.NodeType == InGameNodeType.Event), Is.GreaterThanOrEqualTo(1));
            Assert.That(nodes.Count(node => node.NodeType == InGameNodeType.RestShop), Is.GreaterThanOrEqualTo(1));
            Assert.That(nodes.Count(node => node.NodeType == InGameNodeType.Treasure), Is.GreaterThanOrEqualTo(1));
            Assert.That(nodes.Count(node => node.Floor == ExpectedFloorCount - 1 && node.NodeType == InGameNodeType.RestShop), Is.GreaterThanOrEqualTo(1));
            AssertEdgesAdvanceToNextFloor(nodes);
            AssertEveryFloorReachable(nodes);
            AssertTreasureNodesConnectAllNextFloor(nodes);
        }

        [Test]
        public void Generate_WithUnknownMapTemplateId_UsesDefaultPresetAndLogsWarning()
        {
            BattleMapGenerator generator = new BattleMapGenerator();
            RuntimeRunDefinition runDefinition = CreateRunDefinition(9999);

            LogAssert.Expect(LogType.Warning, new Regex("MapTemplateId preset is unknown\\. id=9999\\. Fallback to default preset\\."));

            IReadOnlyList<RuntimeMapNode> nodes = generator.Generate(runDefinition, 12345);

            AssertMapStructure(nodes);
            Assert.That(nodes.Single(node => node.Floor == 1).NodeType, Is.EqualTo(InGameNodeType.Battle));
            Assert.That(nodes.Single(node => node.NodeType == InGameNodeType.Boss).Floor, Is.EqualTo(ExpectedFloorCount));
        }

        private static RuntimeRunDefinition CreateRunDefinition(int mapTemplateId = 6301)
        {
            RuntimeRunDefinitionBuilder builder = BattleTestData.RunDefinition();
            builder.RunProfileId = 5501;
            builder.Key = "run_test";
            builder.MapTemplateId = mapTemplateId;
            return builder.Build();
        }

        private static string BuildSignature(IReadOnlyList<RuntimeMapNode> nodes)
        {
            return string.Join(
                "|",
                nodes.Select(node => $"{node.NodeKey}:{node.NodeType}:{string.Join(",", node.NextNodeIndices)}"));
        }

        private static List<int> GetIndicesForFloor(IReadOnlyList<RuntimeMapNode> nodes, int floor)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Floor == floor)
                {
                    indices.Add(i);
                }
            }

            return indices;
        }

        private static void AssertMapStructure(IReadOnlyList<RuntimeMapNode> nodes)
        {
            Assert.That(nodes.Count, Is.InRange(ExpectedFloorCount, ExpectedFloorCount * MaxNodesPerFloor));
            Assert.That(
                nodes.Select(node => node.Floor).Distinct(),
                Is.EquivalentTo(Enumerable.Range(1, ExpectedFloorCount)));

            Assert.That(nodes.Count(node => node.Floor == 1), Is.EqualTo(1));
            Assert.That(nodes.Count(node => node.Floor == ExpectedFloorCount), Is.EqualTo(1));

            for (int floor = 1; floor <= ExpectedFloorCount; floor++)
            {
                int floorNodeCount = nodes.Count(node => node.Floor == floor);
                Assert.That(floorNodeCount, Is.InRange(MinNodesPerFloor, MaxNodesPerFloor), $"Floor {floor} node count should be in range.");
            }
        }

        private static string BuildFloorWidthSignature(IReadOnlyList<RuntimeMapNode> nodes)
        {
            return string.Join(
                ",",
                Enumerable.Range(1, ExpectedFloorCount)
                    .Select(floor => nodes.Count(node => node.Floor == floor)));
        }

        private static void AssertEdgesAdvanceToNextFloor(IReadOnlyList<RuntimeMapNode> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                RuntimeMapNode node = nodes[i];
                for (int j = 0; j < node.NextNodeIndices.Count; j++)
                {
                    int nextIndex = node.NextNodeIndices[j];
                    Assert.That(nextIndex, Is.GreaterThanOrEqualTo(0));
                    Assert.That(nextIndex, Is.LessThan(nodes.Count));
                    Assert.That(nodes[nextIndex].Floor, Is.EqualTo(node.Floor + 1));
                }
            }
        }

        private static void AssertEveryFloorReachable(IReadOnlyList<RuntimeMapNode> nodes)
        {
            for (int floor = 1; floor < ExpectedFloorCount; floor++)
            {
                List<int> currentFloorIndices = GetIndicesForFloor(nodes, floor);
                List<int> nextFloorIndices = GetIndicesForFloor(nodes, floor + 1);
                foreach (int nextIndex in nextFloorIndices)
                {
                    bool reachable = currentFloorIndices.Any(currentIndex => nodes[currentIndex].NextNodeIndices.Contains(nextIndex));
                    Assert.That(reachable, Is.True, $"Floor {floor + 1} node {nextIndex} should be reachable.");
                }
            }
        }

        private static void AssertTreasureNodesConnectAllNextFloor(IReadOnlyList<RuntimeMapNode> nodes)
        {
            IEnumerable<RuntimeMapNode> treasureNodes = nodes.Where(node => node.NodeType == InGameNodeType.Treasure);
            foreach (RuntimeMapNode treasureNode in treasureNodes)
            {
                List<int> nextFloorIndices = GetIndicesForFloor(nodes, treasureNode.Floor + 1);
                Assert.That(treasureNode.NextNodeIndices, Is.EquivalentTo(nextFloorIndices));
            }
        }
    }
}
