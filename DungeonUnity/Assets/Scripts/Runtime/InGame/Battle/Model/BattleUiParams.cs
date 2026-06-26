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
        public BattleMapPageParam(BattleMapSnapshot snapshot, Action<int> onMapNodeClicked)
        {
            Snapshot = snapshot;
            OnMapNodeClicked = onMapNodeClicked;
        }

        public BattleMapSnapshot Snapshot { get; }
        public Action<int> OnMapNodeClicked { get; }
    }

    /// <summary>
    /// 報酬ダイアログ表示パラメータクラス
    /// </summary>
    public sealed class BattleRewardDialogParam
    {
        public BattleRewardDialogParam(BattleRewardSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public BattleRewardSnapshot Snapshot { get; }
    }

    /// <summary>
    /// カード選択ダイアログ表示パラメータクラス
    /// </summary>
    public sealed class BattleCardPickDialogParam
    {
        public BattleCardPickDialogParam(BattleRewardSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public BattleRewardSnapshot Snapshot { get; }
    }

    /// <summary>
    /// 補給ダイアログ表示パラメータクラス
    /// </summary>
    public sealed class BattleRestShopDialogParam
    {
        public BattleRestShopDialogParam(BattleRestShopSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public BattleRestShopSnapshot Snapshot { get; }
    }

    /// <summary>
    /// 結果ダイアログ表示パラメータクラス
    /// </summary>
    public sealed class BattleResultDialogParam
    {
        public BattleResultDialogParam(BattleResultSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public BattleResultSnapshot Snapshot { get; }
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
        public BattleShopDialogParam(BattleShopSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public BattleShopSnapshot Snapshot { get; }
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
    /// カード選択ダイアログの用途種別
    /// </summary>
    public enum CardSelectMode
    {
        CardRemoval,
        Upgrade
    }

    /// <summary>
    /// カード選択ダイアログ更新データクラス
    /// </summary>
    public sealed class BattleCardSelectDialogRefreshData
    {
        public BattleCardSelectDialogRefreshData(
            IReadOnlyList<RuntimeCard> deckCards,
            IReadOnlyDictionary<int, int> cardPrices,
            IReadOnlyDictionary<int, RuntimeCard> upgradedCards,
            int gold,
            string message)
        {
            DeckCards = deckCards ?? Array.Empty<RuntimeCard>();
            CardPrices = cardPrices ?? new Dictionary<int, int>();
            UpgradedCards = upgradedCards ?? new Dictionary<int, RuntimeCard>();
            Gold = gold;
            Message = message ?? string.Empty;
        }

        public IReadOnlyList<RuntimeCard> DeckCards { get; }
        public IReadOnlyDictionary<int, int> CardPrices { get; }
        public IReadOnlyDictionary<int, RuntimeCard> UpgradedCards { get; }
        public int Gold { get; }
        public string Message { get; }
    }

    /// <summary>
    /// カード選択ダイアログ表示パラメータクラス
    /// </summary>
    public sealed class BattleCardSelectDialogParam
    {
        public BattleCardSelectDialogParam(
            int gold,
            IReadOnlyList<RuntimeCard> deckCards,
            CardSelectMode mode,
            bool showPrice,
            IReadOnlyDictionary<int, int> cardPrices,
            IReadOnlyDictionary<int, RuntimeCard> upgradedCards,
            string message,
            Func<RuntimeCard, BattleCardSelectDialogRefreshData> onCardConfirmed)
        {
            Gold = gold;
            DeckCards = deckCards ?? Array.Empty<RuntimeCard>();
            Mode = mode;
            ShowPrice = showPrice;
            CardPrices = cardPrices ?? new Dictionary<int, int>();
            UpgradedCards = upgradedCards ?? new Dictionary<int, RuntimeCard>();
            Message = message ?? string.Empty;
            OnCardConfirmed = onCardConfirmed;
        }

        public int Gold { get; }
        public IReadOnlyList<RuntimeCard> DeckCards { get; }
        public CardSelectMode Mode { get; }
        public bool ShowPrice { get; }
        public IReadOnlyDictionary<int, int> CardPrices { get; }
        public IReadOnlyDictionary<int, RuntimeCard> UpgradedCards { get; }
        public string Message { get; }
        public Func<RuntimeCard, BattleCardSelectDialogRefreshData> OnCardConfirmed { get; }
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
        public BattlePotionReplaceDialogParam(BattlePotionReplaceSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public BattlePotionReplaceSnapshot Snapshot { get; }
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
        public BattleEventDialogParam(BattleEventSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public BattleEventSnapshot Snapshot { get; }
    }
}
