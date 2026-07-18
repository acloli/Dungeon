using System.Collections.Generic;
using System.Linq;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Domain;
using Dungeon.Runtime.InGame.Save.Model;
using Dungeon.Tests.EditMode.Support;
using Game.MasterData.Generated;
using NUnit.Framework;
using UnityEngine;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleCheckpointServiceのEditorモードテストクラス
    /// </summary>
    [TestFixture]
    public sealed class BattleCheckpointServiceTests
    {
        [Test]
        public void BuildSaveData_WritesRunRelicActivationsAndFreeUpgradeCount_ButOmitsTurnActivations()
        {
            BattleSceneState state = new BattleSceneState();
            state.TryReserveRunRelicEffectActivation(7101);
            state.TryReserveRunRelicEffectActivation(7102);
            state.TryReserveTurnRelicEffectActivation(7199);
            state.GrantRestShopFreeUpgrade(2);
            RuntimeRunDefinition runDefinition = CreateRunDefinition();

            RunSaveData saveData = new BattleCheckpointService().BuildSaveData(
                state,
                runDefinition,
                111,
                222,
                3,
                17);

            Assert.That(saveData.ActivatedRelicEffectIdsThisRun, Is.EquivalentTo(new[] { 7101, 7102 }));
            Assert.That(saveData.ActivatedRelicEffectIdsThisRun.Contains(7199), Is.False);
            Assert.That(saveData.RestShopFreeUpgradeCount, Is.EqualTo(2));
        }

        [Test]
        public void RestoreFromSave_RestoresRestShopStateWithoutChangingRandomMetadata()
        {
            RuntimeCard card = BattleTestData.Card(1001).Build();
            RuntimeRelic relic = BattleTestData.Relic(2001).Build();
            RuntimePotion potion = BattleTestData.Potion(3001).Build();
            RuntimeRunDefinition runDefinition = CreateRunDefinition(card, relic, potion);
            RunSaveData saveData = CreateRestShopSaveData();
            saveData.DeckCardIds.Add(card.Id);
            saveData.ActivatedRelicEffectIdsThisRun.AddRange(new[] { 7101, 7102 });
            saveData.RestShopFreeUpgradeCount = 2;
            saveData.ShopItems.Add(new SaveShopItem
            {
                SlotIndex = 0,
                RewardType = (int)RewardType.Card,
                CardId = card.Id,
                Price = 50
            });
            saveData.ShopItems.Add(new SaveShopItem
            {
                SlotIndex = 1,
                RewardType = (int)RewardType.Relic,
                ItemId = relic.Id,
                Price = 80,
                IsSoldOut = true
            });
            saveData.ShopItems.Add(new SaveShopItem
            {
                SlotIndex = 2,
                RewardType = (int)RewardType.Potion,
                ItemId = potion.Id,
                Price = 60
            });
            BattleSceneState state = new BattleSceneState();
            state.TryReserveRunRelicEffectActivation(7998);
            state.TryReserveTurnRelicEffectActivation(7999);
            state.GrantRestShopFreeUpgrade(4);
            BattleCheckpointService service = new BattleCheckpointService();

            service.RestoreFromSave(
                state,
                runDefinition,
                saveData,
                runDefinition.CardCatalog,
                new BattleRelicService(),
                new BattlePotionService());
            RunSaveData rebuilt = service.BuildSaveData(
                state,
                runDefinition,
                saveData.MasterSeed,
                saveData.MapSeed,
                saveData.MapLayoutVersion,
                saveData.RandomCounter);

            Assert.That(state.ActivatedRelicEffectIdsThisRun, Is.EquivalentTo(new[] { 7101, 7102 }));
            Assert.That(state.ActivatedRelicEffectIdsThisTurn, Is.Empty);
            Assert.That(state.RestShopFreeUpgradeCount, Is.EqualTo(2));
            Assert.That(state.ShopItems, Has.Count.EqualTo(3));
            Assert.That(state.ShopItems[0].Card, Is.SameAs(card));
            Assert.That(state.ShopItems[1].Relic, Is.SameAs(relic));
            Assert.That(state.ShopItems[1].IsSoldOut, Is.True);
            Assert.That(state.ShopItems[2].Potion, Is.SameAs(potion));
            Assert.That(saveData.MasterSeed, Is.EqualTo(111));
            Assert.That(saveData.MapSeed, Is.EqualTo(222));
            Assert.That(saveData.RandomCounter, Is.EqualTo(17));
            Assert.That(rebuilt.MasterSeed, Is.EqualTo(111));
            Assert.That(rebuilt.MapSeed, Is.EqualTo(222));
            Assert.That(rebuilt.MapLayoutVersion, Is.EqualTo(3));
            Assert.That(rebuilt.RandomCounter, Is.EqualTo(17));
            Assert.That(rebuilt.ActivatedRelicEffectIdsThisRun, Is.EquivalentTo(new[] { 7101, 7102 }));
            Assert.That(rebuilt.RestShopFreeUpgradeCount, Is.EqualTo(2));
            Assert.That(rebuilt.ShopItems, Has.Count.EqualTo(3));
            Assert.That(rebuilt.ShopItems[0].SlotIndex, Is.EqualTo(0));
            Assert.That(rebuilt.ShopItems[1].ItemId, Is.EqualTo(relic.Id));
            Assert.That(rebuilt.ShopItems[1].IsSoldOut, Is.True);
            Assert.That(rebuilt.ShopItems[2].ItemId, Is.EqualTo(potion.Id));
        }

        [Test]
        public void RestoreFromSave_LegacyDataUsesEmptyActivationIdsAndZeroFreeUpgradeCount()
        {
            const string LegacyJson = "{\"RunProfileId\":5501,\"PlayerMaxHp\":50,\"PlayerHp\":40,\"PlayerEnergy\":3,\"Gold\":120,\"CurrentNodeIndex\":0,\"CurrentPage\":3,\"MasterSeed\":111,\"MapSeed\":222,\"MapLayoutVersion\":3,\"RandomCounter\":17,\"DeckCardIds\":[],\"OwnedRelicIds\":[],\"OwnedPotionIds\":[],\"ShopItems\":[]}";
            RunSaveData legacySaveData = JsonUtility.FromJson<RunSaveData>(LegacyJson);
            RuntimeRunDefinition runDefinition = CreateRunDefinition();
            BattleSceneState state = new BattleSceneState();
            state.TryReserveRunRelicEffectActivation(7998);
            state.TryReserveTurnRelicEffectActivation(7999);
            state.GrantRestShopFreeUpgrade(3);

            new BattleCheckpointService().RestoreFromSave(
                state,
                runDefinition,
                legacySaveData,
                runDefinition.CardCatalog,
                new BattleRelicService(),
                new BattlePotionService());

            Assert.That(legacySaveData.IsValid, Is.True);
            Assert.That(legacySaveData.ActivatedRelicEffectIdsThisRun, Is.Empty);
            Assert.That(legacySaveData.RestShopFreeUpgradeCount, Is.Zero);
            Assert.That(state.ActivatedRelicEffectIdsThisRun, Is.Empty);
            Assert.That(state.ActivatedRelicEffectIdsThisTurn, Is.Empty);
            Assert.That(state.RestShopFreeUpgradeCount, Is.Zero);
        }

        private static RuntimeRunDefinition CreateRunDefinition(
            RuntimeCard card = null,
            RuntimeRelic relic = null,
            RuntimePotion potion = null)
        {
            RuntimeRunDefinitionBuilder builder = BattleTestData.RunDefinition();
            builder.Nodes = new[]
            {
                new RuntimeMapNodeBuilder(5301)
                {
                    NodeType = InGameNodeType.RestShop
                }.Build()
            };
            builder.CardCatalog = card == null
                ? new Dictionary<int, RuntimeCard>()
                : new Dictionary<int, RuntimeCard> { { card.Id, card } };
            builder.RelicCatalog = relic == null
                ? new Dictionary<int, RuntimeRelic>()
                : new Dictionary<int, RuntimeRelic> { { relic.Id, relic } };
            builder.PotionCatalog = potion == null
                ? new Dictionary<int, RuntimePotion>()
                : new Dictionary<int, RuntimePotion> { { potion.Id, potion } };
            return builder.Build();
        }

        private static RunSaveData CreateRestShopSaveData()
        {
            return new RunSaveData
            {
                RunProfileId = 5501,
                PlayerMaxHp = 50,
                PlayerHp = 40,
                PlayerEnergy = 3,
                Gold = 120,
                CurrentNodeIndex = 0,
                CurrentPage = (int)BattleScenePage.RestShop,
                MasterSeed = 111,
                MapSeed = 222,
                MapLayoutVersion = 3,
                RandomCounter = 17,
                DeckCardIds = new List<int>(),
                MapRouteNodeIndices = new List<int> { 0 },
                OwnedRelicIds = new List<int>(),
                OwnedPotionIds = new List<int>(),
                ShopItems = new List<SaveShopItem>()
            };
        }
    }
}
