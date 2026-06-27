using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using Game.MasterData.Generated;
using UnityEngine;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleScene基本ルールクラス
    /// </summary>
    public sealed class BattleSceneRules : IBattleSceneRules
    {
        private readonly IBattleDeckService _deckService;
        private readonly IBattleEnemyActionSelector _enemyActionSelector;
        private readonly IBattleEncounterSelector _encounterSelector;
        private readonly IBattleRewardRollService _rewardRollService;

        public BattleSceneRules(
            IBattleDeckService deckService,
            IBattleEnemyActionSelector enemyActionSelector,
            IBattleEncounterSelector encounterSelector,
            IBattleRewardRollService rewardRollService)
        {
            _deckService = deckService;
            _enemyActionSelector = enemyActionSelector;
            _encounterSelector = encounterSelector;
            _rewardRollService = rewardRollService;
        }

        /// <summary>
        /// Run状態初期化
        /// </summary>
        public void InitializeRun(BattleSceneState state, RuntimeRunDefinition runDefinition)
        {
            if (state == null)
            {
                return;
            }

            state.PlayerMaxHp = runDefinition != null ? runDefinition.PlayerMaxHp : BattleSceneConstants.DefaultPlayerMaxHp;
            state.PlayerHp = state.PlayerMaxHp;
            state.PlayerEnergy = BattleSceneConstants.DefaultPlayerEnergy;
            state.PlayerBlock = 0;
            state.Gold = runDefinition != null ? runDefinition.StartingGold : BattleSceneConstants.DefaultStartingGold;
            state.CurrentNodeIndex = BattleSceneConstants.DefaultNodeIndex;
            state.CurrentEnemy = null;
            state.EnemyHp = 0;
            state.EnemyBlock = 0;
            state.Enemies.Clear();
            state.BattleFinished = false;
            state.EnemyTurnCount = 0;
            state.EnemyCycleIndex = 0;
            state.SelectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
            state.SelectedEnemyIndex = BattleSceneConstants.DefaultEnemyTargetIndex;
            state.RewardChoices.Clear();
            state.Deck.Clear();
            state.DrawPile.Clear();
            state.DiscardPile.Clear();
            state.ExhaustPile.Clear();
            state.Hand.Clear();
            state.Nodes.Clear();
            state.PlayerStatuses.Clear();
            state.EnemyStatuses.Clear();
            state.PlayerBuffs.Clear();
            state.EnemyBuffs.Clear();

            if (runDefinition != null)
            {
                for (int i = 0; i < runDefinition.StarterDeck.Count; i++)
                {
                    RuntimeCard card = runDefinition.StarterDeck[i];
                    if (card != null)
                    {
                        state.Deck.Add(card);
                    }
                }

                for (int i = 0; i < runDefinition.Nodes.Count; i++)
                {
                    state.Nodes.Add(runDefinition.Nodes[i]);
                }
            }

            if (state.Nodes.Count == 0)
            {
                AddDefaultNodes(state.Nodes);
            }
        }

        /// <summary>
        /// 手札補充
        /// </summary>
        public void DrawHand(BattleSceneState state, IBattleRandomProvider randomProvider)
        {
            _deckService.DrawHand(state, randomProvider);
        }

        /// <summary>
        /// 戦闘用山札準備
        /// </summary>
        public void PrepareBattleDeck(BattleSceneState state, IBattleRandomProvider randomProvider)
        {
            _deckService.PrepareBattleDeck(state, randomProvider);
        }

        /// <summary>
        /// 手札破棄
        /// </summary>
        public void DiscardHand(BattleSceneState state)
        {
            _deckService.DiscardHand(state);
        }

        /// <summary>
        /// 指定枚数だけ手札へ追加する
        /// </summary>
        public int DrawCards(BattleSceneState state, IBattleRandomProvider randomProvider, int drawCount)
        {
            return _deckService.DrawCards(state, randomProvider, drawCount);
        }

        /// <summary>
        /// 敵選出
        /// </summary>
        public RuntimeEncounterFormation SelectEncounterFormation(RuntimeRunDefinition runDefinition, InGameNodeType nodeType, IBattleRandomProvider randomProvider)
        {
            return _encounterSelector.SelectEncounterFormation(runDefinition, nodeType, randomProvider);
        }

        /// <summary>
        /// 敵初期HP取得
        /// </summary>
        public int RollEnemyHp(RuntimeEnemy enemy, IBattleRandomProvider randomProvider)
        {
            return _encounterSelector.RollEnemyHp(enemy, randomProvider);
        }

        /// <summary>
        /// カード報酬候補選出
        /// </summary>
        public IReadOnlyList<RuntimeRewardEntry> SelectCardRewardChoices(BattleSceneState state, RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider)
        {
            return _rewardRollService.SelectCardRewardChoices(state, runDefinition, randomProvider);
        }

        /// <summary>
        /// ポーションドロップ抽選
        /// </summary>
        public bool RollPotionDrop(RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider)
        {
            return _rewardRollService.RollPotionDrop(runDefinition, randomProvider);
        }

        /// <summary>
        /// レリックドロップ抽選
        /// </summary>
        public bool RollRelicDrop(RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider)
        {
            return _rewardRollService.RollRelicDrop(runDefinition, randomProvider);
        }

        /// <summary>
        /// 使用可否判定
        /// </summary>
        public bool CanPlayCard(BattleSceneState state, RuntimeCard card)
        {
            if (state == null || card == null)
            {
                return false;
            }

            return state.PlayerEnergy >= card.Cost;
        }

        /// <summary>
        /// カード適用
        /// </summary>
        public BattleCardResolutionResult PlayCard(BattleSceneState state, int handIndex, IBattleRandomProvider randomProvider)
        {
            if (state == null || handIndex < 0 || handIndex >= state.Hand.Count)
            {
                return default;
            }

            RuntimeCard card = state.Hand[handIndex];
            if (card == null)
            {
                return default;
            }

            _enemyActionSelector.NormalizeSelectedEnemyIndex(state);
            state.Hand.RemoveAt(handIndex);
            state.PlayerEnergy -= card.Cost;

            int totalDamage = 0;
            int totalBlock = 0;
            int totalDraw = 0;
            IReadOnlyList<RuntimeCardEffect> effects = card.Effects;
            for (int i = 0; i < effects.Count; i++)
            {
                RuntimeCardEffect effect = effects[i];
                switch (effect.EffectType)
                {
                    case EffectType.DealDamage:
                        totalDamage += ResolveCardDamage(state, effect);
                        break;
                    case EffectType.GainBlock:
                        state.PlayerBlock += effect.Value;
                        totalBlock += effect.Value;
                        break;
                    case EffectType.GainEnergy:
                        state.PlayerEnergy += effect.Value;
                        break;
                    case EffectType.ApplyStatus:
                        ApplyCardStatus(state, effect);
                        break;
                    case EffectType.DrawCards:
                        totalDraw += _deckService.DrawCards(state, randomProvider, effect.Value);
                        break;
                }
            }

            state.DiscardPile.Add(card);
            return new BattleCardResolutionResult(totalDamage, totalBlock, totalDraw);
        }

        /// <summary>
        /// 敵ターン解決
        /// </summary>
        public BattleEnemyTurnResult ResolveEnemyTurn(BattleSceneState state, IBattleRandomProvider randomProvider)
        {
            if (state == null || state.Enemies.Count == 0)
            {
                return default;
            }

            // プレイヤーターン終了時点で切れる状態を先に整理する
            TickExpiringStatuses(state.PlayerStatuses);

            RuntimeEnemyAction lastAction = null;
            int totalDamage = 0;
            List<BattleEnemyState> orderedEnemies = state.Enemies
                .Where(enemy => enemy != null && !enemy.IsDefeated)
                .OrderBy(enemy => enemy.SlotIndex)
                .ToList();
            for (int i = 0; i < orderedEnemies.Count; i++)
            {
                BattleEnemyState enemyState = orderedEnemies[i];
                enemyState.Block = 0;
                RuntimeEnemyAction action = _enemyActionSelector.SelectEnemyAction(enemyState, randomProvider);
                if (action == null)
                {
                    continue;
                }

                totalDamage += ResolveEnemyDamage(state, enemyState, action);
                if (action.Block > 0)
                {
                    enemyState.Block += action.Block;
                }

                ApplyStatus(state.PlayerStatuses, action.StatusType, action.StatusValue);
                ApplyBuff(enemyState.Buffs, action.BuffType, action.BuffValue);

                enemyState.TurnCount++;
                TickExpiringStatuses(enemyState.Statuses);
                lastAction = action;
            }

            state.PlayerBlock = 0;
            SyncPrimaryEnemyState(state);

            return new BattleEnemyTurnResult(lastAction, totalDamage);
        }

        /// <summary>
        /// 休憩適用
        /// </summary>
        public void ApplyRest(BattleSceneState state)
        {
            if (state == null)
            {
                return;
            }

            state.PlayerHp = Mathf.Min(state.PlayerMaxHp, state.PlayerHp + BattleSceneConstants.RestHealAmount);
        }

        /// <summary>
        /// 購入適用
        /// </summary>
        public bool ApplyShopPurchase(BattleSceneState state, IBattleRandomProvider randomProvider)
        {
            if (state == null || state.Deck.Count == 0)
            {
                return false;
            }

            if (state.Gold < BattleSceneConstants.ShopPurchaseCost)
            {
                return false;
            }

            state.Gold -= BattleSceneConstants.ShopPurchaseCost;
            int index = randomProvider.Range(0, state.Deck.Count);
            state.Deck.Add(state.Deck[index]);
            return true;
        }

        /// <summary>
        /// カードのダメージ効果を解決する
        /// </summary>
        private int ResolveCardDamage(BattleSceneState state, RuntimeCardEffect effect)
        {
            int hitCount = Math.Max(1, effect.HitCount);
            int totalDamage = 0;
            foreach (BattleEnemyState enemyState in _enemyActionSelector.GetTargetEnemies(state, effect.TargetSide))
            {
                for (int i = 0; i < hitCount; i++)
                {
                    int damage = ApplyOutgoingModifiers(effect.Value, state.PlayerStatuses, state.PlayerBuffs);
                    damage = ApplyIncomingModifiers(damage, enemyState.Statuses);
                    totalDamage += ApplyDamageToEnemy(state, enemyState, damage);
                }
            }

            SyncPrimaryEnemyState(state);
            return totalDamage;
        }

        /// <summary>
        /// カードの状態付与を解決する
        /// </summary>
        private void ApplyCardStatus(BattleSceneState state, RuntimeCardEffect effect)
        {
            if (effect.TargetSide == TargetSide.Self)
            {
                ApplyStatus(state.PlayerStatuses, effect.StatusType, effect.StatusValue);
                return;
            }

            foreach (BattleEnemyState enemyState in _enemyActionSelector.GetTargetEnemies(state, effect.TargetSide))
            {
                ApplyStatus(enemyState.Statuses, effect.StatusType, effect.StatusValue);
            }

            SyncPrimaryEnemyState(state);
        }

        /// <summary>
        /// 敵のダメージ行動を解決する
        /// </summary>
        private static int ResolveEnemyDamage(BattleSceneState state, BattleEnemyState enemyState, RuntimeEnemyAction action)
        {
            int hitCount = Math.Max(1, action.HitCount);
            int totalDamage = 0;
            for (int i = 0; i < hitCount; i++)
            {
                int damage = ApplyOutgoingModifiers(action.Damage, enemyState.Statuses, enemyState.Buffs);
                damage = ApplyIncomingModifiers(damage, state.PlayerStatuses);
                totalDamage += ApplyDamageToPlayer(state, damage);
            }

            return totalDamage;
        }

        /// <summary>
        /// プレイヤーへのダメージをBlock込みで適用する
        /// </summary>
        private static int ApplyDamageToPlayer(BattleSceneState state, int damage)
        {
            int remainingDamage = Mathf.Max(0, damage - state.PlayerBlock);
            state.PlayerBlock = Mathf.Max(0, state.PlayerBlock - damage);
            state.PlayerHp -= remainingDamage;
            return remainingDamage;
        }

        /// <summary>
        /// 敵へのダメージをBlock込みで適用する
        /// </summary>
        private int ApplyDamageToEnemy(BattleSceneState state, BattleEnemyState enemyState, int damage)
        {
            int remainingDamage = Mathf.Max(0, damage - enemyState.Block);
            enemyState.Block = Mathf.Max(0, enemyState.Block - damage);
            enemyState.Hp -= remainingDamage;
            if (enemyState.Hp <= 0)
            {
                enemyState.Hp = 0;
                enemyState.IsDefeated = true;
                _enemyActionSelector.NormalizeSelectedEnemyIndex(state);
            }

            return remainingDamage;
        }

        /// <summary>
        /// 与ダメージ側補正
        /// </summary>
        private static int ApplyOutgoingModifiers(
            int baseDamage,
            IReadOnlyDictionary<StatusType, int> statuses,
            IReadOnlyDictionary<BuffType, int> buffs)
        {
            int damage = Mathf.Max(0, baseDamage);
            if (TryGetStatusValue(statuses, StatusType.Weak, out int weakValue) && weakValue > 0)
            {
                damage = Mathf.FloorToInt(damage * 0.75f);
            }

            damage += GetBuffValue(buffs);
            return Mathf.Max(0, damage);
        }

        /// <summary>
        /// 被ダメージ側補正
        /// </summary>
        private static int ApplyIncomingModifiers(int damage, IReadOnlyDictionary<StatusType, int> statuses)
        {
            int result = Mathf.Max(0, damage);
            if (TryGetStatusValue(statuses, StatusType.Vulnerable, out int vulnerableValue) && vulnerableValue > 0)
            {
                result = Mathf.CeilToInt(result * 1.5f);
            }

            return Mathf.Max(0, result);
        }

        /// <summary>
        /// ステータス付与
        /// </summary>
        private static void ApplyStatus(IDictionary<StatusType, int> statuses, StatusType statusType, int value)
        {
            if (statuses == null || statusType == StatusType.None || value <= 0)
            {
                return;
            }

            if (statuses.TryGetValue(statusType, out int currentValue))
            {
                statuses[statusType] = currentValue + value;
                return;
            }

            statuses[statusType] = value;
        }

        /// <summary>
        /// バフ付与
        /// </summary>
        private static void ApplyBuff(IDictionary<BuffType, int> buffs, BuffType buffType, int value)
        {
            if (buffs == null || buffType == BuffType.None || value <= 0)
            {
                return;
            }

            if (buffs.TryGetValue(buffType, out int currentValue))
            {
                buffs[buffType] = currentValue + value;
                return;
            }

            buffs[buffType] = value;
        }

        /// <summary>
        /// ターンで自然減衰する状態を更新する
        /// </summary>
        private static void TickExpiringStatuses(IDictionary<StatusType, int> statuses)
        {
            if (statuses == null || statuses.Count == 0)
            {
                return;
            }

            List<StatusType> keys = new List<StatusType>(statuses.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                StatusType statusType = keys[i];
                if (!ShouldExpire(statusType))
                {
                    continue;
                }

                int nextValue = statuses[statusType] - BattleSceneConstants.DefaultStatusDuration;
                if (nextValue > 0)
                {
                    statuses[statusType] = nextValue;
                    continue;
                }

                statuses.Remove(statusType);
            }
        }

        /// <summary>
        /// 自然減衰対象判定
        /// </summary>
        private static bool ShouldExpire(StatusType statusType)
        {
            return statusType == StatusType.Weak ||
                   statusType == StatusType.Vulnerable ||
                   statusType == StatusType.Slimed;
        }

        /// <summary>
        /// バフ値合算
        /// </summary>
        private static int GetBuffValue(IReadOnlyDictionary<BuffType, int> buffs)
        {
            int total = 0;
            if (TryGetBuffValue(buffs, BuffType.Strength, out int strengthValue))
            {
                total += strengthValue;
            }
            if (TryGetBuffValue(buffs, BuffType.Ritual, out int ritualValue))
            {
                total += ritualValue;
            }
            if (TryGetBuffValue(buffs, BuffType.Enrage, out int enrageValue))
            {
                total += enrageValue;
            }

            return total;
        }

        /// <summary>
        /// ステータス値取得
        /// </summary>
        private static bool TryGetStatusValue(IReadOnlyDictionary<StatusType, int> statuses, StatusType statusType, out int value)
        {
            value = 0;
            return statuses != null && statuses.TryGetValue(statusType, out value);
        }

        /// <summary>
        /// バフ値取得
        /// </summary>
        private static bool TryGetBuffValue(IReadOnlyDictionary<BuffType, int> buffs, BuffType buffType, out int value)
        {
            value = 0;
            return buffs != null && buffs.TryGetValue(buffType, out value);
        }

        /// <summary>
        /// 単体敵表示互換用stateを同期する
        /// </summary>
        private void SyncPrimaryEnemyState(BattleSceneState state)
        {
            BattleEnemyState selectedEnemy = _enemyActionSelector.GetSelectedEnemy(state);
            if (selectedEnemy == null)
            {
                state.CurrentEnemy = null;
                state.EnemyHp = 0;
                state.EnemyBlock = 0;
                state.EnemyStatuses.Clear();
                state.EnemyBuffs.Clear();
                return;
            }

            state.CurrentEnemy = selectedEnemy.Enemy;
            state.EnemyHp = selectedEnemy.Hp;
            state.EnemyBlock = selectedEnemy.Block;
            CopyDictionary(selectedEnemy.Statuses, state.EnemyStatuses);
            CopyDictionary(selectedEnemy.Buffs, state.EnemyBuffs);
            state.EnemyTurnCount = selectedEnemy.TurnCount;
            state.EnemyCycleIndex = selectedEnemy.CycleIndex;
        }

        /// <summary>
        /// 表示互換用辞書コピー
        /// </summary>
        private static void CopyDictionary<TKey>(IReadOnlyDictionary<TKey, int> source, IDictionary<TKey, int> destination)
        {
            destination.Clear();
            foreach (KeyValuePair<TKey, int> entry in source)
            {
                destination[entry.Key] = entry.Value;
            }
        }

        /// <summary>
        /// 既定ノード補完
        /// </summary>
        private static void AddDefaultNodes(List<RuntimeMapNode> nodes)
        {
            nodes.Add(new RuntimeMapNode(1, "default_01", 1, InGameNodeType.Battle, BattleSceneConstants.DefaultBattleNodeLabel, string.Empty, new[] { 1 }));
            nodes.Add(new RuntimeMapNode(2, "default_02", 2, InGameNodeType.RestShop, BattleSceneConstants.DefaultRestNodeLabel, string.Empty, new[] { 2 }));
            nodes.Add(new RuntimeMapNode(3, "default_03", 3, InGameNodeType.Battle, BattleSceneConstants.DefaultBattleNodeTwoLabel, string.Empty, new[] { 3 }));
            nodes.Add(new RuntimeMapNode(4, "default_04", 4, InGameNodeType.EliteBattle, BattleSceneConstants.DefaultEliteNodeLabel, string.Empty, new[] { 4 }));
            nodes.Add(new RuntimeMapNode(5, "default_05", 5, InGameNodeType.RestShop, BattleSceneConstants.DefaultShopNodeLabel, string.Empty, new[] { 5 }));
            nodes.Add(new RuntimeMapNode(6, "default_06", 6, InGameNodeType.Boss, BattleSceneConstants.DefaultBossNodeLabel, string.Empty, Array.Empty<int>()));
        }

    }
}
