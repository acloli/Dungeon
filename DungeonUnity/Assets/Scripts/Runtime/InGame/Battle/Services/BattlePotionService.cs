using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// ポーション所持・提示・使用処理クラス
    /// </summary>
    public sealed class BattlePotionService : IBattlePotionService
    {
        public void RestoreOwnedPotions(BattleSceneState state, RuntimeRunDefinition runDefinition, IReadOnlyList<int> ownedPotionIds)
        {
            if (state == null || runDefinition?.PotionCatalog == null)
            {
                return;
            }

            state.OwnedPotions.Clear();
            if (ownedPotionIds == null)
            {
                return;
            }

            for (int i = 0; i < ownedPotionIds.Count; i++)
            {
                int potionId = ownedPotionIds[i];
                if (runDefinition.PotionCatalog.TryGetValue(potionId, out RuntimePotion potion))
                {
                    AddOwnedPotion(state, potion);
                }
            }
        }

        public bool HasCapacity(BattleSceneState state)
        {
            return state != null && state.OwnedPotions.Count < BattleSceneConstants.MaxPotionCount;
        }

        public bool AddOwnedPotion(BattleSceneState state, RuntimePotion potion)
        {
            if (!HasCapacity(state) || potion == null)
            {
                return false;
            }

            state.OwnedPotions.Add(potion);
            return true;
        }

        public RuntimePotion RollBattleRewardPotion(RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider)
        {
            if (runDefinition?.PotionCatalog == null || runDefinition.PotionCatalog.Count == 0 || randomProvider == null)
            {
                return null;
            }

            List<RuntimePotion> potions = new List<RuntimePotion>();
            foreach (KeyValuePair<int, RuntimePotion> entry in runDefinition.PotionCatalog)
            {
                if (entry.Value != null)
                {
                    potions.Add(entry.Value);
                }
            }

            if (potions.Count == 0)
            {
                return null;
            }

            return potions[randomProvider.Range(0, potions.Count)];
        }

        public PendingPotionOffer CreateOffer(RuntimePotion potion, PotionOfferSource source, int shopSlotIndex = BattleSceneConstants.UnselectedCardIndex)
        {
            return potion == null ? null : new PendingPotionOffer(potion, source, shopSlotIndex);
        }

        public bool CanUsePotionInCurrentPage(BattleSceneState state, RuntimePotion potion)
        {
            if (state == null || potion == null)
            {
                return false;
            }

            return ResolveUseContext(state.CurrentPage, potion.UseContext);
        }

        public bool UsePotion(BattleSceneState state, int potionIndex, BattlePotionUseTarget target, IBattleSceneRules rules, IBattleRandomProvider randomProvider)
        {
            if (state == null || potionIndex < 0 || potionIndex >= state.OwnedPotions.Count)
            {
                return false;
            }

            RuntimePotion potion = state.OwnedPotions[potionIndex];
            if (!CanUsePotionInCurrentPage(state, potion))
            {
                return false;
            }

            if (!TryResolveEnemyTargets(state, potion, target, out BattleEnemyState enemyTarget, out List<BattleEnemyState> allEnemyTargets))
            {
                return false;
            }

            ApplyEffects(state, potion, enemyTarget, allEnemyTargets, rules, randomProvider);
            state.OwnedPotions.RemoveAt(potionIndex);
            return true;
        }

        public bool ReplaceOwnedPotion(BattleSceneState state, int potionIndex, PendingPotionOffer offer)
        {
            if (state == null || offer?.Potion == null)
            {
                return false;
            }

            if (potionIndex < 0 || potionIndex >= state.OwnedPotions.Count)
            {
                return false;
            }

            state.OwnedPotions[potionIndex] = offer.Potion;
            return true;
        }

        private static void ApplyEffects(
            BattleSceneState state,
            RuntimePotion potion,
            BattleEnemyState enemyTarget,
            IReadOnlyList<BattleEnemyState> allEnemyTargets,
            IBattleSceneRules rules,
            IBattleRandomProvider randomProvider)
        {
            if (state == null || potion?.Effects == null)
            {
                return;
            }

            for (int i = 0; i < potion.Effects.Count; i++)
            {
                RuntimePotionEffect effect = potion.Effects[i];
                if (effect == null)
                {
                    continue;
                }

                switch (effect.EffectType)
                {
                    case EffectType.DealDamage:
                        ApplyDamageToTargets(effect, enemyTarget, allEnemyTargets);
                        SyncSelectedEnemyDisplay(state);
                        break;
                    case EffectType.GainBlock:
                        state.PlayerBlock += effect.Value;
                        break;
                    case EffectType.GainEnergy:
                        state.PlayerEnergy += effect.Value;
                        break;
                    case EffectType.DrawCards:
                        rules?.DrawCards(state, randomProvider, effect.Value);
                        break;
                    case EffectType.ApplyStatus:
                        ApplyStatusByTarget(state, effect, enemyTarget, allEnemyTargets);
                        SyncSelectedEnemyDisplay(state);
                        break;
                    case EffectType.GainMaxHp:
                        state.PlayerMaxHp += effect.Value;
                        state.PlayerHp = Math.Min(state.PlayerHp + effect.Value, state.PlayerMaxHp);
                        break;
                }
            }
        }

        private static bool TryResolveEnemyTargets(
            BattleSceneState state,
            RuntimePotion potion,
            BattlePotionUseTarget target,
            out BattleEnemyState enemyTarget,
            out List<BattleEnemyState> allEnemyTargets)
        {
            enemyTarget = null;
            allEnemyTargets = null;

            bool requiresEnemy = false;
            bool requiresAllEnemies = false;
            for (int i = 0; i < potion.Effects.Count; i++)
            {
                RuntimePotionEffect effect = potion.Effects[i];
                if (effect == null)
                {
                    continue;
                }

                if (effect.TargetSide == TargetSide.Enemy)
                {
                    requiresEnemy = true;
                }
                else if (effect.TargetSide == TargetSide.AllEnemies)
                {
                    requiresAllEnemies = true;
                }
            }

            if (requiresEnemy && !TryGetAliveEnemy(state, target.EnemyIndex, out enemyTarget))
            {
                return false;
            }

            if (!requiresAllEnemies)
            {
                return true;
            }

            allEnemyTargets = GetAliveEnemies(state);
            return allEnemyTargets.Count > 0;
        }

        private static void ApplyDamageToTargets(RuntimePotionEffect effect, BattleEnemyState enemyTarget, IReadOnlyList<BattleEnemyState> allEnemyTargets)
        {
            int hitCount = Math.Max(1, effect.HitCount);
            if (effect.TargetSide == TargetSide.Enemy)
            {
                ApplyDamage(enemyTarget, effect.Value, hitCount);
                return;
            }

            if (effect.TargetSide != TargetSide.AllEnemies || allEnemyTargets == null)
            {
                return;
            }

            for (int i = 0; i < allEnemyTargets.Count; i++)
            {
                ApplyDamage(allEnemyTargets[i], effect.Value, hitCount);
            }
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

        private static void ApplyStatusByTarget(
            BattleSceneState state,
            RuntimePotionEffect effect,
            BattleEnemyState enemyTarget,
            IReadOnlyList<BattleEnemyState> allEnemyTargets)
        {
            if (effect.TargetSide == TargetSide.Enemy)
            {
                ApplyStatus(enemyTarget?.Statuses, effect.StatusType, effect.StatusValue);
                return;
            }

            if (effect.TargetSide == TargetSide.AllEnemies)
            {
                if (allEnemyTargets == null)
                {
                    return;
                }

                for (int i = 0; i < allEnemyTargets.Count; i++)
                {
                    ApplyStatus(allEnemyTargets[i]?.Statuses, effect.StatusType, effect.StatusValue);
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

        private static bool TryGetAliveEnemy(BattleSceneState state, int enemyIndex, out BattleEnemyState enemyState)
        {
            enemyState = null;
            if (state == null || enemyIndex < 0 || enemyIndex >= state.Enemies.Count)
            {
                return false;
            }

            enemyState = state.Enemies[enemyIndex];
            return IsAliveEnemy(enemyState);
        }

        private static List<BattleEnemyState> GetAliveEnemies(BattleSceneState state)
        {
            List<BattleEnemyState> enemies = new List<BattleEnemyState>();
            if (state == null)
            {
                return enemies;
            }

            for (int i = 0; i < state.Enemies.Count; i++)
            {
                BattleEnemyState enemyState = state.Enemies[i];
                if (IsAliveEnemy(enemyState))
                {
                    enemies.Add(enemyState);
                }
            }

            return enemies;
        }

        private static bool IsAliveEnemy(BattleEnemyState enemyState)
        {
            return enemyState != null && !enemyState.IsDefeated && enemyState.Hp > 0;
        }

        private static void SyncSelectedEnemyDisplay(BattleSceneState state)
        {
            if (state == null)
            {
                return;
            }

            if (TryGetAliveEnemy(state, state.SelectedEnemyIndex, out BattleEnemyState selectedEnemy))
            {
                state.SyncSelectedEnemyDisplay(selectedEnemy, state.SelectedEnemyIndex);
                return;
            }

            for (int i = 0; i < state.Enemies.Count; i++)
            {
                BattleEnemyState enemyState = state.Enemies[i];
                if (IsAliveEnemy(enemyState))
                {
                    state.SyncSelectedEnemyDisplay(enemyState, i);
                    return;
                }
            }

            state.ClearSelectedEnemyDisplay();
        }

        private static bool ResolveUseContext(BattleScenePage currentPage, PotionUseContext useContext)
        {
            bool isBattle = currentPage == BattleScenePage.Battle;
            return useContext switch
            {
                PotionUseContext.BattleOnly => isBattle,
                PotionUseContext.OutOfBattleOnly => !isBattle,
                PotionUseContext.Both => true,
                _ => false
            };
        }
    }
}
