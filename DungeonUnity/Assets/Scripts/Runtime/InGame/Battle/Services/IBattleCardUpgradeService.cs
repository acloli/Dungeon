using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneのカード強化を扱うインターフェース
    /// </summary>
    public interface IBattleCardUpgradeService
    {
        /// <summary>
        /// 指定カードの強化可否判定と強化後カードの取得
        /// </summary>
        bool TryGetUpgradePreview(
            RuntimeRunDefinition runDefinition,
            RuntimeCard card,
            out RuntimeCard upgradedCard);

        /// <summary>
        /// 指定デッキ位置のカード差し替え
        /// </summary>
        bool TryReplaceDeckCard(BattleSceneState state, int deckIndex, RuntimeCard replacementCard);

        /// <summary>
        /// 指定レアリティ候補からのランダム強化
        /// </summary>
        bool TryUpgradeRandomCard(
            BattleSceneState state,
            RuntimeRunDefinition runDefinition,
            CardRarity rarity,
            IBattleRandomProvider randomProvider);
    }
}
