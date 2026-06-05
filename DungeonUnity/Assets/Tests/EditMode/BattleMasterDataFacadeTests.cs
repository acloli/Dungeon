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
    /// BattleMasterDataFacadeのEditモードテストクラス
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
                    CharacterArchetype = "CrimsonExile",
                    PlayerMaxHp = 80,
                    StartingGold = 99,
                    StarterDeckGroupId = 6001,
                    RewardPoolId = 6101,
                    MapTemplateId = 6301,
                    NormalEncounterGroupId = 6201,
                    EliteEncounterGroupId = 6202,
                    BossEncounterGroupId = 6203,
                    CardRewardChoiceCount = 3
                }
            });
            masterDataService.SetAll(new[]
            {
                new CardMaster { Id = 1001, Key = "card_a", Name = "CardA", LocalizationKey = "card.a", Cost = 1, CardType = "Attack", Rarity = "Basic", CharacterArchetype = "CrimsonExile", CanAppearInReward = false },
                new CardMaster { Id = 1002, Key = "card_b", Name = "CardB", LocalizationKey = "card.b", Cost = 2, CardType = "Skill", Rarity = "Common", CharacterArchetype = "CrimsonExile", CanAppearInReward = true }
            });
            masterDataService.SetAll(new[]
            {
                new CardEffectMaster { Id = 2001, CardId = 1001, Order = 1, EffectType = "DealDamage", Value = 6, HitCount = 1, StatusType = "None", StatusValue = 0, TargetSide = "Enemy" },
                new CardEffectMaster { Id = 2002, CardId = 1002, Order = 1, EffectType = "GainBlock", Value = 5, HitCount = 1, StatusType = "None", StatusValue = 0, TargetSide = "Self" }
            });
            masterDataService.SetAll(new[]
            {
                new DeckGroupMaster { Id = 5001, DeckGroupId = 6001, CardId = 1001, Count = 2, Order = 1 }
            });
            masterDataService.SetAll(new[]
            {
                new RewardPoolMaster { Id = 5101, RewardPoolId = 6101, CardId = 1002, Weight = 10, MinFloor = 1, MaxFloor = 99 }
            });
            masterDataService.SetAll(new[]
            {
                new MapNodeMaster { Id = 5301, MapTemplateId = 6301, NodeKey = "node_01", Floor = 1, NodeType = "Battle", Name = "Node1", LocalizationKey = "map.node.1" },
                new MapNodeMaster { Id = 5302, MapTemplateId = 6301, NodeKey = "node_02", Floor = 2, NodeType = "Boss", Name = "Node2", LocalizationKey = "map.node.2" }
            });
            masterDataService.SetAll(new[]
            {
                new MapEdgeMaster { Id = 5401, MapTemplateId = 6301, FromNodeKey = "node_01", ToNodeKey = "node_02" }
            });
            masterDataService.SetAll(new[]
            {
                new EnemyMaster { Id = 3001, Key = "enemy_a", Name = "EnemyA", LocalizationKey = "enemy.a", EnemyTier = "Normal", HpMin = 10, HpMax = 12, GoldReward = 14, ActionPatternId = 4001 },
                new EnemyMaster { Id = 3002, Key = "enemy_b", Name = "EnemyB", LocalizationKey = "enemy.b", EnemyTier = "Boss", HpMin = 30, HpMax = 30, GoldReward = 100, ActionPatternId = 4002 }
            });
            masterDataService.SetAll(new[]
            {
                new EnemyActionMaster { Id = 4101, EnemyId = 3001, Order = 1, IntentType = "Attack", Damage = 4, HitCount = 1, Block = 0, StatusType = "None", StatusValue = 0, BuffType = "None", BuffValue = 0, RepeatRule = "Random" },
                new EnemyActionMaster { Id = 4201, EnemyId = 3002, Order = 1, IntentType = "Attack", Damage = 12, HitCount = 1, Block = 0, StatusType = "None", StatusValue = 0, BuffType = "None", BuffValue = 0, RepeatRule = "Cycle" }
            });
            masterDataService.SetAll(new[]
            {
                new EncounterGroupMaster { Id = 5201, EncounterGroupId = 6201, EnemyId = 3001, Weight = 10, NodeType = "Battle" },
                new EncounterGroupMaster { Id = 5202, EncounterGroupId = 6203, EnemyId = 3002, Weight = 10, NodeType = "Boss" }
            });

            BattleMasterDataFacade facade = new BattleMasterDataFacade(masterDataService);

            RuntimeRunDefinition runDefinition = facade.BuildRunDefinition(5501);

            Assert.That(runDefinition, Is.Not.Null);
            Assert.That(runDefinition.PlayerMaxHp, Is.EqualTo(80));
            Assert.That(runDefinition.StartingGold, Is.EqualTo(99));
            Assert.That(runDefinition.StarterDeck.Count, Is.EqualTo(2));
            Assert.That(runDefinition.StarterDeck[0].DisplayName, Is.EqualTo("CardA"));
            Assert.That(runDefinition.RewardPool.Count, Is.EqualTo(1));
            Assert.That(runDefinition.RewardPool[0].Card.DisplayName, Is.EqualTo("CardB"));
            Assert.That(runDefinition.Nodes.Count, Is.EqualTo(2));
            Assert.That(runDefinition.Nodes[0].DisplayName, Is.EqualTo("Node1"));
            Assert.That(runDefinition.Nodes[0].NextNodeIndices.Count, Is.EqualTo(1));
            Assert.That(runDefinition.EncountersByNodeType[InGameNodeType.Battle][0].Enemy.DisplayName, Is.EqualTo("EnemyA"));
            Assert.That(runDefinition.EncountersByNodeType[InGameNodeType.Boss][0].Enemy.DisplayName, Is.EqualTo("EnemyB"));
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
