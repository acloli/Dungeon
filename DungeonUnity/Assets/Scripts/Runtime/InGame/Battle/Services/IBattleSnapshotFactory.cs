using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// UI表示用スナップショット構築インターフェース
    /// </summary>
    public interface IBattleSnapshotFactory
    {
        BattleSceneSnapshot CreateSnapshot(BattleSceneState state);
    }
}
