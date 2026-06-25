namespace Dungeon.Runtime.InGame.Battle.View
{
    using System;

    /// <summary>
    /// BattleScene基底表示インターフェース
    /// </summary>
    public interface IBattleSceneHostView
    {
        IBattlePageView BattlePageView { get; }
        void BuildOwnedRelics(System.Collections.Generic.IReadOnlyList<Model.BattleMultiIconViewModel> relics, Action<int> onClicked);
        void SetOwnedRelicHint(string message, int selectedIndex);
        void ClearOwnedRelics();
        void BuildOwnedPotions(System.Collections.Generic.IReadOnlyList<Model.BattleMultiIconViewModel> potions, Action<int> onClicked);
        void SetOwnedPotionHint(string message, int selectedIndex);
        void SetOwnedPotionUseVisible(bool visible, Action onClicked);
        void ClearOwnedPotions();
        void SetHostChromeInteractable(bool interactable);
        void WireHostBackgroundClick(Action onClicked);
        void UnwireHostBackgroundClick();

        /// <summary>
        /// 戦闘基底表示切り替え
        /// </summary>
        void SetBattleVisible(bool visible);

        /// <summary>
        /// 中断ボタン表示切り替え
        /// </summary>
        void SetSaveQuitVisible(bool visible);

        /// <summary>
        /// 中断ボタン登録
        /// </summary>
        void WireSaveQuitButton(Action onSaveQuitClicked);

        /// <summary>
        /// 中断ボタン解除
        /// </summary>
        void UnwireSaveQuitButton();
    }
}
