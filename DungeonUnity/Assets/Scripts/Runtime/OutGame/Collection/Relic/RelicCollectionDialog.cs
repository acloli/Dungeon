using Dungeon.Runtime.Common.UI.List;
using Dungeon.Runtime.OutGame.Collection.Common;
using Dungeon.Runtime.OutGame.Collection.Relic.UI;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Collection.Relic
{
    /// <summary>
    /// 遺物図鑑の表示と入力参照
    /// </summary>
    public sealed class RelicCollectionDialog : CommonBookDialog
    {
        [SerializeField] private RecyclingGridView _grid;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private TFTextUGUI _discoveryCountText;
        [SerializeField] private RelicCollectionItem _itemTemplate;

        public RecyclingGridView Grid => _grid;
        public ScrollRect ScrollRect => _scrollRect;
        public TFTextUGUI DiscoveryCountText => _discoveryCountText;
        public RelicCollectionItem ItemTemplate => _itemTemplate;
    }
}
