using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// Battle用の決定論的マップ生成クラス
    /// </summary>
    public sealed class BattleMapGenerator : IBattleMapGenerator
    {
        private static readonly int[] FloorNodeCounts = { 1, 2, 2, 2, 2, 1 };

        /// <summary>
        /// Run定義とシードからマップを生成する
        /// </summary>
        public IReadOnlyList<RuntimeMapNode> Generate(RuntimeRunDefinition runDefinition, int mapSeed)
        {
            Random random = new Random(mapSeed);
            List<RuntimeMapNode> nodes = new List<RuntimeMapNode>();
            List<InGameNodeType> nodeTypes = BuildNodeTypes(random);
            List<int[]> floorIndices = new List<int[]>();
            int typeCursor = 0;

            for (int floor = 1; floor <= FloorNodeCounts.Length; floor++)
            {
                int nodeCount = FloorNodeCounts[floor - 1];
                int[] indices = new int[nodeCount];
                for (int slot = 0; slot < nodeCount; slot++)
                {
                    InGameNodeType nodeType;
                    if (floor == 1)
                    {
                        nodeType = InGameNodeType.Battle;
                    }
                    else if (floor == FloorNodeCounts.Length)
                    {
                        nodeType = InGameNodeType.Boss;
                    }
                    else
                    {
                        nodeType = nodeTypes[typeCursor];
                        typeCursor++;
                    }

                    indices[slot] = nodes.Count;
                    nodes.Add(new RuntimeMapNode(
                        BuildNodeId(floor, slot),
                        BuildNodeKey(floor, slot),
                        floor,
                        nodeType,
                        ResolveDisplayName(nodeType),
                        string.Empty,
                        Array.Empty<int>()));
                }

                floorIndices.Add(indices);
            }

            Dictionary<int, IReadOnlyList<int>> connections = BuildConnections(nodes, floorIndices, random);
            List<RuntimeMapNode> generatedNodes = new List<RuntimeMapNode>(nodes.Count);
            for (int i = 0; i < nodes.Count; i++)
            {
                RuntimeMapNode node = nodes[i];
                connections.TryGetValue(i, out IReadOnlyList<int> nextNodeIndices);
                generatedNodes.Add(new RuntimeMapNode(
                    node.Id,
                    node.NodeKey,
                    node.Floor,
                    node.NodeType,
                    node.DisplayName,
                    node.LocalizationKey,
                    nextNodeIndices ?? Array.Empty<int>()));
            }

            return generatedNodes;
        }

        /// <summary>
        /// Floor 2-5 のノード種別を構築する
        /// </summary>
        private static List<InGameNodeType> BuildNodeTypes(Random random)
        {
            List<InGameNodeType> nodeTypes = new List<InGameNodeType>
            {
                InGameNodeType.EliteBattle,
                InGameNodeType.Event,
                InGameNodeType.RestShop,
                InGameNodeType.RestShop,
                InGameNodeType.Battle,
                InGameNodeType.Battle,
                InGameNodeType.Battle,
                InGameNodeType.Battle
            };

            Shuffle(nodeTypes, random);

            int floorFiveStart = 6;
            if (nodeTypes[floorFiveStart] != InGameNodeType.RestShop
                && nodeTypes[floorFiveStart + 1] != InGameNodeType.RestShop)
            {
                int replacementIndex = nodeTypes[2] == InGameNodeType.RestShop ? 2 : 3;
                int targetIndex = floorFiveStart + random.Next(0, 2);
                InGameNodeType original = nodeTypes[targetIndex];
                nodeTypes[targetIndex] = InGameNodeType.RestShop;
                nodeTypes[replacementIndex] = original;
            }

            return nodeTypes;
        }

        /// <summary>
        /// フロア間接続を構築する
        /// </summary>
        private static Dictionary<int, IReadOnlyList<int>> BuildConnections(
            IReadOnlyList<RuntimeMapNode> nodes,
            IReadOnlyList<int[]> floorIndices,
            Random random)
        {
            Dictionary<int, IReadOnlyList<int>> connections = new Dictionary<int, IReadOnlyList<int>>();

            for (int floor = 0; floor < floorIndices.Count - 1; floor++)
            {
                int[] currentFloor = floorIndices[floor];
                int[] nextFloor = floorIndices[floor + 1];
                Dictionary<int, List<int>> floorConnections = new Dictionary<int, List<int>>();

                for (int i = 0; i < currentFloor.Length; i++)
                {
                    int currentIndex = currentFloor[i];
                    List<int> targets = CreateTargets(nodes[currentIndex], nextFloor, random);
                    floorConnections[currentIndex] = targets;
                }

                EnsureReachability(currentFloor, nextFloor, floorConnections, random);

                for (int i = 0; i < currentFloor.Length; i++)
                {
                    int currentIndex = currentFloor[i];
                    List<int> targets = floorConnections[currentIndex];
                    targets.Sort();
                    connections[currentIndex] = targets;
                }
            }

            return connections;
        }

        /// <summary>
        /// ノード種別に応じた遷移先を構築する
        /// </summary>
        private static List<int> CreateTargets(RuntimeMapNode node, IReadOnlyList<int> nextFloor, Random random)
        {
            List<int> targets = new List<int>();
            if (nextFloor.Count == 1)
            {
                targets.Add(nextFloor[0]);
                return targets;
            }

            if (node.NodeType == InGameNodeType.EliteBattle || node.NodeType == InGameNodeType.RestShop)
            {
                for (int i = 0; i < nextFloor.Count; i++)
                {
                    targets.Add(nextFloor[i]);
                }

                return targets;
            }

            bool connectBoth = random.Next(0, 2) == 0;
            if (connectBoth)
            {
                targets.Add(nextFloor[0]);
                targets.Add(nextFloor[1]);
                return targets;
            }

            targets.Add(nextFloor[random.Next(0, nextFloor.Count)]);
            return targets;
        }

        /// <summary>
        /// 次フロア全ノードが到達可能になるよう補正する
        /// </summary>
        private static void EnsureReachability(
            IReadOnlyList<int> currentFloor,
            IReadOnlyList<int> nextFloor,
            IDictionary<int, List<int>> floorConnections,
            Random random)
        {
            for (int i = 0; i < nextFloor.Count; i++)
            {
                int targetIndex = nextFloor[i];
                if (HasIncomingPath(targetIndex, currentFloor, floorConnections))
                {
                    continue;
                }

                int sourceIndex = currentFloor[random.Next(0, currentFloor.Count)];
                List<int> targets = floorConnections[sourceIndex];
                if (!targets.Contains(targetIndex))
                {
                    targets.Add(targetIndex);
                }
            }
        }

        /// <summary>
        /// 指定ノードに到達元が存在するかを返す
        /// </summary>
        private static bool HasIncomingPath(
            int targetIndex,
            IReadOnlyList<int> currentFloor,
            IDictionary<int, List<int>> floorConnections)
        {
            for (int i = 0; i < currentFloor.Count; i++)
            {
                int sourceIndex = currentFloor[i];
                if (floorConnections[sourceIndex].Contains(targetIndex))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// リストを Fisher-Yates でシャッフルする
        /// </summary>
        private static void Shuffle<T>(IList<T> values, Random random)
        {
            for (int i = values.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(0, i + 1);
                T current = values[i];
                values[i] = values[swapIndex];
                values[swapIndex] = current;
            }
        }

        /// <summary>
        /// ノードIDを生成する
        /// </summary>
        private static int BuildNodeId(int floor, int slot)
        {
            return 900000 + floor * 10 + slot;
        }

        /// <summary>
        /// ノードキーを生成する
        /// </summary>
        private static string BuildNodeKey(int floor, int slot)
        {
            return $"generated_{floor}_{slot}";
        }

        /// <summary>
        /// ノード種別に応じた表示名を返す
        /// </summary>
        private static string ResolveDisplayName(InGameNodeType nodeType)
        {
            return nodeType switch
            {
                InGameNodeType.Battle => "Battle",
                InGameNodeType.EliteBattle => "Elite",
                InGameNodeType.Event => "Event",
                InGameNodeType.RestShop => "Rest",
                InGameNodeType.Boss => "Boss",
                _ => "Battle"
            };
        }
    }
}
