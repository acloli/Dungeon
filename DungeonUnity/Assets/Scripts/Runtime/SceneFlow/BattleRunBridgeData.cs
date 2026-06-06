using TFramework.Scene;

namespace Dungeon.Runtime.SceneFlow
{
    /// <summary>
    /// BattleSceneへRun開始情報を渡すBridgeDataクラス
    /// </summary>
    public sealed class BattleRunBridgeData : ISceneBridgeData
    {
        public BattleRunBridgeData(int runProfileId)
        {
            RunProfileId = runProfileId;
        }

        public int RunProfileId { get; }
    }
}
