using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using Game.MasterData.Generated;
using TFramework.Debug;
using TFramework.Localization;
using TFramework.MasterData;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// Battle用のMasterData組み立てクラス
    /// </summary>
    public sealed class BattleMasterDataFacade : IBattleMasterDataFacade
    {
        private readonly IMasterDataService _masterDataService;
        private readonly ILocalizationService _localizationService;
        private readonly EventMasterDataFacade _eventMasterDataFacade;
        private readonly ShopMasterDataFacade _shopMasterDataFacade;

        public BattleMasterDataFacade(
            IMasterDataService masterDataService,
            EventMasterDataFacade eventMasterDataFacade,
            ShopMasterDataFacade shopMasterDataFacade,
            ILocalizationService localizationService = null)
        {
            _masterDataService = masterDataService;
            _eventMasterDataFacade = eventMasterDataFacade;
            _shopMasterDataFacade = shopMasterDataFacade;
            _localizationService = localizationService;
        }

        /// <summary>
        /// Battle実行用Run定義を構築する
        /// </summary>
        public RuntimeRunDefinition BuildRunDefinition(int runProfileId)
        {
            RunProfileMaster profile = _masterDataService.Get<RunProfileMaster, int>(runProfileId);
            if (profile == null)
            {
                TLogger.Error($"RunProfileMaster not found. id={runProfileId}", "Battle");
                return null;
            }

            IReadOnlyDictionary<int, RuntimeCard> cardCatalog = BuildCardCatalog();
            IReadOnlyDictionary<int, RuntimeRelic> relicCatalog = BuildRelicCatalog();
            IReadOnlyDictionary<int, RuntimePotion> potionCatalog = BuildPotionCatalog();
            Dictionary<int, RuntimeEnemy> enemyCatalog = BuildEnemyCatalog();
            IReadOnlyList<RuntimeMapNode> nodes = BuildMapNodes(profile.MapTemplateId);
            IReadOnlyDictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>> encounters =
                BuildEncounterTable(profile, enemyCatalog);

            IReadOnlyList<RuntimeEvent> possibleEvents = _eventMasterDataFacade.BuildEvents(profile.EventPoolId);
            RuntimeShopLineup shopLineup = _shopMasterDataFacade.BuildShopLineup(profile.ShopId);
            IReadOnlyDictionary<CardRarity, RuntimeCardPriceRule> cardPriceRules = _shopMasterDataFacade.BuildCardPriceRules();
            IReadOnlyList<RuntimeItemPriceRule> itemPriceRules = _shopMasterDataFacade.BuildItemPriceRules();

            return new RuntimeRunDefinition(
                profile.Id,
                profile.Key,
                profile.MapTemplateId,
                profile.CharacterArchetype,
                profile.PlayerMaxHp,
                profile.StartingGold,
                profile.CardRewardChoiceCount,
                profile.PotionDropChance,
                profile.RelicDropChance,
                BuildStarterDeck(profile.StarterDeckGroupId, cardCatalog),
                cardCatalog,
                BuildRewardPool(profile.RewardPoolId, cardCatalog, relicCatalog, potionCatalog),
                nodes,
                encounters,
                possibleEvents,
                relicCatalog,
                potionCatalog,
                shopLineup,
                cardPriceRules,
                itemPriceRules);
        }

        /// <summary>
        /// カード定義辞書を構築する
        /// </summary>
        public IReadOnlyDictionary<int, RuntimeCard> BuildCardCatalog()
        {
            IReadOnlyList<CardEffectMaster> effectMasters = _masterDataService.GetAll<CardEffectMaster>();
            IReadOnlyList<CardMaster> cardMasters = _masterDataService.GetAll<CardMaster>();
            Dictionary<int, List<RuntimeCardEffect>> effectsByCardId = effectMasters
                .GroupBy(effect => effect.CardId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(effect => effect.Order)
                        .Select(effect => new RuntimeCardEffect(
                            effect.Order,
                            effect.EffectType,
                            effect.Value,
                            effect.HitCount,
                            effect.StatusType,
                            effect.StatusValue,
                            effect.TargetSide))
                        .ToList());

            HashSet<int> upgradedCardIds = new HashSet<int>();
            for (int i = 0; i < cardMasters.Count; i++)
            {
                int upgradeCardId = cardMasters[i].UpgradeCardId;
                if (upgradeCardId > 0)
                {
                    upgradedCardIds.Add(upgradeCardId);
                }
            }

            Dictionary<int, RuntimeCard> cards = new Dictionary<int, RuntimeCard>();
            for (int i = 0; i < cardMasters.Count; i++)
            {
                CardMaster master = cardMasters[i];
                effectsByCardId.TryGetValue(master.Id, out List<RuntimeCardEffect> effects);
                cards[master.Id] = CreateRuntimeCard(
                    master,
                    effects ?? new List<RuntimeCardEffect>(),
                    upgradedCardIds.Contains(master.Id));
            }

            return cards;
        }

        /// <summary>
        /// レリック定義辞書を構築する
        /// </summary>
        public IReadOnlyDictionary<int, RuntimeRelic> BuildRelicCatalog()
        {
            IReadOnlyList<RelicEffectMaster> effectMasters = _masterDataService.GetAll<RelicEffectMaster>();
            Dictionary<int, List<RuntimeRelicEffect>> effectsByRelicId = effectMasters
                .GroupBy(effect => effect.RelicId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(effect => effect.Order)
                        .Select(effect => new RuntimeRelicEffect(
                            effect.Order,
                            effect.TriggerType,
                            effect.EffectType,
                            effect.Value,
                            effect.HitCount,
                            effect.StatusType,
                            effect.StatusValue,
                            effect.TargetSide))
                        .ToList());

            Dictionary<int, RuntimeRelic> relics = new Dictionary<int, RuntimeRelic>();
            IReadOnlyList<RelicMaster> masters = _masterDataService.GetAll<RelicMaster>();
            for (int i = 0; i < masters.Count; i++)
            {
                RelicMaster master = masters[i];
                effectsByRelicId.TryGetValue(master.Id, out List<RuntimeRelicEffect> effects);
                relics[master.Id] = CreateRuntimeRelic(master, effects ?? new List<RuntimeRelicEffect>());
            }

            return relics;
        }

        /// <summary>
        /// ポーション定義辞書を構築する
        /// </summary>
        public IReadOnlyDictionary<int, RuntimePotion> BuildPotionCatalog()
        {
            IReadOnlyList<PotionEffectMaster> effectMasters = _masterDataService.GetAll<PotionEffectMaster>();
            Dictionary<int, List<RuntimePotionEffect>> effectsByPotionId = effectMasters
                .GroupBy(effect => effect.PotionId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(effect => effect.Order)
                        .Select(effect => new RuntimePotionEffect(
                            effect.Order,
                            effect.EffectType,
                            effect.Value,
                            effect.HitCount,
                            effect.StatusType,
                            effect.StatusValue,
                            effect.TargetSide))
                        .ToList());

            Dictionary<int, RuntimePotion> potions = new Dictionary<int, RuntimePotion>();
            IReadOnlyList<PotionMaster> masters = _masterDataService.GetAll<PotionMaster>();
            for (int i = 0; i < masters.Count; i++)
            {
                PotionMaster master = masters[i];
                effectsByPotionId.TryGetValue(master.Id, out List<RuntimePotionEffect> effects);
                potions[master.Id] = CreateRuntimePotion(master, effects ?? new List<RuntimePotionEffect>());
            }

            return potions;
        }

        /// <summary>
        /// 敵定義辞書を構築する
        /// </summary>
        private Dictionary<int, RuntimeEnemy> BuildEnemyCatalog()
        {
            IReadOnlyList<EnemyActionMaster> actionMasters = _masterDataService.GetAll<EnemyActionMaster>();
            Dictionary<int, List<RuntimeEnemyAction>> actionsByEnemyId = actionMasters
                .GroupBy(action => action.EnemyId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(action => action.Order)
                        .Select(CreateRuntimeEnemyAction)
                        .ToList());

            Dictionary<int, RuntimeEnemy> enemies = new Dictionary<int, RuntimeEnemy>();
            IReadOnlyList<EnemyMaster> enemyMasters = _masterDataService.GetAll<EnemyMaster>();
            for (int i = 0; i < enemyMasters.Count; i++)
            {
                EnemyMaster master = enemyMasters[i];
                actionsByEnemyId.TryGetValue(master.Id, out List<RuntimeEnemyAction> actions);
                enemies[master.Id] = CreateRuntimeEnemy(master, actions ?? new List<RuntimeEnemyAction>());
            }

            return enemies;
        }

        /// <summary>
        /// 初期デッキを展開する
        /// </summary>
        private IReadOnlyList<RuntimeCard> BuildStarterDeck(int deckGroupId, IReadOnlyDictionary<int, RuntimeCard> cardCatalog)
        {
            List<RuntimeCard> deck = new List<RuntimeCard>();
            IReadOnlyList<DeckGroupMaster> deckEntries = _masterDataService.GetAll<DeckGroupMaster>();

            foreach (DeckGroupMaster entry in deckEntries.Where(entry => entry.DeckGroupId == deckGroupId).OrderBy(entry => entry.Order))
            {
                if (!cardCatalog.TryGetValue(entry.CardId, out RuntimeCard card))
                {
                    continue;
                }

                for (int i = 0; i < entry.Count; i++)
                {
                    deck.Add(card);
                }
            }

            return deck;
        }

        /// <summary>
        /// 報酬プールを展開する
        /// </summary>
        private IReadOnlyList<RuntimeRewardEntry> BuildRewardPool(
            int rewardPoolId,
            IReadOnlyDictionary<int, RuntimeCard> cardCatalog,
            IReadOnlyDictionary<int, RuntimeRelic> relicCatalog,
            IReadOnlyDictionary<int, RuntimePotion> potionCatalog)
        {
            List<RuntimeRewardEntry> rewards = new List<RuntimeRewardEntry>();
            IReadOnlyList<RewardPoolMaster> rewardEntries = _masterDataService.GetAll<RewardPoolMaster>();
            IReadOnlyList<CardMaster> cardMasters = _masterDataService.GetAll<CardMaster>();
            HashSet<int> rewardableCardIds = cardMasters
                .Where(card => card.CanAppearInReward)
                .Select(card => card.Id)
                .ToHashSet();

            for (int i = 0; i < rewardEntries.Count; i++)
            {
                RewardPoolMaster entry = rewardEntries[i];
                if (entry.RewardPoolId != rewardPoolId)
                {
                    continue;
                }

                RuntimeCard card = null;
                RuntimeRelic relic = null;
                RuntimePotion potion = null;
                if (entry.RewardType == RewardType.Card)
                {
                    if (!rewardableCardIds.Contains(entry.RewardValue))
                    {
                        continue;
                    }

                    if (!cardCatalog.TryGetValue(entry.RewardValue, out card))
                    {
                        continue;
                    }
                }
                else if (entry.RewardType == RewardType.Relic)
                {
                    if (!relicCatalog.TryGetValue(entry.RewardValue, out relic))
                    {
                        continue;
                    }
                }
                else if (entry.RewardType == RewardType.Potion)
                {
                    if (!potionCatalog.TryGetValue(entry.RewardValue, out potion))
                    {
                        continue;
                    }
                }

                rewards.Add(CreateRuntimeRewardEntry(entry, card, relic, potion));
            }

            return rewards;
        }

        /// <summary>
        /// マップノード一覧を構築する
        /// </summary>
        private IReadOnlyList<RuntimeMapNode> BuildMapNodes(int mapTemplateId)
        {
            IReadOnlyList<MapNodeMaster> nodeMasters = _masterDataService.GetAll<MapNodeMaster>();
            List<MapNodeMaster> orderedNodes = nodeMasters
                .Where(node => node.MapTemplateId == mapTemplateId)
                .OrderBy(node => node.Floor)
                .ThenBy(node => node.Id)
                .ToList();

            Dictionary<string, int> indexByNodeKey = new Dictionary<string, int>();
            for (int i = 0; i < orderedNodes.Count; i++)
            {
                indexByNodeKey[orderedNodes[i].NodeKey] = i;
            }

            ILookup<string, MapEdgeMaster> edgesByFromKey = _masterDataService.GetAll<MapEdgeMaster>()
                .Where(edge => edge.MapTemplateId == mapTemplateId)
                .ToLookup(edge => edge.FromNodeKey);

            List<RuntimeMapNode> nodes = new List<RuntimeMapNode>();
            for (int i = 0; i < orderedNodes.Count; i++)
            {
                MapNodeMaster master = orderedNodes[i];
                List<int> nextNodeIndices = new List<int>();
                foreach (MapEdgeMaster edge in edgesByFromKey[master.NodeKey])
                {
                    if (indexByNodeKey.TryGetValue(edge.ToNodeKey, out int nextIndex))
                    {
                        nextNodeIndices.Add(nextIndex);
                    }
                }

                nodes.Add(new RuntimeMapNode(
                    master.Id,
                    master.NodeKey,
                    master.Floor,
                    ConvertNodeType(master.NodeType),
                    ResolveLocalizedText(master.LocalizationKey, master.Name),
                    master.LocalizationKey,
                    nextNodeIndices));
            }

            return nodes;
        }

        /// <summary>
        /// ノード種別ごとの遭遇候補一覧を構築する
        /// </summary>
        private IReadOnlyDictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>> BuildEncounterTable(
            RunProfileMaster profile,
            IReadOnlyDictionary<int, RuntimeEnemy> enemyCatalog)
        {
            Dictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>> encounters =
                new Dictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>>
                {
                    { InGameNodeType.Battle, BuildEncounterEntries(profile.NormalEncounterGroupId, enemyCatalog) },
                    { InGameNodeType.EliteBattle, BuildEncounterEntries(profile.EliteEncounterGroupId, enemyCatalog) },
                    { InGameNodeType.Boss, BuildEncounterEntries(profile.BossEncounterGroupId, enemyCatalog) }
                };

            return encounters;
        }

        /// <summary>
        /// 遭遇グループを個別候補へ変換する
        /// </summary>
        private IReadOnlyList<RuntimeEncounterEntry> BuildEncounterEntries(
            int encounterGroupId,
            IReadOnlyDictionary<int, RuntimeEnemy> enemyCatalog)
        {
            List<RuntimeEncounterEntry> entries = new List<RuntimeEncounterEntry>();
            IReadOnlyList<EncounterGroupMaster> encounterMasters = _masterDataService.GetAll<EncounterGroupMaster>();
            IReadOnlyDictionary<int, RuntimeEncounterFormation> formationCatalog = BuildFormationCatalog(enemyCatalog);
            for (int i = 0; i < encounterMasters.Count; i++)
            {
                EncounterGroupMaster master = encounterMasters[i];
                if (master.EncounterGroupId != encounterGroupId)
                {
                    continue;
                }

                if (!formationCatalog.TryGetValue(master.FormationId, out RuntimeEncounterFormation formation))
                {
                    continue;
                }

                entries.Add(new RuntimeEncounterEntry(formation, master.Weight));
            }

            return entries;
        }

        /// <summary>
        /// 敵編成辞書を構築する
        /// </summary>
        private IReadOnlyDictionary<int, RuntimeEncounterFormation> BuildFormationCatalog(IReadOnlyDictionary<int, RuntimeEnemy> enemyCatalog)
        {
            Dictionary<int, List<RuntimeEncounterEnemyEntry>> enemiesByFormationId = _masterDataService.GetAll<EncounterFormationEnemyMaster>()
                .GroupBy(entry => entry.FormationId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(entry => entry.SlotIndex)
                        .Where(entry => enemyCatalog.ContainsKey(entry.EnemyId))
                        .Select(entry => new RuntimeEncounterEnemyEntry(enemyCatalog[entry.EnemyId], entry.SlotIndex))
                        .ToList());

            Dictionary<int, RuntimeEncounterFormation> formations = new Dictionary<int, RuntimeEncounterFormation>();
            IReadOnlyList<EncounterFormationMaster> formationMasters = _masterDataService.GetAll<EncounterFormationMaster>();
            for (int i = 0; i < formationMasters.Count; i++)
            {
                EncounterFormationMaster master = formationMasters[i];
                enemiesByFormationId.TryGetValue(master.Id, out List<RuntimeEncounterEnemyEntry> enemies);
                formations[master.Id] = new RuntimeEncounterFormation(
                    master.Id,
                    master.Key,
                    master.Name,
                    enemies ?? new List<RuntimeEncounterEnemyEntry>());
            }

            return formations;
        }

        private RuntimeCard CreateRuntimeCard(CardMaster master, IReadOnlyList<RuntimeCardEffect> effects, bool isUpgraded)
        {
            return new RuntimeCard(
                master.Id,
                master.Key,
                ResolveLocalizedText(master.LocalizationKey, master.Name),
                master.LocalizationKey,
                ResolveLocalizedText(master.DescriptionKey, string.Empty),
                master.DescriptionKey,
                master.ImageId,
                master.Cost,
                master.CardType,
                master.Rarity,
                master.CharacterArchetype,
                effects,
                master.UpgradeCardId,
                isUpgraded,
                master.ExhaustsOnPlay);
        }

        private RuntimeRelic CreateRuntimeRelic(RelicMaster master, IReadOnlyList<RuntimeRelicEffect> effects)
        {
            return new RuntimeRelic(
                master.Id,
                master.Key,
                ResolveLocalizedText(master.LocalizationKey, master.Name),
                master.LocalizationKey,
                ResolveLocalizedText(master.DescriptionKey, string.Empty),
                master.DescriptionKey,
                master.ImageId,
                master.Rarity,
                effects);
        }

        private RuntimePotion CreateRuntimePotion(PotionMaster master, IReadOnlyList<RuntimePotionEffect> effects)
        {
            return new RuntimePotion(
                master.Id,
                master.Key,
                ResolveLocalizedText(master.LocalizationKey, master.Name),
                master.LocalizationKey,
                ResolveLocalizedText(master.DescriptionKey, string.Empty),
                master.DescriptionKey,
                master.ImageId,
                master.Rarity,
                ResolvePotionUseContext(effects),
                ResolvePotionTargetMode(effects),
                effects);
        }

        private static RuntimeEnemyAction CreateRuntimeEnemyAction(EnemyActionMaster action)
        {
            return new RuntimeEnemyAction(
                action.Order,
                action.IntentType,
                action.Damage,
                action.HitCount,
                action.Block,
                action.StatusType,
                action.StatusValue,
                action.BuffType,
                action.BuffValue,
                action.RepeatRule);
        }

        private RuntimeEnemy CreateRuntimeEnemy(EnemyMaster master, IReadOnlyList<RuntimeEnemyAction> actions)
        {
            return new RuntimeEnemy(
                master.Id,
                master.Key,
                ResolveLocalizedText(master.LocalizationKey, master.Name),
                master.LocalizationKey,
                master.EnemyTier,
                master.HpMin,
                master.HpMax,
                master.GoldReward,
                actions);
        }

        private static RuntimeRewardEntry CreateRuntimeRewardEntry(
            RewardPoolMaster entry,
            RuntimeCard card,
            RuntimeRelic relic,
            RuntimePotion potion)
        {
            return new RuntimeRewardEntry(
                entry.RewardType,
                entry.RewardValue,
                card,
                relic,
                potion,
                entry.Weight,
                entry.MinFloor,
                entry.MaxFloor);
        }

        /// <summary>
        /// 表示名をローカライズ解決する
        /// </summary>
        private string ResolveLocalizedText(string localizationKey, string fallback)
        {
            if (_localizationService == null || string.IsNullOrEmpty(localizationKey))
            {
                return fallback;
            }

            if (!_localizationService.HasKey(localizationKey))
            {
                return fallback;
            }

            return _localizationService.Get(localizationKey);
        }

        /// <summary>
        /// ポーション使用文脈を既定解決する
        /// </summary>
        private static PotionUseContext ResolvePotionUseContext(IReadOnlyList<RuntimePotionEffect> effects)
        {
            if (effects == null || effects.Count == 0)
            {
                return PotionUseContext.BattleOnly;
            }

            for (int i = 0; i < effects.Count; i++)
            {
                RuntimePotionEffect effect = effects[i];
                if (effect == null)
                {
                    continue;
                }

                if (effect.EffectType == EffectType.GainMaxHp || effect.EffectType == EffectType.GainGold || effect.EffectType == EffectType.LoseHp)
                {
                    return PotionUseContext.Both;
                }
            }

            return PotionUseContext.BattleOnly;
        }

        /// <summary>
        /// ポーション対象モードを既定解決する
        /// </summary>
        private static PotionTargetMode ResolvePotionTargetMode(IReadOnlyList<RuntimePotionEffect> effects)
        {
            if (effects == null || effects.Count == 0)
            {
                return PotionTargetMode.None;
            }

            bool hasEnemyTarget = false;
            for (int i = 0; i < effects.Count; i++)
            {
                RuntimePotionEffect effect = effects[i];
                if (effect == null)
                {
                    continue;
                }

                if (effect.TargetSide == TargetSide.AllEnemies)
                {
                    return PotionTargetMode.AllEnemies;
                }

                if (effect.TargetSide == TargetSide.Enemy)
                {
                    hasEnemyTarget = true;
                }
            }

            if (hasEnemyTarget)
            {
                return PotionTargetMode.AnyEnemy;
            }

            return PotionTargetMode.Self;
        }

        /// <summary>
        /// ノード種別変換
        /// </summary>
        private static InGameNodeType ConvertNodeType(NodeType nodeType)
        {
            return nodeType switch
            {
                NodeType.EliteBattle => InGameNodeType.EliteBattle,
                NodeType.RestShop => InGameNodeType.RestShop,
                NodeType.Boss => InGameNodeType.Boss,
                NodeType.Event => InGameNodeType.Event,
                _ => InGameNodeType.Battle
            };
        }
    }
}
