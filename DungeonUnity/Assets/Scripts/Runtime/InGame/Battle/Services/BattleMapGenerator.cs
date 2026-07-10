using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using TFramework.Debug;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// Battle用の決定論的マップ生成クラス
    /// </summary>
    public sealed class BattleMapGenerator : IBattleMapGenerator
    {
        private const int DefaultPresetMapTemplateId = 6301;
        private static readonly int[] FloorNodeCounts = { 1, 2, 2, 2, 3, 2, 2, 1 };

        /// <summary>
        /// Run定義とシードからマップを生成する
        /// </summary>
        public IReadOnlyList<RuntimeMapNode> Generate(RuntimeRunDefinition runDefinition, int mapSeed)
        {
            WarnIfUnknownMapTemplateId(runDefinition);
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
        /// 未知のMapTemplateId利用時に警告を記録する
        /// </summary>
        private static void WarnIfUnknownMapTemplateId(RuntimeRunDefinition runDefinition)
        {
            if (runDefinition == null || runDefinition.MapTemplateId == DefaultPresetMapTemplateId)
            {
                return;
            }

            TLogger.Warning(
                $"MapTemplateId preset is unknown. id={runDefinition.MapTemplateId}. Fallback to default preset.",
                "Battle");
        }

        /// <summary>
        /// Floor 2-7 のノード種別を構築する
        /// </summary>
        private static List<InGameNodeType> BuildNodeTypes(Random random)
        {
            List<InGameNodeType> nodeTypes = new List<InGameNodeType>
            {
                InGameNodeType.EliteBattle,
                InGameNodeType.Event,
                InGameNodeType.Treasure,
                InGameNodeType.RestShop,
                InGameNodeType.RestShop,
                InGameNodeType.Battle,
                InGameNodeType.Battle,
                InGameNodeType.Battle,
                InGameNodeType.Battle,
                InGameNodeType.Battle,
                InGameNodeType.Battle,
                InGameNodeType.Battle,
                InGameNodeType.Battle
            };

            Shuffle(nodeTypes, random);

            EnsureNodeTypeInFloor(nodeTypes, InGameNodeType.RestShop, 7, random);

            return nodeTypes;
        }

        /// <summary>
        /// 指定フロアに必要なノード種別が含まれるよう補正する
        /// </summary>
        private static void EnsureNodeTypeInFloor(
            IList<InGameNodeType> nodeTypes,
            InGameNodeType nodeType,
            int floor,
            Random random)
        {
            int floorStart = GetNodeTypeStartIndex(floor);
            int floorCount = FloorNodeCounts[floor - 1];
            for (int i = 0; i < floorCount; i++)
            {
                if (nodeTypes[floorStart + i] == nodeType)
                {
                    return;
                }
            }

            int sourceIndex = FindNodeTypeIndexOutsideRange(nodeTypes, nodeType, floorStart, floorCount);
            int targetIndex = floorStart + random.Next(0, floorCount);
            InGameNodeType original = nodeTypes[targetIndex];
            nodeTypes[targetIndex] = nodeType;
            nodeTypes[sourceIndex] = original;
        }

        /// <summary>
        /// 指定フロアのノード種別リスト上の開始位置を返す
        /// </summary>
        private static int GetNodeTypeStartIndex(int floor)
        {
            int startIndex = 0;
            for (int i = 2; i < floor; i++)
            {
                startIndex += FloorNodeCounts[i - 1];
            }

            return startIndex;
        }

        /// <summary>
        /// 指定範囲外から対象ノード種別の位置を検索する
        /// </summary>
        private static int FindNodeTypeIndexOutsideRange(
            IList<InGameNodeType> nodeTypes,
            InGameNodeType nodeType,
            int rangeStart,
            int rangeCount)
        {
            int rangeEnd = rangeStart + rangeCount;
            for (int i = 0; i < nodeTypes.Count; i++)
            {
                if (i >= rangeStart && i < rangeEnd)
                {
                    continue;
                }

                if (nodeTypes[i] == nodeType)
                {
                    return i;
                }
            }

            throw new InvalidOperationException($"Node type guarantee source is missing. nodeType={nodeType}");
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

                EnsureReachability(nodes, currentFloor, nextFloor, floorConnections, random);

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

            if (ShouldConnectAllNextNodes(node.NodeType))
            {
                for (int i = 0; i < nextFloor.Count; i++)
                {
                    targets.Add(nextFloor[i]);
                }

                return targets;
            }

            int targetCount = random.Next(1, Math.Min(2, nextFloor.Count) + 1);
            List<int> candidates = new List<int>(nextFloor);
            Shuffle(candidates, random);
            for (int i = 0; i < targetCount; i++)
            {
                targets.Add(candidates[i]);
            }

            return targets;
        }

        /// <summary>
        /// 次フロア全ノードが到達可能になるよう補正する
        /// </summary>
        private static void EnsureReachability(
            IReadOnlyList<RuntimeMapNode> nodes,
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

                AddReachabilityTarget(nodes, targetIndex, currentFloor, floorConnections, random);
            }
        }

        /// <summary>
        /// 到達保証のために遷移先を追加する
        /// </summary>
        private static void AddReachabilityTarget(
            IReadOnlyList<RuntimeMapNode> nodes,
            int targetIndex,
            IReadOnlyList<int> currentFloor,
            IDictionary<int, List<int>> floorConnections,
            Random random)
        {
            List<int> candidates = new List<int>();
            for (int i = 0; i < currentFloor.Count; i++)
            {
                int sourceIndex = currentFloor[i];
                if (CanAddReachabilityTarget(nodes[sourceIndex], floorConnections[sourceIndex], targetIndex))
                {
                    candidates.Add(sourceIndex);
                }
            }

            if (candidates.Count > 0)
            {
                int sourceIndex = candidates[random.Next(0, candidates.Count)];
                floorConnections[sourceIndex].Add(targetIndex);
                return;
            }

            int replacementSourceIndex = FindReachabilityReplacementSource(currentFloor, floorConnections, random);
            List<int> targets = floorConnections[replacementSourceIndex];
            int replaceIndex = FindReplaceableTargetIndex(targets, currentFloor, floorConnections);
            targets[replaceIndex] = targetIndex;
        }

        /// <summary>
        /// 到達保証用の遷移先を追加できるか返す
        /// </summary>
        private static bool CanAddReachabilityTarget(RuntimeMapNode node, List<int> targets, int targetIndex)
        {
            if (targets.Contains(targetIndex))
            {
                return false;
            }

            return ShouldConnectAllNextNodes(node.NodeType) || targets.Count < 2;
        }

        /// <summary>
        /// 到達保証の置換元ノードを選択する
        /// </summary>
        private static int FindReachabilityReplacementSource(
            IReadOnlyList<int> currentFloor,
            IDictionary<int, List<int>> floorConnections,
            Random random)
        {
            List<int> candidates = new List<int>();
            for (int i = 0; i < currentFloor.Count; i++)
            {
                int sourceIndex = currentFloor[i];
                List<int> targets = floorConnections[sourceIndex];
                for (int j = 0; j < targets.Count; j++)
                {
                    if (CountIncomingPaths(targets[j], currentFloor, floorConnections) > 1)
                    {
                        candidates.Add(sourceIndex);
                        break;
                    }
                }
            }

            if (candidates.Count > 0)
            {
                return candidates[random.Next(0, candidates.Count)];
            }

            return currentFloor[random.Next(0, currentFloor.Count)];
        }

        /// <summary>
        /// 他の到達経路が残る遷移先の位置を返す
        /// </summary>
        private static int FindReplaceableTargetIndex(
            IReadOnlyList<int> targets,
            IReadOnlyList<int> currentFloor,
            IDictionary<int, List<int>> floorConnections)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (CountIncomingPaths(targets[i], currentFloor, floorConnections) > 1)
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>
        /// 指定ノードへの到達元数を返す
        /// </summary>
        private static int CountIncomingPaths(
            int targetIndex,
            IReadOnlyList<int> currentFloor,
            IDictionary<int, List<int>> floorConnections)
        {
            int count = 0;
            for (int i = 0; i < currentFloor.Count; i++)
            {
                int sourceIndex = currentFloor[i];
                if (floorConnections[sourceIndex].Contains(targetIndex))
                {
                    count++;
                }
            }

            return count;
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
        /// 次フロア全体に接続するノード種別か返す
        /// </summary>
        private static bool ShouldConnectAllNextNodes(InGameNodeType nodeType)
        {
            return nodeType == InGameNodeType.EliteBattle
                || nodeType == InGameNodeType.RestShop
                || nodeType == InGameNodeType.Treasure;
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
                InGameNodeType.Treasure => BattleSceneConstants.TreasureNodeDisplayName,
                InGameNodeType.Boss => "Boss",
                _ => "Battle"
            };
        }
    }
}
