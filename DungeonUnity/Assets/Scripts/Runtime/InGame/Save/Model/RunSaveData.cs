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
        public int MaxPotionCount = 3;
        public int Gold;
        public int CurrentNodeIndex;
        public int CurrentPage;
        public int MasterSeed;
        public int MapSeed;
        public int MapLayoutVersion = 1;
        public int RandomCounter;

        public List<int> DeckCardIds = new List<int>();
        public List<int> OwnedRelicIds = new List<int>();
        public List<int> OwnedPotionIds = new List<int>();

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
                               && MasterSeed != 0
                               && MapLayoutVersion > 0
                               && RandomCounter >= 0
                               && DeckCardIds != null
                               && OwnedRelicIds != null
                               && OwnedPotionIds != null;
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
