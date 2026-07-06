using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// ポーション所持・提示・使用処理インターフェース
    /// </summary>
    public interface IBattlePotionService
    {
        void RestoreOwnedPotions(BattleSceneState state, RuntimeRunDefinition runDefinition, IReadOnlyList<int> ownedPotionIds);
        bool HasCapacity(BattleSceneState state);
        bool AddOwnedPotion(BattleSceneState state, RuntimePotion potion);
        RuntimePotion RollBattleRewardPotion(RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider);
        PendingPotionOffer CreateOffer(RuntimePotion potion, PotionOfferSource source, int shopSlotIndex = BattleSceneConstants.UnselectedCardIndex);
        bool CanUsePotionInCurrentPage(BattleSceneState state, RuntimePotion potion);
        bool UsePotion(BattleSceneState state, int potionIndex, BattlePotionUseTarget target, IBattleSceneRules rules, IBattleRandomProvider randomProvider);
        bool ReplaceOwnedPotion(BattleSceneState state, int potionIndex, PendingPotionOffer offer);
    }
}
