using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneの報酬フローを扱うクラス
    /// </summary>
    public sealed class BattleRewardFlowService : IBattleRewardFlowService
    {
        private readonly IBattleRewardService _rewardService;
        private readonly IBattleSceneRules _rules;
        private readonly IBattleRandomProvider _randomProvider;
        private readonly IBattlePotionService _potionService;
        private readonly IBattleRelicService _relicService;

        public BattleRewardFlowService(
            IBattleRewardService rewardService,
            IBattleSceneRules rules,
            IBattleRandomProvider randomProvider,
            IBattlePotionService potionService,
            IBattleRelicService relicService)
        {
            _rewardService = rewardService;
            _rules = rules;
            _randomProvider = randomProvider;
            _potionService = potionService;
            _relicService = relicService;
        }

        /// <summary>
        /// 戦闘勝利時の報酬候補状態を準備する
        /// </summary>
        public void PrepareBattleRewards(BattleSceneState state, RuntimeRunDefinition runDefinition, int goldReward)
        {
            state.BattleFinished = true;
            state.BattleGoldReward = goldReward;
            state.GoldClaimed = false;
            state.PotionClaimed = false;
            state.RelicClaimed = false;
            state.ClearPendingRewards();
            state.PotionDropped = _rules.RollPotionDrop(runDefinition, _randomProvider);
            if (state.PotionDropped)
            {
                state.PendingPotionReward = _potionService.RollBattleRewardPotion(runDefinition, _randomProvider);
            }
 
            state.RelicDropped = false;
            if (_rules.RollRelicDrop(runDefinition, _randomProvider))
            {
                state.PendingRelicReward = _relicService.RollBattleRewardRelic(state, runDefinition, _randomProvider);
                state.RelicDropped = state.PendingRelicReward != null;
            }

            state.CardRewardPicked = false;
        }

        /// <summary>
        /// 報酬画面を開く
        /// </summary>
        public void OpenReward(BattleSceneState state, RuntimeRunDefinition runDefinition, Action<BattleScenePage> setCurrentPage)
        {
            setCurrentPage(BattleScenePage.Reward);
            state.RewardChoices.Clear();
            IReadOnlyList<RuntimeRewardEntry> rewardChoices = _rules.SelectCardRewardChoices(state, runDefinition, _randomProvider);
            for (int i = 0; i < rewardChoices.Count; i++)
            {
                state.RewardChoices.Add(rewardChoices[i]);
            }
        }

        /// <summary>
        /// カード報酬を選択する
        /// </summary>
        public void SelectReward(BattleSceneState state, RuntimeRewardEntry rewardEntry)
        {
            if (rewardEntry == null)
            {
                return;
            }

            _rewardService.ApplyReward(state, rewardEntry);
            state.CardRewardPicked = true;
        }

        /// <summary>
        /// Gold報酬を取得する
        /// </summary>
        public void ClaimGold(BattleSceneState state)
        {
            state.GoldClaimed = true;
            state.Gold += state.BattleGoldReward;
            state.BattleGoldReward = 0;
        }

        /// <summary>
        /// ポーション報酬を取得する
        /// </summary>
        public void ClaimPotion(BattleSceneState state)
        {
            if (state.PendingPotionReward == null)
            {
                return;
            }

            if (_potionService.HasCapacity(state))
            {
                if (_potionService.AddOwnedPotion(state, state.PendingPotionReward))
                {
                    state.PotionClaimed = true;
                    state.PendingPotionReward = null;
                    state.ClearOwnedPotionInspection();
                }

                return;
            }

            state.PendingPotionOffer = _potionService.CreateOffer(state.PendingPotionReward, PotionOfferSource.Reward);
        }

        /// <summary>
        /// レリック報酬を取得する
        /// </summary>
        public void ClaimRelic(BattleSceneState state)
        {
            if (state.PendingRelicReward == null)
            {
                return;
            }

            if (_relicService.AddOwnedRelic(state, state.PendingRelicReward))
            {
                state.RelicClaimed = true;
                state.PendingRelicReward = null;
                state.ClearOwnedRelicInspection();
            }
        }

        /// <summary>
        /// 報酬画面から継続する
        /// </summary>
        public void ContinueFromReward(BattleSceneState state, Action openMap)
        {
            state.GoldClaimed = false;
            state.PotionClaimed = false;
            state.RelicClaimed = false;
            state.PotionDropped = false;
            state.RelicDropped = false;
            state.ClearPendingRewards();
            state.CardRewardPicked = false;
            openMap();
        }

    }
}
