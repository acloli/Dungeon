using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.OutGame.Main;
using Game.MasterData.Generated;
using NUnit.Framework;
using TFramework.MasterData;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// MainRunProfileServiceのEditモードテストクラス
    /// </summary>
    [TestFixture]
    public sealed class MainRunProfileServiceTests
    {
        [Test]
        public void BuildRunProfiles_SortsByIdAndBuildsViewModels()
        {
            FakeMasterDataService masterDataService = new FakeMasterDataService();
            masterDataService.SetAll(new[]
            {
                new RunProfileMaster
                {
                    Id = 5502,
                    Key = "run_b",
                    CharacterArchetype = CharacterArchetype.CrimsonExile,
                    PlayerMaxHp = 90,
                    StartingGold = 25
                },
                new RunProfileMaster
                {
                    Id = 5501,
                    Key = "run_a",
                    CharacterArchetype = CharacterArchetype.CrimsonExile,
                    PlayerMaxHp = 80,
                    StartingGold = 99
                }
            });
            MainRunProfileService service = new MainRunProfileService(masterDataService);

            IReadOnlyList<MainRunProfileViewModel> runProfiles = service.BuildRunProfiles();

            Assert.That(runProfiles.Count, Is.EqualTo(2));
            Assert.That(runProfiles[0].Id, Is.EqualTo(5501));
            Assert.That(runProfiles[0].Key, Is.EqualTo("run_a"));
            Assert.That(runProfiles[0].CharacterArchetype, Is.EqualTo("CrimsonExile"));
            Assert.That(runProfiles[0].PlayerMaxHp, Is.EqualTo(80));
            Assert.That(runProfiles[0].StartingGold, Is.EqualTo(99));
            Assert.That(runProfiles[1].Id, Is.EqualTo(5502));
        }

        [Test]
        public void BuildRunProfiles_WhenMasterDataIsEmpty_ReturnsEmptyList()
        {
            MainRunProfileService service = new MainRunProfileService(new FakeMasterDataService());

            IReadOnlyList<MainRunProfileViewModel> runProfiles = service.BuildRunProfiles();

            Assert.That(runProfiles, Is.Empty);
        }

        [Test]
        public void BuildRunProfiles_WhenMasterDataServiceIsMissing_ReturnsEmptyList()
        {
            MainRunProfileService service = new MainRunProfileService(null);

            IReadOnlyList<MainRunProfileViewModel> runProfiles = service.BuildRunProfiles();

            Assert.That(runProfiles, Is.Empty);
        }

        /// <summary>
        /// テスト用MasterDataService
        /// </summary>
        private sealed class FakeMasterDataService : IMasterDataService
        {
            private readonly Dictionary<Type, object> _allData = new Dictionary<Type, object>();

            public void SetAll<T>(IReadOnlyList<T> values) where T : class, IMasterDataObject
            {
                _allData[typeof(T)] = values;
            }

            public UniTask InitializeAsync(CancellationToken ct)
            {
                return UniTask.CompletedTask;
            }

            public IReadOnlyList<T> GetAll<T>() where T : class, IMasterDataObject
            {
                if (_allData.TryGetValue(typeof(T), out object values))
                {
                    return (IReadOnlyList<T>)values;
                }

                return Array.Empty<T>();
            }

            public T Get<T, TKey>(TKey key) where T : class, IMasterDataObject<TKey>
            {
                IReadOnlyList<T> all = GetAll<T>();
                for (int i = 0; i < all.Count; i++)
                {
                    if (Equals(all[i].GetKey(), key))
                    {
                        return all[i];
                    }
                }

                return null;
            }

            public T GetContainer<T>() where T : class
            {
                return null;
            }

            public UniTask DownloadFromServerAsync(CancellationToken ct)
            {
                return UniTask.CompletedTask;
            }

            public UniTask ReloadAsync(CancellationToken ct)
            {
                return UniTask.CompletedTask;
            }
        }
    }
}
