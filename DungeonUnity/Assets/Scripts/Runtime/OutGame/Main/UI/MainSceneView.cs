using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Main.UI
{
    /// <summary>
    /// MainSceneに配置されたロビーの表示と入力参照
    /// </summary>
    public sealed class MainSceneView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _lobbyGroup;
        [SerializeField] private Button _adventureButton;
        [SerializeField] private UIButton _settingsButton;
        [SerializeField] private UIButton _cardCollectionButton;
        [SerializeField] private UIButton _relicCollectionButton;
        [SerializeField] private UIButton _characterCollectionButton;
        [SerializeField] private UIButton _profileButton;
        [SerializeField] private Toggle _homeToggle;
        [SerializeField] private Toggle _shopToggle;
        [SerializeField] private Toggle _historyToggle;

        public CanvasGroup LobbyGroup => _lobbyGroup;
        public Button AdventureButton => _adventureButton;
        public UIButton SettingsButton => _settingsButton;
        public UIButton CardCollectionButton => _cardCollectionButton;
        public UIButton RelicCollectionButton => _relicCollectionButton;
        public UIButton CharacterCollectionButton => _characterCollectionButton;
        public UIButton ProfileButton => _profileButton;
        public Toggle HomeToggle => _homeToggle;
        public Toggle ShopToggle => _shopToggle;
        public Toggle HistoryToggle => _historyToggle;

        public void SetVisible(bool visible)
        {
            _lobbyGroup.alpha = visible ? 1 : 0;
            _lobbyGroup.interactable = visible;
            _lobbyGroup.blocksRaycasts = visible;
        }

        public void SelectHome()
        {
            _shopToggle.SetIsOnWithoutNotify(false);
            _historyToggle.SetIsOnWithoutNotify(false);
            _homeToggle.SetIsOnWithoutNotify(true);
        }
    }
}
