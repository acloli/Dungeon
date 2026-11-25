using Dungeon.Runtime.Common.UI.Dialog;
using Dungeon.Runtime.OutGame.Settings.UI;
using TFramework.UI;
using UnityEngine;

namespace Dungeon.Runtime.OutGame.Settings
{
    /// <summary>
    /// 設定シートの表示と入力参照
    /// </summary>
    public sealed class SettingsDialog : BoundDialogBase
    {
        [SerializeField] private SettingsAudioToggle _bgmToggle;
        [SerializeField] private SettingsAudioToggle _soundEffectToggle;
        [SerializeField] private UIButton _closeButton;
        [SerializeField] private UIButton _languageButton;
        [SerializeField] private UIButton _accountButton;
        [SerializeField] private UIButton _clearCacheButton;
        [SerializeField] private UIButton _supportButton;
        [SerializeField] private UIButton _creditsButton;
        [SerializeField] private UIButton _logoutButton;
        [SerializeField] private UIButton _termsButton;
        [SerializeField] private UIButton _privacyButton;
        [SerializeField] private TFTextUGUI _versionText;
        [SerializeField] private TFTextUGUI _gameIdText;

        public SettingsAudioToggle BgmToggle => _bgmToggle;
        public SettingsAudioToggle SoundEffectToggle => _soundEffectToggle;
        public UIButton CloseButton => _closeButton;
        public UIButton LanguageButton => _languageButton;
        public UIButton AccountButton => _accountButton;
        public UIButton ClearCacheButton => _clearCacheButton;
        public UIButton SupportButton => _supportButton;
        public UIButton CreditsButton => _creditsButton;
        public UIButton LogoutButton => _logoutButton;
        public UIButton TermsButton => _termsButton;
        public UIButton PrivacyButton => _privacyButton;
        public TFTextUGUI VersionText => _versionText;
        public TFTextUGUI GameIdText => _gameIdText;
    }
}
