using Dungeon.Runtime.InGame.Battle.View;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Main.UI
{
    /// <summary>
    /// カード図鑑の表示と入力参照
    /// </summary>
    public sealed class CardCollectionDialog : CommonBookDialog
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private TFTextUGUI _discoveryCountText;
        [SerializeField] private BattleCardIconView _itemTemplate;

        public ScrollRect ScrollRect => _scrollRect;
        public TFTextUGUI DiscoveryCountText => _discoveryCountText;
        public BattleCardIconView ItemTemplate => _itemTemplate;
    }
}
