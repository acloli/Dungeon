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
        [SerializeField] private TFTextUGUI _nameText;
        [SerializeField] private TFTextUGUI _priceText;

        public void Bind(BattleShopItemViewModel itemViewModel, Action<int> onClick)
        {
            if (_nameText != null)
            {
                _nameText.text = itemViewModel.DisplayName;
            }

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
            _button.onClick.AddListener(() => onClick?.Invoke(itemViewModel.SlotIndex));
        }

        public void Clear()
        {
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
            }
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
