using Dungeon.Runtime.Common.UI.MultiIcon;
using Dungeon.Runtime.Common.UI.Dialog;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.RelicDetail
{
    /// <summary>
    /// 遺物詳細の表示と入力参照
    /// </summary>
    public sealed class RelicDetailDialog : BoundDialogBase
    {
        [SerializeField] private TFTextUGUI _titleText;
        [SerializeField] private RectTransform _effectRoot;
        [SerializeField] private CollectionItemIcon _iconView;
        [SerializeField] private UIButton _closeButton;
        [SerializeField] private TFTextUGUI _conditionText;
        [SerializeField] private TFTextUGUI _effectText;

        public TFTextUGUI TitleText => _titleText;
        public RectTransform EffectRoot => _effectRoot;
        public CollectionItemIcon IconView => _iconView;
        public UIButton CloseButton => _closeButton;
        public TFTextUGUI ConditionText => _conditionText;
        public TFTextUGUI EffectText => _effectText;
    }
}
