using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Battle.View;

namespace Dungeon.Runtime.InGame.Battle
{
    /// <summary>
    /// BattleSceneの表示仲介クラス
    /// </summary>
    public sealed class BattleScenePresenter
    {
        private readonly IBattleSceneFlowService _flowService;
        private readonly BattlePagePresenter _battlePagePresenter;
        private readonly IBattleSceneUiCoordinator _uiCoordinator;
        private readonly CancellationTokenSource _presenterCts = new CancellationTokenSource();

        private IBattleSceneHostView _view;
        private Action _onResultBackClicked;

        public BattleScenePresenter(
            IBattleSceneFlowService flowService,
            BattlePagePresenter battlePagePresenter,
            IBattleSceneUiCoordinator uiCoordinator)
        {
            _flowService = flowService;
            _battlePagePresenter = battlePagePresenter;
            _uiCoordinator = uiCoordinator;
        }

        /// <summary>
        /// View接続初期化
        /// </summary>
        public async UniTask InitializeAsync(IBattleSceneHostView view, int runProfileId, Action onResultBackClicked, CancellationToken ct)
        {
            _view = view;
            _onResultBackClicked = onResultBackClicked;

            _battlePagePresenter.Initialize(_view.BattlePageView, OnHandCardClicked, OnEnemyTargetClicked, OnEndTurnClicked);
            await _uiCoordinator.InitializeAsync(view, ct);

            _flowService.Initialize(runProfileId);
            await RenderAsync(ct);
        }

        /// <summary>
        /// View切り離し処理
        /// </summary>
        public void Dispose()
        {
            _battlePagePresenter.Dispose();
            _presenterCts.Cancel();
            _uiCoordinator.Dispose();
            _view = null;
            _onResultBackClicked = null;
        }

        /// <summary>
        /// マップ選択通知
        /// </summary>
        public void OnMapNodeClicked(int index)
        {
            _flowService.SelectMapNode(index);
            RenderAsync(_presenterCts.Token).Forget();
        }

        /// <summary>
        /// 手札選択通知
        /// </summary>
        public void OnHandCardClicked(int index)
        {
            _flowService.SelectHandCard(index);
            RenderAsync(_presenterCts.Token).Forget();
        }

        /// <summary>
        /// 敵対象クリック通知
        /// </summary>
        public void OnEnemyTargetClicked()
        {
            _flowService.TryPlaySelectedCard();
            RenderAsync(_presenterCts.Token).Forget();
        }

        /// <summary>
        /// ターン終了通知
        /// </summary>
        public void OnEndTurnClicked()
        {
            _flowService.EndTurn();
            RenderAsync(_presenterCts.Token).Forget();
        }

        /// <summary>
        /// スナップショット反映処理
        /// </summary>
        private async UniTask RenderAsync(CancellationToken ct)
        {
            if (_view == null)
            {
                return;
            }

            BattleSceneSnapshot snapshot = _flowService.CreateSnapshot();
            _battlePagePresenter.Clear();

            switch (snapshot.CurrentPage)
            {
                case BattleScenePage.Map:
                    await _uiCoordinator.ShowMapAsync(snapshot, OnMapNodeClicked, ct);
                    break;
                case BattleScenePage.Battle:
                    await _uiCoordinator.ShowBattleAsync(ct);
                    _battlePagePresenter.Render(snapshot);
                    break;
                case BattleScenePage.Reward:
                    RuntimeCard reward = await _uiCoordinator.ShowRewardAsync(snapshot, ct);
                    if (reward != null)
                    {
                        _flowService.SelectReward(reward);
                        await RenderAsync(ct);
                    }
                    break;
                case BattleScenePage.RestShop:
                    RestShopDialogAction action = await _uiCoordinator.ShowRestShopAsync(snapshot, ct);
                    ApplyRestShopAction(action);
                    await RenderAsync(ct);
                    break;
                case BattleScenePage.Result:
                    await _uiCoordinator.ShowResultAsync(snapshot, ct);
                    _onResultBackClicked?.Invoke();
                    break;
            }
        }

        /// <summary>
        /// 補給ダイアログ結果適用
        /// </summary>
        private void ApplyRestShopAction(RestShopDialogAction action)
        {
            switch (action)
            {
                case RestShopDialogAction.Rest:
                    _flowService.ApplyRest();
                    break;
                case RestShopDialogAction.Upgrade:
                    _flowService.ApplyUpgrade();
                    break;
                case RestShopDialogAction.Shop:
                    _flowService.ApplyShopPurchase();
                    break;
                case RestShopDialogAction.Continue:
                    _flowService.ContinueFromRestShop();
                    break;
            }
        }
    }
}
