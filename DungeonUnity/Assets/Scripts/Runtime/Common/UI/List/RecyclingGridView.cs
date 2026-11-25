using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.Common.UI.List
{
    /// <summary>
    /// ScrollRectの可視範囲だけを生成してセルを再利用
    /// </summary>
    public sealed class RecyclingGridView : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _template;
        [SerializeField] private Vector2 _cellSize = new Vector2(400, 562);
        [SerializeField] private Vector2 _spacing = new Vector2(52, 54);
        [SerializeField] private RectOffset _padding = new RectOffset();
        [SerializeField, Min(1)] private int _columns = 2;
        [SerializeField, Min(0)] private int _bufferRows = 1;
        private readonly List<Cell> _pool = new List<Cell>();
        private Func<RectTransform, int, IDisposable> _bind;
        private int _count;
        private int _generation;
        private Vector2 _viewportSize;

        public ScrollRect ScrollRect => _scrollRect;
        public RectTransform Template => _template;
        public int ItemCount => _count;
        public int ActiveCellCount { get; private set; }

        public IDisposable Bind<T>(IReadOnlyList<T> data, Func<RectTransform, T, int, IDisposable> bind)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (bind == null) throw new ArgumentNullException(nameof(bind));
            Clear();
            int generation = _generation;
            _count = data.Count;
            _bind = (cell, index) => bind(cell, data[index], index);
            _scrollRect.StopMovement();
            _scrollRect.content.anchoredPosition = Vector2.zero;
            Refresh(true);
            return new Binding(() =>
            {
                if (_generation == generation) Clear();
            });
        }

        public void Clear()
        {
            _generation++;
            foreach (Cell cell in _pool) Release(cell);
            _count = 0;
            _bind = null;
            ActiveCellCount = 0;
            if (_scrollRect != null && _scrollRect.content != null)
                _scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
        }

        private void OnEnable()
        {
            if (_scrollRect != null) _scrollRect.onValueChanged.AddListener(OnScrolled);
        }

        private void OnDisable()
        {
            if (_scrollRect != null) _scrollRect.onValueChanged.RemoveListener(OnScrolled);
            Clear();
        }

        private void OnScrolled(Vector2 position) => Refresh(false);

        private void LateUpdate()
        {
            if (_bind != null && _viewportSize != _scrollRect.viewport.rect.size) Refresh(true);
        }

        private void Refresh(bool force)
        {
            if (_bind == null || _scrollRect == null || _template == null) return;
            _viewportSize = _scrollRect.viewport.rect.size;
            int columns = Mathf.Max(1, _columns);
            float step = Mathf.Max(1, _cellSize.y + _spacing.y);
            int rows = Mathf.CeilToInt((float)_count / columns);
            float height = rows == 0 ? 0 : _padding.top + _padding.bottom + rows * step - _spacing.y;
            _scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                Mathf.Max(height, _viewportSize.y));
            int visibleRows = Mathf.CeilToInt(_viewportSize.y / step) + 1 + _bufferRows * 2;
            int capacity = Mathf.Min(_count, visibleRows * columns);
            while (_pool.Count < capacity)
            {
                RectTransform rect = Instantiate(_template, _scrollRect.content, false);
                rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                _pool.Add(new Cell { Rect = rect });
            }

            int firstRow = Mathf.Clamp(
                Mathf.FloorToInt((_scrollRect.content.anchoredPosition.y - _padding.top) / step) - _bufferRows,
                0, Mathf.Max(0, rows - visibleRows));
            ActiveCellCount = 0;
            for (int i = 0; i < _pool.Count; i++)
            {
                Cell cell = _pool[i];
                int index = firstRow * columns + i;
                if (i >= capacity || index >= _count)
                {
                    Release(cell);
                    continue;
                }

                cell.Rect.sizeDelta = _cellSize;
                cell.Rect.anchoredPosition = new Vector2(_padding.left + (index % columns) * (_cellSize.x + _spacing.x),
                    -_padding.top - (index / columns) * step);
                if (force || cell.Index != index)
                {
                    Release(cell);
                    cell.Rect.gameObject.SetActive(true);
                    cell.Index = index;
                    cell.Controls = _bind(cell.Rect, index);
                }

                ActiveCellCount++;
            }
        }

        private static void Release(Cell cell)
        {
            cell.Controls?.Dispose();
            cell.Controls = null;
            cell.Index = -1;
            if (cell.Rect != null) cell.Rect.gameObject.SetActive(false);
        }

        private sealed class Cell
        {
            public RectTransform Rect;
            public int Index = -1;
            public IDisposable Controls;
        }

        private sealed class Binding : IDisposable
        {
            private Action _dispose;
            public Binding(Action dispose) => _dispose = dispose;

            public void Dispose()
            {
                Action action = _dispose;
                _dispose = null;
                action?.Invoke();
            }
        }
    }
}
