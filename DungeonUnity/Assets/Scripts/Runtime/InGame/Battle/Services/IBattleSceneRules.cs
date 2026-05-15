using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneルール定義インターフェース
    /// </summary>
    public interface IBattleSceneRules
    {
        /// <summary>
        /// Run状態初期化
        /// </summary>
        void InitializeRun(BattleSceneState state, RunStartConfig runStartConfig);

        /// <summary>
        /// 手札補充
        /// </summary>
        void DrawHand(BattleSceneState state, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 敵選出
        /// </summary>
        EnemyDefinition SelectEnemy(RunStartConfig runStartConfig, InGameNodeType nodeType);

        /// <summary>
        /// 報酬候補選出
        /// </summary>
        IReadOnlyList<CardDefinition> SelectRewardChoices(BattleSceneState state, RunStartConfig runStartConfig, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 使用可否判定
        /// </summary>
        bool CanPlayCard(BattleSceneState state, CardDefinition card);

        /// <summary>
        /// カード適用
        /// </summary>
        void PlayCard(BattleSceneState state, CardDefinition card);

        /// <summary>
        /// 敵ターン解決
        /// </summary>
        int ResolveEnemyTurn(BattleSceneState state);

        /// <summary>
        /// 戦闘報酬金額取得
        /// </summary>
        int GetBattleGoldReward(InGameNodeType nodeType);

        /// <summary>
        /// 休憩適用
        /// </summary>
        void ApplyRest(BattleSceneState state);

        /// <summary>
        /// 購入適用
        /// </summary>
        bool ApplyShopPurchase(BattleSceneState state, IBattleRandomProvider randomProvider);
    }
}
