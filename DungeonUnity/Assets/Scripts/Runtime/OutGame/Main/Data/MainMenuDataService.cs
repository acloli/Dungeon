using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Save.Services;

namespace Dungeon.Runtime.OutGame.Main
{
    /// <summary>
    /// 現在の保存スロットとMasterDataからメニューを読込
    /// </summary>
    public sealed class MainMenuDataService : IMainMenuDataSource
    {
        private readonly MenuMasterDataFacade _facade;
        private readonly ICollectionProgressService _progress;
        private readonly IRunSaveService _run;

        public MainMenuDataService(MenuMasterDataFacade facade, ICollectionProgressService progress,
            IRunSaveService run)
        {
            _facade = facade;
            _progress = progress;
            _run = run;
        }

        public async UniTask<MainMenuData> LoadAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var run = await _run.LoadCurrentRunAsync(ct);
            if (run != null)
            {
                await _progress.RecordAsync(run.DeckCardIds, run.OwnedRelicIds, null, ct);
            }

            var progress = await _progress.LoadAsync(ct);
            var data = _facade.Build(progress);
            if (progress.LatestRun != null)
            {
                var result = progress.LatestRun;
                data.ResultHeadline = result.Victory ? "冒険クリア" : "冒険終了";
                data.ReachedFloor = result.ReachedFloor;
                data.EnemyCount = result.EnemyCount;
                data.EliteCount = result.EliteCount;
                data.BossCount = result.BossCount;
            }

            data.HasResumableAdventure = run != null;
            if (run != null && progress.LatestRun == null)
            {
                data.ResultHeadline = "進行中の冒険";
                data.ReachedFloor = run.MapRouteNodeIndices?.Count ?? 0;
            }

            return data;
        }
    }
}
