using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Save.Model;
using Dungeon.Runtime.InGame.Save.Services;
using NUnit.Framework;
using TFramework.SaveData;
using UnityEngine;

namespace Dungeon.Tests.EditMode.Save
{
    public sealed class CollectionProgressServiceTests
    {
        [Test]
        public void Discovery_IsMergedAndSurvivesDeletingTheRun()
        {
            var save = new MemorySave();
            var service = new CollectionProgressService(save);
            service.RecordAsync(new[] { 1001, 1001, -1 }, new[] { 2001 }, new[] { 1 }, CancellationToken.None).GetAwaiter().GetResult();
            service.RecordAsync(new[] { 1002 }, null, new[] { 2 }, CancellationToken.None).GetAwaiter().GetResult();
            new RunSaveService(save).DeleteSavedRun();
            var result = new CollectionProgressService(save).LoadAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(result.CardIds, Is.EqualTo(new[] { 1001, 1002 }));
            Assert.That(result.RelicIds, Is.EqualTo(new[] { 2001 }));
            Assert.That(result.CharacterIds, Is.EqualTo(new[] { 1, 2 }));
            int writes = save.Writes;
            service.RecordAsync(new[] { 1002 }, null, null, CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(save.Writes, Is.EqualTo(writes));
        }

        [Test]
        public void Checkpoint_RecordsAcquiredItems_AndResultsUseTheSameSlot()
        {
            var save = new MemorySave(); var progress = new CollectionProgressService(save);
            var run = new RunSaveData { RunProfileId = 1, PlayerMaxHp = 80, PlayerHp = 40, MasterSeed = 1,
                DeckCardIds = new List<int> { 1005 }, OwnedRelicIds = new List<int> { 2005 } };
            new RunSaveService(save, progress).SaveCurrentRunAsync(run).GetAwaiter().GetResult();
            progress.RecordResultAsync(new RunHistoryData { Victory = true, BossCount = 1, CardIds = new List<int> { 1010 } }, CancellationToken.None).GetAwaiter().GetResult();
            var result = progress.LoadAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(result.CardIds, Is.EqualTo(new[] { 1005, 1010 }));
            Assert.That(result.LatestRun.Victory, Is.True);
            Assert.That(result.LatestRun.BossCount, Is.EqualTo(1));
            save.SetSlot(1);
            Assert.That(progress.LoadAsync(CancellationToken.None).GetAwaiter().GetResult().CardIds, Is.Empty);
            save.SetSlot(0);
            Assert.That(progress.LoadAsync(CancellationToken.None).GetAwaiter().GetResult().RelicIds, Does.Contain(2005));
        }

        [Test]
        public void CancelledDiscovery_DoesNotWrite()
        {
            var save = new MemorySave(); var service = new CollectionProgressService(save);
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            Assert.Throws<System.OperationCanceledException>(() => service.RecordAsync(new[] { 1 }, null, null, cancellation.Token).GetAwaiter().GetResult());
            Assert.That(save.Writes, Is.Zero);
        }

        private sealed class MemorySave : ISaveDataService
        {
            private readonly Dictionary<string, string> _data = new Dictionary<string, string>();
            public int CurrentSlot { get; private set; }
            public int Writes;
            private string Key(string key) => CurrentSlot + ":" + key;
            public void SetSlot(int index) => CurrentSlot = index;
            public UniTask SaveAsync<T>(string key, T data, CancellationToken token = default)
            { token.ThrowIfCancellationRequested(); _data[Key(key)] = JsonUtility.ToJson(data); Writes++; return UniTask.CompletedTask; }
            public UniTask<T> LoadAsync<T>(string key, T defaultValue = default, CancellationToken token = default)
            { token.ThrowIfCancellationRequested(); return UniTask.FromResult(_data.TryGetValue(Key(key), out string json) ? JsonUtility.FromJson<T>(json) : defaultValue); }
            public bool Exists(string key) => _data.ContainsKey(Key(key));
            public void Delete(string key) => _data.Remove(Key(key));
            public void DeleteAll() => _data.Clear();
        }
    }
}
