using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Model
{
    /// <summary>
    /// ショップ商品の個別の状態を保持するクラス
    /// </summary>
    public sealed class BattleShopItemState
    {
        public int SlotIndex { get; }
        public RewardType RewardType { get; }
        public RuntimeCard Card { get; }      // Card以外の場合は null
        public int ItemId { get; }             // Potion/Relic ID
        public int Price { get; }
        public bool IsSoldOut { get; set; }

        public BattleShopItemState(int slotIndex, RewardType rewardType, RuntimeCard card, int itemId, int price, bool isSoldOut = false)
        {
            SlotIndex = slotIndex;
            RewardType = rewardType;
            Card = card;
            ItemId = itemId;
            Price = price;
            IsSoldOut = isSoldOut;
        }
    }
}
