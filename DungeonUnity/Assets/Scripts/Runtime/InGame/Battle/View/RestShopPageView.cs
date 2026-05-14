using System;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// 補給画面Viewクラス
    /// </summary>
    public sealed class RestShopPageView : MonoBehaviour, IRestShopPageView
    {
        [SerializeField] private TFTextUGUI _restShopText;
        [SerializeField] private Button _restButton;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _continueButton;

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

        /// <summary>
        /// 参照補完
        /// </summary>
        private void ResolveBindings()
        {
            _restShopText ??= BattleSceneViewBindingUtility.FindComponent<TFTextUGUI>("RestShopText");
            _restButton ??= BattleSceneViewBindingUtility.FindComponent<Button>("RestButton");
            _upgradeButton ??= BattleSceneViewBindingUtility.FindComponent<Button>("UpgradeButton");
            _shopButton ??= BattleSceneViewBindingUtility.FindComponent<Button>("ShopButton");
            _continueButton ??= BattleSceneViewBindingUtility.FindComponent<Button>("RestShopContinueButton");
        }
    }
}
