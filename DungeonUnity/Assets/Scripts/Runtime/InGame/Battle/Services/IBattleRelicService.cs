using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// レリック効果と所持状態の仲介インターフェース
    /// </summary>
    public interface IBattleRelicService
    {
        /// <summary>
        /// セーブデータから所持レリックを復元する
        /// </summary>
        void RestoreOwnedRelics(BattleSceneState state, RuntimeRunDefinition runDefinition, IReadOnlyList<int> ownedRelicIds);

        /// <summary>
        /// 所持レリックを追加する
        /// </summary>
        bool AddOwnedRelic(BattleSceneState state, RuntimeRelic relic);

        /// <summary>
        /// 戦闘報酬候補レリックを抽選する
        /// </summary>
        RuntimeRelic RollBattleRewardRelic(BattleSceneState state, RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 宝箱報酬候補レリックを報酬プールグループから抽選する
        /// </summary>
        RuntimeRelic RollTreasureRewardRelic(BattleSceneState state, RuntimeRunDefinition runDefinition, int relicGroupId, IBattleRandomProvider randomProvider)
        {
            return RollBattleRewardRelic(state, runDefinition, randomProvider);
        }

        /// <summary>
        /// 指定コンテキストの効果を適用する
        /// </summary>
        void ApplyEffects(BattleSceneState state, RelicTriggerContext context);

        /// <summary>
        /// 指定トリガーの効果を適用する
        /// </summary>
        void ApplyEffects(BattleSceneState state, RelicTriggerType triggerType);
    }
}
