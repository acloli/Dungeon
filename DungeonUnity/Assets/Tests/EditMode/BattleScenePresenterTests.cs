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
using Game.MasterData.Generated;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleScenePresenterのEditモードテストクラス
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
            Assert.That(uiCoordinator.LastSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.Map));
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
            return new BattleSceneSnapshot(
                page,
                new List<RuntimeMapNode>(),
                new List<RuntimeCard>(),
                new List<RuntimeCard>(),
                -1,
                40,
                40,
                3,
                0,
                100,
                null,
                0,
                0,
                false,
                -1,
                false,
                "map",
                "battle",
                "rest",
                "result");
        }

        private static BattleSceneSnapshot CreateBattleSnapshotWithDisplayInfo()
        {
            return new BattleSceneSnapshot(
                BattleScenePage.Battle,
                new List<RuntimeMapNode>(),
                new List<RuntimeCard>(),
                new List<RuntimeCard>(),
                -1,
                40,
                40,
                3,
                0,
                100,
                null,
                20,
                0,
                false,
                -1,
                false,
                "map",
                "battle",
                "rest",
                "result",
                new BattleIntentViewModel(
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
                new[] { new BattleStatusViewModel("脆弱", 2, false) },
                new[] { new BattleStatusViewModel("脱力", 1, false) },
                new[] { new BattleStatusViewModel("筋力", 1, true) },
                new[] { new BattleStatusViewModel("儀式", 3, true) });
        }

        private static BattleSceneSnapshot CreateMultiEnemySnapshot()
        {
            return new BattleSceneSnapshot(
                BattleScenePage.Battle,
                new List<RuntimeMapNode>(),
                new List<RuntimeCard>(),
                new List<RuntimeCard>(),
                -1,
                40,
                40,
                3,
                0,
                100,
                null,
                0,
                0,
                false,
                -1,
                false,
                "map",
                "battle",
                "rest",
                "result",
                null,
                Array.Empty<BattleStatusViewModel>(),
                Array.Empty<BattleStatusViewModel>(),
                Array.Empty<BattleStatusViewModel>(),
                Array.Empty<BattleStatusViewModel>(),
                new[]
                {
                    new BattleEnemyViewModel(0, "Red Mite", 12, 0, false, null, Array.Empty<BattleStatusViewModel>(), Array.Empty<BattleStatusViewModel>()),
                    new BattleEnemyViewModel(1, "Green Mite", 8, 0, false, null, Array.Empty<BattleStatusViewModel>(), Array.Empty<BattleStatusViewModel>())
                },
                1);
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

        private sealed class FakeBattleSceneFlowService : IBattleSceneFlowService
        {
            private readonly BattleSceneSnapshot _snapshot;

            public FakeBattleSceneFlowService(BattleSceneSnapshot snapshot)
            {
                _snapshot = snapshot;
            }

            public int EndTurnCallCount { get; private set; }
            public int SelectHandCardCallCount { get; private set; }
            public int TryPlaySelectedCardCallCount { get; private set; }
            public int InitializeCallCount { get; private set; }
            public int InitializeFromSaveCallCount { get; private set; }
            public bool DoesSelectedCardRequireEnemyTargetResult { get; set; } = true;

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

            public void SelectReward(RuntimeCard card)
            {
            }

            public void ApplyRest()
            {
            }

            public void ApplyUpgrade()
            {
            }

            public void ApplyShopPurchase()
            {
            }

            public void ContinueFromRestShop()
            {
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
            private Action _onSaveQuitClicked;

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
        }

        private sealed class FakeBattlePageView : IBattlePageView
        {
            public int BuildCallCount { get; private set; }
            public int LastEnemyButtonCount { get; private set; }
            public string LastPlayerText { get; private set; }
            public string LastEnemyText { get; private set; }
            public string LastHintText { get; private set; }
            public BattleHudViewModel LastHud { get; private set; }

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

            public void BuildHandButtons(IReadOnlyList<RuntimeCard> hand, Action<int> onClicked)
            {
                BuildCallCount++;
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
            public BattleSceneSnapshot LastSnapshot { get; private set; }

            public UniTask InitializeAsync(IBattleSceneHostView hostView, CancellationToken ct)
            {
                InitializeCallCount++;
                return UniTask.CompletedTask;
            }

            public UniTask ShowMapAsync(BattleSceneSnapshot snapshot, Action<int> onMapNodeClicked, CancellationToken ct)
            {
                ShowMapCallCount++;
                LastSnapshot = snapshot;
                return UniTask.CompletedTask;
            }

            public UniTask ShowBattleAsync(CancellationToken ct)
            {
                ShowBattleCallCount++;
                return UniTask.CompletedTask;
            }

            public UniTask<RuntimeCard> ShowRewardAsync(BattleSceneSnapshot snapshot, CancellationToken ct)
            {
                LastSnapshot = snapshot;
                return UniTask.FromResult<RuntimeCard>(null);
            }

            public UniTask<RestShopDialogAction> ShowRestShopAsync(BattleSceneSnapshot snapshot, CancellationToken ct)
            {
                LastSnapshot = snapshot;
                return UniTask.FromResult(RestShopDialogAction.None);
            }

            public UniTask ShowResultAsync(BattleSceneSnapshot snapshot, CancellationToken ct)
            {
                LastSnapshot = snapshot;
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
