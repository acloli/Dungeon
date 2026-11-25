using Dungeon.Runtime.Common.UI.List;
using Dungeon.Runtime.Common.UI.MultiIcon;
using Dungeon.Runtime.OutGame.Collection.Common;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Collection.Card
{
    /// <summary>
    /// カード図鑑の表示と入力参照
    /// </summary>
    public sealed class CardCollectionDialog : CommonBookDialog
    {
        [SerializeField] private RecyclingGridView _grid;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private TFTextUGUI _discoveryCountText;
        [SerializeField] private CardIconView _itemTemplate;

        public RecyclingGridView Grid => _grid;
        public ScrollRect ScrollRect => _scrollRect;
        public TFTextUGUI DiscoveryCountText => _discoveryCountText;
        public CardIconView ItemTemplate => _itemTemplate;
    }
}
