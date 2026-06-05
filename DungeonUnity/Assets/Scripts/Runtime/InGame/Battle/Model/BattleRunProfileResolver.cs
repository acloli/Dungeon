using Dungeon.Runtime.SceneFlow;
using TFramework.Scene;

namespace Dungeon.Runtime.InGame.Battle.Model
{
    /// <summary>
    /// BattleScene起動時のRunProfileId解決クラス
    /// </summary>
    public static class BattleRunProfileResolver
    {
        /// <summary>
        /// BridgeDataとfallback設定からRunProfileIdを解決
        /// </summary>
        public static int ResolveRunProfileId(ISceneBridgeData bridgeData, int fallbackRunProfileId)
        {
            if (bridgeData is BattleRunBridgeData battleRunBridgeData &&
                battleRunBridgeData.RunProfileId > 0)
            {
                return battleRunBridgeData.RunProfileId;
            }

            return fallbackRunProfileId;
        }
    }
}
