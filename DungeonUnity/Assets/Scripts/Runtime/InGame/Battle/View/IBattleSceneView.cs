using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// BattleSceneルート表示インターフェース
    /// </summary>
    public interface IBattleSceneView
    {
        IMapPageView MapPageView { get; }
        IBattlePageView BattlePageView { get; }
        IRewardPageView RewardPageView { get; }
        IRestShopPageView RestShopPageView { get; }
        IResultPageView ResultPageView { get; }

        /// <summary>
        /// 表示ページ切り替え
        /// </summary>
        void ShowPage(BattleScenePage page);
    }
}
