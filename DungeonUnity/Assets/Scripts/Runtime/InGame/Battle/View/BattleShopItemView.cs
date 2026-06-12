using System;
using Dungeon.Runtime.InGame.Battle.Model;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    public sealed class BattleShopItemView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private BattleMultiIconView _iconView;
        [SerializeField] private TFTextUGUI _priceText;

        public void Bind(BattleShopItemViewModel itemViewModel, Action<int> onClick)
        {
            _iconView?.Bind(itemViewModel.Icon, null);

            if (itemViewModel.IsSoldOut)
            {
                SetPriceText(BattleSceneConstants.SoldOutLabel);
                SetInteractable(false);
            }
            else
            {
                SetPriceText(itemViewModel.Price.ToString());
                SetInteractable(true);
            }

            if (_button == null)
            {
                return;
            }

            _button.onClick.RemoveAllListeners();
            _button.interactable = !itemViewModel.IsSoldOut;
            _button.onClick.AddListener(() => onClick?.Invoke(itemViewModel.SlotIndex));
        }

        public void Clear()
        {
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
            }

            _iconView?.Clear();
        }

        private void SetPriceText(string label)
        {
            if (_priceText != null)
            {
                _priceText.text = label;
            }
        }

        private void SetInteractable(bool interactable)
        {
            if (_button != null)
            {
                _button.interactable = interactable;
            }
        }
    }
}
