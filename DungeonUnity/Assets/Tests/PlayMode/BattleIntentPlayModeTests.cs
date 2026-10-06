using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using Dungeon.Runtime.InGame.Save.Model;
using Dungeon.Runtime.InGame.Battle.View;
using Dungeon.Tests.PlayMode.Support;
using Game.MasterData.Generated;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Dungeon.Tests.PlayMode
{
    /// <summary>
    /// 実シーンで敵行動予告と解決結果の一致を確認するクラス
    /// </summary>
    public sealed class BattleIntentPlayModeTests
    {
        private const int SceneInitializationFrameLimit = 300;
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
        public IEnumerator EnemyIntentBeforeTurn_MatchesDamageAndNextTurnRemainsPlanned()
        {
            _harness = new BattleScenePlayModeHarness(
                new[] { CreateNode(9501, InGameNodeType.Battle) },
                Enumerable.Range(0, 64).ToArray());
            yield return _harness.LoadAsync();

            int waitedFrames = 0;
            while (_harness.SavedRun == null && waitedFrames++ < SceneInitializationFrameLimit)
            {
                yield return null;
            }

            Assert.That(_harness.SavedRun, Is.Not.Null);
            ClickFirstAvailableMapNode();
            yield return WaitForDisplayedIntent();

            BattleSceneSnapshot openingSnapshot = _harness.QueryService.CreateSnapshot();
            Assert.That(openingSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.Battle));
            Assert.That(openingSnapshot.Combat.Enemies, Is.Not.Empty);
            AssertVisibleIntent(openingSnapshot.Combat.EnemyIntent);
            int expectedFirstTurnDamage = CalculateExpectedHpLoss(openingSnapshot.Combat);
            int hpBeforeFirstTurn = openingSnapshot.Combat.PlayerHp;

            ClickEndTurn();
            yield return WaitForDisplayedIntent();

            BattleSceneSnapshot nextTurnSnapshot = _harness.QueryService.CreateSnapshot();
            Assert.That(nextTurnSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.Battle));
            Assert.That(nextTurnSnapshot.Combat.PlayerHp, Is.EqualTo(hpBeforeFirstTurn - expectedFirstTurnDamage));
            Assert.That(nextTurnSnapshot.Combat.Enemies.Where(enemy => !enemy.IsDefeated).All(enemy => enemy.Intent != null), Is.True);
            AssertVisibleIntent(nextTurnSnapshot.Combat.EnemyIntent);

            int expectedSecondTurnDamage = CalculateExpectedHpLoss(nextTurnSnapshot.Combat);
            int hpBeforeSecondTurn = nextTurnSnapshot.Combat.PlayerHp;
            ClickEndTurn();
            yield return WaitForDisplayedIntent();

            BattleSceneSnapshot followingTurnSnapshot = _harness.QueryService.CreateSnapshot();
            Assert.That(followingTurnSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.Battle));
            Assert.That(
                followingTurnSnapshot.Combat.PlayerHp,
                Is.EqualTo(hpBeforeSecondTurn - expectedSecondTurnDamage));
            Assert.That(
                followingTurnSnapshot.Combat.Enemies.Where(enemy => !enemy.IsDefeated).All(enemy => enemy.Intent != null),
                Is.True);
            AssertVisibleIntent(followingTurnSnapshot.Combat.EnemyIntent);
        }

        [UnityTest]
        public IEnumerator BattleVictory_ContinueStartsBattleWithFreshEnemyPlans()
        {
            _harness = new BattleScenePlayModeHarness(
                new[]
                {
                    CreateNode(9501, InGameNodeType.Battle, 1, 1),
                    CreateNode(9502, InGameNodeType.Battle, 2)
                },
                Enumerable.Range(0, 256).ToArray());
            yield return _harness.LoadAsync();
            yield return WaitForInitialSave();

            RunSaveData durableCheckpoint = _harness.SavedRun;
            durableCheckpoint.PlayerMaxHp = 5000;
            durableCheckpoint.PlayerHp = 5000;
            _harness.FlowService.InitializeFromSave(durableCheckpoint);
            _harness.FlowService.SelectMapNode(0);
            RuntimeEnemy firstEnemy = _harness.QueryService.CreateSnapshot().Combat.CurrentEnemy;

            int battleTurns = 0;
            while (_harness.QueryService.CreateSnapshot().CurrentPage == BattleScenePage.Battle
                   && battleTurns++ < 150)
            {
                PlayAffordableAttackCards();
                if (_harness.QueryService.CreateSnapshot().CurrentPage == BattleScenePage.Battle)
                {
                    _harness.FlowService.EndTurn();
                }
            }

            BattleSceneSnapshot rewardSnapshot = _harness.QueryService.CreateSnapshot();
            Assert.That(battleTurns, Is.LessThan(150), "実シーンの敵を所定ターン内に倒せなかった。");
            Assert.That(rewardSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.Reward));
            Assert.That(rewardSnapshot.Combat.Enemies.All(enemy => enemy.Intent == null), Is.True);

            _harness.FlowService.ContinueFromReward();
            Assert.That(_harness.QueryService.CreateSnapshot().CurrentPage, Is.EqualTo(BattleScenePage.Map));
            _harness.FlowService.SelectMapNode(1);
            BattleSceneSnapshot nextBattleSnapshot = _harness.QueryService.CreateSnapshot();
            int randomCounterAfterNextBattlePreparation = _harness.RandomCounter;
            BattleSceneSnapshot repeatedNextBattleSnapshot = _harness.QueryService.CreateSnapshot();

            Assert.That(nextBattleSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.Battle));
            Assert.That(nextBattleSnapshot.Combat.CurrentEnemy, Is.Not.SameAs(firstEnemy));
            Assert.That(
                nextBattleSnapshot.Combat.Enemies.Where(enemy => !enemy.IsDefeated).All(enemy => enemy.Intent != null),
                Is.True);
            Assert.That(
                repeatedNextBattleSnapshot.Combat.Enemies.Select(enemy => enemy.Intent?.ActionOrder),
                Is.EqualTo(nextBattleSnapshot.Combat.Enemies.Select(enemy => enemy.Intent?.ActionOrder)));
            Assert.That(_harness.RandomCounter, Is.EqualTo(randomCounterAfterNextBattlePreparation));
        }

        [UnityTest]
        public IEnumerator BattleDefeat_EntersResultAndDoesNotResolveAnotherEnemyPlan()
        {
            _harness = new BattleScenePlayModeHarness(
                new[] { CreateNode(9501, InGameNodeType.Battle) },
                Enumerable.Range(0, 256).ToArray());
            yield return _harness.LoadAsync();
            yield return WaitForInitialSave();

            RunSaveData fragileCheckpoint = _harness.SavedRun;
            fragileCheckpoint.PlayerHp = 1;
            _harness.FlowService.InitializeFromSave(fragileCheckpoint);
            _harness.FlowService.SelectMapNode(0);

            int enemyTurns = 0;
            while (_harness.QueryService.CreateSnapshot().CurrentPage == BattleScenePage.Battle
                   && enemyTurns++ < 100)
            {
                _harness.FlowService.EndTurn();
            }

            BattleSceneSnapshot resultSnapshot = _harness.QueryService.CreateSnapshot();
            int randomCounterAfterDefeat = _harness.RandomCounter;
            Assert.That(enemyTurns, Is.LessThan(100), "実シーンでプレイヤーが敗北しなかった。");
            Assert.That(resultSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.Result));
            Assert.That(resultSnapshot.Combat.Enemies.All(enemy => enemy.Intent == null), Is.True);

            _harness.FlowService.EndTurn();
            BattleSceneSnapshot afterRejectedTurnSnapshot = _harness.QueryService.CreateSnapshot();

            Assert.That(afterRejectedTurnSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.Result));
            Assert.That(_harness.RandomCounter, Is.EqualTo(randomCounterAfterDefeat));
        }

        private IEnumerator WaitForInitialSave()
        {
            int waitedFrames = 0;
            while (_harness.SavedRun == null && waitedFrames++ < SceneInitializationFrameLimit)
            {
                yield return null;
            }

            Assert.That(_harness.SavedRun, Is.Not.Null);
        }

        private void PlayAffordableAttackCards()
        {
            for (int cardCountGuard = 0; cardCountGuard < 12; cardCountGuard++)
            {
                BattleSceneSnapshot snapshot = _harness.QueryService.CreateSnapshot();
                if (snapshot.CurrentPage != BattleScenePage.Battle)
                {
                    return;
                }

                int selectedCardIndex = -1;
                for (int handIndex = snapshot.Combat.HandCards.Count - 1; handIndex >= 0; handIndex--)
                {
                    RuntimeCard card = snapshot.Combat.HandCards[handIndex].Card;
                    if (card.Cost <= snapshot.Combat.PlayerEnergy
                        && card.Effects.Any(effect => effect.EffectType == EffectType.DealDamage && effect.Value > 0))
                    {
                        selectedCardIndex = handIndex;
                        break;
                    }
                }

                if (selectedCardIndex < 0)
                {
                    return;
                }

                _harness.FlowService.SelectHandCard(selectedCardIndex);
                _harness.FlowService.TryPlaySelectedCard();
            }
        }

        private IEnumerator WaitForDisplayedIntent()
        {
            int waitedFrames = 0;
            while (waitedFrames++ < SceneInitializationFrameLimit)
            {
                BattleSceneSnapshot snapshot = _harness.QueryService.CreateSnapshot();
                BattleIntentViewModel intent = snapshot.Combat.EnemyIntent;
                string displayedIntent = GetDisplayedIntentText();
                string displayedPlayerSummary = GetDisplayedPlayerSummaryText();
                if (snapshot.CurrentPage == BattleScenePage.Battle
                    && intent != null
                    && !string.IsNullOrEmpty(displayedIntent)
                    && displayedIntent.Contains(intent.IntentName)
                    && (intent.Damage <= 0 || displayedIntent.Contains(intent.Damage.ToString()))
                    && displayedPlayerSummary.Contains($"Player HP {snapshot.Combat.PlayerHp}/{snapshot.Combat.PlayerMaxHp}"))
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail("BattleSceneのIntent表示が最新Snapshotへ更新されなかった。");
        }

        private static void ClickFirstAvailableMapNode()
        {
            GameObject mapPage = GameObject.Find("MapPage") ?? GameObject.Find("MapPage(Clone)");
            Assert.That(mapPage, Is.Not.Null, "MapPageが表示されていない。");
            BattleOptionButtonView nodeButtonView = mapPage
                .GetComponentsInChildren<BattleOptionButtonView>(true)
                .FirstOrDefault(view => view.gameObject.activeInHierarchy);
            Assert.That(nodeButtonView, Is.Not.Null, "利用可能なマップノードボタンがない。");

            Button button = nodeButtonView.GetComponent<Button>();
            Assert.That(button, Is.Not.Null);
            Assert.That(button.interactable, Is.True);
            button.onClick.Invoke();
        }

        private static void ClickEndTurn()
        {
            GameObject buttonObject = GameObject.Find("EndTurnButton");
            Assert.That(buttonObject, Is.Not.Null, "EndTurnButtonが表示されていない。");
            Button button = buttonObject.GetComponent<Button>();
            Assert.That(button, Is.Not.Null);
            Assert.That(button.interactable, Is.True);
            button.onClick.Invoke();
        }

        private static TComponent FindSceneComponent<TComponent>() where TComponent : Component
        {
            return UnityEngine.Object.FindObjectsByType<TComponent>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None)
                .FirstOrDefault(component => component.gameObject.activeInHierarchy);
        }

        private static string GetDisplayedIntentText()
        {
            return GetDisplayedBattlePageText("_intentText");
        }

        private static string GetDisplayedPlayerSummaryText()
        {
            return GetDisplayedBattlePageText("_playerSummaryText");
        }

        private static string GetDisplayedBattlePageText(string fieldName)
        {
            BattlePageView view = FindSceneComponent<BattlePageView>();
            if (view == null)
            {
                return string.Empty;
            }

            System.Reflection.FieldInfo field = typeof(BattlePageView).GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            object component = field?.GetValue(view);
            System.Reflection.PropertyInfo textProperty = component?.GetType().GetProperty("text");
            return textProperty?.GetValue(component) as string ?? string.Empty;
        }

        private static void AssertVisibleIntent(BattleIntentViewModel intent)
        {
            Assert.That(intent, Is.Not.Null);
            string displayedIntent = GetDisplayedIntentText();
            Assert.That(displayedIntent, Does.Contain(intent.IntentName));
            if (intent.Damage > 0)
            {
                Assert.That(displayedIntent, Does.Contain(intent.Damage.ToString()));
                Assert.That(displayedIntent, Does.Contain(Math.Max(1, intent.HitCount).ToString()));
            }
        }

        private static int CalculateExpectedHpLoss(BattleCombatSnapshot combat)
        {
            int playerBlock = combat.PlayerBlock;
            int totalHpLoss = 0;
            IEnumerable<BattleEnemyViewModel> orderedEnemies = combat.Enemies
                .Where(enemy => !enemy.IsDefeated)
                .OrderBy(enemy => enemy.SlotIndex);
            foreach (BattleEnemyViewModel enemy in orderedEnemies)
            {
                BattleIntentViewModel intent = enemy.Intent;
                Assert.That(intent, Is.Not.Null);
                int hitCount = Math.Max(1, intent.HitCount);
                for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
                {
                    int damage = Math.Max(0, intent.Damage);
                    totalHpLoss += Math.Max(0, damage - playerBlock);
                    playerBlock = Math.Max(0, playerBlock - damage);
                }
            }

            return totalHpLoss;
        }

        private static RuntimeMapNode CreateNode(
            int id,
            InGameNodeType nodeType,
            int floor = 1,
            int nextNodeIndex = -1)
        {
            return new RuntimeMapNode(
                id,
                $"node_{id}",
                floor,
                nodeType,
                $"Node {id}",
                string.Empty,
                nextNodeIndex >= 0 ? new[] { nextNodeIndex } : Array.Empty<int>());
        }
    }
}
