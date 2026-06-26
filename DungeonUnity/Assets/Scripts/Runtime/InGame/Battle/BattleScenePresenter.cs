using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Battle.View;
using Dungeon.Runtime.InGame.Save.Model;
using Dungeon.Runtime.InGame.Save.Services;
using UnityEngine;

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
        private BattleSceneSnapshot _lastSnapshot;
        private Action _onResultBackClicked;
        private Action _onSaveQuitClicked;
        private bool _isRendering;
        private bool _renderRequested;

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
            _view.WireHostBackgroundClick(OnHostBackgroundClicked);
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
                _view.ClearOwnedRelics();
                _view.SetOwnedRelicHint(string.Empty, BattleSceneConstants.UnselectedCardIndex);
                _view.ClearOwnedPotions();
                _view.SetOwnedPotionHint(string.Empty, BattleSceneConstants.UnselectedCardIndex);
                _view.SetOwnedPotionUseVisible(false, null);
                _view.UnwireHostBackgroundClick();
            }
            _uiCoordinator.Dispose();
            _lastSnapshot = null;
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
            RequestRender();
        }

        /// <summary>
        /// 手札選択通知
        /// </summary>
        public void OnHandCardClicked(int index)
        {
            _flowService.SelectHandCard(index);
            _flowService.TryPlaySelectedCard();
            RequestRender();
        }

        /// <summary>
        /// 敵対象クリック通知
        /// </summary>
        public void OnEnemyTargetClicked(int index)
        {
            _flowService.SelectEnemyTarget(index);
            RequestRender();
        }

        /// <summary>
        /// 所持レリッククリック通知
        /// </summary>
        public void OnOwnedRelicClicked(int index)
        {
            _flowService.InspectOwnedRelic(index);
            RequestRender();
        }

        /// <summary>
        /// 所持薬水クリック通知
        /// </summary>
        public void OnOwnedPotionClicked(int index)
        {
            _flowService.InspectOwnedPotion(index);
            RequestRender();
        }

        /// <summary>
        /// 薬水使用通知
        /// </summary>
        public void OnUsePotionClicked()
        {
            if (_lastSnapshot == null || _lastSnapshot.HostChrome.SelectedOwnedPotionIndex < 0)
            {
                return;
            }

            _flowService.UsePotion(_lastSnapshot.HostChrome.SelectedOwnedPotionIndex);
            RequestRender();
        }

        /// <summary>
        /// ターン終了通知
        /// </summary>
        public void OnEndTurnClicked()
        {
            _flowService.EndTurn();
            RequestRender();
        }

        /// <summary>
        /// 中断ボタン通知
        /// </summary>
        public void OnSaveQuitClicked()
        {
            _onSaveQuitClicked?.Invoke();
        }

        /// <summary>
        /// Host空き領域クリック通知
        /// </summary>
        public void OnHostBackgroundClicked()
        {
            _flowService.ClearOwnedInspections();
            RequestRender();
        }

        private void RequestRender()
        {
            _renderRequested = true;
            if (_isRendering)
            {
                return;
            }

            RunRenderLoopAsync(_presenterCts.Token).Forget(Debug.LogException);
        }

        private async UniTask RunRenderLoopAsync(CancellationToken ct)
        {
            while (_renderRequested && !ct.IsCancellationRequested)
            {
                _renderRequested = false;
                _isRendering = true;
                try
                {
                    await RenderAsync(ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                finally
                {
                    _isRendering = false;
                }
            }
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
            _lastSnapshot = snapshot;
            _battlePagePresenter.Clear();
            RenderHostChrome(snapshot);

            if (await TryResolvePotionOfferAsync(snapshot, ct))
            {
                await RenderAsync(ct);
                return;
            }

            switch (snapshot.CurrentPage)
            {
                case BattleScenePage.Map:
                    _view.SetSaveQuitVisible(true);
                    await _uiCoordinator.ShowMapAsync(snapshot.Map, OnMapNodeClicked, ct);
                    break;
                case BattleScenePage.Battle:
                    _view.SetSaveQuitVisible(false);
                    await _uiCoordinator.ShowBattleAsync(ct);
                    _battlePagePresenter.Render(snapshot.Combat);
                    break;
                case BattleScenePage.Reward:
                    _view.SetSaveQuitVisible(false);
                    bool rewardActive = true;
                    while (rewardActive)
                    {
                        RewardDialogResult rewardResult = await _uiCoordinator.ShowRewardAsync(snapshot.Reward, ct);
                        switch (rewardResult.Action)
                        {
                            case RewardDialogActionType.ClaimGold:
                                _flowService.ClaimGold();
                                snapshot = _flowService.CreateSnapshot();
                                break;
                            case RewardDialogActionType.ClaimPotion:
                                _flowService.ClaimPotion();
                                snapshot = _flowService.CreateSnapshot();
                                break;
                            case RewardDialogActionType.ClaimRelic:
                                _flowService.ClaimRelic();
                                snapshot = _flowService.CreateSnapshot();
                                break;
                            case RewardDialogActionType.PickCard:
                                RuntimeRewardEntry cardEntry = await _uiCoordinator.ShowCardPickAsync(snapshot.Reward, ct);
                                if (cardEntry != null)
                                {
                                    _flowService.SelectReward(cardEntry);
                                    snapshot = _flowService.CreateSnapshot();
                                }
                                break;
                            case RewardDialogActionType.Continue:
                                _flowService.ContinueFromReward();
                                rewardActive = false;
                                break;
                        }

                        if (rewardActive && await TryResolvePotionOfferAsync(snapshot, ct))
                        {
                            snapshot = _flowService.CreateSnapshot();
                            _lastSnapshot = snapshot;
                            RenderHostChrome(snapshot);
                        }
                    }
                    await RenderAsync(ct);
                    break;
                case BattleScenePage.RestShop:
                    _view.SetSaveQuitVisible(true);
                    RestShopDialogAction action = await _uiCoordinator.ShowRestShopAsync(snapshot.RestShop, ct);
                    ApplyRestShopAction(action);
                    await RenderAsync(ct);
                    break;
                case BattleScenePage.Shop:
                    _view.SetSaveQuitVisible(true);
                    ShopDialogResult shopResult = await _uiCoordinator.ShowShopAsync(snapshot.Shop, ct);
                    ApplyShopAction(shopResult);
                    await RenderAsync(ct);
                    break;
                case BattleScenePage.CardSelect:
                    _view.SetSaveQuitVisible(true);
                    CardSelectMode cardSelectMode = _flowService.GetCardSelectMode();
                    BattleCardSelectDialogParam cardSelectParam = CreateCardSelectDialogParam(snapshot, cardSelectMode);
                    CardSelectDialogResult cardSelectResult = await _uiCoordinator.ShowCardSelectAsync(cardSelectParam, ct);
                    if (cardSelectResult.IsCanceled)
                    {
                        _flowService.CancelCardSelect();
                    }
                    else
                    {
                        _flowService.ConfirmCardSelect(cardSelectResult.SelectedCard);
                    }
                    await RenderAsync(ct);
                    break;
                case BattleScenePage.Event:
                    _view.SetSaveQuitVisible(false);
                    EventDialogResult eventResult = await _uiCoordinator.ShowEventAsync(snapshot.Event, ct);
                    _flowService.SelectEventChoice(eventResult.ChoiceId);
                    await RenderAsync(ct);
                    break;
                case BattleScenePage.Result:
                    _view.SetSaveQuitVisible(false);
                    await _uiCoordinator.ShowResultAsync(snapshot.Result, ct);
                    _onResultBackClicked?.Invoke();
                    break;
            }
        }

        private BattleCardSelectDialogRefreshData OnUpgradeCardConfirmed(RuntimeCard card)
        {
            _flowService.ConfirmCardSelect(card);
            BattleSceneSnapshot snapshot = _flowService.CreateSnapshot();
            return new BattleCardSelectDialogRefreshData(
                _flowService.GetCardSelectCards(),
                _flowService.GetCardSelectPrices(),
                _flowService.GetCardSelectUpgradedCards(),
                snapshot.Shop.Gold,
                _flowService.GetCardSelectMessage());
        }

        private BattleCardSelectDialogParam CreateCardSelectDialogParam(BattleSceneSnapshot snapshot, CardSelectMode cardSelectMode)
        {
            return new BattleCardSelectDialogParam(
                snapshot.Shop.Gold,
                _flowService.GetCardSelectCards(),
                cardSelectMode,
                cardSelectMode == CardSelectMode.Upgrade,
                _flowService.GetCardSelectPrices(),
                _flowService.GetCardSelectUpgradedCards(),
                _flowService.GetCardSelectMessage(),
                cardSelectMode == CardSelectMode.Upgrade ? OnUpgradeCardConfirmed : null);
        }

        private void RenderHostChrome(BattleSceneSnapshot snapshot)
        {
            BattleHostChromeSnapshot hostChrome = snapshot.HostChrome;
            _view.ClearOwnedRelics();
            _view.BuildOwnedRelics(hostChrome.OwnedRelics, OnOwnedRelicClicked);
            _view.SetOwnedRelicHint(hostChrome.OwnedRelicHintMessage, hostChrome.SelectedOwnedRelicIndex);

            _view.ClearOwnedPotions();
            _view.BuildOwnedPotions(hostChrome.OwnedPotions, OnOwnedPotionClicked);
            _view.SetOwnedPotionHint(hostChrome.OwnedPotionHintMessage, hostChrome.SelectedOwnedPotionIndex);
            _view.SetOwnedPotionUseVisible(hostChrome.CanUseSelectedPotion, OnUsePotionClicked);
        }

        private async UniTask<bool> TryResolvePotionOfferAsync(BattleSceneSnapshot snapshot, CancellationToken ct)
        {
            if (snapshot.PotionReplace.PendingPotionOffer != null)
            {
                PotionReplaceDialogResult result = await _uiCoordinator.ShowPotionReplaceAsync(snapshot.PotionReplace, ct);
                if (result.IsCanceled)
                {
                    _flowService.CancelPendingPotionReplace();
                }
                else
                {
                    _flowService.ReplaceOwnedPotion(result.SelectedPotionIndex);
                }

                return true;
            }

            return false;
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
