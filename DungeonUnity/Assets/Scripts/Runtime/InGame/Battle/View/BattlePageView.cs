using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// 戦闘画面Viewクラス
    /// </summary>
    public sealed class BattlePageView : MonoBehaviour, IBattlePageView
    {
        [SerializeField] private TFTextUGUI _playerStatText;
        [SerializeField] private TFTextUGUI _enemyStatText;
        [SerializeField] private TFTextUGUI _battleHintText;
        [SerializeField] private Transform _handCardRoot;
        [SerializeField] private BattleOptionButtonView _handCardButtonTemplate;
        [SerializeField] private Button _enemyTargetButton;
        [SerializeField] private Button _endTurnButton;

        private readonly List<BattleOptionButtonView> _handButtons = new List<BattleOptionButtonView>();

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
        /// 固定ボタン登録
        /// </summary>
        public void WireButtons(Action onEnemyTargetClicked, Action onEndTurnClicked)
        {
            UnwireButtons();
            if (_enemyTargetButton != null)
            {
                _enemyTargetButton.onClick.AddListener(() => onEnemyTargetClicked?.Invoke());
            }
            if (_endTurnButton != null)
            {
                _endTurnButton.onClick.AddListener(() => onEndTurnClicked?.Invoke());
            }
        }

        /// <summary>
        /// 固定ボタン解除
        /// </summary>
        public void UnwireButtons()
        {
            if (_enemyTargetButton != null)
            {
                _enemyTargetButton.onClick.RemoveAllListeners();
            }
            if (_endTurnButton != null)
            {
                _endTurnButton.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// 戦闘状態文言反映
        /// </summary>
        public void SetBattleStateText(string playerText, string enemyText, string hintText)
        {
            if (_playerStatText != null)
            {
                _playerStatText.text = playerText;
            }
            if (_enemyStatText != null)
            {
                _enemyStatText.text = enemyText;
            }
            if (_battleHintText != null)
            {
                _battleHintText.text = hintText;
            }
        }

        /// <summary>
        /// 手札ボタン構築
        /// </summary>
        public void BuildHandButtons(IReadOnlyList<CardDefinition> hand, Action<int> onClicked)
        {
            ClearDynamicButtons();

            if (_handCardRoot == null || _handCardButtonTemplate == null || hand == null)
            {
                return;
            }

            _handCardButtonTemplate.gameObject.SetActive(false);

            for (int i = 0; i < hand.Count; i++)
            {
                int handIndex = i;
                CardDefinition card = hand[handIndex];
                BattleOptionButtonView button = Instantiate(_handCardButtonTemplate, _handCardRoot);
                button.gameObject.SetActive(true);
                button.Configure(
                    string.Format(BattleSceneConstants.CardLabelFormat, card.DisplayName, card.Cost, card.Damage),
                    delegate
                    {
                        onClicked?.Invoke(handIndex);
                    });
                _handButtons.Add(button);
            }
        }

        /// <summary>
        /// 動的手札ボタン消去
        /// </summary>
        public void ClearDynamicButtons()
        {
            ClearButtons(_handButtons);
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
            _playerStatText ??= BattleSceneViewBindingUtility.FindComponent<TFTextUGUI>("PlayerStatText");
            _enemyStatText ??= BattleSceneViewBindingUtility.FindComponent<TFTextUGUI>("EnemyStatText");
            _battleHintText ??= BattleSceneViewBindingUtility.FindComponent<TFTextUGUI>("BattleHintText");
            _handCardRoot ??= BattleSceneViewBindingUtility.FindTransform("HandCardRoot");
            _handCardButtonTemplate ??= BattleSceneViewBindingUtility.FindComponent<BattleOptionButtonView>("HandCardTemplate");
            _enemyTargetButton ??= BattleSceneViewBindingUtility.FindComponent<Button>("EnemyTargetButton");
            _endTurnButton ??= BattleSceneViewBindingUtility.FindComponent<Button>("EndTurnButton");
        }
    }
}
