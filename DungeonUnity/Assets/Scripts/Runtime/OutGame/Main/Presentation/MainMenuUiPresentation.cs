using System;
using System.Collections.Generic;
using Dungeon.Runtime.Common.UI.MultiIcon;
using Game.MasterData.Generated;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Main.Presentation
{
    /// <summary>
    /// メニューの表示データを共通カードと一覧へ反映
    /// </summary>
    internal static class MainMenuUiPresentation
    {
        public static void Card(CardIconView view, MainMenuCardData card, bool upgraded = false, int quantity = 0)
        {
            bool discovered = card != null && card.IsDiscovered;
            view.Bind(new MultiIconViewModel(
                MultiIconKind.Card,
                discovered ? (upgraded ? card.UpgradedName ?? card.Name + "+" : card.Name) : string.Empty,
                discovered ? (upgraded ? card.UpgradedDescription : card.Description) : string.Empty,
                string.Empty, discovered ? card.Rarity : CardRarity.Basic,
                discovered ? (upgraded ? card.UpgradedCost : card.Cost) : 0,
                showCost: discovered, footerLabel: quantity > 0 ? "×" + quantity : string.Empty,
                showFooterLabel: quantity > 0), null);
            view.SetPresentation(card?.Artwork, card?.Category, discovered);
            view.SetFrameColor(card?.Rarity == CardRarity.Uncommon
                ? new Color(0.3f, 0.63f, 0.91f)
                : new Color(0.22f, 0.2f, 0.31f));
            view.GetComponent<Button>().interactable = discovered;
        }

    }
}
