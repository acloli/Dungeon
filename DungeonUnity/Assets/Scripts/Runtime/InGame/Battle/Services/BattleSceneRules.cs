using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using UnityEngine;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleScene基本ルールクラス
    /// </summary>
    public sealed class BattleSceneRules : IBattleSceneRules
    {
        private readonly IBattleDeckService _deckService;
        private readonly IBattleCombatResolver _combatResolver;
        private readonly IBattleEncounterSelector _encounterSelector;
        private readonly IBattleRewardRollService _rewardRollService;

        public BattleSceneRules(
            IBattleDeckService deckService,
            IBattleCombatResolver combatResolver,
            IBattleEncounterSelector encounterSelector,
            IBattleRewardRollService rewardRollService)
        {
            _deckService = deckService;
            _combatResolver = combatResolver;
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
            state.ClearSelectedEnemyDisplay();
            state.Enemies.Clear();
            state.BattleFinished = false;
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
            state.PlayerBuffs.Clear();

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
        /// 現在階層に対応する宝箱定義を取得する
        /// </summary>
        public RuntimeTreasureDefinition GetTreasureDefinition(BattleSceneState state, RuntimeRunDefinition runDefinition)
        {
            return GetTreasureDefinition(runDefinition, GetCurrentFloor(state));
        }

        /// <summary>
        /// 宝箱Gold報酬を抽選する
        /// </summary>
        public int RollTreasureGold(BattleSceneState state, RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider)
        {
            return RollTreasureGold(GetTreasureDefinition(state, runDefinition), randomProvider);
        }

        /// <summary>
        /// 宝箱Gold報酬を抽選する
        /// </summary>
        public int RollTreasureGold(RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider)
        {
            return RollTreasureGold(GetTreasureDefinition(runDefinition, 1), randomProvider);
        }

        /// <summary>
        /// 使用可否判定
        /// </summary>
        public bool CanPlayCard(BattleSceneState state, RuntimeCard card)
        {
            return _combatResolver.CanPlayCard(state, card);
        }

        /// <summary>
        /// カード適用
        /// </summary>
        public BattleCardResolutionResult PlayCard(BattleSceneState state, int handIndex, IBattleRandomProvider randomProvider)
        {
            return _combatResolver.PlayCard(state, handIndex, randomProvider);
        }

        /// <summary>
        /// 敵ターン解決
        /// </summary>
        public BattleEnemyTurnResult ResolveEnemyTurn(BattleSceneState state, IBattleRandomProvider randomProvider)
        {
            return _combatResolver.ResolveEnemyTurn(state, randomProvider);
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
        /// 現在階層を取得する
        /// </summary>
        private static int GetCurrentFloor(BattleSceneState state)
        {
            if (state == null ||
                state.CurrentNodeIndex < 0 ||
                state.CurrentNodeIndex >= state.Nodes.Count)
            {
                return 1;
            }

            return state.Nodes[state.CurrentNodeIndex].Floor;
        }

        /// <summary>
        /// 階層に対応する宝箱定義を取得する
        /// </summary>
        private static RuntimeTreasureDefinition GetTreasureDefinition(RuntimeRunDefinition runDefinition, int floor)
        {
            if (runDefinition == null || runDefinition.TreasureDefinitions.Count == 0)
            {
                return null;
            }

            RuntimeTreasureDefinition fallback = null;
            for (int i = 0; i < runDefinition.TreasureDefinitions.Count; i++)
            {
                RuntimeTreasureDefinition treasureDefinition = runDefinition.TreasureDefinitions[i];
                if (treasureDefinition == null)
                {
                    continue;
                }

                fallback ??= treasureDefinition;
                if (floor >= treasureDefinition.MinFloor && floor <= treasureDefinition.MaxFloor)
                {
                    return treasureDefinition;
                }
            }

            return fallback;
        }

        /// <summary>
        /// 宝箱定義からGold報酬を抽選する
        /// </summary>
        private static int RollTreasureGold(RuntimeTreasureDefinition treasureDefinition, IBattleRandomProvider randomProvider)
        {
            if (treasureDefinition == null)
            {
                return 0;
            }

            int minGold = Mathf.Max(0, treasureDefinition.GoldMin);
            int maxGold = Mathf.Max(minGold, treasureDefinition.GoldMax);
            if (minGold == maxGold || randomProvider == null)
            {
                return maxGold;
            }

            return randomProvider.Range(minGold, maxGold + 1);
        }
    }
}
