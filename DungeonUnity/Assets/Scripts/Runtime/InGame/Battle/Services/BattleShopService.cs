using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;
using TFramework.MasterData;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// ショップ機能の実装クラス
    /// </summary>
    public sealed class BattleShopService : IBattleShopService
    {
        private readonly IMasterDataService _masterDataService;

        public BattleShopService(IMasterDataService masterDataService)
        {
            _masterDataService = masterDataService;
        }

        public void InitializeShop(BattleSceneState state, RuntimeRunDefinition runDef, IBattleRandomProvider random)
        {
            if (state == null || runDef == null || random == null)
            {
                return;
            }

            state.ShopItems.Clear();
            state.IsCardRemovalSoldOut = false;

            if (runDef.ShopLineup == null || runDef.ShopLineup.Slots == null)
            {
                return;
            }

            foreach (RuntimeShopSlot slot in runDef.ShopLineup.Slots)
            {
                if (slot == null)
                {
                    continue;
                }

                switch (slot.RewardType)
                {
                    case RewardType.Card:
                        RuntimeCard card = PickCardForSlot(runDef, slot.RequiredCardType, random);
                        if (card != null)
                        {
                            int price = CalculateCardPrice(runDef, card, random);
                            state.ShopItems.Add(new BattleShopItemState(slot.SlotIndex, RewardType.Card, card, null, null, 0, price));
                        }
                        break;

                    case RewardType.Relic:
                        RuntimeItemPriceRule relicRule = PickRelicRuleForSlot(state, runDef, random);
                        if (relicRule != null)
                        {
                            int price = CalculateItemPrice(relicRule, random);
                            runDef.RelicCatalog.TryGetValue(relicRule.ItemId, out RuntimeRelic relic);
                            state.ShopItems.Add(new BattleShopItemState(slot.SlotIndex, RewardType.Relic, null, relic, null, relicRule.ItemId, price));
                        }
                        break;

                    case RewardType.Potion:
                        RuntimeItemPriceRule potionRule = PickItemRuleForSlot(runDef, RewardType.Potion, random);
                        if (potionRule != null)
                        {
                            int price = CalculateItemPrice(potionRule, random);
                            runDef.PotionCatalog.TryGetValue(potionRule.ItemId, out RuntimePotion potion);
                            state.ShopItems.Add(new BattleShopItemState(slot.SlotIndex, RewardType.Potion, null, null, potion, potionRule.ItemId, price));
                        }
                        else
                        {
                            // Fallback default potion
                            runDef.PotionCatalog.TryGetValue(1, out RuntimePotion potion);
                            state.ShopItems.Add(new BattleShopItemState(slot.SlotIndex, RewardType.Potion, null, null, potion, 1, 50));
                        }
                        break;
                }
            }
        }

        public bool PurchaseShopItem(BattleSceneState state, int slotIndex)
        {
            if (state == null)
            {
                return false;
            }

            BattleShopItemState item = state.ShopItems.FirstOrDefault(i => i.SlotIndex == slotIndex);
            if (item == null || item.IsSoldOut || state.Gold < item.Price)
            {
                return false;
            }

            if (item.RewardType == RewardType.Relic
                && state.OwnedRelics.Any(ownedRelic => ownedRelic != null
                    && (ownedRelic.Id == item.ItemId
                        || item.Relic != null && ownedRelic.Id == item.Relic.Id)))
            {
                return false;
            }

            state.Gold -= item.Price;
            item.IsSoldOut = true;

            if (item.RewardType == RewardType.Card && item.Card != null)
            {
                state.Deck.Add(item.Card);
            }

            return true;
        }

        public int GetCardRemovalPrice(BattleSceneState state)
        {
            if (state == null)
            {
                return 75;
            }

            ShopCardRemovalMaster removalMaster = _masterDataService.GetAll<ShopCardRemovalMaster>().FirstOrDefault();
            int basePrice = removalMaster != null ? removalMaster.BasePrice : 75;
            int priceIncrease = removalMaster != null ? removalMaster.PriceIncreasePerPurchase : 25;

            return basePrice + state.CardRemovalCount * priceIncrease;
        }

        public bool PurchaseCardRemoval(BattleSceneState state, RuntimeCard card)
        {
            if (state == null || card == null || state.IsCardRemovalSoldOut)
            {
                return false;
            }

            int price = GetCardRemovalPrice(state);
            if (state.Gold < price)
            {
                return false;
            }

            bool removed = state.Deck.Remove(card);
            if (!removed)
            {
                return false;
            }

            state.Gold -= price;
            state.IsCardRemovalSoldOut = true;
            state.CardRemovalCount++;

            return true;
        }

        public int GetCardUpgradePrice(RuntimeRunDefinition runDefinition, RuntimeCard card)
        {
            int basePrice = 50;
            if (runDefinition?.CardPriceRules != null
                && card != null
                && runDefinition.CardPriceRules.TryGetValue(card.Rarity, out RuntimeCardPriceRule rule))
            {
                basePrice = rule.BasePrice;
            }

            return Math.Max(1, (basePrice + 1) / 2);
        }

        private static RuntimeCard PickCardForSlot(RuntimeRunDefinition runDef, CardType requiredType, IBattleRandomProvider random)
        {
            List<RuntimeCard> candidates = new List<RuntimeCard>();
            if (runDef.RewardPool != null)
            {
                foreach (RuntimeRewardEntry reward in runDef.RewardPool)
                {
                    if (reward.RewardType == RewardType.Card && reward.Card != null)
                    {
                        if (requiredType == CardType.None || reward.Card.CardType == requiredType)
                        {
                            candidates.Add(reward.Card);
                        }
                    }
                }
            }

            if (candidates.Count == 0 && requiredType != CardType.None && runDef.RewardPool != null)
            {
                // Fallback: ignore card type filter
                foreach (RuntimeRewardEntry reward in runDef.RewardPool)
                {
                    if (reward.RewardType == RewardType.Card && reward.Card != null)
                    {
                        candidates.Add(reward.Card);
                    }
                }
            }

            if (candidates.Count == 0 && runDef.StarterDeck != null)
            {
                // Fallback: starter deck cards
                candidates.AddRange(runDef.StarterDeck);
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            int index = random.Range(0, candidates.Count);
            return candidates[index];
        }

        private static RuntimeItemPriceRule PickItemRuleForSlot(RuntimeRunDefinition runDef, RewardType itemType, IBattleRandomProvider random)
        {
            if (runDef.ItemPriceRules == null)
            {
                return null;
            }

            List<RuntimeItemPriceRule> rules = runDef.ItemPriceRules.Where(r => r.ItemType == itemType).ToList();
            if (rules.Count == 0)
            {
                return null;
            }

            int index = random.Range(0, rules.Count);
            return rules[index];
        }

        private static RuntimeItemPriceRule PickRelicRuleForSlot(BattleSceneState state, RuntimeRunDefinition runDef, IBattleRandomProvider random)
        {
            if (runDef.ItemPriceRules == null || runDef.RelicCatalog == null)
            {
                return null;
            }

            HashSet<int> excludedRelicIds = new HashSet<int>();
            for (int i = 0; i < state.OwnedRelics.Count; i++)
            {
                RuntimeRelic ownedRelic = state.OwnedRelics[i];
                if (ownedRelic != null)
                {
                    excludedRelicIds.Add(ownedRelic.Id);
                }
            }

            for (int i = 0; i < state.ShopItems.Count; i++)
            {
                BattleShopItemState shopItem = state.ShopItems[i];
                if (shopItem != null && shopItem.RewardType == RewardType.Relic)
                {
                    excludedRelicIds.Add(shopItem.ItemId);
                }
            }

            List<RuntimeItemPriceRule> rules = runDef.ItemPriceRules
                .Where(rule => rule != null
                    && rule.ItemType == RewardType.Relic
                    && !excludedRelicIds.Contains(rule.ItemId)
                    && runDef.RelicCatalog.TryGetValue(rule.ItemId, out RuntimeRelic relic)
                    && relic != null)
                .ToList();
            if (rules.Count == 0)
            {
                return null;
            }

            int index = random.Range(0, rules.Count);
            return rules[index];
        }

        private static int CalculateCardPrice(RuntimeRunDefinition runDef, RuntimeCard card, IBattleRandomProvider random)
        {
            int basePrice = 50;
            int jitterPercent = 10;

            if (runDef.CardPriceRules != null && runDef.CardPriceRules.TryGetValue(card.Rarity, out RuntimeCardPriceRule rule))
            {
                basePrice = rule.BasePrice;
                jitterPercent = rule.JitterPercent;
            }

            int jitterLimit = basePrice * jitterPercent / 100;
            return basePrice + random.Range(-jitterLimit, jitterLimit + 1);
        }

        private static int CalculateItemPrice(RuntimeItemPriceRule rule, IBattleRandomProvider random)
        {
            int jitterLimit = rule.BasePrice * rule.JitterPercent / 100;
            return rule.BasePrice + random.Range(-jitterLimit, jitterLimit + 1);
        }
    }
}
