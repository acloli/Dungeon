using TFramework.UI;
using UnityEngine;

namespace Dungeon.Runtime.OutGame.Main.UI
{
    /// <summary>
    /// 冒険再開ダイアログの表示と入力参照
    /// </summary>
    public sealed class ResumeDialog : UIDialogBase
    {
        [SerializeField] private UIButton _continueButton;
        [SerializeField] private UIButton _newAdventureButton;
        [SerializeField] private UIButton _closeButton;

        public UIButton ContinueButton => _continueButton;
        public UIButton NewAdventureButton => _newAdventureButton;
        public UIButton CloseButton => _closeButton;
    }
}
