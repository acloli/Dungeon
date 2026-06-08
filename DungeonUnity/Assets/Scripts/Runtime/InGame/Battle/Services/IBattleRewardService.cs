using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// 報酬付与処理インターフェース
    /// </summary>
    public interface IBattleRewardService
    {
        void ApplyReward(BattleSceneState state, RuntimeRewardEntry entry);
    }
}
