using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneの報酬抽選を扱うインターフェース
    /// </summary>
    public interface IBattleRewardRollService
    {
        /// <summary>
        /// カード報酬候補を選出する
        /// </summary>
        IReadOnlyList<RuntimeRewardEntry> SelectCardRewardChoices(BattleSceneState state, RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider);

        /// <summary>
        /// ポーションドロップ有無を抽選する
        /// </summary>
        bool RollPotionDrop(RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider);

        /// <summary>
        /// レリックドロップ有無を抽選する
        /// </summary>
        bool RollRelicDrop(RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider);
    }
}
