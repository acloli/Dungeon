using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Battle.View;
using Dungeon.Runtime.InGame.Domain;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleScenePresenterのEditモードのテストクラス
    /// </summary>
    [TestFixture]
    public sealed class BattleScenePresenterTests
    {
        [Test]
        public void Initialize_MapPageOnlyShown()
        {
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(CreateSnapshot(BattleScenePage.Map));
            FakeBattleSceneView view = new FakeBattleSceneView();
            BattleScenePresenter presenter = CreatePresenter(flowService);

            presenter.Initialize(view, null, () => { });

            Assert.That(view.LastShownPage, Is.EqualTo(BattleScenePage.Map));
            Assert.That(view.MapPageView.BuildCallCount, Is.EqualTo(1));
            Assert.That(view.BattlePageView.BuildCallCount, Is.EqualTo(0));
            Assert.That(view.RewardPageView.BuildCallCount, Is.EqualTo(0));
        }

        [Test]
        public void OnEndTurnClicked_BattlePageRerendered()
        {
            FakeBattleSceneFlowService flowService = new FakeBattleSceneFlowService(CreateSnapshot(BattleScenePage.Battle));
            FakeBattleSceneView view = new FakeBattleSceneView();
            BattleScenePresenter presenter = CreatePresenter(flowService);

            presenter.Initialize(view, null, () => { });
            presenter.OnEndTurnClicked();

            Assert.That(flowService.EndTurnCallCount, Is.EqualTo(1));
            Assert.That(view.LastShownPage, Is.EqualTo(BattleScenePage.Battle));
            Assert.That(view.BattlePageView.BuildCallCount, Is.EqualTo(2));
        }

        private static BattleScenePresenter CreatePresenter(IBattleSceneFlowService flowService)
        {
            return new BattleScenePresenter(
                flowService,
                new MapPagePresenter(),
                new BattlePagePresenter(),
                new RewardPagePresenter(),
                new RestShopPagePresenter(),
                new ResultPagePresenter());
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

        private sealed class FakeBattleSceneView : IBattleSceneView
        {
            public FakeBattleSceneView()
            {
                MapPageView = new FakeMapPageView();
                BattlePageView = new FakeBattlePageView();
                RewardPageView = new FakeRewardPageView();
                RestShopPageView = new FakeRestShopPageView();
                ResultPageView = new FakeResultPageView();
            }

            public FakeMapPageView MapPageView { get; }
            IMapPageView IBattleSceneView.MapPageView => MapPageView;

            public FakeBattlePageView BattlePageView { get; }
            IBattlePageView IBattleSceneView.BattlePageView => BattlePageView;

            public FakeRewardPageView RewardPageView { get; }
            IRewardPageView IBattleSceneView.RewardPageView => RewardPageView;

            public FakeRestShopPageView RestShopPageView { get; }
            IRestShopPageView IBattleSceneView.RestShopPageView => RestShopPageView;

            public FakeResultPageView ResultPageView { get; }
            IResultPageView IBattleSceneView.ResultPageView => ResultPageView;

            public BattleScenePage LastShownPage { get; private set; }

            public void ShowPage(BattleScenePage page)
            {
                LastShownPage = page;
            }
        }

        private sealed class FakeMapPageView : IMapPageView
        {
            public int BuildCallCount { get; private set; }

            public void SetMapStateText(string message)
            {
            }

            public void BuildMapButtons(IReadOnlyList<MapTemplate.Node> nodes, Action<int> onClicked)
            {
                BuildCallCount++;
            }

            public void SetMapButtonInteractable(int allowedIndex)
            {
            }

            public void ClearDynamicButtons()
            {
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

        private sealed class FakeRewardPageView : IRewardPageView
        {
            public int BuildCallCount { get; private set; }

            public void BuildRewardButtons(IReadOnlyList<CardDefinition> cards, Action<CardDefinition> onClicked)
            {
                BuildCallCount++;
            }

            public void ClearDynamicButtons()
            {
            }
        }

        private sealed class FakeRestShopPageView : IRestShopPageView
        {
            public void WireButtons(Action onRestClicked, Action onUpgradeClicked, Action onShopClicked, Action onContinueClicked)
            {
            }

            public void UnwireButtons()
            {
            }

            public void SetRestShopText(string message)
            {
            }

            public void SetRestShopContinueInteractable(bool interactable)
            {
            }
        }

        private sealed class FakeResultPageView : IResultPageView
        {
            public void WireButtons(Action onBackClicked)
            {
            }

            public void UnwireButtons()
            {
            }

            public void SetResultText(string message)
            {
            }
        }
    }
}
