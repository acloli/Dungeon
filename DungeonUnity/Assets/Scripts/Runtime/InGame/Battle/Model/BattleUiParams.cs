using System;
using System.Collections.Generic;
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
    /// カード選択ダイアログ表示パラメータクラス
    /// </summary>
    public sealed class BattleCardPickDialogParam
    {
        public BattleCardPickDialogParam(BattleSceneSnapshot snapshot)
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
    /// 報酬ダイアログのアクション種別
    /// </summary>
    public enum RewardDialogActionType
    {
        PickCard,
        ClaimGold,
        ClaimPotion,
        ClaimRelic,
        Continue
    }

    public struct RewardDialogResult
    {
        public RewardDialogActionType Action;
    }

    /// <summary>
    /// ショップダイアログのアクション種別
    /// </summary>
    public enum ShopDialogActionType
    {
        Leave,
        PurchaseItem,
        PurchaseCardRemoval
    }

    /// <summary>
    /// ショップダイアログの返却結果
    /// </summary>
    public struct ShopDialogResult
    {
        public ShopDialogActionType Action;
        public int SlotIndex; // PurchaseItemの時に使用
    }

    /// <summary>
    /// ショップダイアログ表示パラメータクラス
    /// </summary>
    public sealed class BattleShopDialogParam
    {
        public BattleShopDialogParam(BattleSceneSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public BattleSceneSnapshot Snapshot { get; }
    }

    /// <summary>
    /// カード選択ダイアログの返却結果
    /// </summary>
    public struct CardSelectDialogResult
    {
        public bool IsCanceled;
        public RuntimeCard SelectedCard;
    }

    /// <summary>
    /// カード選択ダイアログ表示パラメータクラス
    /// </summary>
    public sealed class BattleCardSelectDialogParam
    {
        public BattleCardSelectDialogParam(BattleSceneSnapshot snapshot, IReadOnlyList<RuntimeCard> deckCards)
        {
            Snapshot = snapshot;
            DeckCards = deckCards ?? System.Array.Empty<RuntimeCard>();
        }

        public BattleSceneSnapshot Snapshot { get; }
        public System.Collections.Generic.IReadOnlyList<RuntimeCard> DeckCards { get; }
    }

    /// <summary>
    /// 薬水交換ダイアログの返却結果
    /// </summary>
    public struct PotionReplaceDialogResult
    {
        public bool IsCanceled;
        public int SelectedPotionIndex;
    }

    /// <summary>
    /// 薬水交換ダイアログ表示パラメータクラス
    /// </summary>
    public sealed class BattlePotionReplaceDialogParam
    {
        public BattlePotionReplaceDialogParam(BattleSceneSnapshot snapshot)
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

    /// <summary>
    /// イベントダイアログのアクション種別
    /// </summary>
    public enum EventDialogActionType
    {
        SelectChoice,
    }

    /// <summary>
    /// イベントダイアログの返却結果
    /// </summary>
    public struct EventDialogResult
    {
        public EventDialogActionType Action;
        public int ChoiceId;
    }

    /// <summary>
    /// イベントダイアログ表示パラメータクラス
    /// </summary>
    public sealed class BattleEventDialogParam
    {
        public BattleEventDialogParam(BattleSceneSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public BattleSceneSnapshot Snapshot { get; }
    }
}
