using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// ショップ機能のビジネスロジックを提供するインターフェース
    /// </summary>
    public interface IBattleShopService
    {
        /// <summary>
        /// ショップの商品ラインナップと価格を初期化する
        /// </summary>
        void InitializeShop(BattleSceneState state, RuntimeRunDefinition runDef, IBattleRandomProvider random);

        /// <summary>
        /// 指定スロットの商品を購入する
        /// </summary>
        bool PurchaseShopItem(BattleSceneState state, int slotIndex);

        /// <summary>
        /// 現在のカード削除価格を取得する
        /// </summary>
        int GetCardRemovalPrice(BattleSceneState state);

        /// <summary>
        /// カード削除を実行する
        /// </summary>
        bool PurchaseCardRemoval(BattleSceneState state, RuntimeCard card);
    }
}
