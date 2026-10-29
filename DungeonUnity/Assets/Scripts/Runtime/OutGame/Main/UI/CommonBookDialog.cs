using Dungeon.Runtime.Common.UI.Dialog;
using TFramework.UI;
using UnityEngine;

namespace Dungeon.Runtime.OutGame.Main.UI
{
    /// <summary>
    /// 図鑑共通の本型ダイアログと入力参照
    /// </summary>
    public class CommonBookDialog : BoundDialogBase
    {
        [SerializeField] private TFTextUGUI _titleText;
        [SerializeField] private UIButton _closeButton;
        [SerializeField] private RectTransform _contentRoot;

        public TFTextUGUI TitleText => _titleText;
        public UIButton CloseButton => _closeButton;
        public RectTransform ContentRoot => _contentRoot;
    }
}
