using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using TFramework.UI;
using UnityEngine;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// マップ画面Viewクラス
    /// </summary>
    public sealed class MapPageView : MonoBehaviour, IMapPageView
    {
        [SerializeField] private Transform _nodeRoot;
        [SerializeField] private BattleOptionButtonView _nodeButtonTemplate;
        [SerializeField] private TFTextUGUI _stateText;

        private readonly List<BattleOptionButtonView> _buttons = new List<BattleOptionButtonView>();

        /// <summary>
        /// 実行時参照補完
        /// </summary>
        private void Awake()
        {
            ResolveBindings();
        }

        /// <summary>
        /// エディタ参照補完
        /// </summary>
        private void OnValidate()
        {
            ResolveBindings();
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
        public void BuildMapButtons(IReadOnlyList<MapTemplate.Node> nodes, Action<int> onClicked)
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
                MapTemplate.Node node = nodes[nodeIndex];
                BattleOptionButtonView button = Instantiate(_nodeButtonTemplate, _nodeRoot);
                button.gameObject.SetActive(true);
                button.Configure(
                    string.Format(BattleSceneConstants.MapNodeLabelFormat, nodeIndex + 1, node.Label),
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
        /// 参照補完
        /// </summary>
        private void ResolveBindings()
        {
            _nodeRoot ??= BattleSceneViewBindingUtility.FindTransform("MapNodeRoot");
            _nodeButtonTemplate ??= BattleSceneViewBindingUtility.FindComponent<BattleOptionButtonView>("MapNodeTemplate");
            _stateText ??= BattleSceneViewBindingUtility.FindComponent<TFTextUGUI>("MapStateText");
        }
    }
}
