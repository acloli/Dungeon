using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// 報酬付与処理クラス
    /// </summary>
    public sealed class BattleRewardService : IBattleRewardService
    {
        public void ApplyReward(BattleSceneState state, RuntimeRewardEntry entry)
        {
            if (state == null || entry == null)
            {
                return;
            }

            switch (entry.RewardType)
            {
                case RewardType.Card:
                    if (entry.Card != null)
                    {
                        state.Deck.Add(entry.Card);
                    }
                    break;
                case RewardType.Gold:
                    state.Gold += entry.RewardValue;
                    break;
                case RewardType.Potion:
                    // TODO: Potion実装時に追加
                    break;
                case RewardType.Relic:
                    // TODO: Relic実装時に追加
                    break;
            }
        }
    }
}
