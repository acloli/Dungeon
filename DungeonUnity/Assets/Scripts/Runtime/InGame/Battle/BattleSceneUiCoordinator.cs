using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.View;
using Dungeon.Runtime.InGame.Domain;
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
            if (_uiService.CurrentPage is MapPageView currentMapPage)
            {
                currentMapPage.Apply(param);
                return;
            }

            await _uiService.ShowPageAsync<MapPageView>(param, ct);
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
        public async UniTask<CardDefinition> ShowRewardAsync(BattleSceneSnapshot snapshot, CancellationToken ct)
        {
            await _uiService.ClearStackAsync(ct);
            if (_hostView != null)
            {
                _hostView.SetBattleVisible(false);
            }

            return await _uiService.ShowDialogAsync<RewardDialogView, CardDefinition>(
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

            return await _uiService.ShowDialogAsync<RestShopDialogView, RestShopDialogAction>(
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

            await _uiService.ShowDialogAsync<ResultDialogView>(
                BattleDialogOpenParams.SingleUse(new BattleResultDialogParam(snapshot)),
                ct);
        }

        /// <summary>
        /// UI切り離し処理
        /// </summary>
        public void Dispose()
        {
            if (_hostView != null)
            {
                _hostView.SetBattleVisible(false);
            }

            _hostView = null;
        }
    }
}
