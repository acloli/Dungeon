using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Save.Model;
using TFramework.SaveData;

namespace Dungeon.Runtime.InGame.Save.Services
{
    /// <summary>
    /// 現在のローカル保存スロットへ発見と解放を累積保存
    /// </summary>
    public sealed class CollectionProgressService : ICollectionProgressService
    {
        private const string SaveKey = "collection_progress";
        private static readonly SemaphoreSlim Gate = new SemaphoreSlim(1, 1);
        private readonly ISaveDataService _save;
        public CollectionProgressService(ISaveDataService save) => _save = save;

        public async UniTask<CollectionProgressData> LoadAsync(CancellationToken ct)
        {
            await Gate.WaitAsync(ct);
            try
            {
                return await _save.LoadAsync(SaveKey, new CollectionProgressData(), ct) ?? new CollectionProgressData();
            }
            finally
            {
                Gate.Release();
            }
        }

        public async UniTask RecordAsync(IEnumerable<int> cardIds, IEnumerable<int> relicIds,
            IEnumerable<int> characterIds, CancellationToken ct)
        {
            await Gate.WaitAsync(ct);
            try
            {
                var data = await _save.LoadAsync(SaveKey, new CollectionProgressData(), ct) ??
                           new CollectionProgressData();
                bool changed = false;
                data.CardIds = Merge(data.CardIds, cardIds, ref changed);
                data.RelicIds = Merge(data.RelicIds, relicIds, ref changed);
                data.CharacterIds = Merge(data.CharacterIds, characterIds, ref changed);
                if (changed) await _save.SaveAsync(SaveKey, data, ct);
            }
            finally
            {
                Gate.Release();
            }
        }

        public async UniTask RecordResultAsync(RunHistoryData result, CancellationToken ct)
        {
            await Gate.WaitAsync(ct);
            try
            {
                var data = await _save.LoadAsync(SaveKey, new CollectionProgressData(), ct) ??
                           new CollectionProgressData();
                bool changed = false;
                data.CardIds = Merge(data.CardIds, result.CardIds, ref changed);
                data.RelicIds = Merge(data.RelicIds, result.RelicIds, ref changed);
                data.LatestRun = result;
                await _save.SaveAsync(SaveKey, data, ct);
            }
            finally
            {
                Gate.Release();
            }
        }

        private static List<int> Merge(List<int> current, IEnumerable<int> added, ref bool changed)
        {
            var result = (current ?? new List<int>()).Concat(added ?? Enumerable.Empty<int>()).Where(id => id > 0)
                .Distinct().OrderBy(id => id).ToList();
            if (current == null || !current.SequenceEqual(result)) changed = true;
            return result;
        }
    }
}
