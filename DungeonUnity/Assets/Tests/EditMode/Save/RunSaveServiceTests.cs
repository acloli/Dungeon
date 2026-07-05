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
    /// <summary>
    /// RunSaveDataおよびRunSaveServiceのEditorモードテストクラス
    /// </summary>
    [TestFixture]
    public sealed class RunSaveServiceTests
    {
        [Test]
        public void RunSaveData_CanBeSerializedToJson()
        {
            RunSaveData data = new RunSaveData
            {
                RunProfileId = 5501,
                PlayerMaxHp = 80,
                PlayerHp = 45,
                PlayerEnergy = 3,
                Gold = 150,
                CurrentNodeIndex = 5,
                CurrentPage = 1, // Map
                MasterSeed = 12345,
                MapSeed = 67890,
                MapLayoutVersion = 1,
                RandomCounter = 7,
                DeckCardIds = new List<int> { 101, 102, 103 }
            };

            string json = JsonUtility.ToJson(data);

            Assert.That(json, Does.Contain("\"RunProfileId\":5501"));
            Assert.That(json, Does.Contain("\"PlayerMaxHp\":80"));
            Assert.That(json, Does.Contain("\"PlayerHp\":45"));
            Assert.That(json, Does.Contain("\"PlayerEnergy\":3"));
            Assert.That(json, Does.Contain("\"Gold\":150"));
            Assert.That(json, Does.Contain("\"CurrentNodeIndex\":5"));
            Assert.That(json, Does.Contain("\"CurrentPage\":1"));
            Assert.That(json, Does.Contain("\"MasterSeed\":12345"));
            Assert.That(json, Does.Contain("\"MapSeed\":67890"));
            Assert.That(json, Does.Contain("\"MapLayoutVersion\":1"));
            Assert.That(json, Does.Contain("\"RandomCounter\":7"));
            Assert.That(json, Does.Contain("\"DeckCardIds\":[101,102,103]"));
        }

        [Test]
        public void RunSaveData_CanBeDeserializedFromJson()
        {
            string json = "{\"RunProfileId\":5501,\"PlayerMaxHp\":80,\"PlayerHp\":45,\"PlayerEnergy\":3,\"Gold\":150,\"CurrentNodeIndex\":5,\"CurrentPage\":0,\"MasterSeed\":12345,\"MapSeed\":67890,\"MapLayoutVersion\":1,\"RandomCounter\":7,\"DeckCardIds\":[101,102,103]}";

            RunSaveData data = JsonUtility.FromJson<RunSaveData>(json);

            Assert.That(data.RunProfileId, Is.EqualTo(5501));
            Assert.That(data.PlayerMaxHp, Is.EqualTo(80));
            Assert.That(data.PlayerHp, Is.EqualTo(45));
            Assert.That(data.PlayerEnergy, Is.EqualTo(3));
            Assert.That(data.Gold, Is.EqualTo(150));
            Assert.That(data.CurrentNodeIndex, Is.EqualTo(5));
            Assert.That(data.CurrentPage, Is.EqualTo(0));
            Assert.That(data.MasterSeed, Is.EqualTo(12345));
            Assert.That(data.MapSeed, Is.EqualTo(67890));
            Assert.That(data.MapLayoutVersion, Is.EqualTo(1));
            Assert.That(data.RandomCounter, Is.EqualTo(7));
            Assert.That(data.DeckCardIds.Count, Is.EqualTo(3));
            Assert.That(data.DeckCardIds[0], Is.EqualTo(101));
            Assert.That(data.DeckCardIds[1], Is.EqualTo(102));
            Assert.That(data.DeckCardIds[2], Is.EqualTo(103));
            Assert.That(data.IsValid, Is.True);
        }

        [Test]
        public void RunSaveData_IsValid_WhenRunProfileIdIsGreaterThanZero()
        {
            RunSaveData data1 = CreateValidSaveData();
            data1.RunProfileId = 0;
            Assert.That(data1.IsValid, Is.False);

            RunSaveData data2 = CreateValidSaveData();
            Assert.That(data2.IsValid, Is.True);
        }

        [Test]
        public void RunSaveData_IsInvalid_WhenSeedMetadataIsMissing()
        {
            RunSaveData data = CreateValidSaveData();
            data.MasterSeed = 0;

            Assert.That(data.IsValid, Is.False);
        }

        [Test]
        public void SaveCurrentRunAsync_ValidData_SavesWithRunSaveKey()
        {
            FakeSaveDataService saveDataService = new FakeSaveDataService();
            RunSaveService service = new RunSaveService(saveDataService);

            service.SaveCurrentRunAsync(CreateValidSaveData()).GetAwaiter().GetResult();

            Assert.That(saveDataService.LastSavedKey, Is.EqualTo("run_save"));
            Assert.That(saveDataService.SaveCallCount, Is.EqualTo(1));
        }

        [Test]
        public void SaveCurrentRunAsync_InvalidData_DoesNotSave()
        {
            FakeSaveDataService saveDataService = new FakeSaveDataService();
            RunSaveService service = new RunSaveService(saveDataService);

            service.SaveCurrentRunAsync(new RunSaveData { RunProfileId = 0 }).GetAwaiter().GetResult();

            Assert.That(saveDataService.SaveCallCount, Is.EqualTo(0));
        }

        [Test]
        public void LoadCurrentRunAsync_InvalidData_ReturnsNull()
        {
            FakeSaveDataService saveDataService = new FakeSaveDataService
            {
                LoadResult = new RunSaveData { RunProfileId = 0 }
            };
            RunSaveService service = new RunSaveService(saveDataService);

            RunSaveData data = service.LoadCurrentRunAsync().GetAwaiter().GetResult();

            Assert.That(data, Is.Null);
        }

        [Test]
        public void DeleteSavedRun_WhenExists_DeletesRunSaveKey()
        {
            FakeSaveDataService saveDataService = new FakeSaveDataService
            {
                ExistsResult = true
            };
            RunSaveService service = new RunSaveService(saveDataService);

            service.DeleteSavedRun();

            Assert.That(saveDataService.LastDeletedKey, Is.EqualTo("run_save"));
        }

        private static RunSaveData CreateValidSaveData()
        {
            return new RunSaveData
            {
                RunProfileId = 5501,
                PlayerMaxHp = 80,
                PlayerHp = 45,
                PlayerEnergy = 3,
                Gold = 150,
                CurrentNodeIndex = 5,
                CurrentPage = 0,
                MasterSeed = 12345,
                MapSeed = 67890,
                MapLayoutVersion = 1,
                RandomCounter = 7,
                DeckCardIds = new List<int> { 101, 102, 103 }
            };
        }

        private sealed class FakeSaveDataService : ISaveDataService
        {
            public int CurrentSlot { get; private set; }
            public bool ExistsResult { get; set; }
            public RunSaveData LoadResult { get; set; }
            public string LastSavedKey { get; private set; }
            public string LastDeletedKey { get; private set; }
            public int SaveCallCount { get; private set; }

            public UniTask SaveAsync<T>(string key, T data, CancellationToken token = default)
            {
                LastSavedKey = key;
                SaveCallCount++;
                return UniTask.CompletedTask;
            }

            public UniTask<T> LoadAsync<T>(string key, T defaultValue = default, CancellationToken token = default)
            {
                object result = LoadResult;
                return UniTask.FromResult(result == null ? defaultValue : (T)result);
            }

            public bool Exists(string key)
            {
                return ExistsResult;
            }

            public void Delete(string key)
            {
                LastDeletedKey = key;
            }

            public void DeleteAll()
            {
            }

            public void SetSlot(int slotIndex)
            {
                CurrentSlot = slotIndex;
            }
        }
    }
}
