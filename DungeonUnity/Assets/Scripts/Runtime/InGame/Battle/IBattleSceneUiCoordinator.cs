using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.View;
using Dungeon.Runtime.InGame.Domain;

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
        UniTask<CardDefinition> ShowRewardAsync(BattleSceneSnapshot snapshot, CancellationToken ct);
        UniTask<RestShopDialogAction> ShowRestShopAsync(BattleSceneSnapshot snapshot, CancellationToken ct);
        UniTask ShowResultAsync(BattleSceneSnapshot snapshot, CancellationToken ct);
        void Dispose();
    }
}
