using Game.MasterData.Generated;
using TFramework.Localization;
using TFramework.MasterData;

namespace Dungeon.Runtime.OutGame.Main
{
    /// <summary>
    /// MainSceneRunProfile概要取得クラス
    /// </summary>
    public sealed class MainRunProfileService : IMainRunProfileService
    {
        private readonly IMasterDataService _masterDataService;
        private readonly ILocalizationService _localizationService;

        public MainRunProfileService(IMasterDataService masterDataService, ILocalizationService localizationService = null)
        {
            _masterDataService = masterDataService;
            _localizationService = localizationService;
        }

        /// <summary>
        /// RunProfile概要取得
        /// </summary>
        public MainRunProfileViewModel BuildRunProfile(int runProfileId)
        {
            if (_masterDataService == null)
            {
                return null;
            }

            RunProfileMaster profile = _masterDataService.Get<RunProfileMaster, int>(runProfileId);
            if (profile == null)
            {
                return null;
            }

            return new MainRunProfileViewModel(
                profile.Id,
                profile.Key,
                ResolveDisplayName(profile),
                profile.LocalizationKey,
                profile.CharacterArchetype.ToString(),
                profile.PlayerMaxHp,
                profile.StartingGold);
        }

        /// <summary>
        /// RunProfile表示名解決
        /// </summary>
        private string ResolveDisplayName(RunProfileMaster profile)
        {
            if (_localizationService != null &&
                !string.IsNullOrEmpty(profile.LocalizationKey) &&
                _localizationService.HasKey(profile.LocalizationKey))
            {
                return _localizationService.Get(profile.LocalizationKey);
            }

            if (!string.IsNullOrEmpty(profile.Name))
            {
                return profile.Name;
            }

            return profile.Key;
        }
    }
}
