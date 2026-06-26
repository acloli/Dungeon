using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using Game.MasterData.Generated;

namespace Dungeon.Tests.EditMode.Support
{
    /// <summary>
    /// Battle系テストデータ生成入口クラス
    /// </summary>
    public static class BattleTestData
    {
        public static RuntimeCardBuilder Card(int id = 1001)
        {
            return new RuntimeCardBuilder(id);
        }

        public static RuntimeEnemyActionBuilder EnemyAction(int order = 1)
        {
            return new RuntimeEnemyActionBuilder(order);
        }

        public static RuntimeEnemyBuilder Enemy(int id = 3001)
        {
            return new RuntimeEnemyBuilder(id);
        }

        public static RuntimeMapNodeBuilder MapNode(int id = 5301)
        {
            return new RuntimeMapNodeBuilder(id);
        }

        public static RuntimeRewardEntryBuilder RewardEntry()
        {
            return new RuntimeRewardEntryBuilder();
        }

        public static RuntimeRelicBuilder Relic(int id = 1)
        {
            return new RuntimeRelicBuilder(id);
        }

        public static RuntimePotionBuilder Potion(int id = 1)
        {
            return new RuntimePotionBuilder(id);
        }

        public static RuntimeRunDefinitionBuilder RunDefinition()
        {
            return new RuntimeRunDefinitionBuilder();
        }

        public static BattleSceneSnapshotBuilder Snapshot(BattleScenePage page)
        {
            return new BattleSceneSnapshotBuilder(page)
            {
                HostChrome = new BattleHostChromeSnapshot(),
                Map = new BattleMapSnapshot(mapMessage: "map"),
                Combat = new BattleCombatSnapshot(
                    playerMaxHp: 40,
                    playerHp: 40,
                    playerEnergy: 3,
                    gold: 100,
                    battleHintMessage: "battle"),
                Reward = new BattleRewardSnapshot(),
                RestShop = new BattleRestShopSnapshot("rest"),
                Shop = new BattleShopSnapshot(gold: 100),
                Event = new BattleEventSnapshot(),
                Result = new BattleResultSnapshot("result"),
                PotionReplace = new BattlePotionReplaceSnapshot()
            };
        }
    }

    /// <summary>
    /// RuntimeCard構築クラス
    /// </summary>
    public sealed class RuntimeCardBuilder
    {
        public RuntimeCardBuilder(int id)
        {
            Id = id;
            Key = $"card_{id}";
            DisplayName = $"Card{id}";
        }

        public int Id { get; set; }
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string LocalizationKey { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DescriptionKey { get; set; } = string.Empty;
        public string ImageId { get; set; } = string.Empty;
        public int Cost { get; set; } = 1;
        public CardType CardType { get; set; } = CardType.Attack;
        public CardRarity Rarity { get; set; } = CardRarity.Common;
        public CharacterArchetype CharacterArchetype { get; set; } = CharacterArchetype.CrimsonExile;
        public IReadOnlyList<RuntimeCardEffect> Effects { get; set; } = Array.Empty<RuntimeCardEffect>();
        public int UpgradeCardId { get; set; }
        public bool IsUpgraded { get; set; }

        public RuntimeCard Build()
        {
            return new RuntimeCard(
                Id,
                Key,
                DisplayName,
                LocalizationKey,
                Description,
                DescriptionKey,
                ImageId,
                Cost,
                CardType,
                Rarity,
                CharacterArchetype,
                Effects,
                UpgradeCardId,
                IsUpgraded);
        }
    }

    /// <summary>
    /// RuntimeEnemyAction構築クラス
    /// </summary>
    public sealed class RuntimeEnemyActionBuilder
    {
        public RuntimeEnemyActionBuilder(int order)
        {
            Order = order;
        }

        public int Order { get; set; }
        public IntentType IntentType { get; set; } = IntentType.Attack;
        public int Damage { get; set; }
        public int HitCount { get; set; } = 1;
        public int Block { get; set; }
        public StatusType StatusType { get; set; } = StatusType.None;
        public int StatusValue { get; set; }
        public BuffType BuffType { get; set; } = BuffType.None;
        public int BuffValue { get; set; }
        public RepeatRule RepeatRule { get; set; } = RepeatRule.RepeatAfterOpening;

        public RuntimeEnemyAction Build()
        {
            return new RuntimeEnemyAction(
                Order,
                IntentType,
                Damage,
                HitCount,
                Block,
                StatusType,
                StatusValue,
                BuffType,
                BuffValue,
                RepeatRule);
        }
    }

    /// <summary>
    /// RuntimeEnemy構築クラス
    /// </summary>
    public sealed class RuntimeEnemyBuilder
    {
        public RuntimeEnemyBuilder(int id)
        {
            Id = id;
            Key = $"enemy_{id}";
            DisplayName = $"Enemy{id}";
        }

        public int Id { get; set; }
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string LocalizationKey { get; set; } = string.Empty;
        public EnemyTier EnemyTier { get; set; } = EnemyTier.Normal;
        public int HpMin { get; set; } = 10;
        public int HpMax { get; set; } = 10;
        public int GoldReward { get; set; } = 10;
        public IReadOnlyList<RuntimeEnemyAction> Actions { get; set; } = Array.Empty<RuntimeEnemyAction>();

        public RuntimeEnemy Build()
        {
            return new RuntimeEnemy(
                Id,
                Key,
                DisplayName,
                LocalizationKey,
                EnemyTier,
                HpMin,
                HpMax,
                GoldReward,
                Actions);
        }
    }

    /// <summary>
    /// RuntimeMapNode構築クラス
    /// </summary>
    public sealed class RuntimeMapNodeBuilder
    {
        public RuntimeMapNodeBuilder(int id)
        {
            Id = id;
            NodeKey = $"node_{id}";
            DisplayName = $"Node{id}";
        }

        public int Id { get; set; }
        public string NodeKey { get; set; }
        public int Floor { get; set; } = 1;
        public InGameNodeType NodeType { get; set; } = InGameNodeType.Battle;
        public string DisplayName { get; set; }
        public string LocalizationKey { get; set; } = string.Empty;
        public IReadOnlyList<int> NextNodeIndices { get; set; } = Array.Empty<int>();

        public RuntimeMapNode Build()
        {
            return new RuntimeMapNode(Id, NodeKey, Floor, NodeType, DisplayName, LocalizationKey, NextNodeIndices);
        }
    }

    /// <summary>
    /// RuntimeRewardEntry構築クラス
    /// </summary>
    public sealed class RuntimeRewardEntryBuilder
    {
        public RewardType RewardType { get; set; } = RewardType.Card;
        public int RewardValue { get; set; }
        public RuntimeCard Card { get; set; }
        public RuntimeRelic Relic { get; set; }
        public RuntimePotion Potion { get; set; }
        public int Weight { get; set; } = 1;
        public int MinFloor { get; set; } = 1;
        public int MaxFloor { get; set; } = 99;

        public RuntimeRewardEntry Build()
        {
            int rewardValue = RewardValue;
            if (rewardValue == 0 && RewardType == RewardType.Card && Card != null)
            {
                rewardValue = Card.Id;
            }

            return new RuntimeRewardEntry(RewardType, rewardValue, Card, Relic, Potion, Weight, MinFloor, MaxFloor);
        }
    }

    /// <summary>
    /// RuntimeRelic構築クラス
    /// </summary>
    public sealed class RuntimeRelicBuilder
    {
        public RuntimeRelicBuilder(int id)
        {
            Id = id;
            Key = $"relic_{id}";
            DisplayName = $"Relic{id}";
            ImageId = $"relic_icon_{id}";
        }

        public int Id { get; set; }
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string LocalizationKey { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DescriptionKey { get; set; } = string.Empty;
        public string ImageId { get; set; }
        public CardRarity Rarity { get; set; } = CardRarity.Uncommon;
        public IReadOnlyList<RuntimeRelicEffect> Effects { get; set; } = Array.Empty<RuntimeRelicEffect>();

        public RuntimeRelic Build()
        {
            return new RuntimeRelic(
                Id,
                Key,
                DisplayName,
                LocalizationKey,
                Description,
                DescriptionKey,
                ImageId,
                Rarity,
                Effects);
        }
    }

    /// <summary>
    /// RuntimePotion構築クラス
    /// </summary>
    public sealed class RuntimePotionBuilder
    {
        public RuntimePotionBuilder(int id)
        {
            Id = id;
            Key = $"potion_{id}";
            DisplayName = $"Potion{id}";
            ImageId = $"potion_icon_{id}";
        }

        public int Id { get; set; }
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string LocalizationKey { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DescriptionKey { get; set; } = string.Empty;
        public string ImageId { get; set; }
        public CardRarity Rarity { get; set; } = CardRarity.Uncommon;
        public PotionUseContext UseContext { get; set; } = PotionUseContext.BattleOnly;
        public PotionTargetMode TargetMode { get; set; } = PotionTargetMode.None;
        public IReadOnlyList<RuntimePotionEffect> Effects { get; set; } = Array.Empty<RuntimePotionEffect>();

        public RuntimePotion Build()
        {
            return new RuntimePotion(
                Id,
                Key,
                DisplayName,
                LocalizationKey,
                Description,
                DescriptionKey,
                ImageId,
                Rarity,
                UseContext,
                TargetMode,
                Effects);
        }
    }

    /// <summary>
    /// RuntimeRunDefinition構築クラス
    /// </summary>
    public sealed class RuntimeRunDefinitionBuilder
    {
        public int RunProfileId { get; set; } = 5501;
        public string Key { get; set; } = "run_test";
        public CharacterArchetype CharacterArchetype { get; set; } = CharacterArchetype.CrimsonExile;
        public int PlayerMaxHp { get; set; } = 50;
        public int StartingGold { get; set; } = 120;
        public int CardRewardChoiceCount { get; set; } = 3;
        public int PotionDropChance { get; set; }
        public int RelicDropChance { get; set; }
        public IReadOnlyList<RuntimeCard> StarterDeck { get; set; } = Array.Empty<RuntimeCard>();
        public IReadOnlyDictionary<int, RuntimeCard> CardCatalog { get; set; } = new Dictionary<int, RuntimeCard>();
        public IReadOnlyList<RuntimeRewardEntry> RewardPool { get; set; } = Array.Empty<RuntimeRewardEntry>();
        public IReadOnlyList<RuntimeMapNode> Nodes { get; set; } = Array.Empty<RuntimeMapNode>();
        public IReadOnlyDictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>> EncountersByNodeType { get; set; }
            = new Dictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>>();
        public IReadOnlyList<RuntimeEvent> PossibleEvents { get; set; } = Array.Empty<RuntimeEvent>();
        public IReadOnlyDictionary<int, RuntimeRelic> RelicCatalog { get; set; } = new Dictionary<int, RuntimeRelic>();
        public IReadOnlyDictionary<int, RuntimePotion> PotionCatalog { get; set; } = new Dictionary<int, RuntimePotion>();
        public RuntimeShopLineup ShopLineup { get; set; }
        public IReadOnlyDictionary<CardRarity, RuntimeCardPriceRule> CardPriceRules { get; set; }
            = new Dictionary<CardRarity, RuntimeCardPriceRule>();
        public IReadOnlyList<RuntimeItemPriceRule> ItemPriceRules { get; set; } = Array.Empty<RuntimeItemPriceRule>();

        public RuntimeRunDefinition Build()
        {
            return new RuntimeRunDefinition(
                RunProfileId,
                Key,
                CharacterArchetype,
                PlayerMaxHp,
                StartingGold,
                CardRewardChoiceCount,
                PotionDropChance,
                RelicDropChance,
                StarterDeck,
                CardCatalog,
                RewardPool,
                Nodes,
                EncountersByNodeType,
                PossibleEvents,
                RelicCatalog,
                PotionCatalog,
                ShopLineup,
                CardPriceRules,
                ItemPriceRules);
        }
    }
}
