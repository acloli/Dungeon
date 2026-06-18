using System;
using System.Collections.Generic;
using TFramework.UI;
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
        [SerializeField] private Image _battlePanelBackgroundImage;
        [SerializeField] private BattlePageView _battlePageView;
        [SerializeField] private Transform _ownedRelicRoot;
        [SerializeField] private BattleMultiIconView _ownedRelicTemplate;
        [SerializeField] private GameObject _ownedRelicHintRoot;
        [SerializeField] private TFTextUGUI _ownedRelicHintText;
        [SerializeField] private CanvasGroup _ownedRelicCanvasGroup;
        [SerializeField] private CanvasGroup _ownedRelicHintCanvasGroup;
        [SerializeField] private Transform _ownedPotionRoot;
        [SerializeField] private BattleMultiIconView _ownedPotionTemplate;
        [SerializeField] private GameObject _ownedPotionHintRoot;
        [SerializeField] private TFTextUGUI _ownedPotionHintText;
        [SerializeField] private Button _ownedPotionUseButton;
        [SerializeField] private CanvasGroup _ownedPotionCanvasGroup;
        [SerializeField] private CanvasGroup _ownedPotionHintCanvasGroup;
        [SerializeField] private CanvasGroup _ownedPotionUseCanvasGroup;
        [SerializeField] private Button _hostBackgroundButton;
        [SerializeField] private Button _saveQuitButton;

        private readonly List<BattleMultiIconView> _ownedRelicViews = new List<BattleMultiIconView>();
        private readonly List<BattleMultiIconView> _ownedPotionViews = new List<BattleMultiIconView>();

        /// <summary>
        /// 戦闘画面View取得
        /// </summary>
        public IBattlePageView BattlePageView => _battlePageView;

        public void BuildOwnedRelics(IReadOnlyList<Model.BattleMultiIconViewModel> relics, Action<int> onClicked)
        {
            ClearOwnedRelics();

            if (_ownedRelicRoot == null || _ownedRelicTemplate == null || relics == null || relics.Count == 0)
            {
                SetOwnedRelicHint(string.Empty, Model.BattleSceneConstants.UnselectedCardIndex);
                return;
            }

            _ownedRelicRoot.gameObject.SetActive(true);
            _ownedRelicTemplate.gameObject.SetActive(false);

            for (int i = 0; i < relics.Count; i++)
            {
                int relicIndex = i;
                Model.BattleMultiIconViewModel relic = relics[relicIndex];
                if (relic == null)
                {
                    continue;
                }

                BattleMultiIconView relicView = Instantiate(_ownedRelicTemplate, _ownedRelicRoot);
                relicView.gameObject.SetActive(true);
                relicView.Bind(relic, () => onClicked?.Invoke(relicIndex));
                relicView.SetCompactLayout(true);
                _ownedRelicViews.Add(relicView);
            }
        }

        public void BuildOwnedPotions(IReadOnlyList<Model.BattleMultiIconViewModel> potions, Action<int> onClicked)
        {
            ClearOwnedPotions();

            if (_ownedPotionRoot == null || _ownedPotionTemplate == null || potions == null || potions.Count == 0)
            {
                SetOwnedPotionHint(string.Empty, Model.BattleSceneConstants.UnselectedCardIndex);
                SetOwnedPotionUseVisible(false, null);
                return;
            }

            _ownedPotionRoot.gameObject.SetActive(true);
            _ownedPotionTemplate.gameObject.SetActive(false);

            for (int i = 0; i < potions.Count; i++)
            {
                int potionIndex = i;
                Model.BattleMultiIconViewModel potion = potions[potionIndex];
                if (potion == null)
                {
                    continue;
                }

                BattleMultiIconView potionView = Instantiate(_ownedPotionTemplate, _ownedPotionRoot);
                potionView.gameObject.SetActive(true);
                potionView.Bind(potion, () => onClicked?.Invoke(potionIndex));
                potionView.SetCompactLayout(true);
                _ownedPotionViews.Add(potionView);
            }
        }

        public void SetOwnedRelicHint(string message, int selectedIndex)
        {
            bool visible = selectedIndex >= 0 && !string.IsNullOrEmpty(message);
            if (_ownedRelicHintRoot != null)
            {
                _ownedRelicHintRoot.SetActive(visible);
            }

            if (_ownedRelicHintText != null)
            {
                _ownedRelicHintText.text = visible ? message : string.Empty;
            }
        }

        public void SetOwnedPotionHint(string message, int selectedIndex)
        {
            bool visible = selectedIndex >= 0 && !string.IsNullOrEmpty(message);
            if (_ownedPotionHintRoot != null)
            {
                _ownedPotionHintRoot.SetActive(visible);
            }

            if (_ownedPotionHintText != null)
            {
                _ownedPotionHintText.text = visible ? message : string.Empty;
            }
        }

        public void SetOwnedPotionUseVisible(bool visible, Action onClicked)
        {
            if (_ownedPotionUseButton == null)
            {
                return;
            }

            _ownedPotionUseButton.onClick.RemoveAllListeners();
            _ownedPotionUseButton.gameObject.SetActive(visible);
            _ownedPotionUseButton.interactable = visible;
            if (visible)
            {
                _ownedPotionUseButton.onClick.AddListener(() => onClicked?.Invoke());
            }
        }

        public void ClearOwnedRelics()
        {
            for (int i = 0; i < _ownedRelicViews.Count; i++)
            {
                BattleMultiIconView relicView = _ownedRelicViews[i];
                if (relicView == null)
                {
                    continue;
                }

                relicView.Clear();
                Destroy(relicView.gameObject);
            }

            _ownedRelicViews.Clear();

            if (_ownedRelicRoot != null)
            {
                _ownedRelicRoot.gameObject.SetActive(false);
            }
        }

        public void ClearOwnedPotions()
        {
            for (int i = 0; i < _ownedPotionViews.Count; i++)
            {
                BattleMultiIconView potionView = _ownedPotionViews[i];
                if (potionView == null)
                {
                    continue;
                }

                potionView.Clear();
                Destroy(potionView.gameObject);
            }

            _ownedPotionViews.Clear();

            if (_ownedPotionRoot != null)
            {
                _ownedPotionRoot.gameObject.SetActive(false);
            }

            SetOwnedPotionUseVisible(false, null);
        }

        public void SetHostChromeInteractable(bool interactable)
        {
            ApplyCanvasGroupState(_ownedRelicCanvasGroup, interactable);
            ApplyCanvasGroupState(_ownedRelicHintCanvasGroup, interactable);
            ApplyCanvasGroupState(_ownedPotionCanvasGroup, interactable);
            ApplyCanvasGroupState(_ownedPotionHintCanvasGroup, interactable);
            ApplyCanvasGroupState(_ownedPotionUseCanvasGroup, interactable);

            if (_hostBackgroundButton != null)
            {
                _hostBackgroundButton.interactable = interactable;
            }
        }

        public void WireHostBackgroundClick(Action onClicked)
        {
            UnwireHostBackgroundClick();
            if (_hostBackgroundButton != null)
            {
                _hostBackgroundButton.onClick.AddListener(() => onClicked?.Invoke());
            }
        }

        public void UnwireHostBackgroundClick()
        {
            if (_hostBackgroundButton != null)
            {
                _hostBackgroundButton.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// 戦闘基底表示切り替え
        /// </summary>
        public void SetBattleVisible(bool visible)
        {
            if (_battlePanel != null)
            {
                if (!_battlePanel.activeSelf)
                {
                    _battlePanel.SetActive(true);
                }

                if (_battlePanelBackgroundImage != null)
                {
                    _battlePanelBackgroundImage.enabled = visible;
                }

                for (int i = 0; i < _battlePanel.transform.childCount; i++)
                {
                    Transform child = _battlePanel.transform.GetChild(i);
                    if (child == null)
                    {
                        continue;
                    }

                    if ((_ownedRelicRoot != null && child == _ownedRelicRoot) ||
                        (_ownedRelicHintRoot != null && child.gameObject == _ownedRelicHintRoot) ||
                        (_ownedPotionRoot != null && child == _ownedPotionRoot) ||
                        (_ownedPotionHintRoot != null && child.gameObject == _ownedPotionHintRoot) ||
                        (_ownedPotionUseButton != null && child.gameObject == _ownedPotionUseButton.gameObject) ||
                        (_hostBackgroundButton != null && child.gameObject == _hostBackgroundButton.gameObject))
                    {
                        continue;
                    }

                    child.gameObject.SetActive(visible);
                }
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

        private static void ApplyCanvasGroupState(CanvasGroup canvasGroup, bool interactable)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }
    }
}
