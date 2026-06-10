using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Domain;
using Game.MasterData.Generated;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TFramework.MasterData;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Dungeon.Tests.EditMode
{
    [TestFixture]
    public sealed class BattleShopServiceTests
    {
        private sealed class FakeMasterDataService : IMasterDataService
        {
            private readonly Dictionary<Type, object> _allData = new Dictionary<Type, object>();

            public void SetAll<T>(IReadOnlyList<T> values) where T : class, IMasterDataObject
            {
                _allData[typeof(T)] = values;
            }

            public UniTask InitializeAsync(CancellationToken ct) => UniTask.CompletedTask;

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

            public T GetContainer<T>() where T : class => null;
            public UniTask DownloadFromServerAsync(CancellationToken ct) => UniTask.CompletedTask;
            public UniTask ReloadAsync(CancellationToken ct) => UniTask.CompletedTask;
        }

        private sealed class FakeRandomProvider : IBattleRandomProvider
        {
            public float NextFloat(float min, float max) => min;
            public int NextInt(int min, int max) => min;
            public int Range(int min, int max) => min;
            public float NextFloat() => 0f;
            public int RollWeightedIndex(IReadOnlyList<int> weights) => 0;
            public T SelectRandom<T>(IReadOnlyList<T> list) => list[0];
            public void Shuffle<T>(IList<T> list) { }
        }

        private static RuntimeCard CreateCard(int id)
        {
            return new RuntimeCard(
                id,
                $"card_{id}",
                "Card",
                string.Empty,
                1,
                CardType.Attack,
                CardRarity.Common,
                CharacterArchetype.CrimsonExile,
                Array.Empty<RuntimeCardEffect>());
        }

        private static RuntimeRunDefinition CreateRunDefinition()
        {
            return new RuntimeRunDefinition(
                1,
                "profile_1",
                CharacterArchetype.CrimsonExile,
                40,
                3,
                100,
                new[] { CreateCard(1) },
                Array.Empty<RuntimeRewardEntry>(),
                Array.Empty<RuntimeMapNode>(),
                new Dictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>>(),
                Array.Empty<RuntimeEvent>(),
                new RuntimeShopLineup(1, new[] { new RuntimeShopSlot(0, RewardType.Card, CardType.Attack, 100), new RuntimeShopSlot(1, RewardType.Relic, CardType.Attack, 100) }),
                new Dictionary<CardRarity, RuntimeCardPriceRule> { { CardRarity.Common, new RuntimeCardPriceRule(CardRarity.Common, 50, 10) } },
                new[] { new RuntimeItemPriceRule(RewardType.Relic, 1, 100, 20) }
            );
        }

        [Test]
        public void InitializeShop_ShouldPopulateShopItems()
        {
            var masterData = new FakeMasterDataService();
            var service = new BattleShopService(masterData);
            var state = new BattleSceneState();
            var runDef = CreateRunDefinition();

            service.InitializeShop(state, runDef, new FakeRandomProvider());

            Assert.AreEqual(2, state.ShopItems.Count);
            Assert.AreEqual(RewardType.Card, state.ShopItems[0].RewardType);
            Assert.AreEqual(RewardType.Relic, state.ShopItems[1].RewardType);
        }

        [Test]
        public void PurchaseShopItem_WithSufficientGold_ShouldDeductGoldAndReturnTrue()
        {
            var masterData = new FakeMasterDataService();
            var service = new BattleShopService(masterData);
            var state = new BattleSceneState { Gold = 200 };
            state.ShopItems.Add(new BattleShopItemState(0, RewardType.Card, CreateCard(1), 0, 50, false));

            bool result = service.PurchaseShopItem(state, 0);

            Assert.IsTrue(result);
            Assert.AreEqual(150, state.Gold);
            Assert.IsTrue(state.ShopItems[0].IsSoldOut);
            Assert.AreEqual(1, state.Deck.Count); // Purchased card should be added to deck
        }

        [Test]
        public void PurchaseShopItem_WithInsufficientGold_ShouldReturnFalse()
        {
            var masterData = new FakeMasterDataService();
            var service = new BattleShopService(masterData);
            var state = new BattleSceneState { Gold = 20 };
            state.ShopItems.Add(new BattleShopItemState(0, RewardType.Card, CreateCard(1), 0, 50, false));

            bool result = service.PurchaseShopItem(state, 0);

            Assert.IsFalse(result);
            Assert.AreEqual(20, state.Gold);
            Assert.IsFalse(state.ShopItems[0].IsSoldOut);
            Assert.AreEqual(0, state.Deck.Count);
        }

        [Test]
        public void GetCardRemovalPrice_ShouldCalculateCorrectly()
        {
            var masterData = new FakeMasterDataService();
            masterData.SetAll(new[] { new ShopCardRemovalMaster { Id = 1, BasePrice = 75, PriceIncreasePerPurchase = 25 } });
            var service = new BattleShopService(masterData);
            var state = new BattleSceneState { CardRemovalCount = 2 };

            int price = service.GetCardRemovalPrice(state);

            Assert.AreEqual(125, price); // 75 + 25 * 2
        }

        [Test]
        public void PurchaseCardRemoval_WithSufficientGold_ShouldDeductGoldAndRemoveCard()
        {
            var masterData = new FakeMasterDataService();
            masterData.SetAll(new[] { new ShopCardRemovalMaster { Id = 1, BasePrice = 75, PriceIncreasePerPurchase = 25 } });
            var service = new BattleShopService(masterData);
            var state = new BattleSceneState { Gold = 100, CardRemovalCount = 0 };
            var card = CreateCard(1);
            state.Deck.Add(card);

            bool result = service.PurchaseCardRemoval(state, card);

            Assert.IsTrue(result);
            Assert.AreEqual(25, state.Gold); // 100 - 75
            Assert.AreEqual(1, state.CardRemovalCount);
            Assert.IsTrue(state.IsCardRemovalSoldOut);
            Assert.AreEqual(0, state.Deck.Count);
        }
    }
}
