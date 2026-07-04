using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using TFramework.UI;
using UnityEngine;

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

        private readonly List<BattleOptionButtonView> _buttons = new List<BattleOptionButtonView>();
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

            BuildMapButtons(_param.Snapshot.Nodes, _param.OnMapNodeClicked);
            SetMapButtonInteractable(_param.Snapshot.AvailableNodeIndices);
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

            DeactivateTemplate(_battleNodeButtonTemplate);
            DeactivateTemplate(_eliteNodeButtonTemplate);
            DeactivateTemplate(_eventNodeButtonTemplate);
            DeactivateTemplate(_restNodeButtonTemplate);
            DeactivateTemplate(_bossNodeButtonTemplate);

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
            }
        }

        /// <summary>
        /// 遷移可能ノード反映
        /// </summary>
        public void SetMapButtonInteractable(IReadOnlyList<int> allowedIndices)
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].SetInteractable(ContainsIndex(allowedIndices, i));
            }
        }

        /// <summary>
        /// 動的ノードボタン消去
        /// </summary>
        public void ClearDynamicButtons()
        {
            ClearButtons(_buttons);
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
    }
}
