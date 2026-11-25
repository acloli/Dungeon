using System;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.Common.UI.MultiIcon
{
    /// <summary>
    /// カード表示用の汎用アイコンViewクラス
    /// </summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Dungeon.Runtime.InGame.Battle.View", "Dungeon.Runtime.InGame.Battle", "BattleCardIconView")]
    public sealed class CardIconView : MultiIconView
    {
        [SerializeField] private GameObject _cardFace;
        [SerializeField] private GameObject _undiscoveredState;
        [SerializeField] private TFTextUGUI _typeText;
        [SerializeField] private Image _nameplateImage;

        public override void Bind(MultiIconViewModel icon, Action onClick)
        {
            base.Bind(icon, onClick);
            SetFooterLabel(icon?.FooterLabel ?? string.Empty, icon?.ShowFooterLabel == true);
        }

        /// <summary>
        /// 共通カードの図柄と発見状態を反映
        /// </summary>
        public void SetPresentation(Sprite artwork, string category, bool isDiscovered)
        {
            SetArtSprite(artwork);
            if (_cardFace != null) _cardFace.SetActive(isDiscovered);
            if (_undiscoveredState != null) _undiscoveredState.SetActive(!isDiscovered);
            if (_typeText != null)
            {
                _typeText.text = category ?? string.Empty;
                _typeText.gameObject.SetActive(!string.IsNullOrEmpty(category));
            }

            if (_nameplateImage != null)
                _nameplateImage.color = category == "スキル"
                    ? new Color(0.28f, 0.46f, 0.49f)
                    : new Color(0.36f, 0.33f, 0.5f);
        }
    }
}
