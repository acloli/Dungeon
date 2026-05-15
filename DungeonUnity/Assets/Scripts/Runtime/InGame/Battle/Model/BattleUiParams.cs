using System;
using TFramework.UI;

namespace Dungeon.Runtime.InGame.Battle.Model
{
    /// <summary>
    /// マップページ表示パラメータクラス
    /// </summary>
    public sealed class BattleMapPageParam
    {
        public BattleMapPageParam(BattleSceneSnapshot snapshot, Action<int> onMapNodeClicked)
        {
            Snapshot = snapshot;
            OnMapNodeClicked = onMapNodeClicked;
        }

        public BattleSceneSnapshot Snapshot { get; }
        public Action<int> OnMapNodeClicked { get; }
    }

    /// <summary>
    /// 報酬ダイアログ表示パラメータクラス
    /// </summary>
    public sealed class BattleRewardDialogParam
    {
        public BattleRewardDialogParam(BattleSceneSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public BattleSceneSnapshot Snapshot { get; }
    }

    /// <summary>
    /// 補給ダイアログ表示パラメータクラス
    /// </summary>
    public sealed class BattleRestShopDialogParam
    {
        public BattleRestShopDialogParam(BattleSceneSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public BattleSceneSnapshot Snapshot { get; }
    }

    /// <summary>
    /// 結果ダイアログ表示パラメータクラス
    /// </summary>
    public sealed class BattleResultDialogParam
    {
        public BattleResultDialogParam(BattleSceneSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public BattleSceneSnapshot Snapshot { get; }
    }

    /// <summary>
    /// ダイアログ表示要求生成クラス
    /// </summary>
    public static class BattleDialogOpenParams
    {
        public static UIDialogOpenParam Cached(object payload)
        {
            return new UIDialogOpenParam(payload, true);
        }

        public static UIDialogOpenParam SingleUse(object payload)
        {
            return new UIDialogOpenParam(payload, false);
        }
    }
}
