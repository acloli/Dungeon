using Dungeon.Runtime.OutGame.Collection.Common;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Collection.Character
{
    /// <summary>
    /// キャラクター図鑑の表示と入力参照
    /// </summary>
    public class CharacterCollectionDialog : CommonBookDialog
    {
        [SerializeField] private GameObject _unlockedProfile;
        [SerializeField] private GameObject _lockedProfile;
        [SerializeField] private Image _portraitImage;
        [SerializeField] private TFTextUGUI _nameText;
        [SerializeField] private TFTextUGUI _descriptionText;
        [SerializeField] private TFTextUGUI _combatText;
        [SerializeField] private UIButton _previousButton;
        [SerializeField] private UIButton _nextButton;
        [SerializeField] private UIButton _initialDeckButton;

        public GameObject UnlockedProfile => _unlockedProfile;
        public GameObject LockedProfile => _lockedProfile;
        public Image PortraitImage => _portraitImage;
        public TFTextUGUI NameText => _nameText;
        public TFTextUGUI DescriptionText => _descriptionText;
        public TFTextUGUI CombatText => _combatText;
        public UIButton PreviousButton => _previousButton;
        public UIButton NextButton => _nextButton;
        public UIButton InitialDeckButton => _initialDeckButton;

        /// <summary>
        /// 解放状態に対応する表示を切り替える
        /// </summary>
        public void SetLocked(bool isLocked)
        {
            _unlockedProfile.SetActive(!isLocked);
            _lockedProfile.SetActive(isLocked);
        }
    }
}
