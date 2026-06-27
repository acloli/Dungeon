using System;
using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneの報酬フローを扱うインターフェース
    /// </summary>
    public interface IBattleRewardFlowService
    {
        /// <summary>
        /// 戦闘勝利時の報酬候補状態を準備する
        /// </summary>
        void PrepareBattleRewards(BattleSceneState state, RuntimeRunDefinition runDefinition, int goldReward);

        /// <summary>
        /// 報酬画面を開く
        /// </summary>
        void OpenReward(BattleSceneState state, RuntimeRunDefinition runDefinition, Action<BattleScenePage> setCurrentPage);

        /// <summary>
        /// カード報酬を選択する
        /// </summary>
        void SelectReward(BattleSceneState state, RuntimeRewardEntry rewardEntry);

        /// <summary>
        /// Gold報酬を取得する
        /// </summary>
        void ClaimGold(BattleSceneState state);

        /// <summary>
        /// ポーション報酬を取得する
        /// </summary>
        void ClaimPotion(BattleSceneState state);

        /// <summary>
        /// レリック報酬を取得する
        /// </summary>
        void ClaimRelic(BattleSceneState state);

        /// <summary>
        /// 報酬画面から継続する
        /// </summary>
        void ContinueFromReward(BattleSceneState state, Action openMap);
    }
}
