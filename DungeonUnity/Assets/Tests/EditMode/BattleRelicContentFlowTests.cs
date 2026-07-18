using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Domain;
using Dungeon.Runtime.InGame.Save.Model;
using Dungeon.Runtime.InGame.Save.Services;
using Dungeon.Tests.EditMode.Support;
using Game.MasterData.Generated;
using NUnit.Framework;
using TFramework.MasterData;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// Chapter 1レリックコンテンツのフロー結合テストクラス
    /// </summary>
    [TestFixture]
    public sealed class BattleRelicContentFlowTests
    {
        private const int CurrentMapLayoutVersionForTest = 2;

        [TestCase(InGameNodeType.Battle, 130, 1001)]
        [TestCase(InGameNodeType.EliteBattle, 120, 1002)]
        [TestCase(InGameNodeType.Boss, 120, 1001)]
        public void CombatVictory_AppliesNodeSpecificRelicsBeforePreparingRewards(
            InGameNodeType nodeType,
            int expectedGold,
            int expectedDeckCardId)
        {
            RuntimeRunDefinition runDefinition = CreateRunDefinition(new[]
            {
                CreateNode(5301, nodeType, Array.Empty<int>())
            });
            SequenceRandomProvider randomProvider = new SequenceRandomProvider(0, 0, 0, 0);
            RecordingRewardFlowService rewardFlowService = new RecordingRewardFlowService();
            BattleSceneFlowService service = CreateService(
                runDefinition,
                randomProvider,
                rewardFlowService);

            service.InitializeFromSave(CreateMapSave(
                playerHp: 50,
                ownedRelicIds: new[] { 2, 6 }));
            service.SelectMapNode(0);
            service.SelectHandCard(0);
            service.TryPlaySelectedCard();

            Assert.That(rewardFlowService.PrepareCallCount, Is.EqualTo(1));
            Assert.That(rewardFlowService.GoldAtPrepare, Is.EqualTo(expectedGold));
            Assert.That(rewardFlowService.DeckCardIdsAtPrepare, Is.EqualTo(new[] { expectedDeckCardId }));
        }

        [Test]
        public void RestShop_ReopenAndCancel_DoNotRepeatRelicOrRerollLineup()
        {
            RuntimeRunDefinition runDefinition = CreateRunDefinition(new[]
            {
                CreateNode(5301, InGameNodeType.RestShop, new[] { 1 }),
                CreateNode(5302, InGameNodeType.RestShop, Array.Empty<int>())
            });
            SequenceRandomProvider randomProvider = new SequenceRandomProvider(0, 0, 1, 0);
            FakeRunSaveService runSaveService = new FakeRunSaveService();
            BattleSceneFlowService service = CreateService(
                runDefinition,
                randomProvider,
                runSaveService: runSaveService);

            service.InitializeFromSave(CreateMapSave(
                playerHp: 40,
                ownedRelicIds: new[] { 5 }));
            service.SelectMapNode(0);
            BattleSceneSnapshot enteredSnapshot = service.CreateSnapshot();
            string enteredLineup = BuildLineupSignature(enteredSnapshot);
            int enteredRandomCounter = randomProvider.Counter;

            Assert.That(enteredSnapshot.Combat.PlayerHp, Is.EqualTo(45));
            Assert.That(runSaveService.LastSavedData.RestShopFreeUpgradeCount, Is.EqualTo(1));
            Assert.That(runSaveService.LastSavedData.ActivatedRelicEffectIdsThisRun, Is.EquivalentTo(new[] { 30005, 30006 }));

            service.ApplyUpgrade();
            Assert.That(service.GetCardSelectPrices()[1001], Is.Zero);
            service.CancelCardSelect();
            service.OpenShop();
            service.LeaveShop();
            service.ApplyUpgrade();
            Assert.That(service.GetCardSelectPrices()[1001], Is.Zero);
            service.CancelCardSelect();
            BattleSceneSnapshot reopenedSnapshot = service.CreateSnapshot();

            Assert.That(reopenedSnapshot.Combat.PlayerHp, Is.EqualTo(45));
            Assert.That(BuildLineupSignature(reopenedSnapshot), Is.EqualTo(enteredLineup));
            Assert.That(randomProvider.Counter, Is.EqualTo(enteredRandomCounter));
        }

        [Test]
        public void RestShop_SaveContinue_PreservesVisitAndNextVisitCreatesNewLineup()
        {
            RuntimeRunDefinition runDefinition = CreateRunDefinition(new[]
            {
                CreateNode(5301, InGameNodeType.RestShop, new[] { 1 }),
                CreateNode(5302, InGameNodeType.RestShop, Array.Empty<int>())
            });
            SequenceRandomProvider originalRandomProvider = new SequenceRandomProvider(0, 0, 1, 0);
            FakeRunSaveService originalRunSaveService = new FakeRunSaveService();
            BattleSceneFlowService originalService = CreateService(
                runDefinition,
                originalRandomProvider,
                runSaveService: originalRunSaveService);
            originalService.InitializeFromSave(CreateMapSave(
                playerHp: 40,
                ownedRelicIds: new[] { 5 }));
            originalService.SelectMapNode(0);
            RunSaveData checkpoint = CloneSaveData(originalRunSaveService.LastSavedData);
            string originalLineup = BuildLineupSignature(originalService.CreateSnapshot());

            SequenceRandomProvider restoredRandomProvider = new SequenceRandomProvider(0, 0, 1, 0);
            FakeRunSaveService restoredRunSaveService = new FakeRunSaveService();
            BattleSceneFlowService restoredService = CreateService(
                runDefinition,
                restoredRandomProvider,
                runSaveService: restoredRunSaveService);
            restoredService.InitializeFromSave(checkpoint);
            BattleSceneSnapshot restoredSnapshot = restoredService.CreateSnapshot();

            Assert.That(restoredSnapshot.Combat.PlayerHp, Is.EqualTo(45));
            Assert.That(BuildLineupSignature(restoredSnapshot), Is.EqualTo(originalLineup));
            Assert.That(restoredRandomProvider.Counter, Is.EqualTo(checkpoint.RandomCounter));
            restoredService.ApplyUpgrade();
            Assert.That(restoredService.GetCardSelectPrices()[1001], Is.Zero);
            restoredService.CancelCardSelect();

            restoredService.ContinueFromRestShop();
            restoredService.SelectMapNode(1);
            RunSaveData nextVisitSave = restoredRunSaveService.LastSavedData;
            BattleSceneSnapshot nextVisitSnapshot = restoredService.CreateSnapshot();

            Assert.That(nextVisitSnapshot.Combat.PlayerHp, Is.EqualTo(45));
            Assert.That(nextVisitSave.RestShopFreeUpgradeCount, Is.Zero);
            Assert.That(nextVisitSave.ActivatedRelicEffectIdsThisRun, Is.EquivalentTo(new[] { 30005, 30006 }));
            Assert.That(BuildLineupSignature(nextVisitSnapshot), Is.Not.EqualTo(originalLineup));
            Assert.That(restoredRandomProvider.Counter, Is.GreaterThan(checkpoint.RandomCounter));
        }

        private static BattleSceneFlowService CreateService(
            RuntimeRunDefinition runDefinition,
            SequenceRandomProvider randomProvider,
            IBattleRewardFlowService rewardFlowService = null,
            IRunSaveService runSaveService = null)
        {
            BattleDeckService deckService = new BattleDeckService();
            BattleEnemyActionSelector enemyActionSelector = new BattleEnemyActionSelector();
            BattleSceneRules rules = new BattleSceneRules(
                deckService,
                new BattleCombatResolver(deckService, enemyActionSelector),
                new BattleEncounterSelector(),
                new BattleRewardRollService());
            EmptyMasterDataService masterDataService = new EmptyMasterDataService();
            BattleCardUpgradeService cardUpgradeService = new BattleCardUpgradeService();
            BattleRelicService relicService = new BattleRelicService(
                rules,
                randomProvider,
                masterDataService,
                cardUpgradeService);
            BattlePotionService potionService = new BattlePotionService();
            BattleShopService shopService = new BattleShopService(masterDataService);
            IBattleRewardFlowService resolvedRewardFlowService = rewardFlowService
                ?? new BattleRewardFlowService(
                    new BattleRewardService(),
                    rules,
                    randomProvider,
                    potionService,
                    relicService);

            return new BattleSceneFlowService(
                rules,
                randomProvider,
                new FakeBattleMasterDataFacade(runDefinition),
                new FixedBattleMapGenerator(runDefinition.Nodes),
                resolvedRewardFlowService,
                new BattleSnapshotFactory(
                    new BattleDisplayTextService(),
                    shopService,
                    enemyActionSelector,
                    new BattlePileOrderService()),
                shopService,
                new BattleCombatEventService(relicService),
                relicService,
                potionService,
                new BattleEventFlowService(randomProvider, new BattleEventService()),
                new BattleRestShopFlowService(
                    rules,
                    randomProvider,
                    shopService,
                    potionService,
                    relicService,
                    cardUpgradeService),
                runSaveService,
                new BattleCheckpointService(),
                enemyActionSelector);
        }

        private static RuntimeRunDefinition CreateRunDefinition(IReadOnlyList<RuntimeMapNode> nodes)
        {
            RuntimeCard baseCard = CreateCard(1001, 1002, false, 99);
            RuntimeCard upgradedCard = CreateCard(1002, 0, true, 120);
            RuntimeEnemy enemy = CreateEnemy();
            RuntimeEncounterEntry encounter = new RuntimeEncounterEntry(
                new RuntimeEncounterFormation(
                    7001,
                    "formation_chapter_one",
                    "Chapter One",
                    new[] { new RuntimeEncounterEnemyEntry(enemy, 0) }),
                10);
            RuntimePotion firstPotion = CreatePotion(1);
            RuntimePotion secondPotion = CreatePotion(2);
            RuntimeRunDefinitionBuilder builder = BattleTestData.RunDefinition();
            builder.StartingGold = 120;
            builder.StarterDeck = new[] { baseCard };
            builder.CardCatalog = new Dictionary<int, RuntimeCard>
            {
                { baseCard.Id, baseCard },
                { upgradedCard.Id, upgradedCard }
            };
            builder.RewardPool = Array.Empty<RuntimeRewardEntry>();
            builder.Nodes = nodes;
            builder.EncountersByNodeType = new Dictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>>
            {
                { InGameNodeType.Battle, new[] { encounter } },
                { InGameNodeType.EliteBattle, new[] { encounter } },
                { InGameNodeType.Boss, new[] { encounter } }
            };
            builder.RelicCatalog = CreateChapterOneRelics();
            builder.PotionCatalog = new Dictionary<int, RuntimePotion>
            {
                { firstPotion.Id, firstPotion },
                { secondPotion.Id, secondPotion }
            };
            builder.ShopLineup = new RuntimeShopLineup(
                1,
                new[] { new RuntimeShopSlot(0, RewardType.Potion, CardType.None, 100) });
            builder.ItemPriceRules = new[]
            {
                new RuntimeItemPriceRule(RewardType.Potion, 1, 50, 0),
                new RuntimeItemPriceRule(RewardType.Potion, 2, 60, 0)
            };
            return builder.Build();
        }

        private static IReadOnlyDictionary<int, RuntimeRelic> CreateChapterOneRelics()
        {
            RuntimeRelic hunterTalisman = CreateRelic(
                2,
                "Hunter's Talisman",
                new RuntimeRelicEffect(
                    30002,
                    1,
                    RelicTriggerType.CombatVictory,
                    EffectType.GainGold,
                    10,
                    1,
                    StatusType.None,
                    0,
                    TargetSide.Self,
                    conditions: new[]
                    {
                        new RuntimeRelicCondition(40020, 1, RelicConditionType.NodeTypeEquals, -1, 0, InGameNodeType.Battle)
                    }));
            RuntimeRelic refiningPrism = CreateRelic(
                5,
                "Refining Prism",
                new RuntimeRelicEffect(
                    30005,
                    1,
                    RelicTriggerType.RestShopEntered,
                    EffectType.HealHp,
                    5,
                    1,
                    StatusType.None,
                    0,
                    TargetSide.Self,
                    activationLimit: RelicActivationLimit.OncePerRun),
                new RuntimeRelicEffect(
                    30006,
                    2,
                    RelicTriggerType.RestShopEntered,
                    EffectType.GainFreeCardUpgrade,
                    1,
                    1,
                    StatusType.None,
                    0,
                    TargetSide.Self,
                    activationLimit: RelicActivationLimit.OncePerRun));
            RuntimeRelic diceBox = CreateRelic(
                6,
                "Bone-White Dice Box",
                new RuntimeRelicEffect(
                    30007,
                    1,
                    RelicTriggerType.CombatVictory,
                    EffectType.UpgradeRandomCommonCard,
                    1,
                    1,
                    StatusType.None,
                    0,
                    TargetSide.Self,
                    conditions: new[]
                    {
                        new RuntimeRelicCondition(40003, 1, RelicConditionType.NodeTypeEquals, -1, 0, InGameNodeType.EliteBattle)
                    }));

            return new Dictionary<int, RuntimeRelic>
            {
                { hunterTalisman.Id, hunterTalisman },
                { refiningPrism.Id, refiningPrism },
                { diceBox.Id, diceBox }
            };
        }

        private static RuntimeRelic CreateRelic(int id, string displayName, params RuntimeRelicEffect[] effects)
        {
            RuntimeRelicBuilder builder = BattleTestData.Relic(id);
            builder.DisplayName = displayName;
            builder.Effects = effects;
            return builder.Build();
        }

        private static RuntimeCard CreateCard(int id, int upgradeCardId, bool isUpgraded, int damage)
        {
            RuntimeCardBuilder builder = BattleTestData.Card(id);
            builder.DisplayName = isUpgraded ? "Strike+" : "Strike";
            builder.Rarity = CardRarity.Common;
            builder.UpgradeCardId = upgradeCardId;
            builder.IsUpgraded = isUpgraded;
            builder.Effects = new[]
            {
                new RuntimeCardEffect(1, EffectType.DealDamage, damage, 1, StatusType.None, 0, TargetSide.Enemy)
            };
            return builder.Build();
        }

        private static RuntimeEnemy CreateEnemy()
        {
            RuntimeEnemyBuilder builder = BattleTestData.Enemy(3001);
            builder.HpMin = 10;
            builder.HpMax = 10;
            builder.GoldReward = 10;
            builder.Actions = new[]
            {
                new RuntimeEnemyAction(1, IntentType.Attack, 0, 1, 0, StatusType.None, 0, BuffType.None, 0, RepeatRule.Cycle)
            };
            return builder.Build();
        }

        private static RuntimePotion CreatePotion(int id)
        {
            RuntimePotionBuilder builder = BattleTestData.Potion(id);
            builder.DisplayName = $"Potion{id}";
            return builder.Build();
        }

        private static RuntimeMapNode CreateNode(int id, InGameNodeType nodeType, IReadOnlyList<int> nextNodeIndices)
        {
            RuntimeMapNodeBuilder builder = BattleTestData.MapNode(id);
            builder.NodeType = nodeType;
            builder.NextNodeIndices = nextNodeIndices;
            return builder.Build();
        }

        private static RunSaveData CreateMapSave(int playerHp, IReadOnlyList<int> ownedRelicIds)
        {
            return new RunSaveData
            {
                RunProfileId = 5501,
                PlayerMaxHp = 50,
                PlayerHp = playerHp,
                PlayerEnergy = 3,
                MaxPotionCount = 3,
                Gold = 120,
                CurrentNodeIndex = -1,
                CurrentPage = (int)BattleScenePage.Map,
                MasterSeed = 111,
                MapSeed = 222,
                MapLayoutVersion = CurrentMapLayoutVersionForTest,
                RandomCounter = 0,
                DeckCardIds = new List<int> { 1001 },
                OwnedRelicIds = new List<int>(ownedRelicIds),
                OwnedPotionIds = new List<int>()
            };
        }

        private static RunSaveData CloneSaveData(RunSaveData source)
        {
            return new RunSaveData
            {
                RunProfileId = source.RunProfileId,
                PlayerMaxHp = source.PlayerMaxHp,
                PlayerHp = source.PlayerHp,
                PlayerEnergy = source.PlayerEnergy,
                MaxPotionCount = source.MaxPotionCount,
                Gold = source.Gold,
                CurrentNodeIndex = source.CurrentNodeIndex,
                CurrentPage = source.CurrentPage,
                MasterSeed = source.MasterSeed,
                MapSeed = source.MapSeed,
                MapLayoutVersion = source.MapLayoutVersion,
                RandomCounter = source.RandomCounter,
                DeckCardIds = new List<int>(source.DeckCardIds),
                MapRouteNodeIndices = new List<int>(source.MapRouteNodeIndices),
                OwnedRelicIds = new List<int>(source.OwnedRelicIds),
                OwnedPotionIds = new List<int>(source.OwnedPotionIds),
                ActivatedRelicEffectIdsThisRun = new List<int>(source.ActivatedRelicEffectIdsThisRun),
                ShopItems = new List<SaveShopItem>(source.ShopItems),
                IsCardRemovalSoldOut = source.IsCardRemovalSoldOut,
                CardRemovalCount = source.CardRemovalCount,
                RestShopFreeUpgradeCount = source.RestShopFreeUpgradeCount
            };
        }

        private static string BuildLineupSignature(BattleSceneSnapshot snapshot)
        {
            return string.Join(
                "|",
                snapshot.Shop.ShopItems.Select(item => $"{item.SlotIndex}:{item.RewardType}:{item.ItemId}:{item.Price}"));
        }

        /// <summary>
        /// テスト用MasterDataFacade
        /// </summary>
        private sealed class FakeBattleMasterDataFacade : IBattleMasterDataFacade
        {
            private readonly RuntimeRunDefinition _runDefinition;

            public FakeBattleMasterDataFacade(RuntimeRunDefinition runDefinition)
            {
                _runDefinition = runDefinition;
            }

            public RuntimeRunDefinition BuildRunDefinition(int runProfileId)
            {
                return _runDefinition;
            }

            public IReadOnlyDictionary<int, RuntimeCard> BuildCardCatalog()
            {
                return _runDefinition.CardCatalog;
            }
        }

        /// <summary>
        /// テスト用固定マップ生成クラス
        /// </summary>
        private sealed class FixedBattleMapGenerator : IBattleMapGenerator
        {
            private readonly IReadOnlyList<RuntimeMapNode> _nodes;

            public FixedBattleMapGenerator(IReadOnlyList<RuntimeMapNode> nodes)
            {
                _nodes = nodes;
            }

            public IReadOnlyList<RuntimeMapNode> Generate(RuntimeRunDefinition runDefinition, int mapSeed)
            {
                return _nodes;
            }
        }

        /// <summary>
        /// テスト用報酬フロー記録クラス
        /// </summary>
        private sealed class RecordingRewardFlowService : IBattleRewardFlowService
        {
            public int PrepareCallCount { get; private set; }
            public int GoldAtPrepare { get; private set; }
            public IReadOnlyList<int> DeckCardIdsAtPrepare { get; private set; } = Array.Empty<int>();

            public void PrepareBattleRewards(BattleSceneState state, RuntimeRunDefinition runDefinition, int goldReward)
            {
                PrepareCallCount++;
                GoldAtPrepare = state.Gold;
                DeckCardIdsAtPrepare = state.Deck.Select(card => card.Id).ToArray();
            }

            public void OpenReward(BattleSceneState state, RuntimeRunDefinition runDefinition, Action<BattleScenePage> setCurrentPage)
            {
                setCurrentPage(BattleScenePage.Reward);
            }

            public void SelectReward(BattleSceneState state, RuntimeRewardEntry rewardEntry) { }
            public void ClaimGold(BattleSceneState state) { }
            public void ClaimPotion(BattleSceneState state) { }
            public void ClaimRelic(BattleSceneState state) { }
            public void ContinueFromReward(BattleSceneState state, Action openMap) { }
        }

        /// <summary>
        /// テスト用乱数提供クラス
        /// </summary>
        private sealed class SequenceRandomProvider : IBattleRandomProvider
        {
            private readonly IReadOnlyList<int> _values;
            private int _index;

            public SequenceRandomProvider(params int[] values)
            {
                _values = values;
            }

            public int Seed { get; private set; }
            public int Counter { get; private set; }

            public int Range(int minInclusive, int maxExclusive)
            {
                int value = _index < _values.Count ? _values[_index] : minInclusive;
                _index++;
                Counter++;
                return Math.Max(minInclusive, Math.Min(maxExclusive - 1, value));
            }

            public void Initialize(int seed)
            {
                Seed = seed;
                Counter = 0;
                _index = 0;
            }

            public void Restore(int seed, int counter)
            {
                Seed = seed;
                Counter = counter;
                _index = counter;
            }
        }

        /// <summary>
        /// テスト用RunSaveService
        /// </summary>
        private sealed class FakeRunSaveService : IRunSaveService
        {
            public RunSaveData LastSavedData { get; private set; }

            public UniTask SaveCurrentRunAsync(RunSaveData data, CancellationToken token = default)
            {
                LastSavedData = data;
                return UniTask.CompletedTask;
            }

            public UniTask<RunSaveData> LoadCurrentRunAsync(CancellationToken token = default)
            {
                return UniTask.FromResult<RunSaveData>(null);
            }

            public bool HasSavedRun()
            {
                return LastSavedData != null;
            }

            public void DeleteSavedRun() { }
        }

        /// <summary>
        /// 空のMasterDataService
        /// </summary>
        private sealed class EmptyMasterDataService : IMasterDataService
        {
            public UniTask InitializeAsync(CancellationToken ct) => UniTask.CompletedTask;
            public IReadOnlyList<T> GetAll<T>() where T : class, IMasterDataObject => Array.Empty<T>();
            public T Get<T, TKey>(TKey key) where T : class, IMasterDataObject<TKey> => null;
            public T GetContainer<T>() where T : class => null;
            public UniTask DownloadFromServerAsync(CancellationToken ct) => UniTask.CompletedTask;
            public UniTask ReloadAsync(CancellationToken ct) => UniTask.CompletedTask;
        }
    }
}
