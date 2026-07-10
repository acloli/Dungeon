using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// マップ画面クラス
    /// </summary>
    public sealed class MapPage : UIPageBase, IMapPageView
    {
        [SerializeField] private Transform _nodeRoot;
        [SerializeField] private BattleOptionButtonView _battleNodeButtonTemplate;
        [SerializeField] private BattleOptionButtonView _eliteNodeButtonTemplate;
        [SerializeField] private BattleOptionButtonView _eventNodeButtonTemplate;
        [SerializeField] private BattleOptionButtonView _restNodeButtonTemplate;
        [SerializeField] private BattleOptionButtonView _bossNodeButtonTemplate;
        [SerializeField] private TFTextUGUI _stateText;
        [SerializeField] private Transform _connectionRoot;
        [SerializeField] private Image _connectionLineTemplate;
        [SerializeField] private Vector2 _nodeSpacing = new Vector2(160f, 120f);
        [SerializeField] private Vector2 _nodeOffset = Vector2.zero;
        [SerializeField] private float _connectionLineThickness = 6f;
        [SerializeField] private Color _availableConnectionColor = new Color(0.95f, 0.74f, 0.28f, 0.8f);
        [SerializeField] private Color _visitedConnectionColor = new Color(0.57f, 0.78f, 0.59f, 0.55f);
        [SerializeField] private Color _fogConnectionColor = new Color(0.44f, 0.47f, 0.55f, 0.25f);
        [SerializeField] private float _currentNodeAlpha = 1f;
        [SerializeField] private float _availableNodeAlpha = 1f;
        [SerializeField] private float _visitedNodeAlpha = 0.68f;
        [SerializeField] private float _fogNodeAlpha = 0.32f;
        [SerializeField] private Vector3 _currentNodeScale = new Vector3(1.08f, 1.08f, 1f);

        private readonly List<BattleOptionButtonView> _buttons = new List<BattleOptionButtonView>();
        private readonly List<int> _buttonNodeIndices = new List<int>();
        private readonly List<Image> _connectionLines = new List<Image>();
        private BattleMapPageParam _param;

        /// <summary>
        /// ページ表示内容適用
        /// </summary>
        public void Apply(BattleMapPageParam param)
        {
            _param = param;
            if (_param == null)
            {
                ClearDynamicButtons();
                SetMapStateText(string.Empty);
                return;
            }

            BuildMapGraph(
                _param.Snapshot.Nodes,
                _param.Snapshot.NodeLayouts,
                _param.Snapshot.AvailableNodeIndices,
                _param.Snapshot.CurrentFloor,
                _param.Snapshot.CurrentNodeIndex,
                _param.OnMapNodeClicked);
            SetMapStateText(_param.Snapshot.MapMessage);
        }

        /// <summary>
        /// マップ状態文言反映
        /// </summary>
        public void SetMapStateText(string message)
        {
            if (_stateText != null)
            {
                _stateText.text = message;
            }
        }

        /// <summary>
        /// ノード選択ボタン構築
        /// </summary>
        public void BuildMapButtons(IReadOnlyList<RuntimeMapNode> nodes, Action<int> onClicked)
        {
            ClearDynamicButtons();
            if (_nodeRoot == null || nodes == null)
            {
                return;
            }

            SetNodeRootLayoutEnabled(true);
            DeactivateTemplate(_battleNodeButtonTemplate);
            DeactivateTemplate(_eliteNodeButtonTemplate);
            DeactivateTemplate(_eventNodeButtonTemplate);
            DeactivateTemplate(_restNodeButtonTemplate);
            DeactivateTemplate(_bossNodeButtonTemplate);
            DeactivateConnectionTemplate(_connectionLineTemplate);

            for (int i = 0; i < nodes.Count; i++)
            {
                int nodeIndex = i;
                RuntimeMapNode node = nodes[nodeIndex];
                BattleOptionButtonView template = ResolveTemplate(node.NodeType);
                if (template == null)
                {
                    continue;
                }

                BattleOptionButtonView button = Instantiate(template, _nodeRoot);
                button.gameObject.SetActive(true);
                button.Configure(
                    string.Format(BattleSceneConstants.MapNodeLabelFormat, node.Floor, node.DisplayName),
                    delegate
                    {
                        onClicked?.Invoke(nodeIndex);
                    });
                _buttons.Add(button);
                _buttonNodeIndices.Add(nodeIndex);
            }
        }

        /// <summary>
        /// 遷移可能ノード反映
        /// </summary>
        public void SetMapButtonInteractable(IReadOnlyList<int> allowedIndices)
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                int nodeIndex = i < _buttonNodeIndices.Count ? _buttonNodeIndices[i] : i;
                _buttons[i].SetInteractable(ContainsIndex(allowedIndices, nodeIndex));
            }
        }

        /// <summary>
        /// グラフ形式マップ構築
        /// </summary>
        public void BuildMapGraph(
            IReadOnlyList<RuntimeMapNode> nodes,
            IReadOnlyList<MapNodeLayout> layouts,
            IReadOnlyList<int> availableNodeIndices,
            int currentFloor,
            int currentNodeIndex,
            Action<int> onNodeClicked)
        {
            ClearDynamicButtons();
            if (_nodeRoot == null || nodes == null)
            {
                return;
            }

            SetNodeRootLayoutEnabled(false);
            DeactivateTemplate(_battleNodeButtonTemplate);
            DeactivateTemplate(_eliteNodeButtonTemplate);
            DeactivateTemplate(_eventNodeButtonTemplate);
            DeactivateTemplate(_restNodeButtonTemplate);
            DeactivateTemplate(_bossNodeButtonTemplate);
            DeactivateConnectionTemplate(_connectionLineTemplate);

            Dictionary<int, MapNodeLayout> layoutByNodeIndex = BuildLayoutLookup(layouts);
            Dictionary<int, Vector2> positionByNodeIndex = BuildPositionLookup(nodes, layoutByNodeIndex);
            BuildConnectionLines(nodes, positionByNodeIndex, availableNodeIndices, currentFloor, currentNodeIndex);
            BuildGraphButtons(nodes, positionByNodeIndex, availableNodeIndices, currentFloor, currentNodeIndex, onNodeClicked);
        }

        /// <summary>
        /// 表示フロア位置反映
        /// </summary>
        public void SetScrollPosition(int floor)
        {
        }

        /// <summary>
        /// 動的ノードボタン消去
        /// </summary>
        public void ClearDynamicButtons()
        {
            ClearButtons(_buttons);
            _buttonNodeIndices.Clear();
            ClearConnectionLines(_connectionLines);
        }

        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            Apply(param as BattleMapPageParam);
            return UniTask.CompletedTask;
        }

        protected override void OnClosed()
        {
            ClearDynamicButtons();
        }

        protected override void OnTerminate()
        {
            ClearDynamicButtons();
        }

        /// <summary>
        /// ノード種別に応じたテンプレートを返す
        /// </summary>
        private BattleOptionButtonView ResolveTemplate(InGameNodeType nodeType)
        {
            return nodeType switch
            {
                InGameNodeType.Battle => _battleNodeButtonTemplate,
                InGameNodeType.EliteBattle => _eliteNodeButtonTemplate ?? _battleNodeButtonTemplate,
                InGameNodeType.Event => _eventNodeButtonTemplate ?? _battleNodeButtonTemplate,
                InGameNodeType.RestShop => _restNodeButtonTemplate ?? _battleNodeButtonTemplate,
                InGameNodeType.Boss => _bossNodeButtonTemplate ?? _battleNodeButtonTemplate,
                _ => _battleNodeButtonTemplate
            };
        }

        /// <summary>
        /// グラフ用ノードボタン構築
        /// </summary>
        private void BuildGraphButtons(
            IReadOnlyList<RuntimeMapNode> nodes,
            IReadOnlyDictionary<int, Vector2> positionByNodeIndex,
            IReadOnlyList<int> availableNodeIndices,
            int currentFloor,
            int currentNodeIndex,
            Action<int> onNodeClicked)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                int nodeIndex = i;
                RuntimeMapNode node = nodes[nodeIndex];
                BattleOptionButtonView template = ResolveTemplate(node.NodeType);
                if (template == null)
                {
                    continue;
                }

                BattleOptionButtonView button = Instantiate(template, _nodeRoot);
                button.gameObject.SetActive(true);
                button.Configure(
                    string.Format(BattleSceneConstants.MapNodeLabelFormat, node.Floor, node.DisplayName),
                    delegate
                    {
                        onNodeClicked?.Invoke(nodeIndex);
                    });

                RectTransform rectTransform = button.transform as RectTransform;
                if (rectTransform != null && positionByNodeIndex.TryGetValue(nodeIndex, out Vector2 position))
                {
                    rectTransform.anchoredPosition = position;
                }

                MapNodeViewState state = ResolveNodeState(
                    node,
                    nodeIndex,
                    availableNodeIndices,
                    currentFloor,
                    currentNodeIndex);
                ApplyNodeViewState(button, state);

                _buttons.Add(button);
                _buttonNodeIndices.Add(nodeIndex);
            }
        }

        /// <summary>
        /// ノード間接続線構築
        /// </summary>
        private void BuildConnectionLines(
            IReadOnlyList<RuntimeMapNode> nodes,
            IReadOnlyDictionary<int, Vector2> positionByNodeIndex,
            IReadOnlyList<int> availableNodeIndices,
            int currentFloor,
            int currentNodeIndex)
        {
            Transform parent = _connectionRoot != null ? _connectionRoot : _nodeRoot;
            if (parent == null)
            {
                return;
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                if (!positionByNodeIndex.TryGetValue(i, out Vector2 startPosition))
                {
                    continue;
                }

                RuntimeMapNode sourceNode = nodes[i];
                IReadOnlyList<int> nextNodeIndices = sourceNode.NextNodeIndices;
                for (int j = 0; j < nextNodeIndices.Count; j++)
                {
                    int targetIndex = nextNodeIndices[j];
                    if (targetIndex < 0 ||
                        targetIndex >= nodes.Count ||
                        !positionByNodeIndex.TryGetValue(targetIndex, out Vector2 endPosition))
                    {
                        continue;
                    }

                    Image line = CreateConnectionLine(parent);
                    ConfigureConnectionLine(
                        line,
                        startPosition,
                        endPosition,
                        ResolveConnectionColor(nodes, i, targetIndex, availableNodeIndices, currentFloor, currentNodeIndex));
                    _connectionLines.Add(line);
                }
            }
        }

        /// <summary>
        /// 接続線を生成する
        /// </summary>
        private Image CreateConnectionLine(Transform parent)
        {
            Image line;
            if (_connectionLineTemplate != null)
            {
                line = Instantiate(_connectionLineTemplate, parent);
            }
            else
            {
                GameObject lineObject = new GameObject("MapConnectionLine", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                lineObject.transform.SetParent(parent, false);
                line = lineObject.GetComponent<Image>();
            }

            line.gameObject.SetActive(true);
            line.raycastTarget = false;
            return line;
        }

        /// <summary>
        /// 接続線の位置と色を反映する
        /// </summary>
        private void ConfigureConnectionLine(Image line, Vector2 startPosition, Vector2 endPosition, Color color)
        {
            if (line == null)
            {
                return;
            }

            Vector2 delta = endPosition - startPosition;
            RectTransform rectTransform = line.transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = startPosition + (delta * 0.5f);
                rectTransform.sizeDelta = new Vector2(delta.magnitude, _connectionLineThickness);
                rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            }

            line.color = color;
        }

        /// <summary>
        /// ノード表示状態を反映する
        /// </summary>
        private void ApplyNodeViewState(BattleOptionButtonView button, MapNodeViewState state)
        {
            if (button == null)
            {
                return;
            }

            button.SetInteractable(state == MapNodeViewState.Available);
            button.transform.localScale = state == MapNodeViewState.Current ? _currentNodeScale : Vector3.one;

            CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = ResolveNodeAlpha(state);
        }

        /// <summary>
        /// ノード表示状態を判定する
        /// </summary>
        private MapNodeViewState ResolveNodeState(
            RuntimeMapNode node,
            int nodeIndex,
            IReadOnlyList<int> availableNodeIndices,
            int currentFloor,
            int currentNodeIndex)
        {
            if (nodeIndex == currentNodeIndex)
            {
                return MapNodeViewState.Current;
            }

            if (ContainsIndex(availableNodeIndices, nodeIndex))
            {
                return MapNodeViewState.Available;
            }

            if (currentNodeIndex >= 0 && node.Floor < currentFloor)
            {
                return MapNodeViewState.Visited;
            }

            return MapNodeViewState.Fog;
        }

        /// <summary>
        /// ノード表示状態の透明度を返す
        /// </summary>
        private float ResolveNodeAlpha(MapNodeViewState state)
        {
            return state switch
            {
                MapNodeViewState.Current => _currentNodeAlpha,
                MapNodeViewState.Available => _availableNodeAlpha,
                MapNodeViewState.Visited => _visitedNodeAlpha,
                _ => _fogNodeAlpha
            };
        }

        /// <summary>
        /// 接続線の色を返す
        /// </summary>
        private Color ResolveConnectionColor(
            IReadOnlyList<RuntimeMapNode> nodes,
            int sourceIndex,
            int targetIndex,
            IReadOnlyList<int> availableNodeIndices,
            int currentFloor,
            int currentNodeIndex)
        {
            if (sourceIndex == currentNodeIndex && ContainsIndex(availableNodeIndices, targetIndex))
            {
                return _availableConnectionColor;
            }

            RuntimeMapNode targetNode = nodes[targetIndex];
            if (targetIndex == currentNodeIndex || (currentNodeIndex >= 0 && targetNode.Floor < currentFloor))
            {
                return _visitedConnectionColor;
            }

            return _fogConnectionColor;
        }

        /// <summary>
        /// レイアウト位置辞書を構築する
        /// </summary>
        private Dictionary<int, MapNodeLayout> BuildLayoutLookup(IReadOnlyList<MapNodeLayout> layouts)
        {
            Dictionary<int, MapNodeLayout> lookup = new Dictionary<int, MapNodeLayout>();
            if (layouts == null)
            {
                return lookup;
            }

            for (int i = 0; i < layouts.Count; i++)
            {
                MapNodeLayout layout = layouts[i];
                if (layout != null)
                {
                    lookup[layout.NodeIndex] = layout;
                }
            }

            return lookup;
        }

        /// <summary>
        /// ノード位置辞書を構築する
        /// </summary>
        private Dictionary<int, Vector2> BuildPositionLookup(
            IReadOnlyList<RuntimeMapNode> nodes,
            IReadOnlyDictionary<int, MapNodeLayout> layoutByNodeIndex)
        {
            Dictionary<int, Vector2> lookup = new Dictionary<int, Vector2>();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (layoutByNodeIndex.TryGetValue(i, out MapNodeLayout layout))
                {
                    lookup[i] = new Vector2(
                        _nodeOffset.x + (layout.X * _nodeSpacing.x),
                        _nodeOffset.y + (layout.Y * _nodeSpacing.y));
                }
            }

            return lookup;
        }

        /// <summary>
        /// ノードルートの自動レイアウト有効状態を切り替える
        /// </summary>
        private void SetNodeRootLayoutEnabled(bool enabled)
        {
            if (_nodeRoot == null)
            {
                return;
            }

            LayoutGroup layoutGroup = _nodeRoot.GetComponent<LayoutGroup>();
            if (layoutGroup != null)
            {
                layoutGroup.enabled = enabled;
            }
        }

        /// <summary>
        /// テンプレートを非表示にする
        /// </summary>
        private static void DeactivateTemplate(BattleOptionButtonView template)
        {
            if (template != null)
            {
                template.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 接続線テンプレートを非表示にする
        /// </summary>
        private static void DeactivateConnectionTemplate(Image template)
        {
            if (template != null)
            {
                template.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 動的ボタン一覧消去
        /// </summary>
        private static void ClearButtons(List<BattleOptionButtonView> buttons)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                BattleOptionButtonView button = buttons[i];
                if (button == null)
                {
                    continue;
                }

                button.Clear();
                Destroy(button.gameObject);
            }

            buttons.Clear();
        }

        /// <summary>
        /// 動的接続線一覧消去
        /// </summary>
        private static void ClearConnectionLines(List<Image> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                Image line = lines[i];
                if (line == null)
                {
                    continue;
                }

                Destroy(line.gameObject);
            }

            lines.Clear();
        }

        /// <summary>
        /// 遷移可能ノード判定
        /// </summary>
        private static bool ContainsIndex(IReadOnlyList<int> indices, int index)
        {
            if (indices == null)
            {
                return false;
            }

            for (int i = 0; i < indices.Count; i++)
            {
                if (indices[i] == index)
                {
                    return true;
                }
            }

            return false;
        }

        private enum MapNodeViewState
        {
            Current,
            Available,
            Visited,
            Fog
        }
    }
}
