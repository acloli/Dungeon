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
        [SerializeField] private Button _saveQuitButton;

        private readonly List<BattleMultiIconView> _ownedRelicViews = new List<BattleMultiIconView>();

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

        /// <summary>
        /// 戦闘基底表示切り替え
        /// </summary>
        public void SetBattleVisible(bool visible)
        {
            if (_battlePanel != null)
            {
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
                        (_ownedRelicHintRoot != null && child.gameObject == _ownedRelicHintRoot))
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
    }
}
