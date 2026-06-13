using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using Dungeon.Runtime.InGame.Save.Model;
using Dungeon.Runtime.InGame.Save.Services;
using Game.MasterData.Generated;
using Cysharp.Threading.Tasks;
using TFramework.Debug;

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
        private readonly IBattleRewardService _rewardService;
        private readonly IBattleSnapshotFactory _snapshotFactory;
        private readonly IBattleShopService _shopService;
        private readonly IBattleCombatEventService _combatEventService;
        private readonly IBattleRelicService _relicService;
        private readonly IBattleEventService _eventService;
        private readonly IRunSaveService _runSaveService;

        private RuntimeRunDefinition _runDefinition;

        public BattleSceneFlowService(
            IBattleSceneRules rules,
            IBattleRandomProvider randomProvider,
            IBattleMasterDataFacade masterDataFacade,
            IBattleRewardService rewardService,
            IBattleSnapshotFactory snapshotFactory,
            IBattleShopService shopService,
            IBattleCombatEventService combatEventService,
            IBattleRelicService relicService,
            IBattleEventService eventService,
            IRunSaveService runSaveService = null)
        {
            _rules = rules;
            _randomProvider = randomProvider;
            _masterDataFacade = masterDataFacade;
            _rewardService = rewardService;
            _snapshotFactory = snapshotFactory;
            _shopService = shopService;
            _combatEventService = combatEventService;
            _relicService = relicService;
            _eventService = eventService;
            _runSaveService = runSaveService;
        }

        /// <summary>
        /// Run初期化
        /// </summary>
        public void Initialize(int runProfileId)
        {
            _runDefinition = _masterDataFacade.BuildRunDefinition(runProfileId);
            _rules.InitializeRun(_state, _runDefinition);
            _state.SelectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
            ClearOwnedRelicInspection();
            _state.OwnedRelics.Clear();
            _state.PendingRelicReward = null;
            OpenMap();
            RequestSave();
        }

        /// <summary>
        /// セーブデータからの初期化
        /// </summary>
        public void InitializeFromSave(RunSaveData saveData)
        {
            _runDefinition = _masterDataFacade.BuildRunDefinition(saveData.RunProfileId);
            _rules.InitializeRun(_state, _runDefinition);

            _state.PlayerMaxHp = saveData.PlayerMaxHp;
            _state.PlayerHp = saveData.PlayerHp;
            _state.PlayerEnergy = saveData.PlayerEnergy;
            _state.Gold = saveData.Gold;
            _state.CurrentNodeIndex = saveData.CurrentNodeIndex;
            _state.CurrentPage = (BattleScenePage)saveData.CurrentPage;

            IReadOnlyDictionary<int, RuntimeCard> cardCatalog = _masterDataFacade.BuildCardCatalog();
            _state.Deck.Clear();
            if (saveData.DeckCardIds != null)
            {
                foreach (int cardId in saveData.DeckCardIds)
                {
                    if (cardCatalog.TryGetValue(cardId, out RuntimeCard card))
                    {
                        _state.Deck.Add(card);
                    }
                }
            }

            _state.SelectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
            ClearOwnedRelicInspection();
            _state.PendingRelicReward = null;
            _relicService.RestoreOwnedRelics(_state, _runDefinition, saveData.OwnedRelicIds);

            _state.ShopItems.Clear();
            _state.IsCardRemovalSoldOut = saveData.IsCardRemovalSoldOut;
            _state.CardRemovalCount = saveData.CardRemovalCount;
            if (saveData.ShopItems != null)
            {
                foreach (SaveShopItem savedItem in saveData.ShopItems)
                {
                    RuntimeCard card = null;
                    RuntimeRelic relic = null;
                    RuntimePotion potion = null;
                    if (savedItem.RewardType == (int)RewardType.Card && savedItem.CardId > 0)
                    {
                        cardCatalog.TryGetValue(savedItem.CardId, out card);
                    }
                    else if (savedItem.RewardType == (int)RewardType.Relic && savedItem.ItemId > 0)
                    {
                        _runDefinition.RelicCatalog.TryGetValue(savedItem.ItemId, out relic);
                    }
                    else if (savedItem.RewardType == (int)RewardType.Potion && savedItem.ItemId > 0)
                    {
                        _runDefinition.PotionCatalog.TryGetValue(savedItem.ItemId, out potion);
                    }
                    _state.ShopItems.Add(new BattleShopItemState(
                        savedItem.SlotIndex,
                        (RewardType)savedItem.RewardType,
                        card,
                        relic,
                        potion,
                        savedItem.ItemId,
                        savedItem.Price,
                        savedItem.IsSoldOut));
                }
            }

            if (_state.CurrentPage == BattleScenePage.Map)
            {
                OpenMap();
            }
            else if (_state.CurrentPage == BattleScenePage.RestShop)
            {
                OpenRestShop();
            }
            else
            {
                OpenMap();
            }
        }

        /// <summary>
        /// 現在状態取得
        /// </summary>
        public BattleSceneSnapshot CreateSnapshot()
        {
            return _snapshotFactory.CreateSnapshot(_state);
        }

        /// <summary>
        /// 現在デッキ取得
        /// </summary>
        public IReadOnlyList<RuntimeCard> GetDeckCards()
        {
            return _state.Deck;
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
                RequestSave();
                return;
            }

            if (nodeType == InGameNodeType.Event)
            {
                OpenEvent();
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

            BattleCardResolutionResult result = _rules.PlayCard(_state, _state.SelectedCardIndex, _randomProvider);
            _combatEventService.OnCardPlayed(_state, card, result);
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

            _combatEventService.OnPlayerTurnEnd(_state);
            _rules.DiscardHand(_state);
            BattleEnemyTurnResult result = _rules.ResolveEnemyTurn(_state, _randomProvider);
            if (result.DamageDealt > 0)
            {
                _combatEventService.OnPlayerDamaged(_state, result.DamageDealt);
            }

            _state.BattleHintMessage = string.Format(BattleSceneConstants.EnemyTurnFormat, result.DamageDealt);
            if (_state.PlayerHp <= 0)
            {
                OpenResult(false);
                return;
            }

            _state.PlayerEnergy = BattleSceneConstants.DefaultPlayerEnergy;
            _combatEventService.OnPlayerTurnStart(_state);
            _rules.DrawHand(_state, _randomProvider);
        }

        /// <summary>
        /// 報酬選択処理
        /// </summary>
        public void SelectReward(RuntimeRewardEntry rewardEntry)
        {
            if (rewardEntry == null) return;
            _rewardService.ApplyReward(_state, rewardEntry);
            _state.CardRewardPicked = true;
        }

        /// <summary>
        /// 報酬画面継続処理
        /// </summary>
        public void ContinueFromReward()
        {
            _state.GoldClaimed = false;
            _state.PotionClaimed = false;
            _state.RelicClaimed = false;
            _state.PotionDropped = false;
            _state.PendingRelicReward = null;
            _state.CardRewardPicked = false;
            OpenMap();
            RequestSave();
        }

        /// <summary>
        /// 報酬 Gold 取得
        /// </summary>
        public void ClaimGold()
        {
            _state.GoldClaimed = true;
            _state.Gold += _state.BattleGoldReward;
            _state.BattleGoldReward = 0;
        }

        /// <summary>
        /// ポーション取得
        /// </summary>
        public void ClaimPotion()
        {
            _state.PotionClaimed = true;
        }

        /// <summary>
        /// レリック取得
        /// </summary>
        public void ClaimRelic()
        {
            if (_state.PendingRelicReward == null)
            {
                return;
            }

            if (_relicService.AddOwnedRelic(_state, _state.PendingRelicReward))
            {
                _state.RelicClaimed = true;
                _state.PendingRelicReward = null;
                ClearOwnedRelicInspection();
            }
        }

        /// <summary>
        /// 所持レリック説明を表示する
        /// </summary>
        public void InspectOwnedRelic(int index)
        {
            if (index < 0 || index >= _state.OwnedRelics.Count)
            {
                return;
            }

            RuntimeRelic relic = _state.OwnedRelics[index];
            if (relic == null)
            {
                return;
            }

            if (_state.SelectedOwnedRelicIndex == index)
            {
                ClearOwnedRelicInspection();
                return;
            }

            _state.SelectedOwnedRelicIndex = index;
            _state.OwnedRelicHintMessage = string.IsNullOrEmpty(relic.Description)
                ? relic.DisplayName
                : string.Format("{0}\n{1}", relic.DisplayName, relic.Description);
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
        /// ショップを開く
        /// </summary>
        public void OpenShop()
        {
            SetCurrentPage(BattleScenePage.Shop);
        }

        /// <summary>
        /// ショップアイテム購入
        /// </summary>
        public void PurchaseShopItem(int slotIndex)
        {
            if (_shopService.PurchaseShopItem(_state, slotIndex))
            {
                GrantPurchasedRelic(slotIndex);
                ClearOwnedRelicInspection();
                RequestSave();
            }
        }

        /// <summary>
        /// カード削除選択を開く
        /// </summary>
        public void OpenCardRemoval()
        {
            SetCurrentPage(BattleScenePage.CardSelect);
        }

        /// <summary>
        /// カード削除購入
        /// </summary>
        public void PurchaseCardRemoval(RuntimeCard card)
        {
            if (_shopService.PurchaseCardRemoval(_state, card))
            {
                RequestSave();
            }
            // 削除後ショップに戻る
            SetCurrentPage(BattleScenePage.Shop);
        }

        /// <summary>
        /// ショップから退出
        /// </summary>
        public void LeaveShop()
        {
            SetCurrentPage(BattleScenePage.RestShop);
            _state.RestShopMessage = string.Format(
                BattleSceneConstants.RestShopStateFormat,
                _state.PlayerHp,
                _state.PlayerMaxHp,
                _state.Gold);
            _state.IsRestShopContinueEnabled = true;
        }

        /// <summary>
        /// 補給画面継続処理
        /// </summary>
        public void ContinueFromRestShop()
        {
            OpenMap();
            RequestSave();
        }

        /// <summary>
        /// イベント選択肢決定処理
        /// </summary>
        public void SelectEventChoice(int choiceId)
        {
            if (_state.CurrentEvent != null)
            {
                _eventService.ApplyEventChoice(_state, _state.CurrentEvent, choiceId);
            }

            _state.CurrentEvent = null;
            _state.EventMessage = string.Empty;
            OpenMap();
            RequestSave();
        }

        /// <summary>
        /// マップ画面遷移
        /// </summary>
        private void OpenMap()
        {
            SetCurrentPage(BattleScenePage.Map);
            _state.BattleFinished = false;
            _state.SelectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
            _state.RewardChoices.Clear();
            _state.PendingRelicReward = null;
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
            SetCurrentPage(BattleScenePage.Battle);
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
            _combatEventService.OnCombatStart(_state);
            _rules.PrepareBattleDeck(_state, _randomProvider);
            _combatEventService.OnPlayerTurnStart(_state);
            _rules.DrawHand(_state, _randomProvider);
            _state.BattleHintMessage = BattleSceneConstants.SelectCardAndTarget;
        }

        /// <summary>
        /// 戦闘勝利処理
        /// </summary>
        private void OnBattleVictory()
        {
            _state.BattleFinished = true;
            _state.BattleGoldReward = CalculateBattleGoldReward();
            _state.PotionDropped = _rules.RollPotionDrop(_runDefinition, _randomProvider);
            _state.PendingRelicReward = null;
            if (_rules.RollRelicDrop(_runDefinition, _randomProvider))
            {
                _state.PendingRelicReward = _relicService.RollBattleRewardRelic(_state, _runDefinition, _randomProvider);
            }
            _state.CardRewardPicked = false;

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
            SetCurrentPage(BattleScenePage.Reward);
            _state.RewardChoices.Clear();
            IReadOnlyList<RuntimeRewardEntry> rewardChoices = _rules.SelectCardRewardChoices(_state, _runDefinition, _randomProvider);
            for (int i = 0; i < rewardChoices.Count; i++)
            {
                _state.RewardChoices.Add(rewardChoices[i]);
            }
        }

        /// <summary>
        /// イベント画面遷移
        /// </summary>
        private void OpenEvent()
        {
            if (_runDefinition == null || _runDefinition.PossibleEvents == null || _runDefinition.PossibleEvents.Count == 0)
            {
                TLogger.Warning(BattleSceneConstants.NoEventAvailable, "Battle");
                OpenMap();
                return;
            }

            int index = _randomProvider.Range(0, _runDefinition.PossibleEvents.Count);
            _state.CurrentEvent = _runDefinition.PossibleEvents[index];
            SetCurrentPage(BattleScenePage.Event);
            _state.EventMessage = string.Format(
                BattleSceneConstants.EventStateFormat,
                _state.PlayerHp,
                _state.PlayerMaxHp,
                _state.Gold);
        }

        /// <summary>
        /// 補給画面遷移
        /// </summary>
        private void OpenRestShop()
        {
            SetCurrentPage(BattleScenePage.RestShop);
            _state.IsRestShopContinueEnabled = false;

            if (_state.ShopItems == null || _state.ShopItems.Count == 0)
            {
                _shopService.InitializeShop(_state, _runDefinition, _randomProvider);
            }

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
            SetCurrentPage(BattleScenePage.Result);
            _state.ResultMessage = victory
                ? string.Format(BattleSceneConstants.ResultVictoryFormat, _state.PlayerHp, _state.PlayerMaxHp, _state.Gold)
                : BattleSceneConstants.RunFailedMessage;
                
            _runSaveService?.DeleteSavedRun();
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
        /// 購入済みレリックを所持へ反映する
        /// </summary>
        private void GrantPurchasedRelic(int slotIndex)
        {
            for (int i = 0; i < _state.ShopItems.Count; i++)
            {
                BattleShopItemState item = _state.ShopItems[i];
                if (item == null || item.SlotIndex != slotIndex || item.RewardType != RewardType.Relic || item.Relic == null)
                {
                    continue;
                }

                _relicService.AddOwnedRelic(_state, item.Relic);
                return;
            }
        }

        /// <summary>
        /// 所持レリック選択状態消去
        /// </summary>
        private void ClearOwnedRelicInspection()
        {
            _state.SelectedOwnedRelicIndex = BattleSceneConstants.UnselectedCardIndex;
            _state.OwnedRelicHintMessage = string.Empty;
        }

        /// <summary>
        /// 画面遷移共通処理
        /// </summary>
        private void SetCurrentPage(BattleScenePage page)
        {
            if (_state.CurrentPage != page)
            {
                ClearOwnedRelicInspection();
            }

            _state.CurrentPage = page;
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
        /// 状態をセーブする
        /// </summary>
        private void RequestSave()
        {
            if (_runSaveService == null || _runDefinition == null)
            {
                return;
            }
            
            RunSaveData data = new RunSaveData
            {
                RunProfileId = _runDefinition.RunProfileId,
                PlayerMaxHp = _state.PlayerMaxHp,
                PlayerHp = _state.PlayerHp,
                PlayerEnergy = _state.PlayerEnergy,
                Gold = _state.Gold,
                CurrentNodeIndex = _state.CurrentNodeIndex,
                CurrentPage = (int)ResolveCheckpointPage(_state.CurrentPage),
                DeckCardIds = new List<int>(),
                OwnedRelicIds = new List<int>(),
                ShopItems = new List<SaveShopItem>(),
                IsCardRemovalSoldOut = _state.IsCardRemovalSoldOut,
                CardRemovalCount = _state.CardRemovalCount
            };
            
            for (int i = 0; i < _state.Deck.Count; i++)
            {
                if (_state.Deck[i] != null)
                {
                    data.DeckCardIds.Add(_state.Deck[i].Id);
                }
            }

            for (int i = 0; i < _state.OwnedRelics.Count; i++)
            {
                if (_state.OwnedRelics[i] != null)
                {
                    data.OwnedRelicIds.Add(_state.OwnedRelics[i].Id);
                }
            }

            for (int i = 0; i < _state.ShopItems.Count; i++)
            {
                BattleShopItemState item = _state.ShopItems[i];
                if (item != null)
                {
                    data.ShopItems.Add(new SaveShopItem
                    {
                        SlotIndex = item.SlotIndex,
                        RewardType = (int)item.RewardType,
                        CardId = item.Card != null ? item.Card.Id : 0,
                        ItemId = item.ItemId,
                        Price = item.Price,
                        IsSoldOut = item.IsSoldOut
                    });
                }
            }
            
            _runSaveService.SaveCurrentRunAsync(data).Forget(ex =>
            {
                TLogger.Error($"RunSave request failed: {ex.Message}", "Battle");
            });
        }

        /// <summary>
        /// checkpoint保存用ページ正規化
        /// </summary>
        private static BattleScenePage ResolveCheckpointPage(BattleScenePage currentPage)
        {
            return currentPage switch
            {
                BattleScenePage.Shop => BattleScenePage.RestShop,
                BattleScenePage.CardSelect => BattleScenePage.RestShop,
                _ => currentPage
            };
        }
    }
}
