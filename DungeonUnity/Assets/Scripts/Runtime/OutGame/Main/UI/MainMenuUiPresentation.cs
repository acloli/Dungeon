using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.View;
using Game.MasterData.Generated;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Main.UI
{
    /// <summary>
    /// メニューの表示データを共通カードと一覧へ反映
    /// </summary>
    internal static class MainMenuUiPresentation
    {
        public static void Card(BattleCardIconView view, MainMenuCardData card, bool upgraded = false, int quantity = 0)
        {
            bool discovered = card != null;
            view.Bind(new BattleMultiIconViewModel(
                BattleIconKind.Card,
                discovered ? card.Name + (upgraded ? "+" : string.Empty) : string.Empty,
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

        public static List<Transform> Slots(ScrollRect scroll, int count)
        {
            Transform root = scroll.content;
            if (root.childCount == 0) throw new InvalidOperationException("Menu list template is missing");
            while (root.childCount < count)
                UnityEngine.Object.Instantiate(root.GetChild(0).gameObject, root);

            var slots = new List<Transform>();
            for (int i = 0; i < root.childCount; i++)
            {
                Transform slot = root.GetChild(i);
                slot.gameObject.SetActive(i < count);
                if (i < count) slots.Add(slot);
            }

            GridLayoutGroup grid = root.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                int rows = Mathf.CeilToInt((float)count / Mathf.Max(1, grid.constraintCount));
                float height = grid.padding.top + grid.padding.bottom + rows * grid.cellSize.y + Mathf.Max(0, rows - 1) * grid.spacing.y;
                scroll.content.sizeDelta = new Vector2(scroll.content.sizeDelta.x, Mathf.Max(height, scroll.viewport.rect.height));
            }

            scroll.verticalNormalizedPosition = 1;
            return slots;
        }
    }
}
