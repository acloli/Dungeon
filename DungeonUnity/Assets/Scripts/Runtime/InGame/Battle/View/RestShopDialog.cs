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
    /// 補給ダイアログクラス
    /// </summary>
    public sealed class RestShopDialog : UIDialogBase<RestShopDialogAction>, IRestShopDialogView
    {
        [SerializeField] private TFTextUGUI _restShopText;
        [SerializeField] private Button _restButton;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _continueButton;

        private BattleRestShopDialogParam _param;

        /// <summary>
        /// 固定ボタン登録
        /// </summary>
        public void WireButtons(Action onRestClicked, Action onUpgradeClicked, Action onShopClicked, Action onContinueClicked)
        {
            UnwireButtons();
            if (_restButton != null)
            {
                _restButton.onClick.AddListener(() => onRestClicked?.Invoke());
            }
            if (_upgradeButton != null)
            {
                _upgradeButton.onClick.AddListener(() => onUpgradeClicked?.Invoke());
            }
            if (_shopButton != null)
            {
                _shopButton.onClick.AddListener(() => onShopClicked?.Invoke());
            }
            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(() => onContinueClicked?.Invoke());
            }
        }

        /// <summary>
        /// 固定ボタン解除
        /// </summary>
        public void UnwireButtons()
        {
            if (_restButton != null)
            {
                _restButton.onClick.RemoveAllListeners();
            }
            if (_upgradeButton != null)
            {
                _upgradeButton.onClick.RemoveAllListeners();
            }
            if (_shopButton != null)
            {
                _shopButton.onClick.RemoveAllListeners();
            }
            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// 補給状態文言反映
        /// </summary>
        public void SetRestShopText(string message)
        {
            if (_restShopText != null)
            {
                _restShopText.text = message;
            }
        }

        /// <summary>
        /// 継続ボタン活性反映
        /// </summary>
        public void SetRestShopContinueInteractable(bool interactable)
        {
            if (_continueButton != null)
            {
                _continueButton.interactable = interactable;
            }
        }

        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            _param = param as BattleRestShopDialogParam;
            return UniTask.CompletedTask;
        }

        protected override void OnOpened()
        {
            UnwireButtons();

            if (_param == null)
            {
                SetRestShopText(string.Empty);
                SetRestShopContinueInteractable(false);
                return;
            }

            SetRestShopText(_param.Snapshot.RestShopMessage);
            SetRestShopContinueInteractable(_param.Snapshot.IsRestShopContinueEnabled);

            if (_restButton != null)
            {
                _restButton.onClick.AddListener(() => CloseWithResult(RestShopDialogAction.Rest));
            }
            if (_upgradeButton != null)
            {
                _upgradeButton.onClick.AddListener(() => CloseWithResult(RestShopDialogAction.Upgrade));
            }
            if (_shopButton != null)
            {
                _shopButton.onClick.AddListener(() => CloseWithResult(RestShopDialogAction.Shop));
            }
            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(() => CloseWithResult(RestShopDialogAction.Continue));
            }
        }

        protected override void OnClosed()
        {
            UnwireButtons();
        }
    }
}
