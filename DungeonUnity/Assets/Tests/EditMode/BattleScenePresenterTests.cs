using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Battle.View;
using Dungeon.Runtime.InGame.Domain;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleScenePresenterのEditモード試験クラス
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

            presenter.InitializeAsync(view, null, () => { }, CancellationToken.None).GetAwaiter().GetResult();

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

            presenter.InitializeAsync(view, null, () => { }, CancellationToken.None).GetAwaiter().GetResult();
            presenter.OnEndTurnClicked();

            Assert.That(flowService.EndTurnCallCount, Is.EqualTo(1));
            Assert.That(uiCoordinator.ShowBattleCallCount, Is.GreaterThanOrEqualTo(1));
        }

        private static BattleSceneSnapshot CreateSnapshot(BattleScenePage page)
        {
            return new BattleSceneSnapshot(
                page,
                new List<MapTemplate.Node>(),
                new List<CardDefinition>(),
                new List<CardDefinition>(),
                -1,
                40,
                40,
                3,
                100,
                null,
                0,
                false,
                -1,
                false,
                "map",
                "battle",
                "rest",
                "result");
        }

        private sealed class FakeBattleSceneFlowService : IBattleSceneFlowService
        {
            private readonly BattleSceneSnapshot _snapshot;

            public FakeBattleSceneFlowService(BattleSceneSnapshot snapshot)
            {
                _snapshot = snapshot;
            }

            public int EndTurnCallCount { get; private set; }

            public void Initialize(RunStartConfig runStartConfig)
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

            public void SelectReward(CardDefinition card)
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

            public void WireButtons(Action onEnemyTargetClicked, Action onEndTurnClicked)
            {
            }

            public void UnwireButtons()
            {
            }

            public void SetBattleStateText(string playerText, string enemyText, string hintText)
            {
            }

            public void BuildHandButtons(IReadOnlyList<CardDefinition> hand, Action<int> onClicked)
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

            public UniTask<CardDefinition> ShowRewardAsync(BattleSceneSnapshot snapshot, CancellationToken ct)
            {
                LastSnapshot = snapshot;
                return UniTask.FromResult<CardDefinition>(null);
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
