using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Runtime.Common.UI.List
{
    /// <summary>
    /// 少数の部品を外部 Prefab からデータ件数に合わせて再利用
    /// </summary>
    public sealed class PrefabListView : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private RectTransform _template;
        private readonly List<RectTransform> _items = new List<RectTransform>();
        public RectTransform Template => _template;
        public RectTransform Root => _root;

        public void Bind<T>(IReadOnlyList<T> data, Action<RectTransform, T, int> bind)
        {
            while (_items.Count < data.Count) _items.Add(Instantiate(_template, _root, false));
            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].gameObject.SetActive(i < data.Count);
                if (i < data.Count) bind(_items[i], data[i], i);
            }
        }

        public void Clear()
        {
            foreach (var item in _items)
                if (item != null)
                    item.gameObject.SetActive(false);
        }

        private void OnDisable() => Clear();
    }
}
