using System;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// 補給画面表示インターフェース
    /// </summary>
    public interface IRestShopDialogView
    {
        void WireButtons(Action onRestClicked, Action onUpgradeClicked, Action onShopClicked, Action onContinueClicked);
        void UnwireButtons();
        void SetRestShopText(string message);
        void SetRestShopContinueInteractable(bool interactable);
    }
}
