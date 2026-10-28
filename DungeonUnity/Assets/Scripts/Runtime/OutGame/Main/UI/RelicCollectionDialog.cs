using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Main.UI
{
    /// <summary>
    /// 遺物図鑑の表示と入力参照
    /// </summary>
    public sealed class RelicCollectionDialog : CommonBookDialog
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private TFTextUGUI _discoveryCountText;
        [SerializeField] private RelicCollectionItem _itemTemplate;

        public ScrollRect ScrollRect => _scrollRect;
        public TFTextUGUI DiscoveryCountText => _discoveryCountText;
        public RelicCollectionItem ItemTemplate => _itemTemplate;
    }
}

