using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using Game.MasterData.Generated;

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
        private readonly IBattleDisplayTextService _displayTextService;

        private RuntimeRunDefinition _runDefinition;

        public BattleSceneFlowService(
            IBattleSceneRules rules,
            IBattleRandomProvider randomProvider,
            IBattleMasterDataFacade masterDataFacade,
            IBattleDisplayTextService displayTextService = null)
        {
            _rules = rules;
            _randomProvider = randomProvider;
            _masterDataFacade = masterDataFacade;
            _displayTextService = displayTextService ?? new BattleDisplayTextService();
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
                _state.ResultMessage,
                BuildEnemyIntent(),
                BuildStatusViews(_state.PlayerStatuses),
                BuildStatusViews(_state.EnemyStatuses),
                BuildBuffViews(_state.PlayerBuffs),
                BuildBuffViews(_state.EnemyBuffs),
                BuildEnemyViews(),
                _state.SelectedEnemyIndex);
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
        /// 敵対象選択処理
        /// </summary>
        public void SelectEnemyTarget(int index)
        {
            if (index < 0 || index >= _state.Enemies.Count)
            {
                return;
            }

            BattleEnemyState enemyState = _state.Enemies[index];
            if (enemyState == null || enemyState.IsDefeated)
            {
                return;
            }

            _state.SelectedEnemyIndex = index;
            _state.CurrentEnemy = enemyState.Enemy;
            _state.EnemyHp = enemyState.Hp;
            _state.EnemyBlock = enemyState.Block;
            CopyDictionary(enemyState.Statuses, _state.EnemyStatuses);
            CopyDictionary(enemyState.Buffs, _state.EnemyBuffs);
            _state.BattleHintMessage = string.Format(BattleSceneConstants.EnemyTargetSelectedFormat, enemyState.Enemy.DisplayName);
        }

        /// <summary>
        /// 選択カードが敵個別対象を必要とするか
        /// </summary>
        public bool DoesSelectedCardRequireEnemyTarget()
        {
            if (_state.SelectedCardIndex < 0 || _state.SelectedCardIndex >= _state.Hand.Count)
            {
                return false;
            }

            RuntimeCard card = _state.Hand[_state.SelectedCardIndex];
            if (card == null)
            {
                return false;
            }

            for (int i = 0; i < card.Effects.Count; i++)
            {
                RuntimeCardEffect effect = card.Effects[i];
                if (effect.TargetSide == TargetSide.Enemy)
                {
                    return true;
                }
            }

            return false;
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

            if (AreAllEnemiesDefeated())
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
            _state.Enemies.Clear();
            _state.SelectedEnemyIndex = BattleSceneConstants.DefaultEnemyTargetIndex;
            RuntimeEncounterFormation formation = _rules.SelectEncounterFormation(_runDefinition, nodeType, _randomProvider);
            if (formation != null)
            {
                for (int i = 0; i < formation.Enemies.Count; i++)
                {
                    RuntimeEncounterEnemyEntry entry = formation.Enemies[i];
                    if (entry == null || entry.Enemy == null)
                    {
                        continue;
                    }

                    _state.Enemies.Add(new BattleEnemyState(entry.Enemy, entry.SlotIndex, _rules.RollEnemyHp(entry.Enemy, _randomProvider)));
                }
            }

            SyncSelectedEnemyForDisplay();
            _rules.DrawHand(_state, _randomProvider);
            _state.BattleHintMessage = BattleSceneConstants.SelectCardAndTarget;
        }

        /// <summary>
        /// 戦闘勝利処理
        /// </summary>
        private void OnBattleVictory()
        {
            _state.BattleFinished = true;
            _state.Gold += CalculateBattleGoldReward();

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

        /// <summary>
        /// 表示用敵意図構築
        /// </summary>
        private BattleIntentViewModel BuildEnemyIntent()
        {
            BattleEnemyState enemyState = GetSelectedEnemy();
            RuntimeEnemyAction action = SelectEnemyActionPreview(enemyState);
            if (action == null)
            {
                return null;
            }

            return new BattleIntentViewModel(
                action.IntentType,
                _displayTextService.GetIntentName(action.IntentType),
                action.Damage,
                action.HitCount,
                action.Block,
                action.StatusType,
                _displayTextService.GetStatusName(action.StatusType),
                action.StatusValue,
                action.BuffType,
                _displayTextService.GetBuffName(action.BuffType),
                action.BuffValue);
        }

        /// <summary>
        /// 表示用敵行動選択
        /// </summary>
        private RuntimeEnemyAction SelectEnemyActionPreview(BattleEnemyState enemyState)
        {
            if (_state.CurrentPage != BattleScenePage.Battle ||
                enemyState == null ||
                enemyState.Enemy == null ||
                enemyState.Enemy.Actions == null ||
                enemyState.Enemy.Actions.Count == 0 ||
                enemyState.IsDefeated)
            {
                return null;
            }

            RuntimeEnemyAction openingAction = FindFirstAction(enemyState, RepeatRule.OpeningOnly);
            if (enemyState.TurnCount == 0 && openingAction != null)
            {
                return openingAction;
            }

            RuntimeEnemyAction repeatAction = FindFirstAction(enemyState, RepeatRule.RepeatAfterOpening);
            if (enemyState.TurnCount > 0 && repeatAction != null)
            {
                return repeatAction;
            }

            RuntimeEnemyAction afterOpeningRandomAction = FindFirstAction(enemyState, RepeatRule.AfterOpeningRandom);
            if (enemyState.TurnCount > 0 && afterOpeningRandomAction != null)
            {
                return afterOpeningRandomAction;
            }

            RuntimeEnemyAction randomAction = FindFirstAction(enemyState, RepeatRule.Random);
            if (randomAction != null)
            {
                return randomAction;
            }

            RuntimeEnemyAction cycleAction = FindCycleActionPreview(enemyState);
            return cycleAction ?? enemyState.Enemy.Actions[0];
        }

        /// <summary>
        /// 指定反復規則の先頭行動取得
        /// </summary>
        private RuntimeEnemyAction FindFirstAction(BattleEnemyState enemyState, RepeatRule repeatRule)
        {
            IReadOnlyList<RuntimeEnemyAction> actions = enemyState.Enemy.Actions;
            for (int i = 0; i < actions.Count; i++)
            {
                RuntimeEnemyAction action = actions[i];
                if (action.RepeatRule == repeatRule)
                {
                    return action;
                }
            }

            return null;
        }

        /// <summary>
        /// cycle行動の表示用取得
        /// </summary>
        private RuntimeEnemyAction FindCycleActionPreview(BattleEnemyState enemyState)
        {
            List<RuntimeEnemyAction> cycleActions = new List<RuntimeEnemyAction>();
            IReadOnlyList<RuntimeEnemyAction> actions = enemyState.Enemy.Actions;
            for (int i = 0; i < actions.Count; i++)
            {
                RuntimeEnemyAction action = actions[i];
                if (action.RepeatRule == RepeatRule.Cycle)
                {
                    cycleActions.Add(action);
                }
            }

            if (cycleActions.Count == 0)
            {
                return null;
            }

            return cycleActions[enemyState.CycleIndex % cycleActions.Count];
        }

        /// <summary>
        /// 敵表示一覧構築
        /// </summary>
        private IReadOnlyList<BattleEnemyViewModel> BuildEnemyViews()
        {
            List<BattleEnemyViewModel> views = new List<BattleEnemyViewModel>();
            for (int i = 0; i < _state.Enemies.Count; i++)
            {
                BattleEnemyState enemyState = _state.Enemies[i];
                if (enemyState == null || enemyState.Enemy == null)
                {
                    continue;
                }

                views.Add(new BattleEnemyViewModel(
                    enemyState.SlotIndex,
                    enemyState.Enemy.DisplayName,
                    enemyState.Hp,
                    enemyState.Block,
                    enemyState.IsDefeated,
                    BuildIntentView(enemyState),
                    BuildStatusViews(enemyState.Statuses),
                    BuildBuffViews(enemyState.Buffs)));
            }

            return views;
        }

        /// <summary>
        /// 敵意図表示モデル構築
        /// </summary>
        private BattleIntentViewModel BuildIntentView(BattleEnemyState enemyState)
        {
            RuntimeEnemyAction action = SelectEnemyActionPreview(enemyState);
            if (action == null)
            {
                return null;
            }

            return new BattleIntentViewModel(
                action.IntentType,
                _displayTextService.GetIntentName(action.IntentType),
                action.Damage,
                action.HitCount,
                action.Block,
                action.StatusType,
                _displayTextService.GetStatusName(action.StatusType),
                action.StatusValue,
                action.BuffType,
                _displayTextService.GetBuffName(action.BuffType),
                action.BuffValue);
        }

        /// <summary>
        /// 選択中敵取得
        /// </summary>
        private BattleEnemyState GetSelectedEnemy()
        {
            if (_state.SelectedEnemyIndex >= 0 && _state.SelectedEnemyIndex < _state.Enemies.Count)
            {
                BattleEnemyState enemyState = _state.Enemies[_state.SelectedEnemyIndex];
                if (enemyState != null && !enemyState.IsDefeated)
                {
                    return enemyState;
                }
            }

            for (int i = 0; i < _state.Enemies.Count; i++)
            {
                BattleEnemyState enemyState = _state.Enemies[i];
                if (enemyState != null && !enemyState.IsDefeated)
                {
                    _state.SelectedEnemyIndex = i;
                    return enemyState;
                }
            }

            return null;
        }

        /// <summary>
        /// 選択敵を旧表示項目へ同期する
        /// </summary>
        private void SyncSelectedEnemyForDisplay()
        {
            BattleEnemyState enemyState = GetSelectedEnemy();
            if (enemyState == null)
            {
                _state.CurrentEnemy = null;
                _state.EnemyHp = 0;
                _state.EnemyBlock = 0;
                _state.EnemyStatuses.Clear();
                _state.EnemyBuffs.Clear();
                return;
            }

            _state.CurrentEnemy = enemyState.Enemy;
            _state.EnemyHp = enemyState.Hp;
            _state.EnemyBlock = enemyState.Block;
            CopyDictionary(enemyState.Statuses, _state.EnemyStatuses);
            CopyDictionary(enemyState.Buffs, _state.EnemyBuffs);
        }

        /// <summary>
        /// 全敵撃破判定
        /// </summary>
        private bool AreAllEnemiesDefeated()
        {
            if (_state.Enemies.Count == 0)
            {
                return _state.EnemyHp <= 0;
            }

            for (int i = 0; i < _state.Enemies.Count; i++)
            {
                BattleEnemyState enemyState = _state.Enemies[i];
                if (enemyState != null && !enemyState.IsDefeated && enemyState.Hp > 0)
                {
                    return false;
                }
            }

            SyncSelectedEnemyForDisplay();
            return true;
        }

        /// <summary>
        /// 戦闘報酬ゴールド合算
        /// </summary>
        private int CalculateBattleGoldReward()
        {
            int total = 0;
            for (int i = 0; i < _state.Enemies.Count; i++)
            {
                BattleEnemyState enemyState = _state.Enemies[i];
                if (enemyState != null && enemyState.Enemy != null)
                {
                    total += enemyState.Enemy.GoldReward;
                }
            }

            return total > 0 ? total : _state.CurrentEnemy != null ? _state.CurrentEnemy.GoldReward : 0;
        }

        /// <summary>
        /// 辞書内容コピー
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
        /// 状態表示一覧構築
        /// </summary>
        private IReadOnlyList<BattleStatusViewModel> BuildStatusViews(IReadOnlyDictionary<StatusType, int> statuses)
        {
            List<BattleStatusViewModel> views = new List<BattleStatusViewModel>();
            if (statuses == null)
            {
                return views;
            }

            foreach (KeyValuePair<StatusType, int> status in statuses)
            {
                if (status.Key == StatusType.None || status.Value <= 0)
                {
                    continue;
                }

                views.Add(new BattleStatusViewModel(_displayTextService.GetStatusName(status.Key), status.Value, false));
            }

            return views;
        }

        /// <summary>
        /// buff表示一覧構築
        /// </summary>
        private IReadOnlyList<BattleStatusViewModel> BuildBuffViews(IReadOnlyDictionary<BuffType, int> buffs)
        {
            List<BattleStatusViewModel> views = new List<BattleStatusViewModel>();
            if (buffs == null)
            {
                return views;
            }

            foreach (KeyValuePair<BuffType, int> buff in buffs)
            {
                if (buff.Key == BuffType.None || buff.Value <= 0)
                {
                    continue;
                }

                views.Add(new BattleStatusViewModel(_displayTextService.GetBuffName(buff.Key), buff.Value, true));
            }

            return views;
        }
    }
}
