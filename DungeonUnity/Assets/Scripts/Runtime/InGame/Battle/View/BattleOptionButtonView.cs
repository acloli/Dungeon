using System;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    [RequireComponent(typeof(Button))]
    public sealed class BattleOptionButtonView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TFTextUGUI _labelText;
        [SerializeField] private Image _icon;

        public void Configure(string label, Action onClick)
        {
            SetLabel(label);
            SetOnClick(onClick);
        }

        public void SetLabel(string label)
        {
            if (_labelText != null)
            {
                _labelText.text = label;
            }
        }

        public void SetIcon(Sprite icon)
        {
            if (_icon != null)
            {
                _icon.sprite = icon;
                _icon.gameObject.SetActive(icon != null);
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (_button != null)
            {
                _button.interactable = interactable;
            }
        }

        public void Clear()
        {
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
            }
        }

        private void SetOnClick(Action onClick)
        {
            if (_button == null)
            {
                return;
            }

            _button.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                _button.onClick.AddListener(() => onClick());
            }
        }
    }
}
