using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
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
        [SerializeField] private BattleOptionButtonView _nodeButtonTemplate;
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
            SetMapButtonInteractable(_param.Snapshot.CurrentNodeIndex + 1);
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
            if (_nodeRoot == null || _nodeButtonTemplate == null || nodes == null)
            {
                return;
            }

            _nodeButtonTemplate.gameObject.SetActive(false);

            for (int i = 0; i < nodes.Count; i++)
            {
                int nodeIndex = i;
                RuntimeMapNode node = nodes[nodeIndex];
                BattleOptionButtonView button = Instantiate(_nodeButtonTemplate, _nodeRoot);
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
        public void SetMapButtonInteractable(int allowedIndex)
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].SetInteractable(i == allowedIndex);
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
    }
}
