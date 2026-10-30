using TFramework.Scene;

namespace Dungeon.Runtime.SceneFlow
{
    public enum BattleRunStartMode
    {
        Automatic, NewAdventure, ContinueAdventure
    }

    /// <summary>
    /// BattleSceneへRun開始情報を渡すBridgeDataクラス
    /// </summary>
    public sealed class BattleRunBridgeData : ISceneBridgeData
    {
        public BattleRunBridgeData(int runProfileId, BattleRunStartMode startMode = BattleRunStartMode.Automatic)
        {
            RunProfileId = runProfileId;
            StartMode = startMode;
        }

        public int RunProfileId { get; }
        public BattleRunStartMode StartMode { get; }
    }
}
