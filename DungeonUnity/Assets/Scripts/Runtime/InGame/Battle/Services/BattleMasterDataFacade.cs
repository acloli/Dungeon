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

        public BattleMasterDataFacade(IMasterDataService masterDataService, ILocalizationService localizationService = null)
        {
            _masterDataService = masterDataService;
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

            Dictionary<int, RuntimeCard> cardCatalog = BuildCardCatalog();
            Dictionary<int, RuntimeEnemy> enemyCatalog = BuildEnemyCatalog();
            IReadOnlyList<RuntimeMapNode> nodes = BuildMapNodes(profile.MapTemplateId);
            IReadOnlyDictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>> encounters =
                BuildEncounterTable(profile, enemyCatalog);

            return new RuntimeRunDefinition(
                profile.Id,
                profile.Key,
                profile.CharacterArchetype,
                profile.PlayerMaxHp,
                profile.StartingGold,
                profile.CardRewardChoiceCount,
                BuildStarterDeck(profile.StarterDeckGroupId, cardCatalog),
                BuildRewardPool(profile.RewardPoolId, cardCatalog),
                nodes,
                encounters);
        }

        /// <summary>
        /// カード定義辞書を構築する
        /// </summary>
        private Dictionary<int, RuntimeCard> BuildCardCatalog()
        {
            IReadOnlyList<CardEffectMaster> effectMasters = _masterDataService.GetAll<CardEffectMaster>();
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

            Dictionary<int, RuntimeCard> cards = new Dictionary<int, RuntimeCard>();
            IReadOnlyList<CardMaster> cardMasters = _masterDataService.GetAll<CardMaster>();
            for (int i = 0; i < cardMasters.Count; i++)
            {
                CardMaster master = cardMasters[i];
                effectsByCardId.TryGetValue(master.Id, out List<RuntimeCardEffect> effects);
                cards[master.Id] = new RuntimeCard(
                    master.Id,
                    master.Key,
                    ResolveLocalizedText(master.LocalizationKey, master.Name),
                    master.LocalizationKey,
                    master.Cost,
                    master.CardType,
                    master.Rarity,
                    master.CharacterArchetype,
                    effects ?? new List<RuntimeCardEffect>());
            }

            return cards;
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
                        .Select(action => new RuntimeEnemyAction(
                            action.Order,
                            action.IntentType,
                            action.Damage,
                            action.HitCount,
                            action.Block,
                            action.StatusType,
                            action.StatusValue,
                            action.BuffType,
                            action.BuffValue,
                            action.RepeatRule))
                        .ToList());

            Dictionary<int, RuntimeEnemy> enemies = new Dictionary<int, RuntimeEnemy>();
            IReadOnlyList<EnemyMaster> enemyMasters = _masterDataService.GetAll<EnemyMaster>();
            for (int i = 0; i < enemyMasters.Count; i++)
            {
                EnemyMaster master = enemyMasters[i];
                actionsByEnemyId.TryGetValue(master.Id, out List<RuntimeEnemyAction> actions);
                enemies[master.Id] = new RuntimeEnemy(
                    master.Id,
                    master.Key,
                    ResolveLocalizedText(master.LocalizationKey, master.Name),
                    master.LocalizationKey,
                    master.EnemyTier,
                    master.HpMin,
                    master.HpMax,
                    master.GoldReward,
                    actions ?? new List<RuntimeEnemyAction>());
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
        private IReadOnlyList<RuntimeRewardEntry> BuildRewardPool(int rewardPoolId, IReadOnlyDictionary<int, RuntimeCard> cardCatalog)
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
                if (entry.RewardPoolId != rewardPoolId || !rewardableCardIds.Contains(entry.CardId))
                {
                    continue;
                }

                if (!cardCatalog.TryGetValue(entry.CardId, out RuntimeCard card))
                {
                    continue;
                }

                rewards.Add(new RuntimeRewardEntry(card, entry.Weight, entry.MinFloor, entry.MaxFloor));
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
        /// ノード種別変換
        /// </summary>
        private static InGameNodeType ConvertNodeType(NodeType nodeType)
        {
            return nodeType switch
            {
                NodeType.EliteBattle => InGameNodeType.EliteBattle,
                NodeType.RestShop => InGameNodeType.RestShop,
                NodeType.Boss => InGameNodeType.Boss,
                _ => InGameNodeType.Battle
            };
        }
    }
}
