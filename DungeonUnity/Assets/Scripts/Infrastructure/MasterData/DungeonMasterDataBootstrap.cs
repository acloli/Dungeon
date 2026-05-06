using System;
using TFramework.MasterData;

namespace Dungeon.Infrastructure.MasterData
{
    /// <summary>
    /// Gate A bootstrap boundary for Dungeon-specific MasterData wiring.
    /// This keeps project-owned paths and the TFramework service boundary explicit
    /// before concrete MasterData containers are introduced.
    /// </summary>
    public sealed class DungeonMasterDataBootstrap
    {
        public const string RuntimeScriptRoot = "Assets/Scripts";
        public const string MasterDataAssetRoot = "Assets/ScriptableObjects/MasterData";

        private readonly IMasterDataService _masterDataService;

        public DungeonMasterDataBootstrap(IMasterDataService masterDataService)
        {
            _masterDataService = masterDataService ?? throw new ArgumentNullException(nameof(masterDataService));
        }

        public bool HasMasterDataService => _masterDataService != null;

        public string[] RequiredRoots => new[]
        {
            RuntimeScriptRoot,
            MasterDataAssetRoot,
        };
    }
}
