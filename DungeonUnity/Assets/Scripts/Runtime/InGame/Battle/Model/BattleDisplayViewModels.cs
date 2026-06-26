using System;
using System.Collections.Generic;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Model
{
    /// <summary>
    /// 敵意図表示用モデル
    /// </summary>
    public sealed class BattleIntentViewModel
    {
        public BattleIntentViewModel(IntentType intentType, string intentName, int damage, int hitCount, int block, StatusType statusType, string statusName, int statusValue, BuffType buffType, string buffName, int buffValue)
        {
            IntentType = intentType;
            IntentName = intentName;
            Damage = damage;
            HitCount = hitCount;
            Block = block;
            StatusType = statusType;
            StatusName = statusName;
            StatusValue = statusValue;
            BuffType = buffType;
            BuffName = buffName;
            BuffValue = buffValue;
        }

        public static BattleIntentViewModel FromAction(RuntimeEnemyAction action, string intentName, string statusName, string buffName)
        {
            if (action == null)
            {
                return null;
            }

            return new BattleIntentViewModel(
                action.IntentType,
                intentName,
                action.Damage,
                action.HitCount,
                action.Block,
                action.StatusType,
                statusName,
                action.StatusValue,
                action.BuffType,
                buffName,
                action.BuffValue);
        }

        public IntentType IntentType { get; }
        public string IntentName { get; }
        public int Damage { get; }
        public int HitCount { get; }
        public int Block { get; }
        public StatusType StatusType { get; }
        public string StatusName { get; }
        public int StatusValue { get; }
        public BuffType BuffType { get; }
        public string BuffName { get; }
        public int BuffValue { get; }
    }

    /// <summary>
    /// 状態表示用モデル
    /// </summary>
    public sealed class BattleStatusViewModel
    {
        public BattleStatusViewModel(string name, int value, bool isBuff)
        {
            Name = name;
            Value = value;
            IsBuff = isBuff;
        }

        public string Name { get; }
        public int Value { get; }
        public bool IsBuff { get; }
    }

    /// <summary>
    /// 戦闘HUD表示用モデル
    /// </summary>
    public sealed class BattleHudViewModel
    {
        public BattleHudViewModel(string playerSummary, string enemySummary, string intentSummary, IReadOnlyList<BattleStatusViewModel> playerStatuses, IReadOnlyList<BattleStatusViewModel> playerBuffs, IReadOnlyList<BattleStatusViewModel> enemyStatuses, IReadOnlyList<BattleStatusViewModel> enemyBuffs)
        {
            PlayerSummary = playerSummary;
            EnemySummary = enemySummary;
            IntentSummary = intentSummary;
            PlayerStatuses = playerStatuses ?? Array.Empty<BattleStatusViewModel>();
            PlayerBuffs = playerBuffs ?? Array.Empty<BattleStatusViewModel>();
            EnemyStatuses = enemyStatuses ?? Array.Empty<BattleStatusViewModel>();
            EnemyBuffs = enemyBuffs ?? Array.Empty<BattleStatusViewModel>();
        }

        public string PlayerSummary { get; }
        public string EnemySummary { get; }
        public string IntentSummary { get; }
        public IReadOnlyList<BattleStatusViewModel> PlayerStatuses { get; }
        public IReadOnlyList<BattleStatusViewModel> PlayerBuffs { get; }
        public IReadOnlyList<BattleStatusViewModel> EnemyStatuses { get; }
        public IReadOnlyList<BattleStatusViewModel> EnemyBuffs { get; }
    }

    /// <summary>
    /// 敵表示用モデル
    /// </summary>
    public sealed class BattleEnemyViewModel
    {
        public BattleEnemyViewModel(
            int slotIndex,
            string displayName,
            int hp,
            int block,
            bool isDefeated,
            BattleIntentViewModel intent,
            IReadOnlyList<BattleStatusViewModel> statuses,
            IReadOnlyList<BattleStatusViewModel> buffs)
        {
            SlotIndex = slotIndex;
            DisplayName = displayName;
            Hp = hp;
            Block = block;
            IsDefeated = isDefeated;
            Intent = intent;
            Statuses = statuses ?? Array.Empty<BattleStatusViewModel>();
            Buffs = buffs ?? Array.Empty<BattleStatusViewModel>();
        }

        public int SlotIndex { get; }
        public string DisplayName { get; }
        public int Hp { get; }
        public int Block { get; }
        public bool IsDefeated { get; }
        public BattleIntentViewModel Intent { get; }
        public IReadOnlyList<BattleStatusViewModel> Statuses { get; }
        public IReadOnlyList<BattleStatusViewModel> Buffs { get; }
    }

    /// <summary>
    /// 汎用アイコン種別
    /// </summary>
    public enum BattleIconKind
    {
        None = 0,
        Card = 1,
        Relic = 2,
        Potion = 3,
        Resource = 4
    }

    /// <summary>
    /// 汎用アイコン表示用モデル
    /// </summary>
    public sealed class BattleMultiIconViewModel
    {
        public BattleMultiIconViewModel(
            BattleIconKind iconKind,
            string displayName,
            string description,
            string imageId,
            CardRarity rarity,
            int cost = 0,
            bool showCost = false,
            bool isInteractable = true,
            bool isSelected = false,
            bool isAffordable = true,
            int quantity = 0,
            bool showQuantity = false,
            string footerLabel = "",
            bool showFooterLabel = false)
        {
            IconKind = iconKind;
            DisplayName = displayName ?? string.Empty;
            Description = description ?? string.Empty;
            ImageId = imageId ?? string.Empty;
            Rarity = rarity;
            Cost = cost;
            ShowCost = showCost;
            IsInteractable = isInteractable;
            IsSelected = isSelected;
            IsAffordable = isAffordable;
            Quantity = quantity;
            ShowQuantity = showQuantity;
            FooterLabel = footerLabel ?? string.Empty;
            ShowFooterLabel = showFooterLabel;
        }

        public static BattleMultiIconViewModel CreateCard(
            RuntimeCard card,
            bool isAffordable = true,
            bool isInteractable = true,
            bool isSelected = false,
            int price = 0,
            bool showPrice = false)
        {
            if (card == null)
            {
                return null;
            }

            return new BattleMultiIconViewModel(
                BattleIconKind.Card,
                card.DisplayName,
                card.Description,
                card.ImageId,
                card.Rarity,
                cost: card.Cost,
                showCost: true,
                isInteractable: isInteractable,
                isSelected: isSelected,
                isAffordable: isAffordable,
                footerLabel: showPrice ? price.ToString() : string.Empty,
                showFooterLabel: showPrice);
        }

        public static BattleMultiIconViewModel CreateRelic(RuntimeRelic relic, bool isInteractable = true, bool isSelected = false, bool isAffordable = true)
        {
            if (relic == null)
            {
                return null;
            }

            return new BattleMultiIconViewModel(
                BattleIconKind.Relic,
                relic.DisplayName,
                relic.Description,
                relic.ImageId,
                relic.Rarity,
                isInteractable: isInteractable,
                isSelected: isSelected,
                isAffordable: isAffordable);
        }

        public static BattleMultiIconViewModel CreatePotion(RuntimePotion potion, bool isInteractable = true, bool isSelected = false, bool isAffordable = true)
        {
            if (potion == null)
            {
                return null;
            }

            return new BattleMultiIconViewModel(
                BattleIconKind.Potion,
                potion.DisplayName,
                potion.Description,
                potion.ImageId,
                potion.Rarity,
                isInteractable: isInteractable,
                isSelected: isSelected,
                isAffordable: isAffordable);
        }

        public static BattleMultiIconViewModel CreatePlaceholder(
            BattleIconKind iconKind,
            string displayName,
            CardRarity rarity,
            bool isInteractable = true,
            bool isAffordable = true)
        {
            return new BattleMultiIconViewModel(
                iconKind,
                displayName,
                string.Empty,
                string.Empty,
                rarity,
                isInteractable: isInteractable,
                isAffordable: isAffordable);
        }

        public BattleIconKind IconKind { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public string ImageId { get; }
        public CardRarity Rarity { get; }
        public int Cost { get; }
        public bool ShowCost { get; }
        public bool IsInteractable { get; }
        public bool IsSelected { get; }
        public bool IsAffordable { get; }
        public int Quantity { get; }
        public bool ShowQuantity { get; }
        public string FooterLabel { get; }
        public bool ShowFooterLabel { get; }
    }

    /// <summary>
    /// 戦闘手札表示用モデル
    /// </summary>
    public sealed class BattleHandCardViewModel
    {
        public BattleHandCardViewModel(RuntimeCard card, BattleMultiIconViewModel icon)
        {
            Card = card;
            Icon = icon;
        }

        public RuntimeCard Card { get; }
        public BattleMultiIconViewModel Icon { get; }
    }

    /// <summary>
    /// ショップ商品表示用モデル
    /// </summary>
    public sealed class BattleShopItemViewModel
    {
        public BattleShopItemViewModel(
            int slotIndex,
            RewardType rewardType,
            string displayName,
            int price,
            bool isSoldOut,
            RuntimeCard card,
            RuntimeRelic relic,
            RuntimePotion potion,
            int itemId,
            BattleMultiIconViewModel icon)
        {
            SlotIndex = slotIndex;
            RewardType = rewardType;
            DisplayName = displayName;
            Price = price;
            IsSoldOut = isSoldOut;
            Card = card;
            Relic = relic;
            Potion = potion;
            ItemId = itemId;
            Icon = icon;
        }

        public int SlotIndex { get; }
        public RewardType RewardType { get; }
        public string DisplayName { get; }
        public int Price { get; }
        public bool IsSoldOut { get; }
        public RuntimeCard Card { get; }
        public RuntimeRelic Relic { get; }
        public RuntimePotion Potion { get; }
        public int ItemId { get; }
        public BattleMultiIconViewModel Icon { get; }
    }
}
