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
        UniTask ShowMapAsync(BattleSceneSnapshot snapshot, Action<int> onMapNodeClicked, CancellationToken ct);
        UniTask ShowBattleAsync(CancellationToken ct);
        UniTask<RewardDialogResult> ShowRewardAsync(BattleSceneSnapshot snapshot, CancellationToken ct);
        UniTask<RestShopDialogAction> ShowRestShopAsync(BattleSceneSnapshot snapshot, CancellationToken ct);
        UniTask ShowResultAsync(BattleSceneSnapshot snapshot, CancellationToken ct);
        UniTask<ShopDialogResult> ShowShopAsync(BattleSceneSnapshot snapshot, CancellationToken ct);
        UniTask<CardSelectDialogResult> ShowCardSelectAsync(BattleSceneSnapshot snapshot, IReadOnlyList<RuntimeCard> deckCards, CancellationToken ct);
        UniTask<EventDialogResult> ShowEventAsync(BattleSceneSnapshot snapshot, CancellationToken ct);
        UniTask<RuntimeRewardEntry> ShowCardPickAsync(BattleSceneSnapshot snapshot, CancellationToken ct);
        UniTask<PotionReplaceDialogResult> ShowPotionReplaceAsync(BattleSceneSnapshot snapshot, CancellationToken ct);
        void Dispose();
    }
}
