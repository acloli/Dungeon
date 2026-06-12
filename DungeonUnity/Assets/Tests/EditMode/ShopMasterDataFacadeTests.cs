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
    /// ShopMasterDataFacadeのEditモードテストクラス
    /// </summary>
    [TestFixture]
    public sealed class ShopMasterDataFacadeTests
    {
        [Test]
        public void BuildShopLineup_FiltersByShopIdAndSortsBySlotIndex()
        {
            FakeMasterDataService masterDataService = new FakeMasterDataService();
            masterDataService.SetAll(new[]
            {
                new ShopLineupMaster { Id = 8302, ShopId = 1, SlotIndex = 2, RewardType = RewardType.Relic, RequiredCardType = CardType.None, Weight = 20 },
                new ShopLineupMaster { Id = 8301, ShopId = 1, SlotIndex = 1, RewardType = RewardType.Card, RequiredCardType = CardType.Attack, Weight = 10 },
                new ShopLineupMaster { Id = 8303, ShopId = 2, SlotIndex = 1, RewardType = RewardType.Potion, RequiredCardType = CardType.None, Weight = 30 }
            });

            ShopMasterDataFacade facade = new ShopMasterDataFacade(masterDataService);

            RuntimeShopLineup lineup = facade.BuildShopLineup(1);

            Assert.That(lineup.ShopId, Is.EqualTo(1));
            Assert.That(lineup.Slots.Count, Is.EqualTo(2));
            Assert.That(lineup.Slots[0].SlotIndex, Is.EqualTo(1));
            Assert.That(lineup.Slots[0].RewardType, Is.EqualTo(RewardType.Card));
            Assert.That(lineup.Slots[1].SlotIndex, Is.EqualTo(2));
            Assert.That(lineup.Slots[1].RewardType, Is.EqualTo(RewardType.Relic));
        }

        [Test]
        public void BuildPriceRules_ExpandsCardAndItemPriceRules()
        {
            FakeMasterDataService masterDataService = new FakeMasterDataService();
            masterDataService.SetAll(new[]
            {
                new ShopCardPriceMaster { Id = 8402, CardRarity = CardRarity.Rare, BasePrice = 120, JitterPercent = 15 },
                new ShopCardPriceMaster { Id = 8401, CardRarity = CardRarity.Common, BasePrice = 50, JitterPercent = 10 }
            });
            masterDataService.SetAll(new[]
            {
                new ShopItemPriceMaster { Id = 8502, ItemType = RewardType.Relic, ItemId = 2, BasePrice = 180, JitterPercent = 20 },
                new ShopItemPriceMaster { Id = 8501, ItemType = RewardType.Potion, ItemId = 1, BasePrice = 60, JitterPercent = 5 }
            });

            ShopMasterDataFacade facade = new ShopMasterDataFacade(masterDataService);

            IReadOnlyDictionary<CardRarity, RuntimeCardPriceRule> cardPriceRules = facade.BuildCardPriceRules();
            IReadOnlyList<RuntimeItemPriceRule> itemPriceRules = facade.BuildItemPriceRules();

            Assert.That(cardPriceRules[CardRarity.Common].BasePrice, Is.EqualTo(50));
            Assert.That(cardPriceRules[CardRarity.Rare].JitterPercent, Is.EqualTo(15));
            Assert.That(itemPriceRules.Count, Is.EqualTo(2));
            Assert.That(itemPriceRules[0].ItemType, Is.EqualTo(RewardType.Potion));
            Assert.That(itemPriceRules[1].ItemType, Is.EqualTo(RewardType.Relic));
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
