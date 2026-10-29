using Game.MasterData.Generated;

namespace Dungeon.Runtime.Common.UI.MultiIcon
{
    /// <summary>
    /// 汎用アイコン表示用モデル
    /// </summary>
    public sealed class MultiIconViewModel
    {
        public MultiIconViewModel(
            MultiIconKind iconKind,
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

        public MultiIconKind IconKind { get; }
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
}
