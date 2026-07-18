using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using Dungeon.Runtime.InGame.Save.Model;
using Dungeon.Tests.PlayMode.Support;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Dungeon.Tests.PlayMode
{
    /// <summary>
    /// 実シーンを跨ぐセーブ継続の結合テストクラス
    /// </summary>
    public sealed class SaveContinuePlayModeTests
    {
        private const int RefiningPrismId = 5;
        private const int PrismBlastPotionId = 7;
        private const int HealRelicEffectId = 30005;
        private const int FreeUpgradeRelicEffectId = 30006;
        private const int SceneStateFrameLimit = 300;

        private BattleScenePlayModeHarness _harness;

        [UnityTest]
        public IEnumerator SaveContinue_RestoresOwnedContentAndRestShopVisitAcrossSceneReload()
        {
            _harness = new BattleScenePlayModeHarness(
                CreateRestShopNodes(),
                Enumerable.Range(0, 64).ToArray());

            yield return _harness.LoadAsync();
            yield return WaitForInitialSave();
            yield return WaitForSceneObject("MapPage");

            RunSaveData restShopReadySave = CloneSaveData(_harness.SavedRun);
            PrepareRestShopEntry(restShopReadySave);
            _harness.FlowService.InitializeFromSave(restShopReadySave);
            _harness.FlowService.SelectMapNode(0);

            BattleSceneSnapshot beforeReloadSnapshot = _harness.QueryService.CreateSnapshot();
            RunSaveData checkpoint = CloneSaveData(_harness.SavedRun);
            string expectedLineup = BuildLineupSignature(beforeReloadSnapshot);

            Assert.That(checkpoint.CurrentPage, Is.EqualTo((int)BattleScenePage.RestShop));
            Assert.That(checkpoint.OwnedRelicIds, Is.EquivalentTo(new[] { RefiningPrismId }));
            Assert.That(checkpoint.OwnedPotionIds, Is.EquivalentTo(new[] { PrismBlastPotionId }));
            Assert.That(
                checkpoint.ActivatedRelicEffectIdsThisRun,
                Is.EquivalentTo(new[] { HealRelicEffectId, FreeUpgradeRelicEffectId }));
            Assert.That(checkpoint.RestShopFreeUpgradeCount, Is.EqualTo(1));
            Assert.That(expectedLineup, Is.Not.Empty);

            yield return _harness.UnloadAsync();
            yield return _harness.LoadAsync();
            yield return WaitForRestoredRestShop(expectedLineup);
            yield return WaitForSceneObject("RestShopDialog");

            BattleSceneSnapshot restoredSnapshot = _harness.QueryService.CreateSnapshot();
            Assert.That(restoredSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.RestShop));
            Assert.That(restoredSnapshot.HostChrome.OwnedRelics, Has.Count.EqualTo(1));
            Assert.That(restoredSnapshot.HostChrome.OwnedPotions, Has.Count.EqualTo(1));
            Assert.That(BuildLineupSignature(restoredSnapshot), Is.EqualTo(expectedLineup));

            _harness.FlowService.ApplyUpgrade();
            IReadOnlyDictionary<int, int> upgradePrices = _harness.FlowService.GetCardSelectPrices();
            Assert.That(upgradePrices, Is.Not.Empty, "復元後に強化可能なカード候補が必要である。");
            Assert.That(upgradePrices.Values, Is.All.EqualTo(0), "無料強化回数を復元する必要がある。");
            _harness.FlowService.CancelCardSelect();

            ClickSceneButton("RestShopContinueButton");
            yield return WaitForContinuedCheckpoint();
            yield return WaitForSceneObject("MapPage");
            RunSaveData continuedCheckpoint = _harness.SavedRun;

            Assert.That(continuedCheckpoint, Is.Not.Null);
            Assert.That(continuedCheckpoint.OwnedRelicIds, Is.EquivalentTo(checkpoint.OwnedRelicIds));
            Assert.That(continuedCheckpoint.OwnedPotionIds, Is.EquivalentTo(checkpoint.OwnedPotionIds));
            Assert.That(
                continuedCheckpoint.ActivatedRelicEffectIdsThisRun,
                Is.EquivalentTo(checkpoint.ActivatedRelicEffectIdsThisRun));
            Assert.That(continuedCheckpoint.RestShopFreeUpgradeCount, Is.Zero);
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
            while (_harness.SavedRun == null && waitedFrames++ < SceneStateFrameLimit)
            {
                yield return null;
            }

            Assert.That(_harness.SavedRun, Is.Not.Null, "BattleScene初期checkpointが生成されなかった。");
        }

        private IEnumerator WaitForRestoredRestShop(string expectedLineup)
        {
            int waitedFrames = 0;
            while (waitedFrames++ < SceneStateFrameLimit)
            {
                BattleSceneSnapshot snapshot = _harness.QueryService.CreateSnapshot();
                if (snapshot.CurrentPage == BattleScenePage.RestShop
                    && BuildLineupSignature(snapshot) == expectedLineup)
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail("BattleScene再ロード後にRestShop checkpointが復元されなかった。");
        }

        private IEnumerator WaitForContinuedCheckpoint()
        {
            int waitedFrames = 0;
            while ((_harness.SavedRun == null
                    || _harness.SavedRun.CurrentPage != (int)BattleScenePage.Map)
                   && waitedFrames++ < SceneStateFrameLimit)
            {
                yield return null;
            }

            Assert.That(_harness.SavedRun, Is.Not.Null, "RestShop継続後のcheckpointが生成されなかった。");
            Assert.That(_harness.SavedRun.CurrentPage, Is.EqualTo((int)BattleScenePage.Map));
        }

        private static IEnumerator WaitForSceneObject(string objectName)
        {
            int waitedFrames = 0;
            GameObject sceneObject = FindSceneObject(objectName);
            while (sceneObject == null && waitedFrames++ < SceneStateFrameLimit)
            {
                yield return null;
                sceneObject = FindSceneObject(objectName);
            }

            Assert.That(sceneObject, Is.Not.Null, $"{objectName}の初期化が完了しなかった。");
            CanvasGroup canvasGroup = sceneObject.GetComponent<CanvasGroup>();
            Assert.That(canvasGroup, Is.Not.Null, $"{objectName}にCanvasGroupが設定されていない。");

            waitedFrames = 0;
            while (!canvasGroup.interactable && waitedFrames++ < SceneStateFrameLimit)
            {
                yield return null;
            }

            Assert.That(canvasGroup.interactable, Is.True, $"{objectName}の表示遷移が完了しなかった。");
        }

        private static GameObject FindSceneObject(string objectName)
        {
            GameObject[] sceneObjects = UnityEngine.Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            return sceneObjects.FirstOrDefault(candidate =>
                candidate.activeInHierarchy
                && (candidate.name == objectName || candidate.name == $"{objectName}(Clone)"));
        }

        private static void ClickSceneButton(string objectName)
        {
            GameObject buttonObject = FindSceneObject(objectName);
            Assert.That(buttonObject, Is.Not.Null, $"{objectName}が見つからなかった。");

            Component button = buttonObject.GetComponent("Button");
            Assert.That(button, Is.Not.Null, $"{objectName}にButtonが設定されていない。");

            object clickEvent = button.GetType().GetProperty("onClick")?.GetValue(button);
            System.Reflection.MethodInfo invokeMethod = clickEvent?.GetType().GetMethod("Invoke", Type.EmptyTypes);
            Assert.That(invokeMethod, Is.Not.Null, $"{objectName}のclick eventを取得できなかった。");
            invokeMethod.Invoke(clickEvent, null);
        }

        private static IReadOnlyList<RuntimeMapNode> CreateRestShopNodes()
        {
            return new[]
            {
                new RuntimeMapNode(
                    5401,
                    "playmode_save_continue_rest_shop",
                    1,
                    InGameNodeType.RestShop,
                    "Rest Shop",
                    string.Empty,
                    Array.Empty<int>())
            };
        }

        private static void PrepareRestShopEntry(RunSaveData saveData)
        {
            saveData.CurrentNodeIndex = -1;
            saveData.CurrentPage = (int)BattleScenePage.Map;
            saveData.PlayerHp = Math.Max(1, saveData.PlayerMaxHp - 10);
            saveData.MapRouteNodeIndices.Clear();
            saveData.ShopItems.Clear();
            saveData.OwnedRelicIds.Clear();
            saveData.OwnedRelicIds.Add(RefiningPrismId);
            saveData.OwnedPotionIds.Clear();
            saveData.OwnedPotionIds.Add(PrismBlastPotionId);
            saveData.ActivatedRelicEffectIdsThisRun.Clear();
            saveData.RestShopFreeUpgradeCount = 0;
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
    }
}
