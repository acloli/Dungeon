using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Domain;
using Game.MasterData.Generated;
using NUnit.Framework;
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
    }
}
