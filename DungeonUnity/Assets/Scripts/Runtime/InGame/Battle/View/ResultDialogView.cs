using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// 結果ダイアログViewクラス
    /// </summary>
    public sealed class ResultDialogView : UIDialogBase, IResultDialogView
    {
        [SerializeField] private TFTextUGUI _resultText;
        [SerializeField] private Button _backButton;

        private Action _onBackClicked;
        private BattleResultDialogParam _param;

        /// <summary>
        /// 戻りボタン登録
        /// </summary>
        public void WireButtons(Action onBackClicked)
        {
            _onBackClicked = onBackClicked;
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

            _onBackClicked = null;
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

        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            _param = param as BattleResultDialogParam;
            return UniTask.CompletedTask;
        }

        protected override void OnOpened()
        {
            UnwireButtons();

            if (_param != null)
            {
                SetResultText(_param.Snapshot.ResultMessage);
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackPressedInternal);
            }
        }

        protected override void OnClosed()
        {
            UnwireButtons();
        }

        private void OnBackPressedInternal()
        {
            _onBackClicked?.Invoke();
            Close();
        }
    }
}
