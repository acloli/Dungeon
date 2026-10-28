using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Main.UI
{
    /// <summary>
    /// 冒険結果ページの表示と入力参照
    /// </summary>
    public sealed class AdventureResultPage : UIPageBase
    {
        [SerializeField] private TFTextUGUI _headlineText;
        [SerializeField] private TFTextUGUI _floorText;
        [SerializeField] private TFTextUGUI _normalEnemyCountText;
        [SerializeField] private TFTextUGUI _eliteEnemyCountText;
        [SerializeField] private TFTextUGUI _bossCountText;
        [SerializeField] private UIButton _inventoryButton;
        [SerializeField] private UIButton _homeButton;

        public TFTextUGUI HeadlineText => _headlineText;
        public TFTextUGUI FloorText => _floorText;
        public TFTextUGUI NormalEnemyCountText => _normalEnemyCountText;
        public TFTextUGUI EliteEnemyCountText => _eliteEnemyCountText;
        public TFTextUGUI BossCountText => _bossCountText;
        public UIButton InventoryButton => _inventoryButton;
        public UIButton HomeButton => _homeButton;
    }
}
