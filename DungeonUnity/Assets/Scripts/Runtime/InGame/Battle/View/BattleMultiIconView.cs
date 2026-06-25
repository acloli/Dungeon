using System;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// 汎用アイコン表示Viewクラス
    /// </summary>
    public class BattleMultiIconView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _frameImage;
        [SerializeField] private Image _artImage;
        [SerializeField] private Image _selectionHighlight;
        [SerializeField] private GameObject _disabledOverlay;
        [SerializeField] private TFTextUGUI _nameText;
        [SerializeField] private TFTextUGUI _descriptionText;
        [SerializeField] private TFTextUGUI _costText;
        [SerializeField] private TFTextUGUI _quantityText;

        public virtual void Bind(BattleMultiIconViewModel icon, Action onClick)
        {
            if (icon == null)
            {
                ClearVisuals();
                SetInteractable(false);
                return;
            }

            SetText(_nameText, icon.DisplayName);
            SetText(_descriptionText, icon.Description);
            SetText(_costText, icon.ShowCost ? icon.Cost.ToString() : string.Empty);
            SetText(_quantityText, icon.ShowQuantity ? icon.Quantity.ToString() : string.Empty);
            SetBadgeVisible(_costText, icon.ShowCost);
            SetActive(_quantityText, icon.ShowQuantity);
            SetActive(_selectionHighlight, icon.IsSelected);
            SetDisabledState(!icon.IsAffordable || !icon.IsInteractable);
            ApplyRarityColor(icon.Rarity);
            WireClick(onClick, icon.IsInteractable);
        }

        public virtual void Clear()
        {
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// アイコン下部の補助ラベルを設定する
        /// </summary>
        protected void SetFooterLabel(string value, bool isVisible)
        {
            SetText(_quantityText, isVisible ? value : string.Empty);
            SetActive(_quantityText, isVisible);
        }

        /// <summary>
        /// コンパクト表示切り替え
        /// </summary>
        public void SetCompactLayout(bool isCompact)
        {
            SetActive(_nameText, !isCompact);
            SetActive(_descriptionText, !isCompact);
            SetBadgeVisible(_costText, !isCompact && _costText != null && !string.IsNullOrEmpty(_costText.text));
            SetActive(_quantityText, !isCompact && _quantityText != null && !string.IsNullOrEmpty(_quantityText.text));
        }

        protected void SetArtSprite(Sprite sprite)
        {
            if (_artImage == null)
            {
                return;
            }

            _artImage.sprite = sprite;
        }

        private void ApplyRarityColor(CardRarity rarity)
        {
            if (_frameImage == null)
            {
                return;
            }

            _frameImage.color = rarity switch
            {
                CardRarity.Basic => new Color(0.72f, 0.72f, 0.72f, 1f),
                CardRarity.Common => new Color(0.88f, 0.88f, 0.88f, 1f),
                CardRarity.Uncommon => new Color(0.42f, 0.82f, 0.58f, 1f),
                CardRarity.Rare => new Color(0.95f, 0.8f, 0.3f, 1f),
                _ => Color.white
            };
        }

        private void ClearVisuals()
        {
            SetText(_nameText, string.Empty);
            SetText(_descriptionText, string.Empty);
            SetText(_costText, string.Empty);
            SetText(_quantityText, string.Empty);
            SetBadgeVisible(_costText, false);
            SetActive(_quantityText, false);
            SetActive(_selectionHighlight, false);
            SetDisabledState(false);
        }

        private void WireClick(Action onClick, bool isInteractable)
        {
            if (_button == null)
            {
                return;
            }

            _button.onClick.RemoveAllListeners();
            _button.interactable = isInteractable;
            if (onClick != null && isInteractable)
            {
                _button.onClick.AddListener(() => onClick.Invoke());
            }
        }

        private void SetInteractable(bool isInteractable)
        {
            if (_button != null)
            {
                _button.interactable = isInteractable;
            }
        }

        private void SetDisabledState(bool isDisabled)
        {
            if (_disabledOverlay != null)
            {
                _disabledOverlay.SetActive(isDisabled);
            }
        }

        private static void SetText(TFTextUGUI text, string value)
        {
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }

        private static void SetActive(Component component, bool isActive)
        {
            if (component != null)
            {
                component.gameObject.SetActive(isActive);
            }
        }

        private void SetBadgeVisible(TFTextUGUI text, bool isActive)
        {
            if (text == null)
            {
                return;
            }

            Transform badgeRoot = text.transform.parent;
            if (badgeRoot != null && badgeRoot != transform)
            {
                badgeRoot.gameObject.SetActive(isActive);
                return;
            }

            text.gameObject.SetActive(isActive);
        }
    }
}
