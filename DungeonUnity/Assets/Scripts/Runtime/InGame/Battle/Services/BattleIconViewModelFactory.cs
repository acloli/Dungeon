using Dungeon.Runtime.Common.UI.MultiIcon;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// 戦闘データを共通アイコンの表示情報へ変換
    /// </summary>
    public static class BattleIconViewModelFactory
    {
        public static MultiIconViewModel CreateCard(
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

            return new MultiIconViewModel(
                MultiIconKind.Card,
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

        public static MultiIconViewModel CreateRelic(RuntimeRelic relic, bool isInteractable = true, bool isSelected = false, bool isAffordable = true)
        {
            if (relic == null)
            {
                return null;
            }

            return new MultiIconViewModel(
                MultiIconKind.Relic,
                relic.DisplayName,
                relic.Description,
                relic.ImageId,
                relic.Rarity,
                isInteractable: isInteractable,
                isSelected: isSelected,
                isAffordable: isAffordable);
        }

        public static MultiIconViewModel CreatePotion(RuntimePotion potion, bool isInteractable = true, bool isSelected = false, bool isAffordable = true)
        {
            if (potion == null)
            {
                return null;
            }

            return new MultiIconViewModel(
                MultiIconKind.Potion,
                potion.DisplayName,
                potion.Description,
                potion.ImageId,
                potion.Rarity,
                isInteractable: isInteractable,
                isSelected: isSelected,
                isAffordable: isAffordable);
        }

        public static MultiIconViewModel CreatePlaceholder(
            MultiIconKind iconKind,
            string displayName,
            CardRarity rarity,
            bool isInteractable = true,
            bool isAffordable = true)
        {
            return new MultiIconViewModel(
                iconKind,
                displayName,
                string.Empty,
                string.Empty,
                rarity,
                isInteractable: isInteractable,
                isAffordable: isAffordable);
        }
    }
}
