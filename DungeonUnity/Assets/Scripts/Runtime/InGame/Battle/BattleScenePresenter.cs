using System;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Battle.View;
using Dungeon.Runtime.InGame.Domain;

namespace Dungeon.Runtime.InGame.Battle
{
    /// <summary>
    /// BattleSceneの表示仲介クラス
    /// </summary>
    public sealed class BattleScenePresenter
    {
        private readonly IBattleSceneFlowService _flowService;
        private readonly MapPagePresenter _mapPagePresenter;
        private readonly BattlePagePresenter _battlePagePresenter;
        private readonly RewardPagePresenter _rewardPagePresenter;
        private readonly RestShopPagePresenter _restShopPagePresenter;
        private readonly ResultPagePresenter _resultPagePresenter;
        private IBattleSceneView _view;

        public BattleScenePresenter(IBattleSceneFlowService flowService,
            MapPagePresenter mapPagePresenter,
            BattlePagePresenter battlePagePresenter,
            RewardPagePresenter rewardPagePresenter,
            RestShopPagePresenter restShopPagePresenter,
            ResultPagePresenter resultPagePresenter)
        {
            _flowService = flowService;
            _mapPagePresenter = mapPagePresenter;
            _battlePagePresenter = battlePagePresenter;
            _rewardPagePresenter = rewardPagePresenter;
            _restShopPagePresenter = restShopPagePresenter;
            _resultPagePresenter = resultPagePresenter;
        }

        /// <summary>
        /// View接続初期化
        /// </summary>
        public void Initialize(IBattleSceneView view, RunStartConfig runStartConfig, Action onResultBackClicked)
        {
            _view = view;
            _mapPagePresenter.Initialize(_view.MapPageView, OnMapNodeClicked);
            _battlePagePresenter.Initialize(_view.BattlePageView, OnHandCardClicked, OnEnemyTargetClicked, OnEndTurnClicked);
            _rewardPagePresenter.Initialize(_view.RewardPageView, OnRewardSelected);
            _restShopPagePresenter.Initialize(_view.RestShopPageView, OnRestClicked, OnUpgradeClicked, OnShopClicked, OnRestShopContinueClicked);
            _resultPagePresenter.Initialize(_view.ResultPageView, onResultBackClicked);
            _flowService.Initialize(runStartConfig);
            Render();
        }

        /// <summary>
        /// View切り離し処理
        /// </summary>
        public void Dispose()
        {
            _battlePagePresenter.Dispose();
            _restShopPagePresenter.Dispose();
            _resultPagePresenter.Dispose();
            _view = null;
        }

        /// <summary>
        /// マップ選択通知
        /// </summary>
        public void OnMapNodeClicked(int index)
        {
            _flowService.SelectMapNode(index);
            Render();
        }

        /// <summary>
        /// 手札選択通知
        /// </summary>
        public void OnHandCardClicked(int index)
        {
            _flowService.SelectHandCard(index);
            Render();
        }

        /// <summary>
        /// 敵対象クリック通知
        /// </summary>
        public void OnEnemyTargetClicked()
        {
            _flowService.TryPlaySelectedCard();
            Render();
        }

        /// <summary>
        /// ターン終了通知
        /// </summary>
        public void OnEndTurnClicked()
        {
            _flowService.EndTurn();
            Render();
        }

        /// <summary>
        /// 報酬選択通知
        /// </summary>
        public void OnRewardSelected(CardDefinition card)
        {
            _flowService.SelectReward(card);
            Render();
        }

        /// <summary>
        /// 休憩選択通知
        /// </summary>
        public void OnRestClicked()
        {
            _flowService.ApplyRest();
            Render();
        }

        /// <summary>
        /// 強化選択通知
        /// </summary>
        public void OnUpgradeClicked()
        {
            _flowService.ApplyUpgrade();
            Render();
        }

        /// <summary>
        /// 購入選択通知
        /// </summary>
        public void OnShopClicked()
        {
            _flowService.ApplyShopPurchase();
            Render();
        }

        /// <summary>
        /// 補給継続通知
        /// </summary>
        public void OnRestShopContinueClicked()
        {
            _flowService.ContinueFromRestShop();
            Render();
        }

        /// <summary>
        /// スナップショット反映処理
        /// </summary>
        private void Render()
        {
            if (_view == null)
            {
                return;
            }

            BattleSceneSnapshot snapshot = _flowService.CreateSnapshot();
            _mapPagePresenter.Clear();
            _battlePagePresenter.Clear();
            _rewardPagePresenter.Clear();
            _view.ShowPage(snapshot.CurrentPage);

            switch (snapshot.CurrentPage)
            {
                case BattleScenePage.Map:
                    _mapPagePresenter.Render(snapshot);
                    break;
                case BattleScenePage.Battle:
                    _battlePagePresenter.Render(snapshot);
                    break;
                case BattleScenePage.Reward:
                    _rewardPagePresenter.Render(snapshot);
                    break;
                case BattleScenePage.RestShop:
                    _restShopPagePresenter.Render(snapshot);
                    break;
                case BattleScenePage.Result:
                    _resultPagePresenter.Render(snapshot);
                    break;
            }
        }
    }
}
