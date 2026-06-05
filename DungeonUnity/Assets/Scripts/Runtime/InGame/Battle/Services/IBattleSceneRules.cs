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
        void InitializeRun(BattleSceneState state, RuntimeRunDefinition runDefinition);

        /// <summary>
        /// 手札補充
        /// </summary>
        void DrawHand(BattleSceneState state, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 敵選出
        /// </summary>
        RuntimeEnemy SelectEnemy(RuntimeRunDefinition runDefinition, InGameNodeType nodeType, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 敵初期HP取得
        /// </summary>
        int RollEnemyHp(RuntimeEnemy enemy, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 報酬候補選出
        /// </summary>
        IReadOnlyList<RuntimeCard> SelectRewardChoices(BattleSceneState state, RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 使用可否判定
        /// </summary>
        bool CanPlayCard(BattleSceneState state, RuntimeCard card);

        /// <summary>
        /// カード適用
        /// </summary>
        BattleCardResolutionResult PlayCard(BattleSceneState state, RuntimeCard card, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 敵ターン解決
        /// </summary>
        BattleEnemyTurnResult ResolveEnemyTurn(BattleSceneState state, IBattleRandomProvider randomProvider);

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
