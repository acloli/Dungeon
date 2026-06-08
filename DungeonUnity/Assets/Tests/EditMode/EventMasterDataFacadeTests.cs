using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Game.MasterData.Generated;
using NUnit.Framework;
using TFramework.MasterData;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// EventMasterDataFacadeのEditモードテストクラス
    /// </summary>
    [TestFixture]
    public sealed class EventMasterDataFacadeTests
    {
        [Test]
        public void BuildEvents_ExpandsChoicesInChoiceOrder()
        {
            FakeMasterDataService masterDataService = new FakeMasterDataService();
            masterDataService.SetAll(new[]
            {
                new EventMaster { Id = 9001, EventName = "Fountain", LocalizationKey = "event.fountain", ImageId = "event_fountain" }
            });
            masterDataService.SetAll(new[]
            {
                new EventChoiceMaster { Id = 9102, EventId = 9001, ChoiceId = 2, LocalizationKey = "event.fountain.choice2", EffectType = EffectType.GainGold, EffectValue = 100 },
                new EventChoiceMaster { Id = 9101, EventId = 9001, ChoiceId = 1, LocalizationKey = "event.fountain.choice1", EffectType = EffectType.GainMaxHp, EffectValue = 5 }
            });

            EventMasterDataFacade facade = new EventMasterDataFacade(masterDataService);

            IReadOnlyList<RuntimeEvent> events = facade.BuildEvents(1);

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].EventName, Is.EqualTo("Fountain"));
            Assert.That(events[0].Choices.Count, Is.EqualTo(2));
            Assert.That(events[0].Choices[0].ChoiceId, Is.EqualTo(1));
            Assert.That(events[0].Choices[0].EffectType, Is.EqualTo(EffectType.GainMaxHp));
            Assert.That(events[0].Choices[1].ChoiceId, Is.EqualTo(2));
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
