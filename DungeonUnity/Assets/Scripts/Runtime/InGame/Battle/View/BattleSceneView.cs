using System;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// BattleScene基底Viewクラス
    /// </summary>
    public sealed class BattleSceneView : MonoBehaviour, IBattleSceneHostView
    {
        [Header("Battle Base")]
        [SerializeField] private GameObject _battlePanel;
        [SerializeField] private BattlePageView _battlePageView;
        [SerializeField] private Button _saveQuitButton;

        /// <summary>
        /// 戦闘画面View取得
        /// </summary>
        public IBattlePageView BattlePageView => _battlePageView != null ? _battlePageView : _battlePageView = GetComponent<BattlePageView>();

        /// <summary>
        /// 戦闘基底表示切り替え
        /// </summary>
        public void SetBattleVisible(bool visible)
        {
            if (_battlePanel != null)
            {
                _battlePanel.SetActive(visible);
            }
        }

        /// <summary>
        /// 中断ボタン表示切り替え
        /// </summary>
        public void SetSaveQuitVisible(bool visible)
        {
            if (_saveQuitButton != null)
            {
                _saveQuitButton.gameObject.SetActive(visible);
            }
        }

        /// <summary>
        /// 中断ボタン登録
        /// </summary>
        public void WireSaveQuitButton(Action onSaveQuitClicked)
        {
            UnwireSaveQuitButton();
            if (_saveQuitButton != null)
            {
                _saveQuitButton.onClick.AddListener(() => onSaveQuitClicked?.Invoke());
            }
        }

        /// <summary>
        /// 中断ボタン解除
        /// </summary>
        public void UnwireSaveQuitButton()
        {
            if (_saveQuitButton != null)
            {
                _saveQuitButton.onClick.RemoveAllListeners();
            }
        }
    }
}
