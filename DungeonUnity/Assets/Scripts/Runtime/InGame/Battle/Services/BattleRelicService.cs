using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;
using VContainer;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// レリック効果適用クラス
    /// </summary>
    public sealed class BattleRelicService : IBattleRelicService
    {
        private readonly IBattleSceneRules _rules;
        private readonly IBattleRandomProvider _randomProvider;

        public BattleRelicService()
        {
        }

        [Inject]
        public BattleRelicService(IBattleSceneRules rules, IBattleRandomProvider randomProvider)
        {
            _rules = rules;
            _randomProvider = randomProvider;
        }

        public void RestoreOwnedRelics(BattleSceneState state, RuntimeRunDefinition runDefinition, IReadOnlyList<int> ownedRelicIds)
        {
            if (state == null || runDefinition?.RelicCatalog == null)
            {
                return;
            }

            state.OwnedRelics.Clear();
            if (ownedRelicIds == null)
            {
                return;
            }

            for (int i = 0; i < ownedRelicIds.Count; i++)
            {
                int relicId = ownedRelicIds[i];
                if (runDefinition.RelicCatalog.TryGetValue(relicId, out RuntimeRelic relic))
                {
                    AddOwnedRelic(state, relic);
                }
            }
        }

        public bool AddOwnedRelic(BattleSceneState state, RuntimeRelic relic)
        {
            if (state == null || relic == null)
            {
                return false;
            }

            for (int i = 0; i < state.OwnedRelics.Count; i++)
            {
                RuntimeRelic ownedRelic = state.OwnedRelics[i];
                if (ownedRelic != null && ownedRelic.Id == relic.Id)
                {
                    return false;
                }
            }

            state.OwnedRelics.Add(relic);
            return true;
        }

        public RuntimeRelic RollBattleRewardRelic(BattleSceneState state, RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider)
        {
            if (state == null || runDefinition?.RelicCatalog == null || randomProvider == null)
            {
                return null;
            }

            List<RuntimeRelic> candidates = new List<RuntimeRelic>();
            foreach (KeyValuePair<int, RuntimeRelic> entry in runDefinition.RelicCatalog)
            {
                if (!HasOwnedRelic(state, entry.Key) && entry.Value != null)
                {
                    candidates.Add(entry.Value);
                }
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            return candidates[randomProvider.Range(0, candidates.Count)];
        }

        public void ApplyEffects(BattleSceneState state, RelicTriggerType triggerType)
        {
            if (state == null)
            {
                return;
            }

            for (int i = 0; i < state.OwnedRelics.Count; i++)
            {
                RuntimeRelic relic = state.OwnedRelics[i];
                if (relic?.Effects == null)
                {
                    continue;
                }

                for (int effectIndex = 0; effectIndex < relic.Effects.Count; effectIndex++)
                {
                    RuntimeRelicEffect effect = relic.Effects[effectIndex];
                    if (effect == null || effect.TriggerType != triggerType)
                    {
                        continue;
                    }

                    ApplyEffect(state, effect);
                }
            }
        }

        private static bool HasOwnedRelic(BattleSceneState state, int relicId)
        {
            for (int i = 0; i < state.OwnedRelics.Count; i++)
            {
                RuntimeRelic relic = state.OwnedRelics[i];
                if (relic != null && relic.Id == relicId)
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyEffect(BattleSceneState state, RuntimeRelicEffect effect)
        {
            switch (effect.EffectType)
            {
                case EffectType.DealDamage:
                    ApplyDamageFromRelic(state, effect);
                    SyncSelectedEnemyDisplay(state);
                    break;
                case EffectType.GainBlock:
                    state.PlayerBlock += effect.Value;
                    break;
                case EffectType.GainEnergy:
                    state.PlayerEnergy += effect.Value;
                    break;
                case EffectType.DrawCards:
                    _rules?.DrawCards(state, _randomProvider, effect.Value);
                    break;
                case EffectType.ApplyStatus:
                    ApplyStatusFromRelic(state, effect);
                    SyncSelectedEnemyDisplay(state);
                    break;
                case EffectType.GainGold:
                    state.Gold += effect.Value;
                    break;
                case EffectType.GainMaxHp:
                    state.PlayerMaxHp += effect.Value;
                    state.PlayerHp = Math.Min(state.PlayerHp + effect.Value, state.PlayerMaxHp);
                    break;
                case EffectType.LoseHp:
                    state.PlayerHp = Math.Max(0, state.PlayerHp - effect.Value);
                    break;
            }

            if (effect.PotionCapacityDelta != 0)
            {
                state.MaxPotionCount = Math.Max(0, state.MaxPotionCount + effect.PotionCapacityDelta);
            }
        }

        private static void ApplyDamageFromRelic(BattleSceneState state, RuntimeRelicEffect effect)
        {
            int hitCount = Math.Max(1, effect.HitCount);
            if (effect.TargetSide == TargetSide.Enemy)
            {
                ApplyDamage(FindFirstAliveEnemy(state), effect.Value, hitCount);
                return;
            }

            if (effect.TargetSide == TargetSide.AllEnemies)
            {
                for (int i = 0; i < state.Enemies.Count; i++)
                {
                    BattleEnemyState enemyState = state.Enemies[i];
                    if (IsAliveEnemy(enemyState))
                    {
                        ApplyDamage(enemyState, effect.Value, hitCount);
                    }
                }

                return;
            }

            ApplyDamageToPlayer(state, effect.Value, hitCount);
        }

        private static void ApplyDamage(BattleEnemyState enemyState, int damage, int hitCount)
        {
            if (enemyState == null)
            {
                return;
            }

            for (int i = 0; i < hitCount; i++)
            {
                int remainingDamage = Math.Max(0, damage - enemyState.Block);
                enemyState.Block = Math.Max(0, enemyState.Block - damage);
                enemyState.Hp -= remainingDamage;
                if (enemyState.Hp <= 0)
                {
                    enemyState.Hp = 0;
                    enemyState.IsDefeated = true;
                    return;
                }
            }
        }

        private static void ApplyDamageToPlayer(BattleSceneState state, int damage, int hitCount)
        {
            for (int i = 0; i < hitCount; i++)
            {
                int remainingDamage = Math.Max(0, damage - state.PlayerBlock);
                state.PlayerBlock = Math.Max(0, state.PlayerBlock - damage);
                state.PlayerHp = Math.Max(0, state.PlayerHp - remainingDamage);
            }
        }

        private static void ApplyStatusFromRelic(BattleSceneState state, RuntimeRelicEffect effect)
        {
            if (effect.TargetSide == TargetSide.Enemy)
            {
                ApplyStatus(FindFirstAliveEnemy(state)?.Statuses, effect.StatusType, effect.StatusValue);
                return;
            }

            if (effect.TargetSide == TargetSide.AllEnemies)
            {
                for (int i = 0; i < state.Enemies.Count; i++)
                {
                    BattleEnemyState enemyState = state.Enemies[i];
                    if (IsAliveEnemy(enemyState))
                    {
                        ApplyStatus(enemyState.Statuses, effect.StatusType, effect.StatusValue);
                    }
                }

                return;
            }

            ApplyStatus(state.PlayerStatuses, effect.StatusType, effect.StatusValue);
        }

        private static void ApplyStatus(IDictionary<StatusType, int> statuses, StatusType statusType, int value)
        {
            if (statuses == null || statusType == StatusType.None || value <= 0)
            {
                return;
            }

            int currentValue = 0;
            statuses.TryGetValue(statusType, out currentValue);
            statuses[statusType] = currentValue + value;
        }

        private static BattleEnemyState FindFirstAliveEnemy(BattleSceneState state)
        {
            for (int i = 0; i < state.Enemies.Count; i++)
            {
                BattleEnemyState enemyState = state.Enemies[i];
                if (IsAliveEnemy(enemyState))
                {
                    return enemyState;
                }
            }

            return null;
        }

        private static bool IsAliveEnemy(BattleEnemyState enemyState)
        {
            return enemyState != null && !enemyState.IsDefeated && enemyState.Hp > 0;
        }

        private static void SyncSelectedEnemyDisplay(BattleSceneState state)
        {
            if (state.SelectedEnemyIndex >= 0 && state.SelectedEnemyIndex < state.Enemies.Count)
            {
                BattleEnemyState selectedEnemy = state.Enemies[state.SelectedEnemyIndex];
                if (IsAliveEnemy(selectedEnemy))
                {
                    state.SyncSelectedEnemyDisplay(selectedEnemy, state.SelectedEnemyIndex);
                    return;
                }
            }

            BattleEnemyState firstAliveEnemy = FindFirstAliveEnemy(state);
            if (firstAliveEnemy == null)
            {
                state.ClearSelectedEnemyDisplay();
                return;
            }

            state.SyncSelectedEnemyDisplay(firstAliveEnemy, state.Enemies.IndexOf(firstAliveEnemy));
        }
    }
}
