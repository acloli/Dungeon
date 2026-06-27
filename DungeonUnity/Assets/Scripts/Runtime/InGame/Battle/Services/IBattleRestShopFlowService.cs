using System;
using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneの休憩所・ショップフローを扱うインターフェース
    /// </summary>
    public interface IBattleRestShopFlowService
    {
        /// <summary>
        /// 休憩所画面を開く
        /// </summary>
        void OpenRestShop(BattleSceneState state, RuntimeRunDefinition runDefinition, Action<BattleScenePage> setCurrentPage);

        /// <summary>
        /// 休憩を適用する
        /// </summary>
        void ApplyRest(BattleSceneState state);

        /// <summary>
        /// 強化候補選択を開く
        /// </summary>
        void ApplyUpgrade(BattleSceneState state, RuntimeRunDefinition runDefinition, Action<BattleScenePage> setCurrentPage);

        /// <summary>
        /// ショップ画面を開く
        /// </summary>
        void OpenShop(BattleSceneState state, Action<BattleScenePage> setCurrentPage);

        /// <summary>
        /// ショップ商品を購入する
        /// </summary>
        bool PurchaseShopItem(BattleSceneState state, int slotIndex);

        /// <summary>
        /// カード削除選択を開く
        /// </summary>
        void OpenCardRemoval(BattleSceneState state, Action<BattleScenePage> setCurrentPage);

        /// <summary>
        /// カード削除を購入する
        /// </summary>
        bool PurchaseCardRemoval(BattleSceneState state, RuntimeCard card, Action<BattleScenePage> setCurrentPage);

        /// <summary>
        /// カード選択をキャンセルする
        /// </summary>
        void CancelCardSelect(BattleSceneState state, Action<BattleScenePage> setCurrentPage, Action reopenRestShop);

        /// <summary>
        /// カード選択を確定する
        /// </summary>
        bool ConfirmCardSelect(
            BattleSceneState state,
            RuntimeRunDefinition runDefinition,
            RuntimeCard card,
            Action<BattleScenePage> setCurrentPage);

        /// <summary>
        /// ショップから退出する
        /// </summary>
        void LeaveShop(BattleSceneState state, Action<BattleScenePage> setCurrentPage);

        /// <summary>
        /// 休憩所から継続する
        /// </summary>
        void ContinueFromRestShop(Action openMap);
    }
}
