using System;
using System.Collections;
using System.Collections.Generic;
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
    public sealed class PotionContentPlayModeTests
    {
        private const int ShardBombPotionId = 5;
        private const int FractureAmpoulePotionId = 6;
        private const int PrismBlastPotionId = 7;
        private const int SceneInitializationTimeoutFrames = 300;

        private BattleScenePlayModeHarness _harness;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_harness == null)
            {
                yield break;
            }

            yield return _harness.UnloadAsync();
            _harness = null;
        }

        [UnityTest]
        public IEnumerator PurchaseShopItem_ShardBomb_AddsNewPotionToInventory()
        {
            yield return LoadHarnessAsync(
                new[]
                {
                    CreateNode(9301, InGameNodeType.RestShop),
                },
                new[] { 4, 0 });

            _harness.FlowService.SelectMapNode(0);
            _harness.FlowService.OpenShop();

            BattleSceneSnapshot shopSnapshot = _harness.QueryService.CreateSnapshot();
            BattleShopItemViewModel shardBomb = shopSnapshot.Shop.ShopItems.First(item =>
                item.RewardType == RewardType.Potion && item.ItemId == ShardBombPotionId);
            RuntimePotionEffect shardBombEffect = shardBomb.Potion.Effects.Single();

            Assert.That(shardBomb.Potion.Key, Is.EqualTo("potion_shard_bomb"));
            Assert.That(shardBombEffect.EffectType, Is.EqualTo(EffectType.DealDamage));
            Assert.That(shardBombEffect.Value, Is.EqualTo(15));
            Assert.That(shardBombEffect.TargetSide, Is.EqualTo(TargetSide.Enemy));

            _harness.FlowService.PurchaseShopItem(shardBomb.SlotIndex);

            BattleSceneSnapshot purchasedSnapshot = _harness.QueryService.CreateSnapshot();
            Assert.That(_harness.SavedRun.OwnedPotionIds, Is.EquivalentTo(new[] { ShardBombPotionId }));
            Assert.That(purchasedSnapshot.HostChrome.OwnedPotions, Has.Count.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator UseShardBomb_DamagesOnlySelectedEnemy()
        {
            yield return LoadBattleWithPotionAsync(ShardBombPotionId);

            BattleSceneSnapshot beforeUse = _harness.QueryService.CreateSnapshot();
            _harness.FlowService.UsePotion(0);
            BattleSceneSnapshot awaitingTarget = _harness.QueryService.CreateSnapshot();

            Assert.That(awaitingTarget.Combat.Enemies.Select(enemy => enemy.Hp),
                Is.EqualTo(beforeUse.Combat.Enemies.Select(enemy => enemy.Hp)));

            _harness.FlowService.SelectEnemyTarget(1);

            BattleSceneSnapshot afterUse = _harness.QueryService.CreateSnapshot();
            Assert.That(afterUse.Combat.Enemies[0].Hp, Is.EqualTo(beforeUse.Combat.Enemies[0].Hp));
            Assert.That(afterUse.Combat.Enemies[1].Hp, Is.EqualTo(beforeUse.Combat.Enemies[1].Hp - 15));
            Assert.That(afterUse.HostChrome.OwnedPotions, Is.Empty);
        }

        [UnityTest]
        public IEnumerator UseFractureAmpoule_AppliesVulnerableOnlyToSelectedEnemy()
        {
            yield return LoadBattleWithPotionAsync(FractureAmpoulePotionId);

            _harness.FlowService.UsePotion(0);
            _harness.FlowService.SelectEnemyTarget(1);

            BattleSceneSnapshot afterUse = _harness.QueryService.CreateSnapshot();
            Assert.That(afterUse.Combat.Enemies[0].Statuses, Is.Empty);
            Assert.That(afterUse.Combat.Enemies[1].Statuses, Has.Count.EqualTo(1));
            Assert.That(afterUse.Combat.Enemies[1].Statuses[0].Value, Is.EqualTo(2));
            Assert.That(afterUse.HostChrome.OwnedPotions, Is.Empty);
        }

        [UnityTest]
        public IEnumerator UsePrismBlast_DamagesAllEnemies()
        {
            yield return LoadBattleWithPotionAsync(PrismBlastPotionId);

            BattleSceneSnapshot beforeUse = _harness.QueryService.CreateSnapshot();
            _harness.FlowService.UsePotion(0);

            BattleSceneSnapshot afterUse = _harness.QueryService.CreateSnapshot();
            Assert.That(afterUse.Combat.Enemies, Has.Count.EqualTo(2));
            for (int index = 0; index < afterUse.Combat.Enemies.Count; index++)
            {
                Assert.That(afterUse.Combat.Enemies[index].Hp,
                    Is.EqualTo(beforeUse.Combat.Enemies[index].Hp - 8));
            }

            Assert.That(afterUse.HostChrome.OwnedPotions, Is.Empty);
        }

        private IEnumerator LoadBattleWithPotionAsync(int potionId)
        {
            yield return LoadHarnessAsync(
                new[]
                {
                    CreateNode(9401, InGameNodeType.Battle),
                },
                new[] { 40, 5, 6, 0, 0, 0 });

            RunSaveData saveData = _harness.SavedRun;
            saveData.CurrentNodeIndex = -1;
            saveData.CurrentPage = (int)BattleScenePage.Map;
            saveData.RandomCounter = 0;
            saveData.MapRouteNodeIndices.Clear();
            saveData.ShopItems.Clear();
            saveData.OwnedPotionIds.Clear();
            saveData.OwnedPotionIds.Add(potionId);

            _harness.FlowService.InitializeFromSave(saveData);
            _harness.FlowService.SelectMapNode(0);

            BattleSceneSnapshot battleSnapshot = _harness.QueryService.CreateSnapshot();
            Assert.That(battleSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.Battle));
            Assert.That(battleSnapshot.Combat.Enemies, Has.Count.EqualTo(2));
            Assert.That(battleSnapshot.HostChrome.OwnedPotions, Has.Count.EqualTo(1));
        }

        private IEnumerator LoadHarnessAsync(
            IReadOnlyList<RuntimeMapNode> mapNodes,
            IReadOnlyList<int> randomValues)
        {
            _harness = new BattleScenePlayModeHarness(mapNodes, randomValues);
            yield return _harness.LoadAsync();

            int elapsedFrames = 0;
            while (_harness.SavedRun == null && elapsedFrames < SceneInitializationTimeoutFrames)
            {
                elapsedFrames++;
                yield return null;
            }

            Assert.That(_harness.SavedRun, Is.Not.Null, "BattleScene の初期化が完了しませんでした。");
        }

        private static RuntimeMapNode CreateNode(int id, InGameNodeType nodeType)
        {
            return new RuntimeMapNode(
                id,
                $"node_{id}",
                1,
                nodeType,
                $"Node {id}",
                string.Empty,
                Array.Empty<int>());
        }
    }
}
