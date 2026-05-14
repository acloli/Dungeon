using System;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// 結果画面Viewクラス
    /// </summary>
    public sealed class ResultPageView : MonoBehaviour, IResultPageView
    {
        [SerializeField] private TFTextUGUI _resultText;
        [SerializeField] private Button _backButton;

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
        /// 戻りボタン登録
        /// </summary>
        public void WireButtons(Action onBackClicked)
        {
            UnwireButtons();
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(() => onBackClicked?.Invoke());
            }
        }

        /// <summary>
        /// 戻りボタン解除
        /// </summary>
        public void UnwireButtons()
        {
            if (_backButton != null)
            {
                _backButton.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// 結果文言反映
        /// </summary>
        public void SetResultText(string message)
        {
            if (_resultText != null)
            {
                _resultText.text = message;
            }
        }

        /// <summary>
        /// 参照補完
        /// </summary>
        private void ResolveBindings()
        {
            _resultText ??= BattleSceneViewBindingUtility.FindComponent<TFTextUGUI>("ResultText");
            _backButton ??= BattleSceneViewBindingUtility.FindComponent<Button>("ResultBackButton");
        }
    }
}
