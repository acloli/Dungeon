using Dungeon.Runtime.Common.UI.Dialog;
using Dungeon.Runtime.Common.UI.MultiIcon;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.InitialDeck
{
    /// <summary>
    /// 初期デッキ一覧の表示と入力参照
    /// </summary>
    public sealed class InitialDeckDialog : BoundDialogBase
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private CardIconView _cardTemplate;
        [SerializeField] private TFTextUGUI _summaryText;
        [SerializeField] private UIButton _closeButton;

        public ScrollRect ScrollRect => _scrollRect;
        public CardIconView CardTemplate => _cardTemplate;
        public TFTextUGUI SummaryText => _summaryText;
        public UIButton CloseButton => _closeButton;
    }
}
