using TFramework.UI;
using UnityEngine;

namespace Dungeon.Runtime.OutGame.Main.UI
{
    /// <summary>
    /// 名称をアイコンの外に表示する遺物図鑑項目
    /// </summary>
    public sealed class RelicCollectionItem : CollectionItemIcon
    {
        [SerializeField] private TFTextUGUI _nameText;

        public TFTextUGUI NameText => _nameText;
    }
}
