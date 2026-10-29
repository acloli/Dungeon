using TFramework.UI;
using UnityEngine;

namespace Dungeon.Runtime.Common.UI.Dialog
{
    /// <summary>
    /// 共通確認ダイアログの表示と入力参照
    /// </summary>
    public sealed class CommonConfirmDialog : BoundDialogBase
    {
        [SerializeField] private TFTextUGUI _bodyText;
        [SerializeField] private UIButton _cancelButton;
        [SerializeField] private UIButton _confirmButton;
        private Vector2 _confirmPosition;
        private bool _positionCaptured;

        public TFTextUGUI BodyText => _bodyText;
        public UIButton CancelButton => _cancelButton;
        public UIButton ConfirmButton => _confirmButton;

        /// <summary>
        /// 操作数に合わせて文言とボタン位置を反映する
        /// </summary>
        public void ApplyText(string body, string cancel, string confirm)
        {
            var rect = (RectTransform)_confirmButton.transform;
            if (!_positionCaptured)
            {
                _confirmPosition = rect.anchoredPosition;
                _positionCaptured = true;
            }

            _bodyText.text = body;
            _cancelButton.gameObject.SetActive(cancel != null);
            _cancelButton.GetComponentInChildren<TFTextUGUI>(true).text = cancel ?? string.Empty;
            _confirmButton.GetComponentInChildren<TFTextUGUI>(true).text = confirm;
            float x = cancel == null
                ? ((RectTransform)rect.parent).rect.width * (0.5f - rect.anchorMin.x) + rect.rect.width * (rect.pivot.x - 0.5f)
                : _confirmPosition.x;
            rect.anchoredPosition = new Vector2(x, _confirmPosition.y);
        }
    }
}
