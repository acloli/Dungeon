using TFramework.UI;
using UnityEngine;

namespace Dungeon.Runtime.OutGame.Main.UI
{
    /// <summary>
    /// 共通確認ダイアログの表示と入力参照
    /// </summary>
    public sealed class CommonConfirmDialog : UIDialogBase
    {
        [SerializeField] private TFTextUGUI _bodyText;
        [SerializeField] private UIButton _cancelButton;
        [SerializeField] private UIButton _confirmButton;

        public TFTextUGUI BodyText => _bodyText;
        public UIButton CancelButton => _cancelButton;
        public UIButton ConfirmButton => _confirmButton;
    }
}
