namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// BattleScene基底表示インターフェース
    /// </summary>
    public interface IBattleSceneHostView
    {
        IBattlePageView BattlePageView { get; }

        /// <summary>
        /// 戦闘基底表示切り替え
        /// </summary>
        void SetBattleVisible(bool visible);
    }
}
