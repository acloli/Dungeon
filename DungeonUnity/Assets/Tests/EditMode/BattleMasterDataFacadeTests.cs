using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Domain;
using Game.MasterData.Generated;
using NUnit.Framework;
using R3;
using TFramework.Localization;
using TFramework.MasterData;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleMasterDataFacadeのEditorモードテストクラス
    /// </summary>
    [TestFixture]
    public sealed class BattleMasterDataFacadeTests
    {
        [Test]
        public void BuildRunDefinition_ExpandsMasterDataIntoRuntimeObjects()
        {
            FakeMasterDataService masterDataService = new FakeMasterDataService();
            masterDataService.SetAll(new[]
            {
                new RunProfileMaster
                {
                    Id = 5501,
                    Key = "run_test",
                    CharacterArchetype = CharacterArchetype.CrimsonExile,
                    PlayerMaxHp = 80,
                    StartingGold = 99,
                    StarterDeckGroupId = 6001,
                    RewardPoolId = 6101,
                    ShopId = 1,
                    EventPoolId = 1,
                    MapTemplateId = 6301,
                    NormalEncounterGroupId = 6201,
                    EliteEncounterGroupId = 6202,
                    BossEncounterGroupId = 6203,
                    CardRewardChoiceCount = 3
                }
            });
            masterDataService.SetAll(new[]
            {
                new CardMaster { Id = 1001, Key = "card_a", Name = "CardA", LocalizationKey = "card.a", DescriptionKey = "card.a.desc", ImageId = "card_a", Cost = 1, CardType = CardType.Attack, Rarity = CardRarity.Basic, CharacterArchetype = CharacterArchetype.CrimsonExile, CanAppearInReward = false, UpgradeCardId = 1002 },
                new CardMaster { Id = 1002, Key = "card_b", Name = "CardB", LocalizationKey = "card.b", DescriptionKey = "card.b.desc", ImageId = "card_b", Cost = 2, CardType = CardType.Skill, Rarity = CardRarity.Common, CharacterArchetype = CharacterArchetype.CrimsonExile, CanAppearInReward = true }
            });
            masterDataService.SetAll(new[]
            {
                new CardEffectMaster { Id = 2001, CardId = 1001, Order = 1, EffectType = EffectType.DealDamage, Value = 6, HitCount = 1, StatusType = StatusType.None, StatusValue = 0, TargetSide = TargetSide.Enemy },
                new CardEffectMaster { Id = 2002, CardId = 1002, Order = 1, EffectType = EffectType.GainBlock, Value = 5, HitCount = 1, StatusType = StatusType.None, StatusValue = 0, TargetSide = TargetSide.Self }
            });
            masterDataService.SetAll(new[]
            {
                new DeckGroupMaster { Id = 5001, DeckGroupId = 6001, CardId = 1001, Count = 2, Order = 1 }
            });
            masterDataService.SetAll(new[]
            {
                new RewardPoolMaster { Id = 5101, RewardPoolId = 6101, RewardType = RewardType.Card, RewardValue = 1002, Weight = 10, MinFloor = 1, MaxFloor = 99 }
            });
            masterDataService.SetAll(new[]
            {
                new EventMaster { Id = 9001, EventName = "Fountain", TitleKey = "event_fountain_title", DescriptionKey = "event_fountain_desc", ImageId = "event_fountain" }
            });
            masterDataService.SetAll(new[]
            {
                new EventChoiceMaster { Id = 9101, EventId = 9001, ChoiceId = 1, LocalizationKey = "event.fountain.choice1", EffectType = EffectType.GainMaxHp, EffectValue = 5 },
                new EventChoiceMaster { Id = 9102, EventId = 9001, ChoiceId = 2, LocalizationKey = "event.fountain.choice2", EffectType = EffectType.GainGold, EffectValue = 100 }
            });
            masterDataService.SetAll(new[]
            {
                new MapNodeMaster { Id = 5301, MapTemplateId = 6301, NodeKey = "node_01", Floor = 1, NodeType = NodeType.Battle, Name = "Node1", LocalizationKey = "map.node.1" },
                new MapNodeMaster { Id = 5302, MapTemplateId = 6301, NodeKey = "node_02", Floor = 2, NodeType = NodeType.Battle, Name = "Node2", LocalizationKey = "map.node.2" },
                new MapNodeMaster { Id = 5303, MapTemplateId = 6301, NodeKey = "node_03", Floor = 2, NodeType = NodeType.Boss, Name = "Node3", LocalizationKey = "map.node.3" }
            });
            masterDataService.SetAll(new[]
            {
                new MapEdgeMaster { Id = 5401, MapTemplateId = 6301, FromNodeKey = "node_01", ToNodeKey = "node_02" },
                new MapEdgeMaster { Id = 5402, MapTemplateId = 6301, FromNodeKey = "node_01", ToNodeKey = "node_03" }
            });
            masterDataService.SetAll(new[]
            {
                new EnemyMaster { Id = 3001, Key = "enemy_a", Name = "EnemyA", LocalizationKey = "enemy.a", EnemyTier = EnemyTier.Normal, HpMin = 10, HpMax = 12, GoldReward = 14, ActionPatternId = 4001 },
                new EnemyMaster { Id = 3002, Key = "enemy_b", Name = "EnemyB", LocalizationKey = "enemy.b", EnemyTier = EnemyTier.Boss, HpMin = 30, HpMax = 30, GoldReward = 100, ActionPatternId = 4002 }
            });
            masterDataService.SetAll(new[]
            {
                new EnemyActionMaster { Id = 4101, EnemyId = 3001, Order = 1, IntentType = IntentType.Attack, Damage = 4, HitCount = 1, Block = 0, StatusType = StatusType.None, StatusValue = 0, BuffType = BuffType.None, BuffValue = 0, RepeatRule = RepeatRule.Random },
                new EnemyActionMaster { Id = 4201, EnemyId = 3002, Order = 1, IntentType = IntentType.Attack, Damage = 12, HitCount = 1, Block = 0, StatusType = StatusType.None, StatusValue = 0, BuffType = BuffType.None, BuffValue = 0, RepeatRule = RepeatRule.Cycle }
            });
            masterDataService.SetAll(new[]
            {
                new EncounterGroupMaster { Id = 5201, EncounterGroupId = 6201, FormationId = 7001, Weight = 10, NodeType = NodeType.Battle },
                new EncounterGroupMaster { Id = 5202, EncounterGroupId = 6203, FormationId = 7002, Weight = 10, NodeType = NodeType.Boss }
            });
            masterDataService.SetAll(new[]
            {
                new EncounterFormationMaster { Id = 7001, Key = "formation_a", Name = "FormationA", NodeType = NodeType.Battle },
                new EncounterFormationMaster { Id = 7002, Key = "formation_b", Name = "FormationB", NodeType = NodeType.Boss }
            });
            masterDataService.SetAll(new[]
            {
                new EncounterFormationEnemyMaster { Id = 7101, FormationId = 7001, EnemyId = 3001, SlotIndex = 0 },
                new EncounterFormationEnemyMaster { Id = 7102, FormationId = 7002, EnemyId = 3002, SlotIndex = 0 }
            });
            masterDataService.SetAll(new[]
            {
                new ShopLineupMaster { Id = 8301, ShopId = 1, SlotIndex = 1, RewardType = RewardType.Card, RequiredCardType = CardType.Attack, Weight = 10 },
                new ShopLineupMaster { Id = 8302, ShopId = 1, SlotIndex = 2, RewardType = RewardType.Relic, RequiredCardType = CardType.None, Weight = 20 }
            });
            masterDataService.SetAll(new[]
            {
                new ShopCardPriceMaster { Id = 8401, CardRarity = CardRarity.Common, BasePrice = 50, JitterPercent = 10 }
            });
            masterDataService.SetAll(new[]
            {
                new ShopItemPriceMaster { Id = 8501, ItemType = RewardType.Potion, ItemId = 1, BasePrice = 60, JitterPercent = 5 }
            });
            masterDataService.SetAll(new[]
            {
                new RelicMaster { Id = 1, Key = "relic_1", Name = "Relic1", LocalizationKey = "relic.1", DescriptionKey = "relic.1.desc", ImageId = "relic_1", Rarity = CardRarity.Uncommon }
            });
            masterDataService.SetAll(new[]
            {
                new RelicEffectMaster { Id = 30001, RelicId = 1, Order = 1, TriggerType = RelicTriggerType.CombatStart, EffectType = EffectType.GainBlock, Value = 6, HitCount = 1, StatusType = StatusType.None, StatusValue = 0, TargetSide = TargetSide.Self }
            });
            masterDataService.SetAll(new[]
            {
                new PotionMaster { Id = 1, Key = "potion_1", Name = "Potion1", LocalizationKey = "potion.1", DescriptionKey = "potion.1.desc", ImageId = "potion_1", Rarity = CardRarity.Common }
            });
            masterDataService.SetAll(new[]
            {
                new PotionEffectMaster { Id = 3201, PotionId = 1, Order = 1, EffectType = EffectType.GainMaxHp, Value = 5, HitCount = 1, StatusType = StatusType.None, StatusValue = 0, TargetSide = TargetSide.Self }
            });
            masterDataService.SetAll(new[]
            {
                new TreasureMaster { Id = 8901, Key = "treasure_act1", MinFloor = 2, MaxFloor = 6, MinGold = 18, MaxGold = 42, RelicGroupId = 7701, PotionDropChance = 25, RelicDropChance = 15 }
            });

            BattleMasterDataFacade facade = new BattleMasterDataFacade(
                masterDataService,
                new EventMasterDataFacade(masterDataService),
                new ShopMasterDataFacade(masterDataService));

            RuntimeRunDefinition runDefinition = facade.BuildRunDefinition(5501);

            Assert.That(runDefinition, Is.Not.Null);
            Assert.That(runDefinition.MapTemplateId, Is.EqualTo(6301));
            Assert.That(runDefinition.PlayerMaxHp, Is.EqualTo(80));
            Assert.That(runDefinition.StartingGold, Is.EqualTo(99));
            Assert.That(runDefinition.StarterDeck.Count, Is.EqualTo(2));
            Assert.That(runDefinition.StarterDeck[0].DisplayName, Is.EqualTo("CardA"));
            Assert.That(runDefinition.StarterDeck[0].DescriptionKey, Is.EqualTo("card.a.desc"));
            Assert.That(runDefinition.StarterDeck[0].UpgradeCardId, Is.EqualTo(1002));
            Assert.That(runDefinition.StarterDeck[0].IsUpgraded, Is.False);
            Assert.That(runDefinition.CardCatalog[1002].IsUpgraded, Is.True);
            Assert.That(runDefinition.RewardPool.Count, Is.EqualTo(1));
            Assert.That(runDefinition.RewardPool[0].Card.DisplayName, Is.EqualTo("CardB"));
            Assert.That(runDefinition.RelicCatalog[1].DisplayName, Is.EqualTo("Relic1"));
            Assert.That(runDefinition.RelicCatalog[1].Effects.Count, Is.EqualTo(1));
            Assert.That(runDefinition.RelicCatalog[1].Effects[0].TriggerType, Is.EqualTo(RelicTriggerType.CombatStart));
            Assert.That(runDefinition.PotionCatalog[1].DisplayName, Is.EqualTo("Potion1"));
            Assert.That(runDefinition.PotionCatalog[1].Effects.Count, Is.EqualTo(1));
            Assert.That(runDefinition.PotionCatalog[1].UseContext, Is.EqualTo(PotionUseContext.Both));
            Assert.That(runDefinition.PotionCatalog[1].TargetMode, Is.EqualTo(PotionTargetMode.Self));
            Assert.That(runDefinition.TreasureDefinitions.Count, Is.EqualTo(1));
            Assert.That(runDefinition.TreasureDefinitions[0].Id, Is.EqualTo(8901));
            Assert.That(runDefinition.TreasureDefinitions[0].MinFloor, Is.EqualTo(2));
            Assert.That(runDefinition.TreasureDefinitions[0].MaxFloor, Is.EqualTo(6));
            Assert.That(runDefinition.TreasureDefinitions[0].GoldMin, Is.EqualTo(18));
            Assert.That(runDefinition.TreasureDefinitions[0].GoldMax, Is.EqualTo(42));
            Assert.That(runDefinition.TreasureDefinitions[0].RelicGroupId, Is.EqualTo(7701));
            Assert.That(runDefinition.TreasureDefinitions[0].PotionDropChance, Is.EqualTo(25));
            Assert.That(runDefinition.TreasureDefinitions[0].RelicDropChance, Is.EqualTo(15));
            Assert.That(runDefinition.PossibleEvents.Count, Is.EqualTo(1));
            Assert.That(runDefinition.PossibleEvents[0].Choices.Count, Is.EqualTo(2));
            Assert.That(runDefinition.ShopLineup.ShopId, Is.EqualTo(1));
            Assert.That(runDefinition.ShopLineup.Slots.Count, Is.EqualTo(2));
            Assert.That(runDefinition.CardPriceRules[CardRarity.Common].BasePrice, Is.EqualTo(50));
            Assert.That(runDefinition.ItemPriceRules.Count, Is.EqualTo(1));
            Assert.That(runDefinition.Nodes.Count, Is.EqualTo(3));
            Assert.That(runDefinition.Nodes[0].DisplayName, Is.EqualTo("Node1"));
            Assert.That(runDefinition.Nodes[0].NextNodeIndices, Is.EqualTo(new[] { 1, 2 }));
            Assert.That(runDefinition.EncountersByNodeType[InGameNodeType.Battle][0].Formation.Enemies[0].Enemy.DisplayName, Is.EqualTo("EnemyA"));
            Assert.That(runDefinition.EncountersByNodeType[InGameNodeType.Boss][0].Formation.Enemies[0].Enemy.DisplayName, Is.EqualTo("EnemyB"));
        }

        [Test]
        public void BuildRunDefinition_PotionWithEnemyTarget_UsesAnyEnemyTargetMode()
        {
            RuntimeRunDefinition runDefinition = BuildRunDefinitionForPotion(new[]
            {
                new PotionEffectMaster { Id = 3201, PotionId = 1, Order = 1, EffectType = EffectType.DealDamage, Value = 10, HitCount = 1, StatusType = StatusType.None, StatusValue = 0, TargetSide = TargetSide.Enemy }
            });

            Assert.That(runDefinition.PotionCatalog[1].TargetMode, Is.EqualTo(PotionTargetMode.AnyEnemy));
        }

        [Test]
        public void BuildRunDefinition_PotionWithAllEnemiesTarget_UsesAllEnemiesTargetMode()
        {
            RuntimeRunDefinition runDefinition = BuildRunDefinitionForPotion(new[]
            {
                new PotionEffectMaster { Id = 3201, PotionId = 1, Order = 1, EffectType = EffectType.DealDamage, Value = 10, HitCount = 1, StatusType = StatusType.None, StatusValue = 0, TargetSide = TargetSide.Enemy },
                new PotionEffectMaster { Id = 3202, PotionId = 1, Order = 2, EffectType = EffectType.ApplyStatus, Value = 0, HitCount = 1, StatusType = StatusType.Weak, StatusValue = 1, TargetSide = TargetSide.AllEnemies }
            });

            Assert.That(runDefinition.PotionCatalog[1].TargetMode, Is.EqualTo(PotionTargetMode.AllEnemies));
        }

        [Test]
        public void BuildRunDefinition_ChapterOneContent_PreservesCatalogMappings()
        {
            FakeMasterDataService masterDataService = new FakeMasterDataService();
            masterDataService.SetAll(new[]
            {
                new RunProfileMaster
                {
                    Id = 5501,
                    Key = "run_chapter_one",
                    CharacterArchetype = CharacterArchetype.CrimsonExile,
                    PlayerMaxHp = 80,
                    StartingGold = 99,
                    StarterDeckGroupId = 6001,
                    RewardPoolId = 6101,
                    ShopId = 1,
                    EventPoolId = 1,
                    MapTemplateId = 6301,
                    NormalEncounterGroupId = 6201,
                    EliteEncounterGroupId = 6202,
                    BossEncounterGroupId = 6203,
                    CardRewardChoiceCount = 3
                }
            });

            RelicMaster[] relicMasters =
            {
                CreateRelicMaster(1, "relic_cracked_crystal_core", "cracked_crystal_core", CardRarity.Common),
                CreateRelicMaster(2, "relic_hunters_talisman", "hunters_talisman", CardRarity.Common),
                CreateRelicMaster(3, "relic_echo_vessel", "echo_vessel", CardRarity.Uncommon),
                CreateRelicMaster(4, "relic_cracked_idol", "cracked_idol", CardRarity.Common),
                CreateRelicMaster(5, "relic_refining_prism", "refining_prism", CardRarity.Rare),
                CreateRelicMaster(6, "relic_bone_white_dice_box", "bone_white_dice_box", CardRarity.Rare)
            };
            masterDataService.SetAll(relicMasters);
            masterDataService.SetAll(new[]
            {
                CreateRelicEffect(30007, 6, 1, RelicTriggerType.CombatVictory, EffectType.UpgradeRandomCommonCard, 1, RelicActivationLimit.Unlimited),
                CreateRelicEffect(30006, 5, 2, RelicTriggerType.RestShopEntered, EffectType.GainFreeCardUpgrade, 1, RelicActivationLimit.OncePerRun),
                CreateRelicEffect(30005, 5, 1, RelicTriggerType.RestShopEntered, EffectType.HealHp, 5, RelicActivationLimit.OncePerRun),
                CreateRelicEffect(30004, 4, 1, RelicTriggerType.PlayerTurnStart, EffectType.GainBlock, 3, RelicActivationLimit.Unlimited),
                CreateRelicEffect(30003, 3, 1, RelicTriggerType.CardPlayed, EffectType.GainBlock, 2, RelicActivationLimit.OncePerTurn),
                CreateRelicEffect(30002, 2, 1, RelicTriggerType.CombatVictory, EffectType.GainGold, 10, RelicActivationLimit.Unlimited),
                CreateRelicEffect(30001, 1, 1, RelicTriggerType.CombatStart, EffectType.GainEnergy, 1, RelicActivationLimit.Unlimited)
            });
            masterDataService.SetAll(new[]
            {
                new RelicEffectConditionMaster { Id = 40004, RelicEffectId = 30003, Order = 2, ConditionType = RelicConditionType.PlayerHpPercentAtMost, CardCost = -1, HpPercent = 90, NodeType = "None" },
                new RelicEffectConditionMaster { Id = 40003, RelicEffectId = 30007, Order = 1, ConditionType = RelicConditionType.NodeTypeEquals, CardCost = -1, HpPercent = 0, NodeType = "EliteBattle" },
                new RelicEffectConditionMaster { Id = 40002, RelicEffectId = 30004, Order = 1, ConditionType = RelicConditionType.PlayerHpPercentAtMost, CardCost = -1, HpPercent = 50, NodeType = "None" },
                new RelicEffectConditionMaster { Id = 40001, RelicEffectId = 30003, Order = 1, ConditionType = RelicConditionType.PlayedCardCostEquals, CardCost = 0, HpPercent = 0, NodeType = "None" }
            });

            PotionMaster[] potionMasters =
            {
                CreatePotionMaster(1, "potion_energy_potion", "energy_potion", CardRarity.Common),
                CreatePotionMaster(2, "potion_block_potion", "block_potion", CardRarity.Common),
                CreatePotionMaster(3, "potion_swift_potion", "swift_potion", CardRarity.Uncommon),
                CreatePotionMaster(4, "potion_fruit_juice", "fruit_juice", CardRarity.Rare),
                CreatePotionMaster(5, "potion_shard_bomb", "shard_bomb", CardRarity.Common),
                CreatePotionMaster(6, "potion_fracture_ampoule", "fracture_ampoule", CardRarity.Uncommon),
                CreatePotionMaster(7, "potion_prism_blast", "prism_blast", CardRarity.Rare)
            };
            masterDataService.SetAll(potionMasters);
            masterDataService.SetAll(new[]
            {
                CreatePotionEffect(3201, 1, EffectType.GainEnergy, 1, StatusType.None, 0, TargetSide.Self),
                CreatePotionEffect(3202, 2, EffectType.GainBlock, 12, StatusType.None, 0, TargetSide.Self),
                CreatePotionEffect(3203, 3, EffectType.DrawCards, 3, StatusType.None, 0, TargetSide.Self),
                CreatePotionEffect(3204, 4, EffectType.GainMaxHp, 5, StatusType.None, 0, TargetSide.Self),
                CreatePotionEffect(3205, 5, EffectType.DealDamage, 15, StatusType.None, 0, TargetSide.Enemy),
                CreatePotionEffect(3206, 6, EffectType.ApplyStatus, 0, StatusType.Vulnerable, 2, TargetSide.Enemy),
                CreatePotionEffect(3207, 7, EffectType.DealDamage, 8, StatusType.None, 0, TargetSide.AllEnemies)
            });

            masterDataService.SetAll(new[]
            {
                CreateRelicReward(5108, 1, 10),
                CreateRelicReward(5109, 2, 10),
                CreateRelicReward(5110, 3, 8),
                CreateRelicReward(5111, 4, 10),
                CreateRelicReward(5112, 5, 5),
                CreateRelicReward(5113, 6, 5)
            });
            masterDataService.SetAll(new[]
            {
                CreateItemPrice(8201, RewardType.Relic, 1, 150, 15),
                CreateItemPrice(8202, RewardType.Relic, 2, 160, 15),
                CreateItemPrice(8203, RewardType.Potion, 1, 50, 10),
                CreateItemPrice(8204, RewardType.Potion, 2, 55, 10),
                CreateItemPrice(8205, RewardType.Potion, 3, 60, 10),
                CreateItemPrice(8206, RewardType.Potion, 4, 70, 10),
                CreateItemPrice(8208, RewardType.Relic, 3, 180, 15),
                CreateItemPrice(8209, RewardType.Relic, 4, 150, 15),
                CreateItemPrice(8210, RewardType.Relic, 5, 220, 15),
                CreateItemPrice(8211, RewardType.Relic, 6, 210, 15),
                CreateItemPrice(8212, RewardType.Potion, 5, 55, 10),
                CreateItemPrice(8213, RewardType.Potion, 6, 60, 10),
                CreateItemPrice(8214, RewardType.Potion, 7, 70, 10)
            });

            FakeLocalizationService localizationService = new FakeLocalizationService();
            foreach (RelicMaster relicMaster in relicMasters)
            {
                localizationService.Set(relicMaster.LocalizationKey, $"localized:{relicMaster.LocalizationKey}");
                localizationService.Set(relicMaster.DescriptionKey, $"localized:{relicMaster.DescriptionKey}");
            }

            foreach (PotionMaster potionMaster in potionMasters)
            {
                localizationService.Set(potionMaster.LocalizationKey, $"localized:{potionMaster.LocalizationKey}");
                localizationService.Set(potionMaster.DescriptionKey, $"localized:{potionMaster.DescriptionKey}");
            }

            BattleMasterDataFacade facade = new BattleMasterDataFacade(
                masterDataService,
                new EventMasterDataFacade(masterDataService),
                new ShopMasterDataFacade(masterDataService),
                localizationService);

            RuntimeRunDefinition runDefinition = facade.BuildRunDefinition(5501);

            Assert.That(runDefinition.RelicCatalog, Has.Count.EqualTo(6));
            Assert.That(runDefinition.PotionCatalog, Has.Count.EqualTo(7));
            AssertLocalizedRelics(runDefinition, relicMasters);
            AssertLocalizedPotions(runDefinition, potionMasters);

            AssertRelicEffect(runDefinition, 1, 0, 30001, EffectType.GainEnergy, RelicActivationLimit.Unlimited);
            AssertRelicEffect(runDefinition, 2, 0, 30002, EffectType.GainGold, RelicActivationLimit.Unlimited);
            AssertRelicEffect(runDefinition, 3, 0, 30003, EffectType.GainBlock, RelicActivationLimit.OncePerTurn);
            AssertRelicEffect(runDefinition, 4, 0, 30004, EffectType.GainBlock, RelicActivationLimit.Unlimited);
            AssertRelicEffect(runDefinition, 5, 0, 30005, EffectType.HealHp, RelicActivationLimit.OncePerRun);
            AssertRelicEffect(runDefinition, 5, 1, 30006, EffectType.GainFreeCardUpgrade, RelicActivationLimit.OncePerRun);
            AssertRelicEffect(runDefinition, 6, 0, 30007, EffectType.UpgradeRandomCommonCard, RelicActivationLimit.Unlimited);

            RuntimeRelicEffect echoVesselEffect = runDefinition.RelicCatalog[3].Effects[0];
            Assert.That(echoVesselEffect.Conditions.Select(condition => condition.Id), Is.EqualTo(new[] { 40001, 40004 }));
            Assert.That(echoVesselEffect.Conditions.Select(condition => condition.Order), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(echoVesselEffect.Conditions[0].ConditionType, Is.EqualTo(RelicConditionType.PlayedCardCostEquals));
            Assert.That(echoVesselEffect.Conditions[0].CardCost, Is.Zero);
            Assert.That(runDefinition.RelicCatalog[4].Effects[0].Conditions[0].HpPercent, Is.EqualTo(50));
            Assert.That(runDefinition.RelicCatalog[6].Effects[0].Conditions[0].NodeType, Is.EqualTo(InGameNodeType.EliteBattle));

            AssertPotionEffect(runDefinition, 5, EffectType.DealDamage, 15, StatusType.None, 0, TargetSide.Enemy, PotionTargetMode.AnyEnemy);
            AssertPotionEffect(runDefinition, 6, EffectType.ApplyStatus, 0, StatusType.Vulnerable, 2, TargetSide.Enemy, PotionTargetMode.AnyEnemy);
            AssertPotionEffect(runDefinition, 7, EffectType.DealDamage, 8, StatusType.None, 0, TargetSide.AllEnemies, PotionTargetMode.AllEnemies);

            RuntimeRewardEntry[] relicRewards = runDefinition.RewardPool
                .Where(entry => entry.RewardType == RewardType.Relic)
                .ToArray();
            Assert.That(relicRewards.Select(entry => entry.RewardValue), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6 }));
            Assert.That(relicRewards.Select(entry => entry.Weight), Is.EqualTo(new[] { 10, 10, 8, 10, 5, 5 }));
            Assert.That(relicRewards.All(entry => entry.Relic != null), Is.True);

            Assert.That(runDefinition.ItemPriceRules, Has.Count.EqualTo(13));
            AssertItemPrice(runDefinition, RewardType.Relic, 1, 150, 15);
            AssertItemPrice(runDefinition, RewardType.Relic, 2, 160, 15);
            AssertItemPrice(runDefinition, RewardType.Relic, 3, 180, 15);
            AssertItemPrice(runDefinition, RewardType.Relic, 4, 150, 15);
            AssertItemPrice(runDefinition, RewardType.Relic, 5, 220, 15);
            AssertItemPrice(runDefinition, RewardType.Relic, 6, 210, 15);
            AssertItemPrice(runDefinition, RewardType.Potion, 1, 50, 10);
            AssertItemPrice(runDefinition, RewardType.Potion, 2, 55, 10);
            AssertItemPrice(runDefinition, RewardType.Potion, 3, 60, 10);
            AssertItemPrice(runDefinition, RewardType.Potion, 4, 70, 10);
            AssertItemPrice(runDefinition, RewardType.Potion, 5, 55, 10);
            AssertItemPrice(runDefinition, RewardType.Potion, 6, 60, 10);
            AssertItemPrice(runDefinition, RewardType.Potion, 7, 70, 10);
        }

        private static RuntimeRunDefinition BuildRunDefinitionForPotion(IReadOnlyList<PotionEffectMaster> potionEffects)
        {
            FakeMasterDataService masterDataService = new FakeMasterDataService();
            masterDataService.SetAll(new[]
            {
                new RunProfileMaster
                {
                    Id = 5501,
                    Key = "run_test",
                    CharacterArchetype = CharacterArchetype.CrimsonExile,
                    PlayerMaxHp = 80,
                    StartingGold = 99,
                    StarterDeckGroupId = 6001,
                    RewardPoolId = 6101,
                    ShopId = 1,
                    EventPoolId = 1,
                    MapTemplateId = 6301,
                    NormalEncounterGroupId = 6201,
                    EliteEncounterGroupId = 6202,
                    BossEncounterGroupId = 6203,
                    CardRewardChoiceCount = 3
                }
            });
            masterDataService.SetAll(new[]
            {
                new PotionMaster { Id = 1, Key = "potion_1", Name = "Potion1", LocalizationKey = "potion.1", DescriptionKey = "potion.1.desc", ImageId = "potion_1", Rarity = CardRarity.Common }
            });
            masterDataService.SetAll(potionEffects);

            BattleMasterDataFacade facade = new BattleMasterDataFacade(
                masterDataService,
                new EventMasterDataFacade(masterDataService),
                new ShopMasterDataFacade(masterDataService));

            return facade.BuildRunDefinition(5501);
        }

        private static RelicMaster CreateRelicMaster(int id, string key, string localizationSuffix, CardRarity rarity)
        {
            return new RelicMaster
            {
                Id = id,
                Key = key,
                Name = key,
                LocalizationKey = $"relic.name.{localizationSuffix}",
                DescriptionKey = $"relic.desc.{localizationSuffix}",
                ImageId = $"relic_art_{localizationSuffix}",
                Rarity = rarity
            };
        }

        private static RelicEffectMaster CreateRelicEffect(
            int id,
            int relicId,
            int order,
            RelicTriggerType triggerType,
            EffectType effectType,
            int value,
            RelicActivationLimit activationLimit)
        {
            return new RelicEffectMaster
            {
                Id = id,
                RelicId = relicId,
                Order = order,
                TriggerType = triggerType,
                EffectType = effectType,
                Value = value,
                HitCount = 1,
                StatusType = StatusType.None,
                StatusValue = 0,
                TargetSide = TargetSide.Self,
                ActivationLimit = activationLimit
            };
        }

        private static PotionMaster CreatePotionMaster(int id, string key, string localizationSuffix, CardRarity rarity)
        {
            return new PotionMaster
            {
                Id = id,
                Key = key,
                Name = key,
                LocalizationKey = $"potion.name.{localizationSuffix}",
                DescriptionKey = $"potion.desc.{localizationSuffix}",
                ImageId = $"potion_art_{localizationSuffix}",
                Rarity = rarity
            };
        }

        private static PotionEffectMaster CreatePotionEffect(
            int id,
            int potionId,
            EffectType effectType,
            int value,
            StatusType statusType,
            int statusValue,
            TargetSide targetSide)
        {
            return new PotionEffectMaster
            {
                Id = id,
                PotionId = potionId,
                Order = 1,
                EffectType = effectType,
                Value = value,
                HitCount = 1,
                StatusType = statusType,
                StatusValue = statusValue,
                TargetSide = targetSide
            };
        }

        private static RewardPoolMaster CreateRelicReward(int id, int relicId, int weight)
        {
            return new RewardPoolMaster
            {
                Id = id,
                RewardPoolId = 6101,
                RewardType = RewardType.Relic,
                RewardValue = relicId,
                Weight = weight,
                MinFloor = 1,
                MaxFloor = 99
            };
        }

        private static ShopItemPriceMaster CreateItemPrice(int id, RewardType itemType, int itemId, int basePrice, int jitterPercent)
        {
            return new ShopItemPriceMaster
            {
                Id = id,
                ItemType = itemType,
                ItemId = itemId,
                BasePrice = basePrice,
                JitterPercent = jitterPercent
            };
        }

        private static void AssertLocalizedRelics(RuntimeRunDefinition runDefinition, IReadOnlyList<RelicMaster> masters)
        {
            foreach (RelicMaster master in masters)
            {
                RuntimeRelic relic = runDefinition.RelicCatalog[master.Id];
                Assert.That(relic.Key, Is.EqualTo(master.Key));
                Assert.That(relic.LocalizationKey, Is.EqualTo(master.LocalizationKey));
                Assert.That(relic.DescriptionKey, Is.EqualTo(master.DescriptionKey));
                Assert.That(relic.DisplayName, Is.EqualTo($"localized:{master.LocalizationKey}"));
                Assert.That(relic.Description, Is.EqualTo($"localized:{master.DescriptionKey}"));
            }
        }

        private static void AssertLocalizedPotions(RuntimeRunDefinition runDefinition, IReadOnlyList<PotionMaster> masters)
        {
            foreach (PotionMaster master in masters)
            {
                RuntimePotion potion = runDefinition.PotionCatalog[master.Id];
                Assert.That(potion.Key, Is.EqualTo(master.Key));
                Assert.That(potion.LocalizationKey, Is.EqualTo(master.LocalizationKey));
                Assert.That(potion.DescriptionKey, Is.EqualTo(master.DescriptionKey));
                Assert.That(potion.DisplayName, Is.EqualTo($"localized:{master.LocalizationKey}"));
                Assert.That(potion.Description, Is.EqualTo($"localized:{master.DescriptionKey}"));
            }
        }

        private static void AssertRelicEffect(
            RuntimeRunDefinition runDefinition,
            int relicId,
            int effectIndex,
            int effectId,
            EffectType effectType,
            RelicActivationLimit activationLimit)
        {
            RuntimeRelicEffect effect = runDefinition.RelicCatalog[relicId].Effects[effectIndex];
            Assert.That(effect.Id, Is.EqualTo(effectId));
            Assert.That(effect.Order, Is.EqualTo(effectIndex + 1));
            Assert.That(effect.EffectType, Is.EqualTo(effectType));
            Assert.That(effect.ActivationLimit, Is.EqualTo(activationLimit));
        }

        private static void AssertPotionEffect(
            RuntimeRunDefinition runDefinition,
            int potionId,
            EffectType effectType,
            int value,
            StatusType statusType,
            int statusValue,
            TargetSide targetSide,
            PotionTargetMode targetMode)
        {
            RuntimePotion potion = runDefinition.PotionCatalog[potionId];
            Assert.That(potion.TargetMode, Is.EqualTo(targetMode));
            Assert.That(potion.Effects[0].EffectType, Is.EqualTo(effectType));
            Assert.That(potion.Effects[0].Value, Is.EqualTo(value));
            Assert.That(potion.Effects[0].StatusType, Is.EqualTo(statusType));
            Assert.That(potion.Effects[0].StatusValue, Is.EqualTo(statusValue));
            Assert.That(potion.Effects[0].TargetSide, Is.EqualTo(targetSide));
        }

        private static void AssertItemPrice(
            RuntimeRunDefinition runDefinition,
            RewardType itemType,
            int itemId,
            int basePrice,
            int jitterPercent)
        {
            RuntimeItemPriceRule rule = runDefinition.ItemPriceRules.Single(item => item.ItemType == itemType && item.ItemId == itemId);
            Assert.That(rule.BasePrice, Is.EqualTo(basePrice));
            Assert.That(rule.JitterPercent, Is.EqualTo(jitterPercent));
        }

        /// <summary>
        /// テスト用MasterDataService
        /// </summary>
        private sealed class FakeMasterDataService : IMasterDataService
        {
            private readonly Dictionary<Type, object> _allData = new Dictionary<Type, object>();

            public void SetAll<T>(IReadOnlyList<T> values) where T : class, IMasterDataObject
            {
                _allData[typeof(T)] = values;
            }

            public UniTask InitializeAsync(CancellationToken ct)
            {
                return UniTask.CompletedTask;
            }

            public IReadOnlyList<T> GetAll<T>() where T : class, IMasterDataObject
            {
                if (_allData.TryGetValue(typeof(T), out object values))
                {
                    return (IReadOnlyList<T>)values;
                }

                return Array.Empty<T>();
            }

            public T Get<T, TKey>(TKey key) where T : class, IMasterDataObject<TKey>
            {
                IReadOnlyList<T> all = GetAll<T>();
                for (int i = 0; i < all.Count; i++)
                {
                    if (Equals(all[i].GetKey(), key))
                    {
                        return all[i];
                    }
                }

                return null;
            }

            public T GetContainer<T>() where T : class
            {
                return null;
            }

            public UniTask DownloadFromServerAsync(CancellationToken ct)
            {
                return UniTask.CompletedTask;
            }

            public UniTask ReloadAsync(CancellationToken ct)
            {
                return UniTask.CompletedTask;
            }
        }

        /// <summary>
        /// テスト用LocalizationService
        /// </summary>
        private sealed class FakeLocalizationService : ILocalizationService
        {
            private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

            public LanguageCode CurrentLanguage { get; set; } = LanguageCode.Japanese;
            public LanguageCode[] SupportedLanguages { get; } = { LanguageCode.Japanese };
            public Observable<LanguageCode> OnLanguageChanged => null;

            public void Set(string key, string value)
            {
                _values[key] = value;
            }

            public string Get(string key)
            {
                return _values.TryGetValue(key, out string value) ? value : key;
            }

            public string Get(string key, params object[] args)
            {
                return string.Format(Get(key), args);
            }

            public bool HasKey(string key)
            {
                return _values.ContainsKey(key);
            }

            public UniTask LoadLanguageAsync(LanguageCode language, CancellationToken ct)
            {
                CurrentLanguage = language;
                return UniTask.CompletedTask;
            }
        }
    }
}
