using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Save.Model;
using Dungeon.Runtime.OutGame.Main;
using Game.MasterData.Generated;
using NUnit.Framework;
using TFramework.MasterData;
using UnityEngine;

namespace Dungeon.Tests.EditMode
{
    public sealed class MenuMasterDataFacadeTests
    {
        [Test]
        public void MasterRows_DriveCardsUpgradesCharactersAndEachStarterDeck()
        {
            var master = new MemoryMaster();
            master.Set(new CardMaster { Id = 10, Name = "Base", Cost = 2, UpgradeCardId = 11 },
                new CardMaster { Id = 11, Name = "Upgrade", Cost = 1 }, new CardMaster { Id = 20, Name = "Other" });
            master.Set(new RunProfileMaster { Id = 100, StarterDeckGroupId = 200, PlayerMaxHp = 80, StartingGold = 99 },
                new RunProfileMaster { Id = 101, StarterDeckGroupId = 201, PlayerMaxHp = 45, StartingGold = 30 });
            master.Set(new CharacterMaster { Id = 1, Name = "First", RunProfileId = 100 }, new CharacterMaster { Id = 2, Name = "Second", RunProfileId = 101 });
            master.Set(new DeckGroupMaster { Id = 1, DeckGroupId = 200, CardId = 10, Count = 5 }, new DeckGroupMaster { Id = 2, DeckGroupId = 201, CardId = 20, Count = 3 });
            master.Set(new RelicMaster { Id = 30, Name = "Relic" });
            var catalog = ScriptableObject.CreateInstance<MenuPresentationCatalog>();
            try
            {
                var facade = new MenuMasterDataFacade(master, catalog);
                var data = facade.Build(new CollectionProgressData());
                Assert.That(data.Cards.Select(c => c.MasterId), Is.EqualTo(new[] { 10, 20 }));
                Assert.That(data.Cards[0].UpgradedName, Is.EqualTo("Upgrade"));
                Assert.That(data.Cards[0].UpgradedCost, Is.EqualTo(1));
                Assert.That(data.Cards[0].HasUpgrade, Is.True);
                Assert.That(data.Cards[1].HasUpgrade, Is.False);
                Assert.That(data.Cards[0].IsDiscovered, Is.True);
                Assert.That(data.Cards[1].IsDiscovered, Is.False);
                Assert.That(data.Characters[0].IsUnlocked, Is.True);
                Assert.That(data.Characters[1].IsUnlocked, Is.False);
                Assert.That(data.Characters[1].Deck[0].Quantity, Is.EqualTo(3));
                data = facade.Build(new CollectionProgressData { CharacterIds = new List<int> { 2 }, RelicIds = new List<int> { 30 } });
                Assert.That(data.Characters[1].IsUnlocked, Is.True);
                Assert.That(data.Cards[1].IsDiscovered, Is.True);
                Assert.That(data.Relics.Single().IsDiscovered, Is.True);
                master.Set(new CharacterMaster { Id = 9, Name = "NotPlayable", RunProfileId = 0 });
                Assert.That(facade.Build(new CollectionProgressData()).Characters.Single().IsUnlocked, Is.False);
            }
            finally { UnityEngine.Object.DestroyImmediate(catalog); }
        }

        [Test]
        public void EmptyMasterData_ProducesEmptyCollections()
        {
            var catalog = ScriptableObject.CreateInstance<MenuPresentationCatalog>();
            try
            {
                var data = new MenuMasterDataFacade(new MemoryMaster(), catalog).Build(new CollectionProgressData());
                Assert.That(data.Cards, Is.Empty); Assert.That(data.Characters, Is.Empty); Assert.That(data.Relics, Is.Empty);
                Assert.That(data.HasResumableAdventure, Is.False);
            }
            finally { UnityEngine.Object.DestroyImmediate(catalog); }
        }

        private sealed class MemoryMaster : IMasterDataService
        {
            private readonly Dictionary<Type, object> _rows = new Dictionary<Type, object>();
            public void Set<T>(params T[] rows) => _rows[typeof(T)] = rows;
            public IReadOnlyList<T> GetAll<T>() where T : class, IMasterDataObject => _rows.TryGetValue(typeof(T), out object rows) ? (T[])rows : Array.Empty<T>();
            public T Get<T,TKey>(TKey key) where T : class, IMasterDataObject<TKey> => GetAll<T>().FirstOrDefault(row => EqualityComparer<TKey>.Default.Equals(row.GetKey(), key));
            public T GetContainer<T>() where T : class => null;
            public UniTask InitializeAsync(CancellationToken ct) => UniTask.CompletedTask;
            public UniTask DownloadFromServerAsync(CancellationToken ct) => UniTask.CompletedTask;
            public UniTask ReloadAsync(CancellationToken ct) => UniTask.CompletedTask;
        }
    }
}
