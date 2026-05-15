using System;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// 結果画面表示インターフェース
    /// </summary>
    public interface IResultDialogView
    {
        void WireButtons(Action onBackClicked);
        void UnwireButtons();
        void SetResultText(string message);
    }
}
