using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Main.UI
{
    /// <summary>
    /// 出発キャラクター選択ページの表示と入力参照
    /// </summary>
    public sealed class CharacterSelectPage : UIPageBase
    {
        [SerializeField] private Image _characterImage;
        [SerializeField] private TFTextUGUI _nameText;
        [SerializeField] private TFTextUGUI _descriptionText;
        [SerializeField] private TFTextUGUI _healthText;
        [SerializeField] private TFTextUGUI _goldText;
        [SerializeField] private UIButton _backButton;
        [SerializeField] private UIButton _initialDeckButton;
        [SerializeField] private UIButton _departButton;
        [SerializeField] private ToggleGroup _characterToggleGroup;

        public Image CharacterImage => _characterImage;
        public TFTextUGUI NameText => _nameText;
        public TFTextUGUI DescriptionText => _descriptionText;
        public TFTextUGUI HealthText => _healthText;
        public TFTextUGUI GoldText => _goldText;
        public UIButton BackButton => _backButton;
        public UIButton InitialDeckButton => _initialDeckButton;
        public UIButton DepartButton => _departButton;
        public ToggleGroup CharacterToggleGroup => _characterToggleGroup;
    }
}

