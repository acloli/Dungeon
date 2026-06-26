using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.View;
using TFramework.UI;

namespace Dungeon.Runtime.InGame.Battle
{
    /// <summary>
    /// BattleScene UI遷移調整クラス
    /// </summary>
    public sealed class BattleSceneUiCoordinator : IBattleSceneUiCoordinator
    {
        private readonly IUIService _uiService;
        private IBattleSceneHostView _hostView;

        public BattleSceneUiCoordinator(IUIService uiService)
        {
            _uiService = uiService;
        }

        /// <summary>
        /// 基底View接続初期化
        /// </summary>
        public async UniTask InitializeAsync(IBattleSceneHostView hostView, CancellationToken ct)
        {
            _hostView = hostView;
            _hostView.SetBattleVisible(false);
            _hostView.SetHostChromeInteractable(true);
            await _uiService.ClearStackAsync(ct);
        }

        /// <summary>
        /// マップページ表示
        /// </summary>
        public async UniTask ShowMapAsync(BattleSceneSnapshot snapshot, Action<int> onMapNodeClicked, CancellationToken ct)
        {
            if (_hostView == null)
            {
                return;
            }

            _hostView.SetBattleVisible(false);

            BattleMapPageParam param = new BattleMapPageParam(snapshot, onMapNodeClicked);
            if (_uiService.CurrentPage is MapPage currentMapPage)
            {
                currentMapPage.Apply(param);
                return;
            }

            await _uiService.ShowPageAsync<MapPage>(param, ct);
        }

        /// <summary>
        /// 戦闘基底表示
        /// </summary>
        public async UniTask ShowBattleAsync(CancellationToken ct)
        {
            if (_hostView == null)
            {
                return;
            }

            await _uiService.ClearStackAsync(ct);
            _hostView.SetBattleVisible(true);
        }

        /// <summary>
        /// 報酬ダイアログ表示
        /// </summary>
        public async UniTask<RewardDialogResult> ShowRewardAsync(BattleSceneSnapshot snapshot, CancellationToken ct)
        {
            await _uiService.ClearStackAsync(ct);
            if (_hostView != null)
            {
                _hostView.SetBattleVisible(false);
            }

            return await _uiService.ShowDialogAsync<RewardDialog, RewardDialogResult>(
                BattleDialogOpenParams.Cached(new BattleRewardDialogParam(snapshot)),
                ct);
        }

        /// <summary>
        /// 補給ダイアログ表示
        /// </summary>
        public async UniTask<RestShopDialogAction> ShowRestShopAsync(BattleSceneSnapshot snapshot, CancellationToken ct)
        {
            await _uiService.ClearStackAsync(ct);
            if (_hostView != null)
            {
                _hostView.SetBattleVisible(false);
            }

            return await _uiService.ShowDialogAsync<RestShopDialog, RestShopDialogAction>(
                BattleDialogOpenParams.Cached(new BattleRestShopDialogParam(snapshot)),
                ct);
        }

        /// <summary>
        /// 結果ダイアログ表示
        /// </summary>
        public async UniTask ShowResultAsync(BattleSceneSnapshot snapshot, CancellationToken ct)
        {
            await _uiService.ClearStackAsync(ct);
            if (_hostView != null)
            {
                _hostView.SetBattleVisible(false);
            }

            await _uiService.ShowDialogAsync<ResultDialog>(
                BattleDialogOpenParams.SingleUse(new BattleResultDialogParam(snapshot)),
                ct);
        }

        /// <summary>
        /// ショップダイアログ表示
        /// </summary>
        public async UniTask<ShopDialogResult> ShowShopAsync(BattleSceneSnapshot snapshot, CancellationToken ct)
        {
            await _uiService.ClearStackAsync(ct);
            if (_hostView != null)
            {
                _hostView.SetBattleVisible(false);
            }

            return await _uiService.ShowDialogAsync<ShopDialog, ShopDialogResult>(
                BattleDialogOpenParams.Cached(new BattleShopDialogParam(snapshot)),
                ct);
        }

        /// <summary>
        /// カード選択ダイアログ表示
        /// </summary>
        public async UniTask<CardSelectDialogResult> ShowCardSelectAsync(
            BattleSceneSnapshot snapshot,
            IReadOnlyList<RuntimeCard> deckCards,
            CardSelectMode mode,
            bool showPrice,
            IReadOnlyDictionary<int, int> cardPrices,
            string message,
            Func<RuntimeCard, BattleCardSelectDialogRefreshData> onCardSelected,
            CancellationToken ct)
        {
            await _uiService.ClearStackAsync(ct);
            if (_hostView != null)
            {
                _hostView.SetBattleVisible(false);
            }

            return await _uiService.ShowDialogAsync<CardSelectDialog, CardSelectDialogResult>(
                BattleDialogOpenParams.Cached(new BattleCardSelectDialogParam(
                    snapshot,
                    deckCards,
                    mode,
                    showPrice,
                    cardPrices,
                    message,
                    onCardSelected)),
                ct);
        }

        /// <summary>
        /// イベントダイアログ表示
        /// </summary>
        public async UniTask<EventDialogResult> ShowEventAsync(BattleSceneSnapshot snapshot, CancellationToken ct)
        {
            await _uiService.ClearStackAsync(ct);
            if (_hostView != null)
            {
                _hostView.SetBattleVisible(false);
            }

            return await _uiService.ShowDialogAsync<EventDialog, EventDialogResult>(
                BattleDialogOpenParams.SingleUse(new BattleEventDialogParam(snapshot)),
                ct);
        }

        /// <summary>
        /// カード選択ダイアログ表示
        /// </summary>
        public async UniTask<RuntimeRewardEntry> ShowCardPickAsync(BattleSceneSnapshot snapshot, CancellationToken ct)
        {
            return await ShowHostChromeModalAsync(
                () => _uiService.ShowDialogAsync<CardPickDialog, RuntimeRewardEntry>(
                    BattleDialogOpenParams.SingleUse(new BattleCardPickDialogParam(snapshot)),
                    ct));
        }

        /// <summary>
        /// 薬水交換ダイアログ表示
        /// </summary>
        public async UniTask<PotionReplaceDialogResult> ShowPotionReplaceAsync(BattleSceneSnapshot snapshot, CancellationToken ct)
        {
            return await ShowHostChromeModalAsync(
                () => _uiService.ShowDialogAsync<PotionReplaceDialog, PotionReplaceDialogResult>(
                    BattleDialogOpenParams.SingleUse(new BattlePotionReplaceDialogParam(snapshot)),
                    ct));
        }

        /// <summary>
        /// UI切り離し処理
        /// </summary>
        public void Dispose()
        {
            if (_hostView != null)
            {
                _hostView.SetBattleVisible(false);
                _hostView.SetHostChromeInteractable(true);
            }

            _hostView = null;
        }

        private async UniTask<TResult> ShowHostChromeModalAsync<TResult>(Func<UniTask<TResult>> showDialogAsync)
        {
            if (_hostView != null)
            {
                _hostView.SetHostChromeInteractable(false);
            }

            try
            {
                return await showDialogAsync();
            }
            finally
            {
                if (_hostView != null)
                {
                    _hostView.SetHostChromeInteractable(true);
                }
            }
        }
    }
}
