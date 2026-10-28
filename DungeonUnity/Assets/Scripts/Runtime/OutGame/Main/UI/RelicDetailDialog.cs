using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Main.UI
{
    /// <summary>
    /// 遺物詳細の表示と入力参照
    /// </summary>
    public sealed class RelicDetailDialog : UIDialogBase
    {
        [SerializeField] private TFTextUGUI _titleText;
        [SerializeField] private RectTransform _effectRoot;
        [SerializeField] private CollectionItemIcon _iconView;
        [SerializeField] private UIButton _closeButton;

        public TFTextUGUI TitleText => _titleText;
        public RectTransform EffectRoot => _effectRoot;
        public CollectionItemIcon IconView => _iconView;
        public UIButton CloseButton => _closeButton;
    }
}
