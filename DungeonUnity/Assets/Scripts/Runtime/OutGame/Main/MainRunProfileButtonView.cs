using System;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Main
{
    /// <summary>
    /// MainSceneRunProfile選択ボタンView
    /// </summary>
    public sealed class MainRunProfileButtonView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TFTextUGUI _labelText;

        private static readonly Color SelectedColor = new Color(0.88f, 0.42f, 0.22f, 1f);
        private static readonly Color NormalColor = new Color(0.2f, 0.35f, 0.55f, 1f);

        /// <summary>
        /// 表示設定
        /// </summary>
        public void Configure(string label, bool isSelected, Action onClicked)
        {
            if (_labelText != null)
            {
                _labelText.text = label;
            }

            ApplySelectedState(isSelected);

            if (_button == null)
            {
                return;
            }

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClicked?.Invoke());
        }

        /// <summary>
        /// 選択状態反映
        /// </summary>
        public void ApplySelectedState(bool isSelected)
        {
            if (_button == null)
            {
                return;
            }

            Image image = _button.GetComponent<Image>();
            if (image != null)
            {
                image.color = isSelected ? SelectedColor : NormalColor;
            }
        }

        /// <summary>
        /// 参照解除
        /// </summary>
        public void Clear()
        {
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
            }
        }
    }
}
