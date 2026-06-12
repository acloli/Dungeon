using System;
using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// カード表示用の汎用アイコンViewクラス
    /// </summary>
    public sealed class BattleCardIconView : BattleMultiIconView
    {
        public void Bind(RuntimeCard card, bool isAffordable, bool isSelected, Action<RuntimeCard> onClick)
        {
            if (card == null)
            {
                base.Bind(null, null);
                return;
            }

            base.Bind(
                new BattleMultiIconViewModel(
                    BattleIconKind.Card,
                    card.DisplayName,
                    card.Description,
                    card.ImageId,
                    card.Rarity,
                    card.Cost,
                    true,
                    true,
                    isSelected,
                    isAffordable),
                () => onClick?.Invoke(card));
        }
    }
}
