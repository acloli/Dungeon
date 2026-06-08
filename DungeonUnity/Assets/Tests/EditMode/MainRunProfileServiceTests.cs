using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.OutGame.Main;
using Game.MasterData.Generated;
using NUnit.Framework;
using R3;
using TFramework.Localization;
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
        public void BuildRunProfile_ValidId_ReturnsRunProfileSummary()
        {
            FakeMasterDataService masterDataService = new FakeMasterDataService();
            masterDataService.SetAll(new[]
            {
                new RunProfileMaster
                {
                    Id = 5501,
                    Key = "run_a",
                    Name = "Run A",
                    LocalizationKey = "run.profile.a",
                    CharacterArchetype = CharacterArchetype.CrimsonExile,
                    PlayerMaxHp = 80,
                    StartingGold = 99
                }
            });
            FakeLocalizationService localizationService = new FakeLocalizationService();
            localizationService.Set("run.profile.a", "ローカライズラン");
            MainRunProfileService service = new MainRunProfileService(masterDataService, localizationService);

            MainRunProfileViewModel runProfile = service.BuildRunProfile(5501);

            Assert.That(runProfile, Is.Not.Null);
            Assert.That(runProfile.Id, Is.EqualTo(5501));
            Assert.That(runProfile.Key, Is.EqualTo("run_a"));
            Assert.That(runProfile.DisplayName, Is.EqualTo("ローカライズラン"));
            Assert.That(runProfile.LocalizationKey, Is.EqualTo("run.profile.a"));
            Assert.That(runProfile.CharacterArchetype, Is.EqualTo("CrimsonExile"));
            Assert.That(runProfile.PlayerMaxHp, Is.EqualTo(80));
            Assert.That(runProfile.StartingGold, Is.EqualTo(99));
        }

        [Test]
        public void BuildRunProfile_WhenLocalizationKeyIsMissing_ReturnsNameFallback()
        {
            FakeMasterDataService masterDataService = new FakeMasterDataService();
            masterDataService.SetAll(new[]
            {
                new RunProfileMaster
                {
                    Id = 5501,
                    Key = "run_a",
                    Name = "Run A",
                    LocalizationKey = "run.profile.missing",
                    CharacterArchetype = CharacterArchetype.CrimsonExile,
                    PlayerMaxHp = 80,
                    StartingGold = 99
                }
            });
            MainRunProfileService service = new MainRunProfileService(masterDataService, new FakeLocalizationService());

            MainRunProfileViewModel runProfile = service.BuildRunProfile(5501);

            Assert.That(runProfile.DisplayName, Is.EqualTo("Run A"));
        }

        [Test]
        public void BuildRunProfile_WhenNameIsMissing_ReturnsKeyFallback()
        {
            FakeMasterDataService masterDataService = new FakeMasterDataService();
            masterDataService.SetAll(new[]
            {
                new RunProfileMaster
                {
                    Id = 5501,
                    Key = "run_a",
                    Name = string.Empty,
                    LocalizationKey = string.Empty,
                    CharacterArchetype = CharacterArchetype.CrimsonExile,
                    PlayerMaxHp = 80,
                    StartingGold = 99
                }
            });
            MainRunProfileService service = new MainRunProfileService(masterDataService);

            MainRunProfileViewModel runProfile = service.BuildRunProfile(5501);

            Assert.That(runProfile.DisplayName, Is.EqualTo("run_a"));
        }

        [Test]
        public void BuildRunProfile_WhenMasterDataIsEmpty_ReturnsNull()
        {
            MainRunProfileService service = new MainRunProfileService(new FakeMasterDataService());

            MainRunProfileViewModel runProfile = service.BuildRunProfile(5501);

            Assert.That(runProfile, Is.Null);
        }

        [Test]
        public void BuildRunProfile_WhenMasterDataServiceIsMissing_ReturnsNull()
        {
            MainRunProfileService service = new MainRunProfileService(null);

            MainRunProfileViewModel runProfile = service.BuildRunProfile(5501);

            Assert.That(runProfile, Is.Null);
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

        /// <summary>
        /// テスト用LocalizationService
        /// </summary>
        private sealed class FakeLocalizationService : ILocalizationService
        {
            private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

            public LanguageCode CurrentLanguage { get; set; } = LanguageCode.Japanese;
            public LanguageCode[] SupportedLanguages { get; } = { LanguageCode.Japanese };
            public Observable<LanguageCode> OnLanguageChanged => null;

            public void Set(string key, string value)
            {
                _values[key] = value;
            }

            public string Get(string key)
            {
                return _values.TryGetValue(key, out string value) ? value : key;
            }

            public string Get(string key, params object[] args)
            {
                return string.Format(Get(key), args);
            }

            public bool HasKey(string key)
            {
                return _values.ContainsKey(key);
            }

            public UniTask LoadLanguageAsync(LanguageCode language, CancellationToken ct)
            {
                CurrentLanguage = language;
                return UniTask.CompletedTask;
            }
        }
    }
}
