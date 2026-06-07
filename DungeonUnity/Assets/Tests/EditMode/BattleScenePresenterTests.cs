using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Battle.View;
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
        public void OnHandCardClicked_WhenCardDoesNotRequireEnemyTarget_PlaysSelectedCard()
        {
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(CreateSnapshot(BattleScenePage.Battle));
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);
            flowService.DoesSelectedCardRequireEnemyTargetResult = false;

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();
            presenter.OnHandCardClicked(0);

            Assert.That(flowService.SelectHandCardCallCount, Is.EqualTo(1));
            Assert.That(flowService.TryPlaySelectedCardCallCount, Is.EqualTo(1));
        }

        [Test]
        public void OnHandCardClicked_WhenCardRequiresEnemyTarget_DoesNotPlaySelectedCard()
        {
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(CreateSnapshot(BattleScenePage.Battle));
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);
            flowService.DoesSelectedCardRequireEnemyTargetResult = true;

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();
            presenter.OnHandCardClicked(0);

            Assert.That(flowService.SelectHandCardCallCount, Is.EqualTo(1));
            Assert.That(flowService.TryPlaySelectedCardCallCount, Is.EqualTo(0));
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
            Assert.That(view.BattlePageView.LastEnemyButtonCount, Is.EqualTo(2));
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
            public bool DoesSelectedCardRequireEnemyTargetResult { get; set; } = true;

            public void Initialize(int runProfileId)
            {
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

            public void SetBattleVisible(bool visible)
            {
                IsBattleVisible = visible;
            }
        }

        private sealed class FakeBattlePageView : IBattlePageView
        {
            public int BuildCallCount { get; private set; }
            public int LastEnemyButtonCount { get; private set; }
            public string LastPlayerText { get; private set; }
            public string LastEnemyText { get; private set; }
            public string LastHintText { get; private set; }

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
    }
}
