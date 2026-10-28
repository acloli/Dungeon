using Dungeon.Runtime.InGame.Battle.View;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Main.UI
{
    /// <summary>
    /// カード詳細の表示と入力参照
    /// </summary>
    public sealed class CardDetailDialog : UIDialogBase
    {
        [SerializeField] private BattleCardIconView _cardIcon;
        [SerializeField] private Toggle _normalToggle;
        [SerializeField] private Toggle _upgradedToggle;
        [SerializeField] private RectTransform _keywordRoot;
        [SerializeField] private UIButton _closeButton;

        public BattleCardIconView CardIcon => _cardIcon;
        public Toggle NormalToggle => _normalToggle;
        public Toggle UpgradedToggle => _upgradedToggle;
        public RectTransform KeywordRoot => _keywordRoot;
        public UIButton CloseButton => _closeButton;
    }
}

