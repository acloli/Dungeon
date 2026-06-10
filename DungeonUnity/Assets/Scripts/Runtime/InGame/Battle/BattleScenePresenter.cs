using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Battle.View;
using Dungeon.Runtime.InGame.Save.Model;
using Dungeon.Runtime.InGame.Save.Services;

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
        private readonly IRunSaveService _runSaveService;
        private readonly CancellationTokenSource _presenterCts = new CancellationTokenSource();

        private IBattleSceneHostView _view;
        private Action _onResultBackClicked;
        private Action _onSaveQuitClicked;

        public BattleScenePresenter(
            IBattleSceneFlowService flowService,
            BattlePagePresenter battlePagePresenter,
            IBattleSceneUiCoordinator uiCoordinator,
            IRunSaveService runSaveService = null)
        {
            _flowService = flowService;
            _battlePagePresenter = battlePagePresenter;
            _uiCoordinator = uiCoordinator;
            _runSaveService = runSaveService;
        }

        /// <summary>
        /// View接続初期化
        /// </summary>
        public async UniTask InitializeAsync(IBattleSceneHostView view, int runProfileId, Action onResultBackClicked, CancellationToken ct, Action onSaveQuitClicked = null)
        {
            _view = view;
            _onResultBackClicked = onResultBackClicked;
            _onSaveQuitClicked = onSaveQuitClicked;

            _battlePagePresenter.Initialize(_view.BattlePageView, OnHandCardClicked, OnEnemyTargetClicked, OnEndTurnClicked);
            _view.WireSaveQuitButton(OnSaveQuitClicked);
            await _uiCoordinator.InitializeAsync(view, ct);

            if (_runSaveService != null && _runSaveService.HasSavedRun())
            {
                RunSaveData saveData = await _runSaveService.LoadCurrentRunAsync(ct);
                if (saveData != null && saveData.IsValid)
                {
                    _flowService.InitializeFromSave(saveData);
                }
                else
                {
                    _runSaveService.DeleteSavedRun();
                    _flowService.Initialize(runProfileId);
                }
            }
            else
            {
                _flowService.Initialize(runProfileId);
            }

            await RenderAsync(ct);
        }

        /// <summary>
        /// View切り離し処理
        /// </summary>
        public void Dispose()
        {
            _battlePagePresenter.Dispose();
            _presenterCts.Cancel();
            if (_view != null)
            {
                _view.UnwireSaveQuitButton();
            }
            _uiCoordinator.Dispose();
            _view = null;
            _onResultBackClicked = null;
            _onSaveQuitClicked = null;
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
            _flowService.TryPlaySelectedCard();
            RenderAsync(_presenterCts.Token).Forget();
        }

        /// <summary>
        /// 敵対象クリック通知
        /// </summary>
        public void OnEnemyTargetClicked(int index)
        {
            _flowService.SelectEnemyTarget(index);
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
        /// 中断ボタン通知
        /// </summary>
        public void OnSaveQuitClicked()
        {
            _onSaveQuitClicked?.Invoke();
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
                    _view.SetSaveQuitVisible(true);
                    await _uiCoordinator.ShowMapAsync(snapshot, OnMapNodeClicked, ct);
                    break;
                case BattleScenePage.Battle:
                    _view.SetSaveQuitVisible(false);
                    await _uiCoordinator.ShowBattleAsync(ct);
                    _battlePagePresenter.Render(snapshot);
                    break;
                case BattleScenePage.Reward:
                    _view.SetSaveQuitVisible(false);
                    RuntimeRewardEntry rewardEntry = await _uiCoordinator.ShowRewardAsync(snapshot, ct);
                    if (rewardEntry != null)
                    {
                        _flowService.SelectReward(rewardEntry);
                        await RenderAsync(ct);
                    }
                    break;
                case BattleScenePage.RestShop:
                    _view.SetSaveQuitVisible(true);
                    RestShopDialogAction action = await _uiCoordinator.ShowRestShopAsync(snapshot, ct);
                    ApplyRestShopAction(action);
                    await RenderAsync(ct);
                    break;
                case BattleScenePage.Shop:
                    _view.SetSaveQuitVisible(true);
                    ShopDialogResult shopResult = await _uiCoordinator.ShowShopAsync(snapshot, ct);
                    ApplyShopAction(shopResult);
                    await RenderAsync(ct);
                    break;
                case BattleScenePage.CardSelect:
                    _view.SetSaveQuitVisible(true);
                    CardSelectDialogResult cardSelectResult = await _uiCoordinator.ShowCardSelectAsync(snapshot, _flowService.GetDeckCards(), ct);
                    if (cardSelectResult.IsCanceled)
                    {
                        // 削除キャンセル時はショップに戻る
                        _flowService.OpenShop();
                    }
                    else
                    {
                        _flowService.PurchaseCardRemoval(cardSelectResult.SelectedCard);
                    }
                    await RenderAsync(ct);
                    break;
                case BattleScenePage.Event:
                    _view.SetSaveQuitVisible(false);
                    EventDialogResult eventResult = await _uiCoordinator.ShowEventAsync(snapshot, ct);
                    _flowService.SelectEventChoice(eventResult.ChoiceId);
                    await RenderAsync(ct);
                    break;
                case BattleScenePage.Result:
                    _view.SetSaveQuitVisible(false);
                    await _uiCoordinator.ShowResultAsync(snapshot, ct);
                    _onResultBackClicked?.Invoke();
                    break;
            }
        }

        /// <summary>
        /// ショップダイアログ結果適用
        /// </summary>
        private void ApplyShopAction(ShopDialogResult result)
        {
            switch (result.Action)
            {
                case ShopDialogActionType.Leave:
                    _flowService.LeaveShop();
                    break;
                case ShopDialogActionType.PurchaseItem:
                    _flowService.PurchaseShopItem(result.SlotIndex);
                    break;
                case ShopDialogActionType.PurchaseCardRemoval:
                    _flowService.OpenCardRemoval();
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
                    _flowService.OpenShop();
                    break;
                case RestShopDialogAction.Continue:
                    _flowService.ContinueFromRestShop();
                    break;
            }
        }
    }
}
