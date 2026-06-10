using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Domain;
using Game.MasterData.Generated;
using NUnit.Framework;
using System.Collections.Generic;

namespace Dungeon.Tests.EditMode
{
    [TestFixture]
    public sealed class BattleShopServiceTests
    {
        private sealed class FakeBattleMasterDataFacade : IBattleMasterDataFacade
        {
            public IReadOnlyDictionary<int, RuntimeCard> BuildCardCatalog() => new Dictionary<int, RuntimeCard>
            {
                { 1, new RuntimeCard { Id = 1, DisplayName = "Card 1" } },
                { 2, new RuntimeCard { Id = 2, DisplayName = "Card 2" } }
            };

            public RuntimeRunDefinition BuildRunDefinition(int runProfileId) => new RuntimeRunDefinition();

            public IReadOnlyList<ShopLineupMaster> GetShopLineupMasters() => new List<ShopLineupMaster>
            {
                new ShopLineupMaster { Id = 1, RunProfileId = 1, SlotIndex = 0, RewardType = (int)RewardType.Card, GroupId = 1 },
                new ShopLineupMaster { Id = 2, RunProfileId = 1, SlotIndex = 1, RewardType = (int)RewardType.Relic, GroupId = 2 }
            };

            public ShopCardPriceMaster GetShopCardPriceMaster(int rarity) => new ShopCardPriceMaster { Rarity = rarity, BasePrice = 50, PriceJitter = 10 };

            public ShopItemPriceMaster GetShopItemPriceMaster(int rewardType, int rarity) => new ShopItemPriceMaster { RewardType = rewardType, Rarity = rarity, BasePrice = 100, PriceJitter = 20 };

            public ShopCardRemovalMaster GetShopCardRemovalMaster() => new ShopCardRemovalMaster { Id = 1, InitialPrice = 75, PriceIncrement = 25 };
        }

        private sealed class FakeRandomProvider : IBattleRandomProvider
        {
            public float NextFloat(float min, float max) => min;
            public int NextInt(int min, int max) => min;
            public float NextFloat() => 0f;
            public int RollWeightedIndex(IReadOnlyList<int> weights) => 0;
            public T SelectRandom<T>(IReadOnlyList<T> list) => list[0];
            public void Shuffle<T>(IList<T> list) { }
        }

        [Test]
        public void InitializeShop_ShouldPopulateShopItems()
        {
            var service = new BattleShopService(new FakeBattleMasterDataFacade());
            var state = new BattleSceneState();
            var runDef = new RuntimeRunDefinition { RunProfileId = 1 };

            service.InitializeShop(state, runDef, new FakeRandomProvider());

            Assert.AreEqual(2, state.ShopItems.Count);
            Assert.AreEqual(RewardType.Card, state.ShopItems[0].RewardType);
            Assert.AreEqual(RewardType.Relic, state.ShopItems[1].RewardType);
        }

        [Test]
        public void PurchaseShopItem_WithSufficientGold_ShouldDeductGoldAndReturnTrue()
        {
            var service = new BattleShopService(new FakeBattleMasterDataFacade());
            var state = new BattleSceneState { Gold = 200 };
            state.ShopItems.Add(new BattleShopItemState(0, RewardType.Card, new RuntimeCard { Id = 1 }, 0, 50, false));

            bool result = service.PurchaseShopItem(state, 0);

            Assert.IsTrue(result);
            Assert.AreEqual(150, state.Gold);
            Assert.IsTrue(state.ShopItems[0].IsSoldOut);
            Assert.AreEqual(1, state.Deck.Count); // Purchased card should be added to deck
        }

        [Test]
        public void PurchaseShopItem_WithInsufficientGold_ShouldReturnFalse()
        {
            var service = new BattleShopService(new FakeBattleMasterDataFacade());
            var state = new BattleSceneState { Gold = 20 };
            state.ShopItems.Add(new BattleShopItemState(0, RewardType.Card, new RuntimeCard { Id = 1 }, 0, 50, false));

            bool result = service.PurchaseShopItem(state, 0);

            Assert.IsFalse(result);
            Assert.AreEqual(20, state.Gold);
            Assert.IsFalse(state.ShopItems[0].IsSoldOut);
            Assert.AreEqual(0, state.Deck.Count);
        }

        [Test]
        public void GetCardRemovalPrice_ShouldCalculateCorrectly()
        {
            var service = new BattleShopService(new FakeBattleMasterDataFacade());
            var state = new BattleSceneState { CardRemovalCount = 2 };

            int price = service.GetCardRemovalPrice(state);

            Assert.AreEqual(75 + 25 * 2, price); // 125
        }

        [Test]
        public void PurchaseCardRemoval_WithSufficientGold_ShouldDeductGoldAndRemoveCard()
        {
            var service = new BattleShopService(new FakeBattleMasterDataFacade());
            var state = new BattleSceneState { Gold = 100, CardRemovalCount = 0 };
            var card = new RuntimeCard { Id = 1 };
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
