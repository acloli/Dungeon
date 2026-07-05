using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Domain;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Model
{
    /// <summary>
    /// ランタイム用カード効果定義
    /// </summary>
    public sealed class RuntimeCardEffect
    {
        public RuntimeCardEffect(int order, EffectType effectType, int value, int hitCount, StatusType statusType, int statusValue, TargetSide targetSide)
        {
            Order = order;
            EffectType = effectType;
            Value = value;
            HitCount = hitCount;
            StatusType = statusType;
            StatusValue = statusValue;
            TargetSide = targetSide;
        }

        public int Order { get; }
        public EffectType EffectType { get; }
        public int Value { get; }
        public int HitCount { get; }
        public StatusType StatusType { get; }
        public int StatusValue { get; }
        public TargetSide TargetSide { get; }
    }

    /// <summary>
    /// ランタイム用カード定義
    /// </summary>
    public sealed class RuntimeCard
    {
        public RuntimeCard(
            int id,
            string key,
            string displayName,
            string localizationKey,
            string description,
            string descriptionKey,
            string imageId,
            int cost,
            CardType cardType,
            CardRarity rarity,
            CharacterArchetype characterArchetype,
            IReadOnlyList<RuntimeCardEffect> effects,
            int upgradeCardId = 0,
            bool isUpgraded = false,
            bool exhaustsOnPlay = false)
        {
            Id = id;
            Key = key;
            DisplayName = displayName;
            LocalizationKey = localizationKey;
            Description = description;
            DescriptionKey = descriptionKey;
            ImageId = imageId;
            Cost = cost;
            CardType = cardType;
            Rarity = rarity;
            CharacterArchetype = characterArchetype;
            Effects = effects ?? Array.Empty<RuntimeCardEffect>();
            UpgradeCardId = upgradeCardId;
            IsUpgraded = isUpgraded;
            ExhaustsOnPlay = exhaustsOnPlay;
        }

        public int Id { get; }
        public string Key { get; }
        public string DisplayName { get; }
        public string LocalizationKey { get; }
        public string Description { get; }
        public string DescriptionKey { get; }
        public string ImageId { get; }
        public int Cost { get; }
        public CardType CardType { get; }
        public CardRarity Rarity { get; }
        public CharacterArchetype CharacterArchetype { get; }
        public IReadOnlyList<RuntimeCardEffect> Effects { get; }
        public int UpgradeCardId { get; }
        public bool IsUpgraded { get; }
        public bool ExhaustsOnPlay { get; }

        /// <summary>
        /// 表示用の概算ダメージ値
        /// </summary>
        public int PreviewDamage
        {
            get
            {
                int total = 0;
                for (int i = 0; i < Effects.Count; i++)
                {
                    RuntimeCardEffect effect = Effects[i];
                    if (effect.EffectType != EffectType.DealDamage)
                    {
                        continue;
                    }

                    total += effect.Value * Math.Max(1, effect.HitCount);
                }

                return total;
            }
        }
    }

    /// <summary>
    /// ランタイム用レリック効果定義
    /// </summary>
    public sealed class RuntimeRelicEffect
    {
        public RuntimeRelicEffect(int order, RelicTriggerType triggerType, EffectType effectType, int value, int hitCount, StatusType statusType, int statusValue, TargetSide targetSide)
        {
            Order = order;
            TriggerType = triggerType;
            EffectType = effectType;
            Value = value;
            HitCount = hitCount;
            StatusType = statusType;
            StatusValue = statusValue;
            TargetSide = targetSide;
        }

        public int Order { get; }
        public RelicTriggerType TriggerType { get; }
        public EffectType EffectType { get; }
        public int Value { get; }
        public int HitCount { get; }
        public StatusType StatusType { get; }
        public int StatusValue { get; }
        public TargetSide TargetSide { get; }
    }

    /// <summary>
    /// ランタイム用レリック定義
    /// </summary>
    public sealed class RuntimeRelic
    {
        public RuntimeRelic(int id, string key, string displayName, string localizationKey, string description, string descriptionKey, string imageId, CardRarity rarity, IReadOnlyList<RuntimeRelicEffect> effects = null)
        {
            Id = id;
            Key = key;
            DisplayName = displayName;
            LocalizationKey = localizationKey;
            Description = description;
            DescriptionKey = descriptionKey;
            ImageId = imageId;
            Rarity = rarity;
            Effects = effects ?? Array.Empty<RuntimeRelicEffect>();
        }

        public int Id { get; }
        public string Key { get; }
        public string DisplayName { get; }
        public string LocalizationKey { get; }
        public string Description { get; }
        public string DescriptionKey { get; }
        public string ImageId { get; }
        public CardRarity Rarity { get; }
        public IReadOnlyList<RuntimeRelicEffect> Effects { get; }
    }

    /// <summary>
    /// ランタイム用ポーション効果定義
    /// </summary>
    public sealed class RuntimePotionEffect
    {
        public RuntimePotionEffect(int order, EffectType effectType, int value, int hitCount, StatusType statusType, int statusValue, TargetSide targetSide)
        {
            Order = order;
            EffectType = effectType;
            Value = value;
            HitCount = hitCount;
            StatusType = statusType;
            StatusValue = statusValue;
            TargetSide = targetSide;
        }

        public int Order { get; }
        public EffectType EffectType { get; }
        public int Value { get; }
        public int HitCount { get; }
        public StatusType StatusType { get; }
        public int StatusValue { get; }
        public TargetSide TargetSide { get; }
    }

    /// <summary>
    /// ランタイム用ポーション定義
    /// </summary>
    public sealed class RuntimePotion
    {
        public RuntimePotion(
            int id,
            string key,
            string displayName,
            string localizationKey,
            string description,
            string descriptionKey,
            string imageId,
            CardRarity rarity,
            PotionUseContext useContext,
            PotionTargetMode targetMode,
            IReadOnlyList<RuntimePotionEffect> effects)
        {
            Id = id;
            Key = key;
            DisplayName = displayName;
            LocalizationKey = localizationKey;
            Description = description;
            DescriptionKey = descriptionKey;
            ImageId = imageId;
            Rarity = rarity;
            UseContext = useContext;
            TargetMode = targetMode;
            Effects = effects ?? Array.Empty<RuntimePotionEffect>();
        }

        public int Id { get; }
        public string Key { get; }
        public string DisplayName { get; }
        public string LocalizationKey { get; }
        public string Description { get; }
        public string DescriptionKey { get; }
        public string ImageId { get; }
        public CardRarity Rarity { get; }
        public PotionUseContext UseContext { get; }
        public PotionTargetMode TargetMode { get; }
        public IReadOnlyList<RuntimePotionEffect> Effects { get; }
    }

    /// <summary>
    /// ランタイム用敵行動定義
    /// </summary>
    public sealed class RuntimeEnemyAction
    {
        public RuntimeEnemyAction(
            int order,
            IntentType intentType,
            int damage,
            int hitCount,
            int block,
            StatusType statusType,
            int statusValue,
            BuffType buffType,
            int buffValue,
            RepeatRule repeatRule)
        {
            Order = order;
            IntentType = intentType;
            Damage = damage;
            HitCount = hitCount;
            Block = block;
            StatusType = statusType;
            StatusValue = statusValue;
            BuffType = buffType;
            BuffValue = buffValue;
            RepeatRule = repeatRule;
        }

        public int Order { get; }
        public IntentType IntentType { get; }
        public int Damage { get; }
        public int HitCount { get; }
        public int Block { get; }
        public StatusType StatusType { get; }
        public int StatusValue { get; }
        public BuffType BuffType { get; }
        public int BuffValue { get; }
        public RepeatRule RepeatRule { get; }
    }

    /// <summary>
    /// ランタイム用敵定義
    /// </summary>
    public sealed class RuntimeEnemy
    {
        public RuntimeEnemy(
            int id,
            string key,
            string displayName,
            string localizationKey,
            EnemyTier enemyTier,
            int hpMin,
            int hpMax,
            int goldReward,
            IReadOnlyList<RuntimeEnemyAction> actions)
        {
            Id = id;
            Key = key;
            DisplayName = displayName;
            LocalizationKey = localizationKey;
            EnemyTier = enemyTier;
            HpMin = hpMin;
            HpMax = hpMax;
            GoldReward = goldReward;
            Actions = actions ?? Array.Empty<RuntimeEnemyAction>();
        }

        public int Id { get; }
        public string Key { get; }
        public string DisplayName { get; }
        public string LocalizationKey { get; }
        public EnemyTier EnemyTier { get; }
        public int HpMin { get; }
        public int HpMax { get; }
        public int GoldReward { get; }
        public IReadOnlyList<RuntimeEnemyAction> Actions { get; }
    }

    /// <summary>
    /// ランタイム用遭遇候補
    /// </summary>
    public sealed class RuntimeEncounterEntry
    {
        public RuntimeEncounterEntry(RuntimeEncounterFormation formation, int weight)
        {
            Formation = formation;
            Weight = weight;
        }

        public RuntimeEncounterFormation Formation { get; }
        public int Weight { get; }
    }

    /// <summary>
    /// ランタイム用敵編成
    /// </summary>
    public sealed class RuntimeEncounterFormation
    {
        public RuntimeEncounterFormation(int id, string key, string displayName, IReadOnlyList<RuntimeEncounterEnemyEntry> enemies)
        {
            Id = id;
            Key = key;
            DisplayName = displayName;
            Enemies = enemies ?? Array.Empty<RuntimeEncounterEnemyEntry>();
        }

        public int Id { get; }
        public string Key { get; }
        public string DisplayName { get; }
        public IReadOnlyList<RuntimeEncounterEnemyEntry> Enemies { get; }
    }

    /// <summary>
    /// ランタイム用敵編成要素
    /// </summary>
    public sealed class RuntimeEncounterEnemyEntry
    {
        public RuntimeEncounterEnemyEntry(RuntimeEnemy enemy, int slotIndex)
        {
            Enemy = enemy;
            SlotIndex = slotIndex;
        }

        public RuntimeEnemy Enemy { get; }
        public int SlotIndex { get; }
    }

    /// <summary>
    /// 戦闘中の敵状態
    /// </summary>
    public sealed class BattleEnemyState
    {
        public BattleEnemyState(RuntimeEnemy enemy, int slotIndex, int hp)
        {
            Enemy = enemy;
            SlotIndex = slotIndex;
            Hp = hp;
        }

        public RuntimeEnemy Enemy { get; }
        public int SlotIndex { get; }
        public int Hp { get; set; }
        public int Block { get; set; }
        public bool IsDefeated { get; set; }
        public int TurnCount { get; set; }
        public int CycleIndex { get; set; }
        public Dictionary<StatusType, int> Statuses { get; } = new Dictionary<StatusType, int>();
        public Dictionary<BuffType, int> Buffs { get; } = new Dictionary<BuffType, int>();
    }

    /// <summary>
    /// ランタイム用報酬候補
    /// </summary>
    public sealed class RuntimeRewardEntry
    {
        public RuntimeRewardEntry(RewardType rewardType, int rewardValue, RuntimeCard card, RuntimeRelic relic, RuntimePotion potion, int weight, int minFloor, int maxFloor)
        {
            RewardType = rewardType;
            RewardValue = rewardValue;
            Card = card;
            Relic = relic;
            Potion = potion;
            Weight = weight;
            MinFloor = minFloor;
            MaxFloor = maxFloor;
        }

        public RewardType RewardType { get; }
        public int RewardValue { get; }
        public RuntimeCard Card { get; }
        public RuntimeRelic Relic { get; }
        public RuntimePotion Potion { get; }
        public int Weight { get; }
        public int MinFloor { get; }
        public int MaxFloor { get; }
    }

    /// <summary>
    /// ランタイム用イベント選択肢定義
    /// </summary>
    public sealed class RuntimeEventChoice
    {
        public RuntimeEventChoice(int choiceId, string localizationKey, EffectType effectType, int effectValue)
        {
            ChoiceId = choiceId;
            LocalizationKey = localizationKey;
            EffectType = effectType;
            EffectValue = effectValue;
        }

        public int ChoiceId { get; }
        public string LocalizationKey { get; }
        public EffectType EffectType { get; }
        public int EffectValue { get; }
    }

    /// <summary>
    /// ランタイム用イベント定義
    /// </summary>
    public sealed class RuntimeEvent
    {
        public RuntimeEvent(int id, string eventName, string titleKey, string descriptionKey, string imageId, IReadOnlyList<RuntimeEventChoice> choices)
        {
            Id = id;
            EventName = eventName;
            TitleKey = titleKey;
            DescriptionKey = descriptionKey;
            ImageId = imageId;
            Choices = choices ?? Array.Empty<RuntimeEventChoice>();
        }

        public int Id { get; }
        public string EventName { get; }
        public string TitleKey { get; }
        public string DescriptionKey { get; }
        public string ImageId { get; }
        public IReadOnlyList<RuntimeEventChoice> Choices { get; }
    }

    /// <summary>
    /// ランタイム用ショップスロット
    /// </summary>
    public sealed class RuntimeShopSlot
    {
        public RuntimeShopSlot(int slotIndex, RewardType rewardType, CardType requiredCardType, int weight)
        {
            SlotIndex = slotIndex;
            RewardType = rewardType;
            RequiredCardType = requiredCardType;
            Weight = weight;
        }

        public int SlotIndex { get; }
        public RewardType RewardType { get; }
        public CardType RequiredCardType { get; }
        public int Weight { get; }
    }

    /// <summary>
    /// ランタイム用ショップラインナップ
    /// </summary>
    public sealed class RuntimeShopLineup
    {
        public RuntimeShopLineup(int shopId, IReadOnlyList<RuntimeShopSlot> slots)
        {
            ShopId = shopId;
            Slots = slots ?? Array.Empty<RuntimeShopSlot>();
        }

        public int ShopId { get; }
        public IReadOnlyList<RuntimeShopSlot> Slots { get; }
    }

    /// <summary>
    /// ランタイム用カード価格ルール
    /// </summary>
    public sealed class RuntimeCardPriceRule
    {
        public RuntimeCardPriceRule(CardRarity rarity, int basePrice, int jitterPercent)
        {
            Rarity = rarity;
            BasePrice = basePrice;
            JitterPercent = jitterPercent;
        }

        public CardRarity Rarity { get; }
        public int BasePrice { get; }
        public int JitterPercent { get; }
    }

    /// <summary>
    /// ランタイム用アイテム価格ルール
    /// </summary>
    public sealed class RuntimeItemPriceRule
    {
        public RuntimeItemPriceRule(RewardType itemType, int itemId, int basePrice, int jitterPercent)
        {
            ItemType = itemType;
            ItemId = itemId;
            BasePrice = basePrice;
            JitterPercent = jitterPercent;
        }

        public RewardType ItemType { get; }
        public int ItemId { get; }
        public int BasePrice { get; }
        public int JitterPercent { get; }
    }

    /// <summary>
    /// ランタイム用マップノード
    /// </summary>
    public sealed class RuntimeMapNode
    {
        public RuntimeMapNode(
            int id,
            string nodeKey,
            int floor,
            InGameNodeType nodeType,
            string displayName,
            string localizationKey,
            IReadOnlyList<int> nextNodeIndices)
        {
            Id = id;
            NodeKey = nodeKey;
            Floor = floor;
            NodeType = nodeType;
            DisplayName = displayName;
            LocalizationKey = localizationKey;
            NextNodeIndices = nextNodeIndices ?? Array.Empty<int>();
        }

        public int Id { get; }
        public string NodeKey { get; }
        public int Floor { get; }
        public InGameNodeType NodeType { get; }
        public string DisplayName { get; }
        public string LocalizationKey { get; }
        public IReadOnlyList<int> NextNodeIndices { get; }
    }

    /// <summary>
    /// ランタイム用Run定義
    /// </summary>
    public sealed class RuntimeRunDefinition
    {
        public RuntimeRunDefinition(
            int runProfileId,
            string key,
            int mapTemplateId,
            CharacterArchetype characterArchetype,
            int playerMaxHp,
            int startingGold,
            int cardRewardChoiceCount,
            int potionDropChance,
            int relicDropChance,
            IReadOnlyList<RuntimeCard> starterDeck,
            IReadOnlyDictionary<int, RuntimeCard> cardCatalog,
            IReadOnlyList<RuntimeRewardEntry> rewardPool,
            IReadOnlyList<RuntimeMapNode> nodes,
            IReadOnlyDictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>> encountersByNodeType,
            IReadOnlyList<RuntimeEvent> possibleEvents,
            IReadOnlyDictionary<int, RuntimeRelic> relicCatalog,
            IReadOnlyDictionary<int, RuntimePotion> potionCatalog,
            RuntimeShopLineup shopLineup,
            IReadOnlyDictionary<CardRarity, RuntimeCardPriceRule> cardPriceRules,
            IReadOnlyList<RuntimeItemPriceRule> itemPriceRules)
        {
            RunProfileId = runProfileId;
            Key = key;
            MapTemplateId = mapTemplateId;
            CharacterArchetype = characterArchetype;
            PlayerMaxHp = playerMaxHp;
            StartingGold = startingGold;
            CardRewardChoiceCount = cardRewardChoiceCount;
            PotionDropChance = potionDropChance;
            RelicDropChance = relicDropChance;
            StarterDeck = starterDeck ?? Array.Empty<RuntimeCard>();
            CardCatalog = cardCatalog ?? new Dictionary<int, RuntimeCard>();
            RewardPool = rewardPool ?? Array.Empty<RuntimeRewardEntry>();
            Nodes = nodes ?? Array.Empty<RuntimeMapNode>();
            EncountersByNodeType = encountersByNodeType
                ?? new Dictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>>();
            PossibleEvents = possibleEvents ?? Array.Empty<RuntimeEvent>();
            RelicCatalog = relicCatalog ?? new Dictionary<int, RuntimeRelic>();
            PotionCatalog = potionCatalog ?? new Dictionary<int, RuntimePotion>();
            ShopLineup = shopLineup;
            CardPriceRules = cardPriceRules ?? new Dictionary<CardRarity, RuntimeCardPriceRule>();
            ItemPriceRules = itemPriceRules ?? Array.Empty<RuntimeItemPriceRule>();
        }

        public int RunProfileId { get; }
        public string Key { get; }
        public int MapTemplateId { get; }
        public CharacterArchetype CharacterArchetype { get; }
        public int PlayerMaxHp { get; }
        public int StartingGold { get; }
        public int CardRewardChoiceCount { get; }
        public int PotionDropChance { get; }
        public int RelicDropChance { get; }
        public IReadOnlyList<RuntimeCard> StarterDeck { get; }
        public IReadOnlyDictionary<int, RuntimeCard> CardCatalog { get; }
        public IReadOnlyList<RuntimeRewardEntry> RewardPool { get; }
        public IReadOnlyList<RuntimeMapNode> Nodes { get; }
        public IReadOnlyDictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>> EncountersByNodeType { get; }
        public IReadOnlyList<RuntimeEvent> PossibleEvents { get; }
        public IReadOnlyDictionary<int, RuntimeRelic> RelicCatalog { get; }
        public IReadOnlyDictionary<int, RuntimePotion> PotionCatalog { get; }
        public RuntimeShopLineup ShopLineup { get; }
        public IReadOnlyDictionary<CardRarity, RuntimeCardPriceRule> CardPriceRules { get; }
        public IReadOnlyList<RuntimeItemPriceRule> ItemPriceRules { get; }
    }

    /// <summary>
    /// カード解決結果
    /// </summary>
    public readonly struct BattleCardResolutionResult
    {
        public BattleCardResolutionResult(int totalDamage, int totalBlock, int totalDraw)
        {
            TotalDamage = totalDamage;
            TotalBlock = totalBlock;
            TotalDraw = totalDraw;
        }

        public int TotalDamage { get; }
        public int TotalBlock { get; }
        public int TotalDraw { get; }
    }

    /// <summary>
    /// 敵ターン解決結果
    /// </summary>
    public readonly struct BattleEnemyTurnResult
    {
        public BattleEnemyTurnResult(RuntimeEnemyAction action, int damageDealt)
        {
            Action = action;
            DamageDealt = damageDealt;
        }

        public RuntimeEnemyAction Action { get; }
        public int DamageDealt { get; }
    }
}
