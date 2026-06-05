using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Domain;

namespace Dungeon.Runtime.InGame.Battle.Model
{
    /// <summary>
    /// Battle用カード効果種別
    /// </summary>
    public enum BattleEffectType
    {
        Unknown = 0,
        DealDamage = 1,
        GainBlock = 2,
        ApplyStatus = 3,
        DrawCards = 4
    }

    /// <summary>
    /// Battle用対象側種別
    /// </summary>
    public enum BattleTargetSide
    {
        None = 0,
        Self = 1,
        Enemy = 2,
        AllEnemies = 3
    }

    /// <summary>
    /// Battle用状態種別
    /// </summary>
    public enum BattleStatusType
    {
        None = 0,
        Weak = 1,
        Vulnerable = 2,
        Slimed = 3,
        Strength = 4,
        Ritual = 5,
        Enrage = 6
    }

    /// <summary>
    /// 敵行動の反復規則
    /// </summary>
    public enum BattleEnemyRepeatRule
    {
        None = 0,
        OpeningOnly = 1,
        RepeatAfterOpening = 2,
        AfterOpeningRandom = 3,
        Random = 4,
        Cycle = 5
    }

    /// <summary>
    /// ランタイム用カード効果定義
    /// </summary>
    public sealed class RuntimeCardEffect
    {
        public RuntimeCardEffect(
            int order,
            BattleEffectType effectType,
            int value,
            int hitCount,
            BattleStatusType statusType,
            int statusValue,
            BattleTargetSide targetSide)
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
        public BattleEffectType EffectType { get; }
        public int Value { get; }
        public int HitCount { get; }
        public BattleStatusType StatusType { get; }
        public int StatusValue { get; }
        public BattleTargetSide TargetSide { get; }
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
            int cost,
            string cardType,
            string rarity,
            string characterArchetype,
            IReadOnlyList<RuntimeCardEffect> effects)
        {
            Id = id;
            Key = key;
            DisplayName = displayName;
            LocalizationKey = localizationKey;
            Cost = cost;
            CardType = cardType;
            Rarity = rarity;
            CharacterArchetype = characterArchetype;
            Effects = effects ?? Array.Empty<RuntimeCardEffect>();
        }

        public int Id { get; }
        public string Key { get; }
        public string DisplayName { get; }
        public string LocalizationKey { get; }
        public int Cost { get; }
        public string CardType { get; }
        public string Rarity { get; }
        public string CharacterArchetype { get; }
        public IReadOnlyList<RuntimeCardEffect> Effects { get; }

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
                    if (effect.EffectType != BattleEffectType.DealDamage)
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
    /// ランタイム用敵行動定義
    /// </summary>
    public sealed class RuntimeEnemyAction
    {
        public RuntimeEnemyAction(
            int order,
            string intentType,
            int damage,
            int hitCount,
            int block,
            BattleStatusType statusType,
            int statusValue,
            BattleStatusType buffType,
            int buffValue,
            BattleEnemyRepeatRule repeatRule)
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
        public string IntentType { get; }
        public int Damage { get; }
        public int HitCount { get; }
        public int Block { get; }
        public BattleStatusType StatusType { get; }
        public int StatusValue { get; }
        public BattleStatusType BuffType { get; }
        public int BuffValue { get; }
        public BattleEnemyRepeatRule RepeatRule { get; }
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
            string enemyTier,
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
        public string EnemyTier { get; }
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
        public RuntimeEncounterEntry(RuntimeEnemy enemy, int weight)
        {
            Enemy = enemy;
            Weight = weight;
        }

        public RuntimeEnemy Enemy { get; }
        public int Weight { get; }
    }

    /// <summary>
    /// ランタイム用報酬候補
    /// </summary>
    public sealed class RuntimeRewardEntry
    {
        public RuntimeRewardEntry(RuntimeCard card, int weight, int minFloor, int maxFloor)
        {
            Card = card;
            Weight = weight;
            MinFloor = minFloor;
            MaxFloor = maxFloor;
        }

        public RuntimeCard Card { get; }
        public int Weight { get; }
        public int MinFloor { get; }
        public int MaxFloor { get; }
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
            string characterArchetype,
            int playerMaxHp,
            int startingGold,
            int cardRewardChoiceCount,
            IReadOnlyList<RuntimeCard> starterDeck,
            IReadOnlyList<RuntimeRewardEntry> rewardPool,
            IReadOnlyList<RuntimeMapNode> nodes,
            IReadOnlyDictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>> encountersByNodeType)
        {
            RunProfileId = runProfileId;
            Key = key;
            CharacterArchetype = characterArchetype;
            PlayerMaxHp = playerMaxHp;
            StartingGold = startingGold;
            CardRewardChoiceCount = cardRewardChoiceCount;
            StarterDeck = starterDeck ?? Array.Empty<RuntimeCard>();
            RewardPool = rewardPool ?? Array.Empty<RuntimeRewardEntry>();
            Nodes = nodes ?? Array.Empty<RuntimeMapNode>();
            EncountersByNodeType = encountersByNodeType
                ?? new Dictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>>();
        }

        public int RunProfileId { get; }
        public string Key { get; }
        public string CharacterArchetype { get; }
        public int PlayerMaxHp { get; }
        public int StartingGold { get; }
        public int CardRewardChoiceCount { get; }
        public IReadOnlyList<RuntimeCard> StarterDeck { get; }
        public IReadOnlyList<RuntimeRewardEntry> RewardPool { get; }
        public IReadOnlyList<RuntimeMapNode> Nodes { get; }
        public IReadOnlyDictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>> EncountersByNodeType { get; }
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
