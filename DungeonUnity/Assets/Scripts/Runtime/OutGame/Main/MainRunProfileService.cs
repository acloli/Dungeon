using System.Collections.Generic;
using System.Linq;
using Game.MasterData.Generated;
using TFramework.MasterData;

namespace Dungeon.Runtime.OutGame.Main
{
    /// <summary>
    /// MainSceneRunProfile取得クラス
    /// </summary>
    public sealed class MainRunProfileService : IMainRunProfileService
    {
        private readonly IMasterDataService _masterDataService;

        public MainRunProfileService(IMasterDataService masterDataService)
        {
            _masterDataService = masterDataService;
        }

        /// <summary>
        /// RunProfile一覧取得
        /// </summary>
        public IReadOnlyList<MainRunProfileViewModel> BuildRunProfiles()
        {
            if (_masterDataService == null)
            {
                return new List<MainRunProfileViewModel>();
            }

            return _masterDataService.GetAll<RunProfileMaster>()
                .OrderBy(profile => profile.Id)
                .Select(profile => new MainRunProfileViewModel(
                    profile.Id,
                    profile.Key,
                    profile.CharacterArchetype.ToString(),
                    profile.PlayerMaxHp,
                    profile.StartingGold))
                .ToList();
        }
    }
}
