using System.Collections.Generic;
using System.Linq;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;
using TFramework.MasterData;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// Shop用のMasterData組み立てクラス
    /// </summary>
    public sealed class ShopMasterDataFacade
    {
        private readonly IMasterDataService _masterDataService;

        public ShopMasterDataFacade(IMasterDataService masterDataService)
        {
            _masterDataService = masterDataService;
        }

        /// <summary>
        /// ショップのラインナップを構築する
        /// </summary>
        public RuntimeShopLineup BuildShopLineup(int shopId)
        {
            IReadOnlyList<ShopLineupMaster> lineupMasters = _masterDataService.GetAll<ShopLineupMaster>();
            List<RuntimeShopSlot> slots = new List<RuntimeShopSlot>();
            foreach (ShopLineupMaster master in lineupMasters
                         .Where(master => master.ShopId == shopId)
                         .OrderBy(master => master.SlotIndex))
            {
                slots.Add(new RuntimeShopSlot(master.SlotIndex, master.RewardType, master.RequiredCardType, master.Weight));
            }

            return new RuntimeShopLineup(shopId, slots);
        }

        /// <summary>
        /// カード価格ルールを構築する
        /// </summary>
        public IReadOnlyDictionary<CardRarity, RuntimeCardPriceRule> BuildCardPriceRules()
        {
            IReadOnlyList<ShopCardPriceMaster> masters = _masterDataService.GetAll<ShopCardPriceMaster>();
            Dictionary<CardRarity, RuntimeCardPriceRule> rules = new Dictionary<CardRarity, RuntimeCardPriceRule>();
            foreach (ShopCardPriceMaster master in masters.OrderBy(master => master.Id))
            {
                rules[master.CardRarity] = new RuntimeCardPriceRule(master.CardRarity, master.BasePrice, master.JitterPercent);
            }

            return rules;
        }

        /// <summary>
        /// アイテム価格ルールを構築する
        /// </summary>
        public IReadOnlyList<RuntimeItemPriceRule> BuildItemPriceRules()
        {
            IReadOnlyList<ShopItemPriceMaster> masters = _masterDataService.GetAll<ShopItemPriceMaster>();
            List<RuntimeItemPriceRule> rules = new List<RuntimeItemPriceRule>();
            foreach (ShopItemPriceMaster master in masters.OrderBy(master => master.Id))
            {
                rules.Add(new RuntimeItemPriceRule(master.ItemType, master.ItemId, master.BasePrice, master.JitterPercent));
            }

            return rules;
        }
    }
}
