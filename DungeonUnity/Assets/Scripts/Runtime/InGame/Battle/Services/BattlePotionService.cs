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

        public PendingPotionUseRequest BuildUseRequest(BattleSceneState state, int potionIndex)
        {
            if (state == null || potionIndex < 0 || potionIndex >= state.OwnedPotions.Count)
            {
                return null;
            }

            RuntimePotion potion = state.OwnedPotions[potionIndex];
            if (!CanUsePotionInCurrentPage(state, potion))
            {
                return null;
            }

            return new PendingPotionUseRequest(
                potion,
                potionIndex,
                potion.UseContext,
                potion.TargetMode,
                true);
        }

        public bool ConsumePotion(BattleSceneState state, PendingPotionUseRequest request, IBattleSceneRules rules, IBattleRandomProvider randomProvider)
        {
            if (state == null || request == null)
            {
                return false;
            }

            if (request.PotionIndex < 0 || request.PotionIndex >= state.OwnedPotions.Count)
            {
                return false;
            }

            RuntimePotion potion = state.OwnedPotions[request.PotionIndex];
            if (potion == null || potion.Id != request.Potion.Id)
            {
                return false;
            }

            ApplyEffects(state, potion, rules, randomProvider);
            state.OwnedPotions.RemoveAt(request.PotionIndex);
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

        private static void ApplyEffects(BattleSceneState state, RuntimePotion potion, IBattleSceneRules rules, IBattleRandomProvider randomProvider)
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
                        if (effect.StatusType != StatusType.None && effect.StatusValue > 0)
                        {
                            int currentValue = 0;
                            state.PlayerStatuses.TryGetValue(effect.StatusType, out currentValue);
                            state.PlayerStatuses[effect.StatusType] = currentValue + effect.StatusValue;
                        }
                        break;
                    case EffectType.GainMaxHp:
                        state.PlayerMaxHp += effect.Value;
                        state.PlayerHp = Math.Min(state.PlayerHp + effect.Value, state.PlayerMaxHp);
                        break;
                }
            }
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
