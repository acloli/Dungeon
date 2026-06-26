using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.View;

namespace Dungeon.Runtime.InGame.Battle
{
    /// <summary>
    /// BattleScene UI遷移調整インターフェース
    /// </summary>
    public interface IBattleSceneUiCoordinator
    {
        UniTask InitializeAsync(IBattleSceneHostView hostView, CancellationToken ct);
        UniTask ShowMapAsync(BattleMapSnapshot snapshot, Action<int> onMapNodeClicked, CancellationToken ct);
        UniTask ShowBattleAsync(CancellationToken ct);
        UniTask<RewardDialogResult> ShowRewardAsync(BattleRewardSnapshot snapshot, CancellationToken ct);
        UniTask<RestShopDialogAction> ShowRestShopAsync(BattleRestShopSnapshot snapshot, CancellationToken ct);
        UniTask ShowResultAsync(BattleResultSnapshot snapshot, CancellationToken ct);
        UniTask<ShopDialogResult> ShowShopAsync(BattleShopSnapshot snapshot, CancellationToken ct);
        UniTask<CardSelectDialogResult> ShowCardSelectAsync(BattleCardSelectDialogParam param, CancellationToken ct);
        UniTask<EventDialogResult> ShowEventAsync(BattleEventSnapshot snapshot, CancellationToken ct);
        UniTask<RuntimeRewardEntry> ShowCardPickAsync(BattleRewardSnapshot snapshot, CancellationToken ct);
        UniTask<PotionReplaceDialogResult> ShowPotionReplaceAsync(BattlePotionReplaceSnapshot snapshot, CancellationToken ct);
        void Dispose();
    }
}
