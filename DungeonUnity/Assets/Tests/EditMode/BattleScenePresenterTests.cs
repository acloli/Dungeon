using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Battle.View;
using Dungeon.Runtime.InGame.Save.Model;
using Dungeon.Runtime.InGame.Save.Services;
using Dungeon.Tests.EditMode.Support;
using Game.MasterData.Generated;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleScenePresenterのEditorモードテストクラス
    /// </summary>
    [TestFixture]
    public sealed class BattleScenePresenterTests
    {
        [Test]
        public void InitializeAsync_MapState_ShowsMapThroughCoordinator()
        {
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(CreateSnapshot(BattleScenePage.Map));
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(uiCoordinator.InitializeCallCount, Is.EqualTo(1));
            Assert.That(uiCoordinator.ShowMapCallCount, Is.EqualTo(1));
            Assert.That(uiCoordinator.LastMapSnapshot, Is.Not.Null);
            Assert.That(view.BattlePageView.BuildCallCount, Is.EqualTo(0));
        }

        [Test]
        public void OnEndTurnClicked_BattleState_RendersBattleBase()
        {
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(CreateSnapshot(BattleScenePage.Battle));
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();
            presenter.OnEndTurnClicked();

            Assert.That(flowService.EndTurnCallCount, Is.EqualTo(1));
            Assert.That(uiCoordinator.ShowBattleCallCount, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void OnHandCardClicked_PlaysSelectedCard()
        {
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(CreateSnapshot(BattleScenePage.Battle));
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();
            presenter.OnHandCardClicked(0);

            Assert.That(flowService.SelectHandCardCallCount, Is.EqualTo(1));
            Assert.That(flowService.TryPlaySelectedCardCallCount, Is.EqualTo(1));
        }



        [Test]
        public void InitializeAsync_BattleState_RendersIntentStatusAndBuffText()
        {
            BattleSceneSnapshot snapshot = CreateBattleSnapshotWithDisplayInfo();
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(snapshot);
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(view.BattlePageView.LastPlayerText, Does.Contain("Status: 脆弱:2"));
            Assert.That(view.BattlePageView.LastPlayerText, Does.Contain("Buff: 筋力:1"));
            Assert.That(view.BattlePageView.LastEnemyText, Does.Contain("Intent: 攻防"));
            Assert.That(view.BattlePageView.LastEnemyText, Does.Contain("D7x2"));
            Assert.That(view.BattlePageView.LastEnemyText, Does.Contain("B5"));
            Assert.That(view.BattlePageView.LastEnemyText, Does.Contain("Status 脱力:2"));
            Assert.That(view.BattlePageView.LastEnemyText, Does.Contain("Buff 儀式:3"));
            Assert.That(view.BattlePageView.LastEnemyText, Does.Contain("Status: 脱力:1"));
            Assert.That(view.BattlePageView.LastEnemyText, Does.Contain("Buff: 儀式:3"));
            Assert.That(view.BattlePageView.LastHud, Is.Not.Null);
            Assert.That(view.BattlePageView.LastHud.IntentSummary, Does.Contain("Intent: 攻防"));
            Assert.That(view.BattlePageView.LastHud.IntentSummary, Does.Contain("D7x2"));
            Assert.That(view.BattlePageView.LastHud.PlayerStatuses[0].Name, Is.EqualTo("脆弱"));
            Assert.That(view.BattlePageView.LastHud.PlayerBuffs[0].Name, Is.EqualTo("筋力"));
            Assert.That(view.BattlePageView.LastHud.EnemyStatuses[0].Name, Is.EqualTo("脱力"));
            Assert.That(view.BattlePageView.LastHud.EnemyBuffs[0].Name, Is.EqualTo("儀式"));
        }

        [Test]
        public void InitializeAsync_MultiEnemyBattleState_RendersSelectedEnemyTextOnly()
        {
            BattleSceneSnapshot snapshot = CreateMultiEnemySnapshot();
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(snapshot);
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(view.BattlePageView.LastEnemyText, Does.Contain("Green Mite"));
            Assert.That(view.BattlePageView.LastEnemyText, Does.Not.Contain("Red Mite"));
            Assert.That(view.BattlePageView.LastHud.EnemySummary, Does.Contain("Green Mite"));
            Assert.That(view.BattlePageView.LastHud.EnemySummary, Does.Not.Contain("Red Mite"));
            Assert.That(view.BattlePageView.LastEnemyButtonCount, Is.EqualTo(2));
        }

        [Test]
        public void InitializeAsync_BattleState_RendersHandCardsAndPileCounters()
        {
            BattleSceneSnapshotBuilder snapshotBuilder = BattleTestData.Snapshot(BattleScenePage.Battle);
            snapshotBuilder.Combat = new BattleCombatSnapshot(
                playerMaxHp: 40,
                playerHp: 40,
                playerEnergy: 2,
                playerBlock: 0,
                gold: 100,
                battleHintMessage: "battle",
                handCards: new[]
                {
                    new BattleHandCardViewModel(
                        CreateCard(1001, "Strike", 1),
                        BattleMultiIconViewModel.CreateCard(CreateCard(1001, "Strike", 1))),
                    new BattleHandCardViewModel(
                        CreateCard(1002, "Guard", 2),
                        BattleMultiIconViewModel.CreateCard(CreateCard(1002, "Guard", 2), isAffordable: false, isSelected: true))
                },
                enemies: Array.Empty<BattleEnemyViewModel>(),
                selectedEnemyIndex: 0,
                drawPileCount: 12,
                discardPileCount: 3,
                handCount: 2,
                maxHandCount: 10,
                enemyHp: 20);
            BattleSceneSnapshot snapshot = snapshotBuilder.Build();

            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(snapshot);
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(view.BattlePageView.LastHandCards.Count, Is.EqualTo(2));
            Assert.That(view.BattlePageView.LastHandCards[1].Icon.IsSelected, Is.True);
            Assert.That(view.BattlePageView.LastHandCards[1].Icon.IsAffordable, Is.False);
            Assert.That(view.LastOwnedRelics, Is.Empty);
            Assert.That(view.BattlePageView.LastDrawPileCount, Is.EqualTo(12));
            Assert.That(view.BattlePageView.LastDiscardPileCount, Is.EqualTo(3));
            Assert.That(view.BattlePageView.LastHandCount, Is.EqualTo(2));
            Assert.That(view.BattlePageView.LastMaxHandCount, Is.EqualTo(10));
        }

        [Test]
        public void InitializeAsync_MapState_RendersOwnedRelicsThroughHostView()
        {
            BattleSceneSnapshotBuilder snapshotBuilder = BattleTestData.Snapshot(BattleScenePage.Map);
            snapshotBuilder.HostChrome = new BattleHostChromeSnapshot(
                ownedRelics: new[]
                {
                    new BattleMultiIconViewModel(BattleIconKind.Relic, "Burning Core", "Gain 6 Block at combat start.", "relic_1", CardRarity.Uncommon)
                },
                selectedOwnedRelicIndex: 0,
                ownedRelicHintMessage: "Burning Core\nGain 6 Block at combat start.");
            BattleSceneSnapshot snapshot = snapshotBuilder.Build();
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(snapshot);
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(view.LastOwnedRelics.Count, Is.EqualTo(1));
            Assert.That(view.LastOwnedRelics[0].DisplayName, Is.EqualTo("Burning Core"));
            Assert.That(view.LastSelectedOwnedRelicIndex, Is.EqualTo(0));
            Assert.That(view.LastOwnedRelicHint, Does.Contain("Gain 6 Block"));
        }

        [Test]
        public void InitializeAsync_CardSelectState_ConfirmRoutesThroughGenericCardSelectFlow()
        {
            RuntimeCard strike = CreateCard(1001, "Strike", 1);
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(CreateSnapshot(BattleScenePage.CardSelect))
            {
                CardSelectMode = CardSelectMode.Upgrade,
                CardSelectCards = new[] { strike },
                CardSelectPrices = new Dictionary<int, int> { { strike.Id, 25 } },
                CardSelectMessage = string.Empty
            };
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator
            {
                CardToSelectBeforeResult = strike,
                CardSelectResult = new CardSelectDialogResult { IsCanceled = true }
            };
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(flowService.ConfirmCardSelectCallCount, Is.EqualTo(1));
            Assert.That(flowService.CancelCardSelectCallCount, Is.EqualTo(1));
            Assert.That(uiCoordinator.LastCardSelectMode, Is.EqualTo(CardSelectMode.Upgrade));
            Assert.That(uiCoordinator.LastCardSelectShowPrice, Is.True);
            Assert.That(uiCoordinator.LastCardSelectPrices[strike.Id], Is.EqualTo(25));
            Assert.That(uiCoordinator.LastCardSelectCallback, Is.Not.Null);
            Assert.That(uiCoordinator.LastCardSelectRefreshData.Message, Is.EqualTo("Upgraded Strike."));
        }

        [Test]
        public void InitializeAsync_CardRemovalState_HidesCardPrices()
        {
            RuntimeCard strike = CreateCard(1001, "Strike", 1);
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(CreateSnapshot(BattleScenePage.CardSelect))
            {
                CardSelectMode = CardSelectMode.CardRemoval,
                CardSelectCards = new[] { strike },
                CardSelectPrices = new Dictionary<int, int> { { strike.Id, 75 } }
            };
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator
            {
                CardSelectResult = new CardSelectDialogResult { IsCanceled = true }
            };
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(uiCoordinator.LastCardSelectMode, Is.EqualTo(CardSelectMode.CardRemoval));
            Assert.That(uiCoordinator.LastCardSelectShowPrice, Is.False);
        }

        [Test]
        public void OnOwnedRelicClicked_UpdatesHintText()
        {
            BattleSceneSnapshotBuilder snapshotBuilder = BattleTestData.Snapshot(BattleScenePage.Battle);
            snapshotBuilder.HostChrome = new BattleHostChromeSnapshot(
                ownedRelics: new[]
                {
                    new BattleMultiIconViewModel(BattleIconKind.Relic, "Burning Core", "Gain 6 Block at combat start.", "relic_1", CardRarity.Uncommon)
                });
            BattleSceneSnapshot snapshot = snapshotBuilder.Build();
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(snapshot);
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();
            presenter.OnOwnedRelicClicked(0);

            Assert.That(flowService.InspectOwnedRelicCallCount, Is.EqualTo(1));
        }

        [Test]
        public void InitializeAsync_MapState_RendersOwnedPotionsThroughHostView()
        {
            BattleSceneSnapshotBuilder snapshotBuilder = BattleTestData.Snapshot(BattleScenePage.Map);
            snapshotBuilder.HostChrome = new BattleHostChromeSnapshot(
                ownedPotions: new[]
                {
                    new BattleMultiIconViewModel(BattleIconKind.Potion, "Fruit Juice", "Gain 5 Max HP.", "potion_fruit_juice", CardRarity.Uncommon)
                },
                selectedOwnedPotionIndex: 0,
                ownedPotionHintMessage: "Fruit Juice\nGain 5 Max HP.",
                canUseSelectedPotion: true);
            BattleSceneSnapshot snapshot = snapshotBuilder.Build();
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(snapshot);
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(view.LastOwnedPotions.Count, Is.EqualTo(1));
            Assert.That(view.LastOwnedPotions[0].DisplayName, Is.EqualTo("Fruit Juice"));
            Assert.That(view.LastSelectedOwnedPotionIndex, Is.EqualTo(0));
            Assert.That(view.LastOwnedPotionHint, Does.Contain("Gain 5 Max HP"));
            Assert.That(view.IsOwnedPotionUseVisible, Is.True);
        }

        [Test]
        public void OnUsePotionClicked_UsesPotionImmediately()
        {
            BattleSceneSnapshotBuilder readySnapshotBuilder = BattleTestData.Snapshot(BattleScenePage.Map);
            readySnapshotBuilder.HostChrome = new BattleHostChromeSnapshot(
                ownedPotions: new[]
                {
                    new BattleMultiIconViewModel(BattleIconKind.Potion, "Fruit Juice", "Gain 5 Max HP.", "potion_fruit_juice", CardRarity.Uncommon)
                },
                selectedOwnedPotionIndex: 0,
                ownedPotionHintMessage: "Fruit Juice\nGain 5 Max HP.",
                canUseSelectedPotion: true);
            BattleSceneSnapshot readySnapshot = readySnapshotBuilder.Build();
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(readySnapshot)
            {
                SnapshotAfterUsePotion = BattleScenePresenterTests.CreateSnapshot(BattleScenePage.Map)
            };
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();
            presenter.OnUsePotionClicked();
            presenter.OnUsePotionClicked();

            Assert.That(flowService.UsePotionCallCount, Is.EqualTo(1));
            Assert.That(view.IsOwnedPotionUseVisible, Is.False);
        }

        [Test]
        public void InitializeAsync_WithValidSave_InitializesFromSave()
        {
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(CreateSnapshot(BattleScenePage.Map));
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            FakeRunSaveService runSaveService = new FakeRunSaveService
            {
                HasSavedRunResult = true,
                LoadResult = CreateValidSaveData()
            };
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator, runSaveService);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(flowService.InitializeFromSaveCallCount, Is.EqualTo(1));
            Assert.That(flowService.InitializeCallCount, Is.EqualTo(0));
            Assert.That(runSaveService.DeleteCallCount, Is.EqualTo(0));
        }

        [Test]
        public void InitializeAsync_WithInvalidSave_DeletesSaveAndInitializesNewRun()
        {
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(CreateSnapshot(BattleScenePage.Map));
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            FakeRunSaveService runSaveService = new FakeRunSaveService
            {
                HasSavedRunResult = true,
                LoadResult = new RunSaveData { RunProfileId = 0 }
            };
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator, runSaveService);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(flowService.InitializeFromSaveCallCount, Is.EqualTo(0));
            Assert.That(flowService.InitializeCallCount, Is.EqualTo(1));
            Assert.That(runSaveService.DeleteCallCount, Is.EqualTo(1));
        }

        [Test]
        public void InitializeAsync_MapState_ShowsSaveQuitButton()
        {
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(CreateSnapshot(BattleScenePage.Map));
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(view.IsSaveQuitVisible, Is.True);
        }

        [Test]
        public void InitializeAsync_BattleState_HidesSaveQuitButton()
        {
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(CreateSnapshot(BattleScenePage.Battle));
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(view.IsSaveQuitVisible, Is.False);
        }

        [Test]
        public void OnSaveQuitClicked_InvokesQuitCallback()
        {
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(CreateSnapshot(BattleScenePage.Map));
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            bool called = false;
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None, () => called = true).GetAwaiter().GetResult();
            view.InvokeSaveQuit();

            Assert.That(called, Is.True);
        }

        private static BattleSceneSnapshot CreateSnapshot(BattleScenePage page)
        {
            BattleSceneSnapshotBuilder builder = BattleTestData.Snapshot(page);
            return builder.Build();
        }

        private static BattleSceneSnapshot CreateBattleSnapshotWithDisplayInfo()
        {
            BattleSceneSnapshotBuilder builder = BattleTestData.Snapshot(BattleScenePage.Battle);
            RuntimeEnemyBuilder currentEnemyBuilder = BattleTestData.Enemy(3001);
            currentEnemyBuilder.DisplayName = "Slime";
            builder.Combat = new BattleCombatSnapshot(
                playerMaxHp: 40,
                playerHp: 40,
                playerEnergy: 3,
                playerBlock: 0,
                gold: 100,
                battleHintMessage: "battle",
                handCards: Array.Empty<BattleHandCardViewModel>(),
                enemies: Array.Empty<BattleEnemyViewModel>(),
                selectedEnemyIndex: 0,
                enemyIntent: new BattleIntentViewModel(
                    IntentType.AttackDefend,
                    "攻防",
                    7,
                    2,
                    5,
                    StatusType.Weak,
                    "脱力",
                    2,
                    BuffType.Ritual,
                    "儀式",
                    3),
                playerStatuses: new[] { new BattleStatusViewModel("脆弱", 2, false) },
                enemyStatuses: new[] { new BattleStatusViewModel("脱力", 1, false) },
                playerBuffs: new[] { new BattleStatusViewModel("筋力", 1, true) },
                enemyBuffs: new[] { new BattleStatusViewModel("儀式", 3, true) },
                currentEnemy: currentEnemyBuilder.Build(),
                enemyHp: 20);
            return builder.Build();
        }

        private static BattleSceneSnapshot CreateMultiEnemySnapshot()
        {
            BattleSceneSnapshotBuilder builder = BattleTestData.Snapshot(BattleScenePage.Battle);
            builder.Combat = new BattleCombatSnapshot(
                playerMaxHp: 40,
                playerHp: 40,
                playerEnergy: 3,
                playerBlock: 0,
                gold: 100,
                battleHintMessage: "battle",
                handCards: Array.Empty<BattleHandCardViewModel>(),
                enemies: new[]
                {
                    new BattleEnemyViewModel(0, "Red Mite", 12, 0, false, null, Array.Empty<BattleStatusViewModel>(), Array.Empty<BattleStatusViewModel>()),
                    new BattleEnemyViewModel(1, "Green Mite", 8, 0, false, null, Array.Empty<BattleStatusViewModel>(), Array.Empty<BattleStatusViewModel>())
                },
                selectedEnemyIndex: 1);
            return builder.Build();
        }

        private static RunSaveData CreateValidSaveData()
        {
            return new RunSaveData
            {
                RunProfileId = 5501,
                PlayerMaxHp = 40,
                PlayerHp = 40,
                PlayerEnergy = 3,
                Gold = 100,
                CurrentNodeIndex = -1,
                CurrentPage = (int)BattleScenePage.Map,
                DeckCardIds = new List<int> { 1001 }
            };
        }

        private static RuntimeCard CreateCard(int id, string displayName, int cost)
        {
            RuntimeCardBuilder builder = BattleTestData.Card(id);
            builder.DisplayName = displayName;
            builder.Cost = cost;
            return builder.Build();
        }

        [Test]
        public void InitializeAsync_ShopState_ShowsShopThroughCoordinator()
        {
            BattleSceneSnapshot snapshot = CreateSnapshot(BattleScenePage.Shop);
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(snapshot);
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(uiCoordinator.ShowShopCallCount, Is.EqualTo(1));
            Assert.That(uiCoordinator.LastShopSnapshot, Is.Not.Null);
        }

        [Test]
        public void InitializeAsync_EventState_ShowsEventThroughCoordinator()
        {
            BattleSceneSnapshot snapshot = CreateSnapshot(BattleScenePage.Event);
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(snapshot);
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(uiCoordinator.ShowEventCallCount, Is.EqualTo(1));
            Assert.That(uiCoordinator.LastEventSnapshot, Is.Not.Null);
        }

        private sealed class FakeBattleSceneFlowService : IBattleSceneFlowService
        {
            private BattleSceneSnapshot _snapshot;

            public FakeBattleSceneFlowService(BattleSceneSnapshot snapshot)
            {
                _snapshot = snapshot;
            }

            public int EndTurnCallCount { get; private set; }
            public int SelectHandCardCallCount { get; private set; }
            public int TryPlaySelectedCardCallCount { get; private set; }
            public int InspectOwnedRelicCallCount { get; private set; }
            public int InspectOwnedPotionCallCount { get; private set; }
            public int UsePotionCallCount { get; private set; }
            public int ReplaceOwnedPotionCallCount { get; private set; }
            public int CancelPendingPotionReplaceCallCount { get; private set; }
            public int ClearOwnedInspectionsCallCount { get; private set; }
            public int ConfirmCardSelectCallCount { get; private set; }
            public int CancelCardSelectCallCount { get; private set; }
            public int InitializeCallCount { get; private set; }
            public int InitializeFromSaveCallCount { get; private set; }
            public bool DoesSelectedCardRequireEnemyTargetResult { get; set; } = true;
            public BattleSceneSnapshot SnapshotAfterUsePotion { get; set; }
            public BattleSceneSnapshot SnapshotAfterReplaceOwnedPotion { get; set; }
            public BattleSceneSnapshot SnapshotAfterCancelPendingPotionReplace { get; set; }
            public IReadOnlyList<RuntimeCard> CardSelectCards { get; set; } = Array.Empty<RuntimeCard>();
            public IReadOnlyDictionary<int, int> CardSelectPrices { get; set; } = new Dictionary<int, int>();
            public IReadOnlyDictionary<int, RuntimeCard> CardSelectUpgradedCards { get; set; } = new Dictionary<int, RuntimeCard>();
            public string CardSelectMessage { get; set; } = string.Empty;
            public CardSelectMode CardSelectMode { get; set; } = CardSelectMode.CardRemoval;

            public void Initialize(int runProfileId)
            {
                InitializeCallCount++;
            }

            public void InitializeFromSave(RunSaveData saveData)
            {
                InitializeFromSaveCallCount++;
            }

            public BattleSceneSnapshot CreateSnapshot()
            {
                return _snapshot;
            }

            public IReadOnlyList<RuntimeCard> GetDeckCards()
            {
                return new List<RuntimeCard>();
            }

            public IReadOnlyList<RuntimeCard> GetCardSelectCards()
            {
                return CardSelectCards;
            }

            public IReadOnlyDictionary<int, int> GetCardSelectPrices()
            {
                return CardSelectPrices;
            }

            public IReadOnlyDictionary<int, RuntimeCard> GetCardSelectUpgradedCards()
            {
                return CardSelectUpgradedCards;
            }

            public string GetCardSelectMessage()
            {
                return CardSelectMessage;
            }

            public CardSelectMode GetCardSelectMode()
            {
                return CardSelectMode;
            }

            public void SelectMapNode(int index)
            {
            }

            public void SelectHandCard(int index)
            {
                SelectHandCardCallCount++;
            }

            public void TryPlaySelectedCard()
            {
                TryPlaySelectedCardCallCount++;
            }

            public bool DoesSelectedCardRequireEnemyTarget()
            {
                return DoesSelectedCardRequireEnemyTargetResult;
            }

            public void EndTurn()
            {
                EndTurnCallCount++;
            }

            public void SelectReward(RuntimeRewardEntry rewardEntry)
            {
            }

            public void ClaimGold()
            {
            }

            public void ClaimPotion()
            {
            }

            public void ClaimRelic()
            {
            }

            public void InspectOwnedRelic(int index)
            {
                InspectOwnedRelicCallCount++;
            }

            public void InspectOwnedPotion(int index)
            {
                InspectOwnedPotionCallCount++;
            }

            public void UsePotion(int index)
            {
                UsePotionCallCount++;
                if (SnapshotAfterUsePotion != null)
                {
                    _snapshot = SnapshotAfterUsePotion;
                }
            }

            public void ReplaceOwnedPotion(int index)
            {
                ReplaceOwnedPotionCallCount++;
                _snapshot = SnapshotAfterReplaceOwnedPotion ?? BattleScenePresenterTests.CreateSnapshot(_snapshot.CurrentPage);
            }

            public void CancelPendingPotionReplace()
            {
                CancelPendingPotionReplaceCallCount++;
                _snapshot = SnapshotAfterCancelPendingPotionReplace ?? BattleScenePresenterTests.CreateSnapshot(_snapshot.CurrentPage);
            }

            public void ClearOwnedInspections()
            {
                ClearOwnedInspectionsCallCount++;
                _snapshot = BattleScenePresenterTests.CreateSnapshot(_snapshot.CurrentPage);
            }

            public void ApplyRest()
            {
                _snapshot = BattleScenePresenterTests.CreateSnapshot(BattleScenePage.Result);
            }

            public void ApplyUpgrade()
            {
                _snapshot = BattleScenePresenterTests.CreateSnapshot(BattleScenePage.Result);
            }

            public void ApplyShopPurchase()
            {
            }

            public void OpenShop()
            {
                _snapshot = BattleScenePresenterTests.CreateSnapshot(BattleScenePage.Result);
            }

            public void PurchaseShopItem(int slotIndex)
            {
                _snapshot = BattleScenePresenterTests.CreateSnapshot(BattleScenePage.Result);
            }

            public void OpenCardRemoval()
            {
                _snapshot = BattleScenePresenterTests.CreateSnapshot(BattleScenePage.Result);
            }

            public void PurchaseCardRemoval(RuntimeCard card)
            {
                _snapshot = BattleScenePresenterTests.CreateSnapshot(BattleScenePage.Result);
            }

            public void CancelCardSelect()
            {
                CancelCardSelectCallCount++;
                _snapshot = BattleScenePresenterTests.CreateSnapshot(BattleScenePage.Result);
            }

            public void ConfirmCardSelect(RuntimeCard card)
            {
                ConfirmCardSelectCallCount++;
                CardSelectCards = Array.Empty<RuntimeCard>();
                CardSelectPrices = new Dictionary<int, int>();
                CardSelectUpgradedCards = new Dictionary<int, RuntimeCard>();
                CardSelectMessage = $"Upgraded {card.DisplayName}.";
            }

            public void LeaveShop()
            {
                _snapshot = BattleScenePresenterTests.CreateSnapshot(BattleScenePage.Result);
            }

            public void ContinueFromRestShop()
            {
                _snapshot = BattleScenePresenterTests.CreateSnapshot(BattleScenePage.Result);
            }

            public void ContinueFromReward()
            {
                _snapshot = BattleScenePresenterTests.CreateSnapshot(BattleScenePage.Map);
            }

            public void SelectEventChoice(int choiceId)
            {
                _snapshot = BattleScenePresenterTests.CreateSnapshot(BattleScenePage.Result);
            }

            public void SelectEnemyTarget(int index)
            {
            }
        }

        private sealed class FakeBattleSceneHostView : IBattleSceneHostView
        {
            public FakeBattlePageView BattlePageView { get; } = new FakeBattlePageView();
            IBattlePageView IBattleSceneHostView.BattlePageView => BattlePageView;

            public bool IsBattleVisible { get; private set; }
            public bool IsSaveQuitVisible { get; private set; }
            public IReadOnlyList<BattleMultiIconViewModel> LastOwnedRelics { get; private set; } = Array.Empty<BattleMultiIconViewModel>();
            public string LastOwnedRelicHint { get; private set; } = string.Empty;
            public int LastSelectedOwnedRelicIndex { get; private set; } = -1;
            public IReadOnlyList<BattleMultiIconViewModel> LastOwnedPotions { get; private set; } = Array.Empty<BattleMultiIconViewModel>();
            public string LastOwnedPotionHint { get; private set; } = string.Empty;
            public int LastSelectedOwnedPotionIndex { get; private set; } = -1;
            public bool IsOwnedPotionUseVisible { get; private set; }
            public bool LastHostChromeInteractable { get; private set; } = true;
            public int HostBackgroundWireCallCount { get; private set; }
            private Action _onSaveQuitClicked;
            private Action _onUsePotionClicked;
            private Action _onHostBackgroundClicked;

            public void BuildOwnedRelics(IReadOnlyList<BattleMultiIconViewModel> relics, Action<int> onClicked)
            {
                LastOwnedRelics = relics ?? Array.Empty<BattleMultiIconViewModel>();
            }

            public void SetOwnedRelicHint(string message, int selectedIndex)
            {
                LastOwnedRelicHint = message ?? string.Empty;
                LastSelectedOwnedRelicIndex = selectedIndex;
            }

            public void ClearOwnedRelics()
            {
                LastOwnedRelics = Array.Empty<BattleMultiIconViewModel>();
            }

            public void BuildOwnedPotions(IReadOnlyList<BattleMultiIconViewModel> potions, Action<int> onClicked)
            {
                LastOwnedPotions = potions ?? Array.Empty<BattleMultiIconViewModel>();
            }

            public void SetOwnedPotionHint(string message, int selectedIndex)
            {
                LastOwnedPotionHint = message ?? string.Empty;
                LastSelectedOwnedPotionIndex = selectedIndex;
            }

            public void SetOwnedPotionUseVisible(bool visible, Action onClicked)
            {
                IsOwnedPotionUseVisible = visible;
                _onUsePotionClicked = visible ? onClicked : null;
            }

            public void ClearOwnedPotions()
            {
                LastOwnedPotions = Array.Empty<BattleMultiIconViewModel>();
                IsOwnedPotionUseVisible = false;
                _onUsePotionClicked = null;
            }

            public void SetHostChromeInteractable(bool interactable)
            {
                LastHostChromeInteractable = interactable;
            }

            public void WireHostBackgroundClick(Action onClicked)
            {
                HostBackgroundWireCallCount++;
                _onHostBackgroundClicked = onClicked;
            }

            public void UnwireHostBackgroundClick()
            {
                _onHostBackgroundClicked = null;
            }

            public void SetBattleVisible(bool visible)
            {
                IsBattleVisible = visible;
            }

            public void SetSaveQuitVisible(bool visible)
            {
                IsSaveQuitVisible = visible;
            }

            public void WireSaveQuitButton(Action onSaveQuitClicked)
            {
                _onSaveQuitClicked = onSaveQuitClicked;
            }

            public void UnwireSaveQuitButton()
            {
                _onSaveQuitClicked = null;
            }

            public void InvokeSaveQuit()
            {
                _onSaveQuitClicked?.Invoke();
            }

            public void InvokeUsePotion()
            {
                _onUsePotionClicked?.Invoke();
            }

            public void InvokeHostBackground()
            {
                _onHostBackgroundClicked?.Invoke();
            }
        }

        private sealed class FakeBattlePageView : IBattlePageView
        {
            public int BuildCallCount { get; private set; }
            public int LastEnemyButtonCount { get; private set; }
            public string LastPlayerText { get; private set; }
            public string LastEnemyText { get; private set; }
            public string LastHintText { get; private set; }
            public BattleHudViewModel LastHud { get; private set; }
            public IReadOnlyList<BattleHandCardViewModel> LastHandCards { get; private set; } = Array.Empty<BattleHandCardViewModel>();
            public int LastDrawPileCount { get; private set; }
            public int LastDiscardPileCount { get; private set; }
            public int LastHandCount { get; private set; }
            public int LastMaxHandCount { get; private set; }

            public void WireButtons(Action onEndTurnClicked)
            {
            }

            public void UnwireButtons()
            {
            }

            public void SetBattleStateText(string playerText, string enemyText, string hintText)
            {
                LastPlayerText = playerText;
                LastEnemyText = enemyText;
                LastHintText = hintText;
            }

            public void SetBattleHud(BattleHudViewModel hud)
            {
                LastHud = hud;
            }

            public void SetPileCounters(int drawPileCount, int discardPileCount, int handCount, int maxHandCount)
            {
                LastDrawPileCount = drawPileCount;
                LastDiscardPileCount = discardPileCount;
                LastHandCount = handCount;
                LastMaxHandCount = maxHandCount;
            }

            public void BuildHandCards(IReadOnlyList<BattleHandCardViewModel> handCards, Action<int> onClicked)
            {
                BuildCallCount++;
                LastHandCards = handCards ?? Array.Empty<BattleHandCardViewModel>();
            }

            public void BuildEnemyButtons(IReadOnlyList<BattleEnemyViewModel> enemies, int selectedEnemyIndex, Action<int> onClicked)
            {
                LastEnemyButtonCount = enemies.Count;
            }

            public void ClearDynamicButtons()
            {
            }
        }

        private sealed class FakeBattleSceneUiCoordinator : IBattleSceneUiCoordinator
        {
            public int InitializeCallCount { get; private set; }
            public int ShowMapCallCount { get; private set; }
            public int ShowBattleCallCount { get; private set; }
            public int ShowShopCallCount { get; private set; }
            public int ShowEventCallCount { get; private set; }
            public int ShowPotionReplaceCallCount { get; private set; }
            public BattleMapSnapshot LastMapSnapshot { get; private set; }
            public BattleShopSnapshot LastShopSnapshot { get; private set; }
            public BattleEventSnapshot LastEventSnapshot { get; private set; }
            public BattlePotionReplaceSnapshot LastPotionReplaceSnapshot { get; private set; }
            public PotionReplaceDialogResult PotionReplaceResult { get; set; }
            public CardSelectDialogResult CardSelectResult { get; set; }
            public RuntimeCard CardToSelectBeforeResult { get; set; }
            public BattleCardSelectDialogParam LastCardSelectParam { get; private set; }
            public CardSelectMode LastCardSelectMode { get; private set; }
            public bool LastCardSelectShowPrice { get; private set; }
            public IReadOnlyDictionary<int, int> LastCardSelectPrices { get; private set; } = new Dictionary<int, int>();
            public IReadOnlyDictionary<int, RuntimeCard> LastCardSelectUpgradedCards { get; private set; } = new Dictionary<int, RuntimeCard>();
            public string LastCardSelectMessage { get; private set; }
            public Func<RuntimeCard, BattleCardSelectDialogRefreshData> LastCardSelectCallback { get; private set; }
            public BattleCardSelectDialogRefreshData LastCardSelectRefreshData { get; private set; }

            public UniTask InitializeAsync(IBattleSceneHostView hostView, CancellationToken ct)
            {
                InitializeCallCount++;
                return UniTask.CompletedTask;
            }

            public UniTask ShowMapAsync(BattleMapSnapshot snapshot, Action<int> onMapNodeClicked, CancellationToken ct)
            {
                ShowMapCallCount++;
                LastMapSnapshot = snapshot;
                return UniTask.CompletedTask;
            }

            public UniTask ShowBattleAsync(CancellationToken ct)
            {
                ShowBattleCallCount++;
                return UniTask.CompletedTask;
            }

            public UniTask<RewardDialogResult> ShowRewardAsync(BattleRewardSnapshot snapshot, CancellationToken ct)
            {
                return UniTask.FromResult(new RewardDialogResult { Action = RewardDialogActionType.Continue });
            }

            public UniTask<RuntimeRewardEntry> ShowCardPickAsync(BattleRewardSnapshot snapshot, CancellationToken ct)
            {
                return UniTask.FromResult<RuntimeRewardEntry>(null);
            }

            public UniTask<PotionReplaceDialogResult> ShowPotionReplaceAsync(BattlePotionReplaceSnapshot snapshot, CancellationToken ct)
            {
                ShowPotionReplaceCallCount++;
                LastPotionReplaceSnapshot = snapshot;
                return UniTask.FromResult(PotionReplaceResult);
            }

            public UniTask<RestShopDialogAction> ShowRestShopAsync(BattleRestShopSnapshot snapshot, CancellationToken ct)
            {
                return UniTask.FromResult(RestShopDialogAction.None);
            }

            public UniTask<ShopDialogResult> ShowShopAsync(BattleShopSnapshot snapshot, CancellationToken ct)
            {
                ShowShopCallCount++;
                LastShopSnapshot = snapshot;
                return UniTask.FromResult(new ShopDialogResult { Action = ShopDialogActionType.Leave });
            }

            public UniTask<CardSelectDialogResult> ShowCardSelectAsync(BattleCardSelectDialogParam param, CancellationToken ct)
            {
                LastCardSelectParam = param;
                LastCardSelectMode = param.Mode;
                LastCardSelectShowPrice = param.ShowPrice;
                LastCardSelectPrices = param.CardPrices;
                LastCardSelectUpgradedCards = param.UpgradedCards;
                LastCardSelectMessage = param.Message;
                LastCardSelectCallback = param.OnCardConfirmed;
                if (CardToSelectBeforeResult != null)
                {
                    LastCardSelectRefreshData = param.OnCardConfirmed?.Invoke(CardToSelectBeforeResult);
                }

                return UniTask.FromResult(CardSelectResult);
            }

            public UniTask<EventDialogResult> ShowEventAsync(BattleEventSnapshot snapshot, CancellationToken ct)
            {
                ShowEventCallCount++;
                LastEventSnapshot = snapshot;
                return UniTask.FromResult(new EventDialogResult { Action = EventDialogActionType.SelectChoice, ChoiceId = 1 });
            }

            public UniTask ShowResultAsync(BattleResultSnapshot snapshot, CancellationToken ct)
            {
                return UniTask.CompletedTask;
            }

            public void Dispose()
            {
            }
        }

        private sealed class FakeRunSaveService : IRunSaveService
        {
            public bool HasSavedRunResult { get; set; }
            public RunSaveData LoadResult { get; set; }
            public int DeleteCallCount { get; private set; }

            public UniTask SaveCurrentRunAsync(RunSaveData data, CancellationToken token = default)
            {
                return UniTask.CompletedTask;
            }

            public UniTask<RunSaveData> LoadCurrentRunAsync(CancellationToken token = default)
            {
                return UniTask.FromResult(LoadResult);
            }

            public bool HasSavedRun()
            {
                return HasSavedRunResult;
            }

            public void DeleteSavedRun()
            {
                DeleteCallCount++;
            }
        }
    }
}
