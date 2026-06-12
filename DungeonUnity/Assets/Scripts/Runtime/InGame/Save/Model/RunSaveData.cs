using System;
using System.Collections.Generic;

namespace Dungeon.Runtime.InGame.Save.Model
{
    /// <summary>
    /// 探索中の状態を保存するデータモデル
    /// </summary>
    [Serializable]
    public class RunSaveData
    {
        private const int MapPage = 0;
        private const int RestShopPage = 3;

        public int RunProfileId;
        public int PlayerMaxHp;
        public int PlayerHp;
        public int PlayerEnergy;
        public int Gold;
        public int CurrentNodeIndex;
        public int CurrentPage;

        public List<int> DeckCardIds = new List<int>();

        public List<SaveShopItem> ShopItems = new List<SaveShopItem>();
        public bool IsCardRemovalSoldOut;
        public int CardRemovalCount;

        /// <summary>
        /// 再開できるcheckpointデータか
        /// </summary>
        public bool IsValid => RunProfileId > 0
                               && PlayerMaxHp > 0
                               && PlayerHp > 0
                               && (CurrentPage == MapPage || CurrentPage == RestShopPage)
                               && DeckCardIds != null;
    }

    [Serializable]
    public struct SaveShopItem
    {
        public int SlotIndex;
        public int RewardType;
        public int CardId;
        public int ItemId;
        public int Price;
        public bool IsSoldOut;
    }
}
