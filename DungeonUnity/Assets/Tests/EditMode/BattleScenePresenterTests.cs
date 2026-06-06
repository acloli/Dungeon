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
        public void InitializeAsync_BattleState_RendersIntentStatusAndBuffText()
        {
            BattleSceneSnapshot snapshot = CreateBattleSnapshotWithDisplayInfo();
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(snapshot);
            FakeBattleSceneHostView view = new FakeBattleSceneHostView();
            FakeBattleSceneUiCoordinator uiCoordinator = new FakeBattleSceneUiCoordinator();
            BattleScenePresenter presenter = new BattleScenePresenter(flowService, new BattlePagePresenter(), uiCoordinator);

            presenter.InitializeAsync(view, 5501, () => { }, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(view.BattlePageView.LastPlayerText, Does.Contain("Status: Vulnerable:2"));
            Assert.That(view.BattlePageView.LastPlayerText, Does.Contain("Buff: Strength:1"));
            Assert.That(view.BattlePageView.LastEnemyText, Does.Contain("Intent: AttackDefend"));
            Assert.That(view.BattlePageView.LastEnemyText, Does.Contain("D7x2"));
            Assert.That(view.BattlePageView.LastEnemyText, Does.Contain("B5"));
            Assert.That(view.BattlePageView.LastEnemyText, Does.Contain("Status Weak:2"));
            Assert.That(view.BattlePageView.LastEnemyText, Does.Contain("Buff Ritual:3"));
            Assert.That(view.BattlePageView.LastEnemyText, Does.Contain("Status: Weak:1"));
            Assert.That(view.BattlePageView.LastEnemyText, Does.Contain("Buff: Ritual:3"));
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
                    7,
                    2,
                    5,
                    StatusType.Weak,
                    2,
                    BuffType.Ritual,
                    3),
                new[] { new BattleStatusViewModel(nameof(StatusType.Vulnerable), 2, false) },
                new[] { new BattleStatusViewModel(nameof(StatusType.Weak), 1, false) },
                new[] { new BattleStatusViewModel(nameof(BuffType.Strength), 1, true) },
                new[] { new BattleStatusViewModel(nameof(BuffType.Ritual), 3, true) });
        }

        private sealed class FakeBattleSceneFlowService : IBattleSceneFlowService
        {
            private readonly BattleSceneSnapshot _snapshot;

            public FakeBattleSceneFlowService(BattleSceneSnapshot snapshot)
            {
                _snapshot = snapshot;
            }

            public int EndTurnCallCount { get; private set; }

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
            }

            public void TryPlaySelectedCard()
            {
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
            public string LastPlayerText { get; private set; }
            public string LastEnemyText { get; private set; }
            public string LastHintText { get; private set; }

            public void WireButtons(Action onEnemyTargetClicked, Action onEndTurnClicked)
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
