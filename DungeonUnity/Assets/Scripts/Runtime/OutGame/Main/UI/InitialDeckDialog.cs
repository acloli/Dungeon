using Dungeon.Runtime.InGame.Battle.View;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Main.UI
{
    /// <summary>
    /// 初期デッキ一覧の表示と入力参照
    /// </summary>
    public sealed class InitialDeckDialog : UIDialogBase
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private BattleCardIconView _cardTemplate;
        [SerializeField] private TFTextUGUI _summaryText;
        [SerializeField] private UIButton _closeButton;

        public ScrollRect ScrollRect => _scrollRect;
        public BattleCardIconView CardTemplate => _cardTemplate;
        public TFTextUGUI SummaryText => _summaryText;
        public UIButton CloseButton => _closeButton;
    }
}
