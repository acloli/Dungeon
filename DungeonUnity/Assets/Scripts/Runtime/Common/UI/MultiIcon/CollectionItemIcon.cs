using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.Common.UI.MultiIcon
{
    /// <summary>
    /// 図鑑用アイコンの共通表示参照
    /// </summary>
    public class CollectionItemIcon : MonoBehaviour
    {
        [SerializeField] private UIButton _button;
        [SerializeField] private Image _artImage;
        [SerializeField] private Image _rarityFrame;

        public UIButton Button => _button;
        public Image ArtImage => _artImage;
        public Image RarityFrame => _rarityFrame;
    }
}
