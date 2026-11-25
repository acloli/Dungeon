using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Save.Model;

namespace Dungeon.Runtime.InGame.Save.Services
{
    public interface ICollectionProgressService
    {
        UniTask<CollectionProgressData> LoadAsync(CancellationToken ct);
        UniTask RecordResultAsync(RunHistoryData result, CancellationToken ct);
        UniTask RecordAsync(IEnumerable<int> cardIds, IEnumerable<int> relicIds, IEnumerable<int> characterIds, CancellationToken ct);
    }
}
