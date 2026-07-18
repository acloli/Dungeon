using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using Dungeon.Runtime.InGame.Save.Model;
using Dungeon.Tests.PlayMode.Support;
using Game.MasterData.Generated;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Dungeon.Tests.PlayMode
{
    /// <summary>
    /// RestShopコンテンツの実シーン結合テストクラス
    /// </summary>
    public sealed class RestShopContentPlayModeTests
    {
        private const int RefiningPrismId = 5;
        private const int InitialSaveFrameLimit = 300;

        private BattleScenePlayModeHarness _harness;

        [UnityTest]
        public IEnumerator RestShopVisit_RefreshesLineupAndKeepsReopenStable_WithOwnedRelicAndFreeUpgradeRules()
        {
            _harness = new BattleScenePlayModeHarness(
                CreateRestShopNodes(),
                Enumerable.Range(0, 64).ToArray());

            yield return _harness.LoadAsync();
            yield return WaitForInitialSave();

            RunSaveData restShopReadySave = CloneSaveData(_harness.SavedRun);
            restShopReadySave.CurrentNodeIndex = -1;
            restShopReadySave.CurrentPage = (int)BattleScenePage.Map;
            restShopReadySave.PlayerHp = Math.Max(1, restShopReadySave.PlayerMaxHp - 10);
            restShopReadySave.MapRouteNodeIndices.Clear();
            restShopReadySave.ShopItems.Clear();
            if (!restShopReadySave.OwnedRelicIds.Contains(RefiningPrismId))
            {
                restShopReadySave.OwnedRelicIds.Add(RefiningPrismId);
            }

            _harness.FlowService.InitializeFromSave(restShopReadySave);
            _harness.FlowService.SelectMapNode(0);

            BattleSceneSnapshot enteredSnapshot = _harness.QueryService.CreateSnapshot();
            string firstVisitLineup = BuildLineupSignature(enteredSnapshot);
            Assert.That(enteredSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.RestShop));
            Assert.That(enteredSnapshot.Shop.ShopItems, Is.Not.Empty, "新しいvisitではlineupを生成する必要がある。");
            AssertOwnedRelicIsNotOffered(enteredSnapshot);
            Assert.That(_harness.SavedRun.RestShopFreeUpgradeCount, Is.EqualTo(1));

            _harness.FlowService.ApplyUpgrade();
            IReadOnlyDictionary<int, int> firstUpgradePrices = _harness.FlowService.GetCardSelectPrices();
            Assert.That(firstUpgradePrices, Is.Not.Empty, "強化可能なカード候補が必要である。");
            Assert.That(firstUpgradePrices.Values, Is.All.EqualTo(0), "visit最初の無料強化価格は0である必要がある。");
            _harness.FlowService.CancelCardSelect();

            _harness.FlowService.OpenShop();
            string openedLineup = BuildLineupSignature(_harness.QueryService.CreateSnapshot());
            _harness.FlowService.LeaveShop();
            _harness.FlowService.OpenShop();
            BattleSceneSnapshot reopenedSnapshot = _harness.QueryService.CreateSnapshot();

            Assert.That(BuildLineupSignature(reopenedSnapshot), Is.EqualTo(openedLineup));
            Assert.That(openedLineup, Is.EqualTo(firstVisitLineup));
            AssertOwnedRelicIsNotOffered(reopenedSnapshot);

            _harness.FlowService.LeaveShop();
            _harness.FlowService.ContinueFromRestShop();
            _harness.FlowService.SelectMapNode(1);
            BattleSceneSnapshot nextVisitSnapshot = _harness.QueryService.CreateSnapshot();

            Assert.That(nextVisitSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.RestShop));
            Assert.That(nextVisitSnapshot.Shop.ShopItems, Is.Not.Empty);
            Assert.That(BuildLineupSignature(nextVisitSnapshot), Is.Not.EqualTo(firstVisitLineup));
            AssertOwnedRelicIsNotOffered(nextVisitSnapshot);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_harness == null)
            {
                yield break;
            }

            yield return _harness.UnloadAsync();
            _harness.Dispose();
            _harness = null;
        }

        private IEnumerator WaitForInitialSave()
        {
            int waitedFrames = 0;
            while (_harness.SavedRun == null && waitedFrames++ < InitialSaveFrameLimit)
            {
                yield return null;
            }

            Assert.That(_harness.SavedRun, Is.Not.Null, "BattleScene初期checkpointが生成されなかった。");
        }

        private static IReadOnlyList<RuntimeMapNode> CreateRestShopNodes()
        {
            return new[]
            {
                new RuntimeMapNode(
                    5301,
                    "playmode_rest_shop_01",
                    1,
                    InGameNodeType.RestShop,
                    "Rest Shop 1",
                    string.Empty,
                    new[] { 1 }),
                new RuntimeMapNode(
                    5302,
                    "playmode_rest_shop_02",
                    2,
                    InGameNodeType.RestShop,
                    "Rest Shop 2",
                    string.Empty,
                    Array.Empty<int>())
            };
        }

        private static RunSaveData CloneSaveData(RunSaveData source)
        {
            return new RunSaveData
            {
                RunProfileId = source.RunProfileId,
                PlayerMaxHp = source.PlayerMaxHp,
                PlayerHp = source.PlayerHp,
                PlayerEnergy = source.PlayerEnergy,
                MaxPotionCount = source.MaxPotionCount,
                Gold = source.Gold,
                CurrentNodeIndex = source.CurrentNodeIndex,
                CurrentPage = source.CurrentPage,
                MasterSeed = source.MasterSeed,
                MapSeed = source.MapSeed,
                MapLayoutVersion = source.MapLayoutVersion,
                RandomCounter = source.RandomCounter,
                DeckCardIds = new List<int>(source.DeckCardIds),
                MapRouteNodeIndices = new List<int>(source.MapRouteNodeIndices),
                OwnedRelicIds = new List<int>(source.OwnedRelicIds),
                OwnedPotionIds = new List<int>(source.OwnedPotionIds),
                ActivatedRelicEffectIdsThisRun = new List<int>(source.ActivatedRelicEffectIdsThisRun),
                ShopItems = new List<SaveShopItem>(source.ShopItems),
                IsCardRemovalSoldOut = source.IsCardRemovalSoldOut,
                CardRemovalCount = source.CardRemovalCount,
                RestShopFreeUpgradeCount = source.RestShopFreeUpgradeCount
            };
        }

        private static string BuildLineupSignature(BattleSceneSnapshot snapshot)
        {
            return string.Join(
                "|",
                snapshot.Shop.ShopItems.Select(item => $"{item.SlotIndex}:{item.RewardType}:{item.ItemId}:{item.Price}"));
        }

        private static void AssertOwnedRelicIsNotOffered(BattleSceneSnapshot snapshot)
        {
            BattleShopItemViewModel[] relicItems = snapshot.Shop.ShopItems
                .Where(item => item.RewardType == RewardType.Relic)
                .ToArray();

            Assert.That(relicItems, Is.Not.Empty, "所持済み除外を確認できるRelic枠が必要である。");
            string refiningPrismItemId = RefiningPrismId.ToString(CultureInfo.InvariantCulture);
            Assert.That(relicItems.Select(item => item.ItemId), Does.Not.Contain(refiningPrismItemId));
            Assert.That(relicItems.Select(item => item.ItemId).Distinct().Count(), Is.EqualTo(relicItems.Length));
        }
    }
}
