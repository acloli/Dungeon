using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Runtime.InGame.Domain
{
    [CreateAssetMenu(fileName = "MapTemplate", menuName = "Dungeon/InGame/Map Template")]
    public class MapTemplate : ScriptableObject
    {
        [Serializable]
        public struct Node
        {
            public InGameNodeType NodeType;
            public string Label;
        }

        [SerializeField] private List<Node> _nodes = new List<Node>();

        public IReadOnlyList<Node> Nodes => _nodes;
    }
}
