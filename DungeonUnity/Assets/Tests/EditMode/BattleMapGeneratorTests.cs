using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Domain;
using Dungeon.Tests.EditMode.Support;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleMapGeneratorのEditorモードテストクラス
    /// </summary>
    [TestFixture]
    public sealed class BattleMapGeneratorTests
    {
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
        public void Generate_SatisfiesExpectedConstraints()
        {
            BattleMapGenerator generator = new BattleMapGenerator();
            RuntimeRunDefinition runDefinition = CreateRunDefinition();

            IReadOnlyList<RuntimeMapNode> nodes = generator.Generate(runDefinition, 12345);

            Assert.That(nodes.Count, Is.EqualTo(10));
            Assert.That(nodes.Count(node => node.Floor == 1), Is.EqualTo(1));
            Assert.That(nodes.Count(node => node.Floor == 2), Is.EqualTo(2));
            Assert.That(nodes.Count(node => node.Floor == 3), Is.EqualTo(2));
            Assert.That(nodes.Count(node => node.Floor == 4), Is.EqualTo(2));
            Assert.That(nodes.Count(node => node.Floor == 5), Is.EqualTo(2));
            Assert.That(nodes.Count(node => node.Floor == 6), Is.EqualTo(1));
            Assert.That(nodes.Count(node => node.NodeType == InGameNodeType.Boss), Is.EqualTo(1));
            Assert.That(nodes.Single(node => node.Floor == 6).NodeType, Is.EqualTo(InGameNodeType.Boss));
            Assert.That(nodes.Count(node => node.NodeType == InGameNodeType.EliteBattle), Is.GreaterThanOrEqualTo(1));
            Assert.That(nodes.Count(node => node.NodeType == InGameNodeType.Event), Is.GreaterThanOrEqualTo(1));
            Assert.That(nodes.Count(node => node.NodeType == InGameNodeType.RestShop), Is.GreaterThanOrEqualTo(2));
            Assert.That(nodes.Count(node => node.Floor == 5 && node.NodeType == InGameNodeType.RestShop), Is.GreaterThanOrEqualTo(1));

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

            for (int floor = 1; floor < 6; floor++)
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

        private static RuntimeRunDefinition CreateRunDefinition()
        {
            RuntimeRunDefinitionBuilder builder = BattleTestData.RunDefinition();
            builder.RunProfileId = 5501;
            builder.Key = "run_test";
            builder.MapTemplateId = 6301;
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
    }
}
