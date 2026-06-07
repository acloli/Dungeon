using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
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
        private readonly List<Button> _enemyButtons = new List<Button>();

        /// <summary>
        /// 固定ボタン登録
        /// </summary>
        public void WireButtons(Action onEndTurnClicked)
        {
            UnwireButtons();
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
        /// 敵対象ボタン構築
        /// </summary>
        public void BuildEnemyButtons(IReadOnlyList<BattleEnemyViewModel> enemies, int selectedEnemyIndex, Action<int> onClicked)
        {
            ClearEnemyButtons();
            if (_enemyTargetButton == null || enemies == null)
            {
                return;
            }

            _enemyTargetButton.gameObject.SetActive(false);
            Transform root = _enemyTargetButton.transform.parent;
            for (int i = 0; i < enemies.Count; i++)
            {
                int enemyIndex = i;
                BattleEnemyViewModel enemy = enemies[enemyIndex];
                Button button = Instantiate(_enemyTargetButton, root);
                button.gameObject.SetActive(true);
                button.interactable = !enemy.IsDefeated;
                TFTextUGUI label = button.GetComponentInChildren<TFTextUGUI>();
                if (label != null)
                {
                    label.text = BuildEnemyButtonLabel(enemy, enemyIndex == selectedEnemyIndex);
                }

                button.onClick.AddListener(() => onClicked?.Invoke(enemyIndex));
                _enemyButtons.Add(button);
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
        public void BuildHandButtons(IReadOnlyList<RuntimeCard> hand, Action<int> onClicked)
        {
            ClearButtons(_handButtons);

            if (_handCardRoot == null || _handCardButtonTemplate == null || hand == null)
            {
                return;
            }

            _handCardButtonTemplate.gameObject.SetActive(false);

            for (int i = 0; i < hand.Count; i++)
            {
                int handIndex = i;
                RuntimeCard card = hand[handIndex];
                BattleOptionButtonView button = Instantiate(_handCardButtonTemplate, _handCardRoot);
                button.gameObject.SetActive(true);
                button.Configure(
                    string.Format(BattleSceneConstants.CardLabelFormat, card.DisplayName, card.Cost, card.PreviewDamage),
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
            ClearEnemyButtons();
        }

        /// <summary>
        /// 敵対象ボタン表示文言構築
        /// </summary>
        private static string BuildEnemyButtonLabel(BattleEnemyViewModel enemy, bool isSelected)
        {
            string marker = isSelected ? ">" : string.Empty;
            string state = enemy.IsDefeated ? BattleSceneConstants.DefeatedEnemyLabel : enemy.Hp.ToString();
            string label = string.Format(
                BattleSceneConstants.EnemyTargetButtonFormat,
                marker,
                enemy.SlotIndex + 1,
                enemy.DisplayName,
                state,
                enemy.Block);

            if (enemy.Intent == null)
            {
                return label;
            }

            return string.Format(BattleSceneConstants.EnemyTargetButtonIntentFormat, label, enemy.Intent.IntentName);
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
        /// 敵対象ボタン消去
        /// </summary>
        private void ClearEnemyButtons()
        {
            for (int i = 0; i < _enemyButtons.Count; i++)
            {
                Button button = _enemyButtons[i];
                if (button == null)
                {
                    continue;
                }

                button.onClick.RemoveAllListeners();
                Destroy(button.gameObject);
            }

            _enemyButtons.Clear();
        }
    }
}
