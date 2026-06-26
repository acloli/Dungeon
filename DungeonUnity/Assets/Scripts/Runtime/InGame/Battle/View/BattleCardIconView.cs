using System;
using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// カード表示用の汎用アイコンViewクラス
    /// </summary>
    public sealed class BattleCardIconView : BattleMultiIconView
    {
        public override void Bind(BattleMultiIconViewModel icon, Action onClick)
        {
            base.Bind(icon, onClick);
            SetFooterLabel(icon?.FooterLabel ?? string.Empty, icon?.ShowFooterLabel == true);
        }
    }
}
