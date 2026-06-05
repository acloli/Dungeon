using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneの進行集約クラス
    /// </summary>
    public sealed class BattleSceneFlowService : IBattleSceneFlowService
    {
        private readonly BattleSceneState _state = new BattleSceneState();
        private readonly IBattleSceneRules _rules;
        private readonly IBattleRandomProvider _randomProvider;
        private readonly IBattleMasterDataFacade _masterDataFacade;

        private RuntimeRunDefinition _runDefinition;

        public BattleSceneFlowService(
            IBattleSceneRules rules,
            IBattleRandomProvider randomProvider,
            IBattleMasterDataFacade masterDataFacade)
        {
            _rules = rules;
            _randomProvider = randomProvider;
            _masterDataFacade = masterDataFacade;
        }

        /// <summary>
        /// Run初期化
        /// </summary>
        public void Initialize(int runProfileId)
        {
            _runDefinition = _masterDataFacade.BuildRunDefinition(runProfileId);
            _rules.InitializeRun(_state, _runDefinition);
            _state.SelectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
            OpenMap();
        }

        /// <summary>
        /// 現在状態取得
        /// </summary>
        public BattleSceneSnapshot CreateSnapshot()
        {
            return new BattleSceneSnapshot(
                _state.CurrentPage,
                _state.Nodes,
                _state.Hand,
                _state.RewardChoices,
                _state.CurrentNodeIndex,
                _state.PlayerMaxHp,
                _state.PlayerHp,
                _state.PlayerEnergy,
                _state.PlayerBlock,
                _state.Gold,
                _state.CurrentEnemy,
                _state.EnemyHp,
                _state.EnemyBlock,
                _state.BattleFinished,
                _state.SelectedCardIndex,
                _state.IsRestShopContinueEnabled,
                _state.MapMessage,
                _state.BattleHintMessage,
                _state.RestShopMessage,
                _state.ResultMessage);
        }

        /// <summary>
        /// マップノード選択処理
        /// </summary>
        public void SelectMapNode(int index)
        {
            if (_state.Nodes == null || index < 0 || index >= _state.Nodes.Count)
            {
                return;
            }

            if (!CanMoveToNode(index))
            {
                _state.MapMessage = BattleSceneConstants.NextNodeOnly;
                return;
            }

            _state.CurrentNodeIndex = index;
            InGameNodeType nodeType = _state.Nodes[index].NodeType;
            if (nodeType == InGameNodeType.RestShop)
            {
                OpenRestShop();
                return;
            }

            OpenBattle(nodeType);
        }

        /// <summary>
        /// 手札選択処理
        /// </summary>
        public void SelectHandCard(int index)
        {
            if (_state.BattleFinished)
            {
                return;
            }

            if (index < 0 || index >= _state.Hand.Count)
            {
                return;
            }

            _state.SelectedCardIndex = index;
            RuntimeCard card = _state.Hand[index];
            if (card != null)
            {
                _state.BattleHintMessage = string.Format(BattleSceneConstants.CardSelectedFormat, card.DisplayName);
            }
        }

        /// <summary>
        /// 選択カード使用処理
        /// </summary>
        public void TryPlaySelectedCard()
        {
            if (_state.BattleFinished)
            {
                return;
            }

            if (_state.SelectedCardIndex < 0 || _state.SelectedCardIndex >= _state.Hand.Count)
            {
                _state.BattleHintMessage = BattleSceneConstants.SelectCardFirst;
                return;
            }

            RuntimeCard card = _state.Hand[_state.SelectedCardIndex];
            if (!_rules.CanPlayCard(_state, card))
            {
                _state.BattleHintMessage = BattleSceneConstants.NotEnoughEnergy;
                _state.SelectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
                return;
            }

            BattleCardResolutionResult result = _rules.PlayCard(_state, card, _randomProvider);
            _state.BattleHintMessage = BuildCardHint(card, result);
            _state.SelectedCardIndex = BattleSceneConstants.UnselectedCardIndex;

            if (_state.EnemyHp <= 0)
            {
                OnBattleVictory();
            }
        }

        /// <summary>
        /// ターン終了処理
        /// </summary>
        public void EndTurn()
        {
            if (_state.BattleFinished)
            {
                return;
            }

            BattleEnemyTurnResult result = _rules.ResolveEnemyTurn(_state, _randomProvider);
            _state.BattleHintMessage = string.Format(BattleSceneConstants.EnemyTurnFormat, result.DamageDealt);
            _state.PlayerEnergy = BattleSceneConstants.DefaultPlayerEnergy;
            _rules.DrawHand(_state, _randomProvider);

            if (_state.PlayerHp <= 0)
            {
                OpenResult(false);
            }
        }

        /// <summary>
        /// 報酬選択処理
        /// </summary>
        public void SelectReward(RuntimeCard card)
        {
            if (card != null)
            {
                _state.Deck.Add(card);
            }

            OpenMap();
        }

        /// <summary>
        /// 休憩適用処理
        /// </summary>
        public void ApplyRest()
        {
            _rules.ApplyRest(_state);
            _state.RestShopMessage = string.Format(
                BattleSceneConstants.RestDoneFormat,
                _state.PlayerHp,
                _state.PlayerMaxHp);
            _state.IsRestShopContinueEnabled = true;
        }

        /// <summary>
        /// 強化適用処理
        /// </summary>
        public void ApplyUpgrade()
        {
            _state.RestShopMessage = BattleSceneConstants.UpgradeDone;
            _state.IsRestShopContinueEnabled = true;
        }

        /// <summary>
        /// 購入適用処理
        /// </summary>
        public void ApplyShopPurchase()
        {
            bool success = _rules.ApplyShopPurchase(_state, _randomProvider);
            _state.RestShopMessage = success
                ? string.Format(BattleSceneConstants.PurchaseSuccessFormat, _state.Gold)
                : BattleSceneConstants.NotEnoughGold;
            _state.IsRestShopContinueEnabled = true;
        }

        /// <summary>
        /// 補給画面継続処理
        /// </summary>
        public void ContinueFromRestShop()
        {
            OpenMap();
        }

        /// <summary>
        /// マップ画面遷移
        /// </summary>
        private void OpenMap()
        {
            _state.CurrentPage = BattleScenePage.Map;
            _state.BattleFinished = false;
            _state.SelectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
            _state.RewardChoices.Clear();
            _state.MapMessage = string.Format(
                BattleSceneConstants.MapStateFormat,
                _state.PlayerHp,
                _state.PlayerMaxHp,
                _state.Gold,
                _state.CurrentNodeIndex + 2,
                _state.Nodes.Count);
        }

        /// <summary>
        /// 戦闘画面遷移
        /// </summary>
        private void OpenBattle(InGameNodeType nodeType)
        {
            _state.CurrentPage = BattleScenePage.Battle;
            _state.BattleFinished = false;
            _state.SelectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
            _state.PlayerEnergy = BattleSceneConstants.DefaultPlayerEnergy;
            _state.PlayerBlock = 0;
            _state.EnemyStatuses.Clear();
            _state.EnemyBuffs.Clear();
            _state.EnemyTurnCount = 0;
            _state.EnemyCycleIndex = 0;
            _state.CurrentEnemy = _rules.SelectEnemy(_runDefinition, nodeType, _randomProvider);
            _state.EnemyHp = _rules.RollEnemyHp(_state.CurrentEnemy, _randomProvider);
            _state.EnemyBlock = 0;
            _rules.DrawHand(_state, _randomProvider);
            _state.BattleHintMessage = BattleSceneConstants.SelectCardAndTarget;
        }

        /// <summary>
        /// 戦闘勝利処理
        /// </summary>
        private void OnBattleVictory()
        {
            _state.BattleFinished = true;
            _state.Gold += _state.CurrentEnemy != null ? _state.CurrentEnemy.GoldReward : 0;

            if (GetCurrentNodeType() == InGameNodeType.Boss)
            {
                OpenResult(true);
                return;
            }

            OpenReward();
        }

        /// <summary>
        /// 報酬画面遷移
        /// </summary>
        private void OpenReward()
        {
            _state.CurrentPage = BattleScenePage.Reward;
            _state.RewardChoices.Clear();
            IReadOnlyList<RuntimeCard> rewardCards = _rules.SelectRewardChoices(_state, _runDefinition, _randomProvider);
            for (int i = 0; i < rewardCards.Count; i++)
            {
                _state.RewardChoices.Add(rewardCards[i]);
            }
        }

        /// <summary>
        /// 補給画面遷移
        /// </summary>
        private void OpenRestShop()
        {
            _state.CurrentPage = BattleScenePage.RestShop;
            _state.IsRestShopContinueEnabled = false;
            _state.RestShopMessage = string.Format(
                BattleSceneConstants.RestShopStateFormat,
                _state.PlayerHp,
                _state.PlayerMaxHp,
                _state.Gold);
        }

        /// <summary>
        /// 結果画面遷移
        /// </summary>
        private void OpenResult(bool victory)
        {
            _state.CurrentPage = BattleScenePage.Result;
            _state.ResultMessage = victory
                ? string.Format(BattleSceneConstants.ResultVictoryFormat, _state.PlayerHp, _state.PlayerMaxHp, _state.Gold)
                : BattleSceneConstants.RunFailedMessage;
        }

        /// <summary>
        /// 現在ノード種別取得
        /// </summary>
        private InGameNodeType GetCurrentNodeType()
        {
            if (_state.Nodes == null)
            {
                return InGameNodeType.Battle;
            }

            if (_state.CurrentNodeIndex < 0 || _state.CurrentNodeIndex >= _state.Nodes.Count)
            {
                return InGameNodeType.Battle;
            }

            return _state.Nodes[_state.CurrentNodeIndex].NodeType;
        }

        /// <summary>
        /// カード使用後ヒント文言生成
        /// </summary>
        private static string BuildCardHint(RuntimeCard card, BattleCardResolutionResult result)
        {
            if (result.TotalDamage > 0)
            {
                return string.Format(BattleSceneConstants.DealDamageFormat, result.TotalDamage);
            }

            if (result.TotalBlock > 0)
            {
                return string.Format(BattleSceneConstants.GainBlockFormat, result.TotalBlock);
            }

            return string.Format(BattleSceneConstants.CardResolvedFormat, card.DisplayName);
        }

        /// <summary>
        /// ノード遷移可能判定
        /// </summary>
        private bool CanMoveToNode(int index)
        {
            if (_state.CurrentNodeIndex < 0)
            {
                return index == 0;
            }

            RuntimeMapNode currentNode = _state.Nodes[_state.CurrentNodeIndex];
            if (currentNode.NextNodeIndices != null && currentNode.NextNodeIndices.Count > 0)
            {
                for (int i = 0; i < currentNode.NextNodeIndices.Count; i++)
                {
                    if (currentNode.NextNodeIndices[i] == index)
                    {
                        return true;
                    }
                }

                return false;
            }

            return index == _state.CurrentNodeIndex + 1;
        }
    }
}
