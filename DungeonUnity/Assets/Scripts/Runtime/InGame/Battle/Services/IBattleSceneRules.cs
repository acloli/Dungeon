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
        /// 戦闘用山札準備
        /// </summary>
        void PrepareBattleDeck(BattleSceneState state, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 手札破棄
        /// </summary>
        void DiscardHand(BattleSceneState state);

        /// <summary>
        /// 指定枚数を手札へ追加する
        /// </summary>
        int DrawCards(BattleSceneState state, IBattleRandomProvider randomProvider, int drawCount);

        /// <summary>
        /// 敵選出
        /// </summary>
        RuntimeEncounterFormation SelectEncounterFormation(RuntimeRunDefinition runDefinition, InGameNodeType nodeType, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 敵初期HP取得
        /// </summary>
        int RollEnemyHp(RuntimeEnemy enemy, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 報酬候補選出
        /// </summary>
        IReadOnlyList<RuntimeRewardEntry> SelectCardRewardChoices(BattleSceneState state, RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider);

        /// <summary>
        /// ポーションドロップ抽選
        /// </summary>
        bool RollPotionDrop(RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider);

        /// <summary>
        /// レリックドロップ抽選
        /// </summary>
        bool RollRelicDrop(RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 現在階層に対応する宝箱定義取得
        /// </summary>
        RuntimeTreasureDefinition GetTreasureDefinition(BattleSceneState state, RuntimeRunDefinition runDefinition);

        /// <summary>
        /// 宝箱Gold報酬抽選
        /// </summary>
        int RollTreasureGold(BattleSceneState state, RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 宝箱Gold報酬抽選
        /// </summary>
        int RollTreasureGold(RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 使用可否判定
        /// </summary>
        bool CanPlayCard(BattleSceneState state, RuntimeCard card);

        /// <summary>
        /// カード適用
        /// </summary>
        BattleCardResolutionResult PlayCard(BattleSceneState state, int handIndex, IBattleRandomProvider randomProvider);

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
