using System;
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
        private const int CurrentMapLayoutVersion = 1;
        private const int MapSeedSalt = 0x4D6170;

        private readonly BattleSceneState _state = new BattleSceneState();
        private readonly IBattleSceneRules _rules;
        private readonly IBattleRandomProvider _randomProvider;
        private readonly IBattleMasterDataFacade _masterDataFacade;
        private readonly IBattleMapGenerator _mapGenerator;
        private readonly IBattleRewardFlowService _rewardFlowService;
        private readonly IBattleSnapshotFactory _snapshotFactory;
        private readonly IBattleShopService _shopService;
        private readonly IBattleCombatEventService _combatEventService;
        private readonly IBattleRelicService _relicService;
        private readonly IBattlePotionService _potionService;
        private readonly IBattleEventFlowService _eventFlowService;
        private readonly IBattleRestShopFlowService _restShopFlowService;
        private readonly IRunSaveService _runSaveService;
        private readonly IBattleCheckpointService _checkpointService;
        private readonly IBattleEnemyActionSelector _enemyActionSelector;

        private RuntimeRunDefinition _runDefinition;
        private int _masterSeed;
        private int _mapSeed;

        public BattleSceneFlowService(
            IBattleSceneRules rules,
            IBattleRandomProvider randomProvider,
            IBattleMasterDataFacade masterDataFacade,
            IBattleMapGenerator mapGenerator,
            IBattleRewardFlowService rewardFlowService,
            IBattleSnapshotFactory snapshotFactory,
            IBattleShopService shopService,
            IBattleCombatEventService combatEventService,
            IBattleRelicService relicService,
            IBattlePotionService potionService,
            IBattleEventFlowService eventFlowService,
            IBattleRestShopFlowService restShopFlowService,
            IRunSaveService runSaveService,
            IBattleCheckpointService checkpointService,
            IBattleEnemyActionSelector enemyActionSelector)
        {
            _rules = rules;
            _randomProvider = randomProvider;
            _masterDataFacade = masterDataFacade;
            _mapGenerator = mapGenerator;
            _rewardFlowService = rewardFlowService;
            _snapshotFactory = snapshotFactory;
            _shopService = shopService;
            _combatEventService = combatEventService;
            _relicService = relicService;
            _potionService = potionService;
            _eventFlowService = eventFlowService;
            _restShopFlowService = restShopFlowService;
            _runSaveService = runSaveService;
            _checkpointService = checkpointService;
            _enemyActionSelector = enemyActionSelector;
        }

        /// <summary>
        /// Run初期化
        /// </summary>
        public void Initialize(int runProfileId)
        {
            _masterSeed = GenerateMasterSeed();
            _mapSeed = BuildMapSeed(_masterSeed);
            _randomProvider.Initialize(_masterSeed);
            _runDefinition = _masterDataFacade.BuildRunDefinition(runProfileId);
            _rules.InitializeRun(_state, _runDefinition);
            ApplyInitialMapNodes(_mapSeed);
            _state.SelectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
            _state.ClearOwnedInspections();
            _state.OwnedRelics.Clear();
            _state.OwnedPotions.Clear();
            _state.ClearPendingRewards();
            OpenMap();
            RequestSave();
        }

        /// <summary>
        /// セーブデータからの初期化
        /// </summary>
        public void InitializeFromSave(RunSaveData saveData)
        {
            if (saveData.MapLayoutVersion != CurrentMapLayoutVersion)
            {
                _runSaveService?.DeleteSavedRun();
                Initialize(saveData.RunProfileId);
                return;
            }

            _masterSeed = saveData.MasterSeed;
            _mapSeed = saveData.MapSeed;
            _randomProvider.Restore(_masterSeed, saveData.RandomCounter);
            _runDefinition = _masterDataFacade.BuildRunDefinition(saveData.RunProfileId);
            _rules.InitializeRun(_state, _runDefinition);
            ApplyInitialMapNodes(_mapSeed);
            IReadOnlyDictionary<int, RuntimeCard> cardCatalog = _masterDataFacade.BuildCardCatalog();
            _checkpointService.RestoreFromSave(
                _state,
                _runDefinition,
                saveData,
                cardCatalog,
                _relicService,
                _potionService);

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
        /// 現在のカード選択候補取得
        /// </summary>
        public IReadOnlyList<RuntimeCard> GetCardSelectCards()
        {
            if (_state.CardSelectMode != CardSelectMode.Upgrade)
            {
                return _state.Deck;
            }

            List<RuntimeCard> upgradeableCards = new List<RuntimeCard>();
            for (int i = 0; i < _state.Deck.Count; i++)
            {
                RuntimeCard card = _state.Deck[i];
                if (CanUpgradeCard(card))
                {
                    upgradeableCards.Add(card);
                }
            }

            return upgradeableCards;
        }

        /// <summary>
        /// 現在のカード選択価格取得
        /// </summary>
        public IReadOnlyDictionary<int, int> GetCardSelectPrices()
        {
            Dictionary<int, int> prices = new Dictionary<int, int>();
            if (_state.CardSelectMode != CardSelectMode.Upgrade)
            {
                return prices;
            }

            IReadOnlyList<RuntimeCard> cards = GetCardSelectCards();
            for (int i = 0; i < cards.Count; i++)
            {
                RuntimeCard card = cards[i];
                if (card != null)
                {
                    prices[card.Id] = _shopService.GetCardUpgradePrice(_runDefinition, card);
                }
            }

            return prices;
        }

        /// <summary>
        /// 現在のカード選択強化後カード取得
        /// </summary>
        public IReadOnlyDictionary<int, RuntimeCard> GetCardSelectUpgradedCards()
        {
            Dictionary<int, RuntimeCard> upgradedCards = new Dictionary<int, RuntimeCard>();
            if (_state.CardSelectMode != CardSelectMode.Upgrade)
            {
                return upgradedCards;
            }

            IReadOnlyList<RuntimeCard> cards = GetCardSelectCards();
            for (int i = 0; i < cards.Count; i++)
            {
                RuntimeCard card = cards[i];
                if (card != null && _runDefinition.CardCatalog.TryGetValue(card.UpgradeCardId, out RuntimeCard upgradedCard))
                {
                    upgradedCards[card.Id] = upgradedCard;
                }
            }

            return upgradedCards;
        }

        /// <summary>
        /// 現在のカード選択メッセージ取得
        /// </summary>
        public string GetCardSelectMessage()
        {
            return _state.CardSelectMessage;
        }

        /// <summary>
        /// 現在のカード選択用途取得
        /// </summary>
        public CardSelectMode GetCardSelectMode()
        {
            return _state.CardSelectMode;
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

            if (!_state.CanMoveToNode(index))
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

            _state.SyncSelectedEnemyDisplay(enemyState, index);
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
            _rewardFlowService.SelectReward(_state, rewardEntry);
        }

        /// <summary>
        /// 報酬画面継続処理
        /// </summary>
        public void ContinueFromReward()
        {
            _rewardFlowService.ContinueFromReward(_state, OpenMap);
            RequestSave();
        }

        /// <summary>
        /// 報酬 Gold 取得
        /// </summary>
        public void ClaimGold()
        {
            _rewardFlowService.ClaimGold(_state);
        }

        /// <summary>
        /// ポーション取得
        /// </summary>
        public void ClaimPotion()
        {
            _rewardFlowService.ClaimPotion(_state);
        }

        /// <summary>
        /// レリック取得
        /// </summary>
        public void ClaimRelic()
        {
            _rewardFlowService.ClaimRelic(_state);
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
                _state.ClearOwnedRelicInspection();
                return;
            }

            _state.ClearOwnedPotionInspection();
            _state.SelectedOwnedRelicIndex = index;
            _state.OwnedRelicHintMessage = string.IsNullOrEmpty(relic.Description)
                ? relic.DisplayName
                : string.Format("{0}\n{1}", relic.DisplayName, relic.Description);
        }

        /// <summary>
        /// 所持ポーション説明を表示する
        /// </summary>
        public void InspectOwnedPotion(int index)
        {
            if (index < 0 || index >= _state.OwnedPotions.Count)
            {
                return;
            }

            RuntimePotion potion = _state.OwnedPotions[index];
            if (potion == null)
            {
                return;
            }

            if (_state.SelectedOwnedPotionIndex == index)
            {
                _state.ClearOwnedPotionInspection();
                return;
            }

            _state.ClearOwnedRelicInspection();
            _state.SelectedOwnedPotionIndex = index;
            _state.OwnedPotionHintMessage = string.IsNullOrEmpty(potion.Description)
                ? potion.DisplayName
                : string.Format("{0}\n{1}", potion.DisplayName, potion.Description);
        }

        /// <summary>
        /// ポーションを使用する
        /// </summary>
        public void UsePotion(int index)
        {
            if (index < 0 || index >= _state.OwnedPotions.Count)
            {
                return;
            }

            RuntimePotion potion = _state.OwnedPotions[index];
            BattlePotionUseTarget target = new BattlePotionUseTarget(_state.SelectedEnemyIndex);
            if (_potionService.UsePotion(_state, index, target, _rules, _randomProvider))
            {
                if (potion != null)
                {
                    _state.BattleHintMessage = string.Format(BattleSceneConstants.CardResolvedFormat, potion.DisplayName);
                }

                if (_state.CurrentPage != BattleScenePage.Battle)
                {
                    RequestSave();
                }
            }

            _state.ClearOwnedPotionInspection();
        }

        /// <summary>
        /// 所持ポーションを入れ替える
        /// </summary>
        public void ReplaceOwnedPotion(int index)
        {
            PendingPotionOffer offer = _state.PendingPotionOffer;
            if (offer == null)
            {
                return;
            }

            if (offer.Source == PotionOfferSource.Shop)
            {
                if (!_shopService.PurchaseShopItem(_state, offer.ShopSlotIndex))
                {
                    _state.PendingPotionOffer = null;
                    _state.ClearOwnedPotionInspection();
                    return;
                }
            }

            if (!_potionService.ReplaceOwnedPotion(_state, index, offer))
            {
                return;
            }

            if (offer.Source == PotionOfferSource.Reward)
            {
                _state.PotionClaimed = true;
                _state.PendingPotionReward = null;
            }

            _state.PendingPotionOffer = null;
            _state.ClearOwnedPotionInspection();
            RequestSave();
        }

        /// <summary>
        /// ポーション入れ替え待ちを取り消す
        /// </summary>
        public void CancelPendingPotionReplace()
        {
            _state.PendingPotionOffer = null;
            _state.ClearOwnedPotionInspection();
        }

        /// <summary>
        /// 所持レリック・ポーション選択状態を解除する
        /// </summary>
        public void ClearOwnedInspections()
        {
            _state.ClearOwnedInspections();
        }

        /// <summary>
        /// 休憩適用処理
        /// </summary>
        public void ApplyRest()
        {
            _restShopFlowService.ApplyRest(_state);
        }

        /// <summary>
        /// 強化適用処理
        /// </summary>
        public void ApplyUpgrade()
        {
            _restShopFlowService.ApplyUpgrade(_state, _runDefinition, SetCurrentPage);
        }

        /// <summary>
        /// ショップを開く
        /// </summary>
        public void OpenShop()
        {
            _restShopFlowService.OpenShop(_state, SetCurrentPage);
        }

        /// <summary>
        /// ショップアイテム購入
        /// </summary>
        public void PurchaseShopItem(int slotIndex)
        {
            if (_restShopFlowService.PurchaseShopItem(_state, slotIndex))
            {
                RequestSave();
            }
        }

        /// <summary>
        /// カード削除選択を開く
        /// </summary>
        public void OpenCardRemoval()
        {
            _restShopFlowService.OpenCardRemoval(_state, SetCurrentPage);
        }

        /// <summary>
        /// カード削除購入
        /// </summary>
        public void PurchaseCardRemoval(RuntimeCard card)
        {
            if (_restShopFlowService.PurchaseCardRemoval(_state, card, SetCurrentPage))
            {
                RequestSave();
            }
        }

        /// <summary>
        /// カード選択キャンセル
        /// </summary>
        public void CancelCardSelect()
        {
            _restShopFlowService.CancelCardSelect(_state, SetCurrentPage, OpenRestShop);
        }

        /// <summary>
        /// カード選択確定
        /// </summary>
        public void ConfirmCardSelect(RuntimeCard card)
        {
            if (_restShopFlowService.ConfirmCardSelect(_state, _runDefinition, card, SetCurrentPage))
            {
                RequestSave();
            }
        }

        /// <summary>
        /// ショップから退出
        /// </summary>
        public void LeaveShop()
        {
            _restShopFlowService.LeaveShop(_state, SetCurrentPage);
        }

        /// <summary>
        /// 補給画面継続処理
        /// </summary>
        public void ContinueFromRestShop()
        {
            _restShopFlowService.ContinueFromRestShop(OpenMap);
            RequestSave();
        }

        /// <summary>
        /// イベント選択肢決定処理
        /// </summary>
        public void SelectEventChoice(int choiceId)
        {
            _eventFlowService.SelectEventChoice(_state, choiceId, OpenMap);
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
            _state.PrepareForNewBattle();
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
            _rewardFlowService.PrepareBattleRewards(_state, _runDefinition, CalculateBattleGoldReward());

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
            _rewardFlowService.OpenReward(_state, _runDefinition, SetCurrentPage);
        }

        /// <summary>
        /// イベント画面遷移
        /// </summary>
        private void OpenEvent()
        {
            _eventFlowService.OpenEvent(_state, _runDefinition, SetCurrentPage, OpenMap);
        }

        /// <summary>
        /// 補給画面遷移
        /// </summary>
        private void OpenRestShop()
        {
            _restShopFlowService.OpenRestShop(_state, _runDefinition, SetCurrentPage);
        }

        /// <summary>
        /// カードが強化可能か
        /// </summary>
        private bool CanUpgradeCard(RuntimeCard card)
        {
            return card != null
                   && card.UpgradeCardId > 0
                   && _runDefinition != null
                   && _runDefinition.CardCatalog.ContainsKey(card.UpgradeCardId);
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
        /// 選択敵を旧表示項目へ同期する
        /// </summary>
        private void SyncSelectedEnemyForDisplay()
        {
            BattleEnemyState enemyState = _enemyActionSelector.GetSelectedEnemy(_state);
            if (enemyState == null)
            {
                _state.ClearSelectedEnemyDisplay();
                return;
            }

            _state.SyncSelectedEnemyDisplay(enemyState, _state.SelectedEnemyIndex);
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
        /// パイル確認を開く
        /// </summary>
        public void OpenPileInspect(BattlePileType pileType)
        {
            if (_state.CurrentPage != BattleScenePage.Battle)
            {
                return;
            }

            _state.OpenPileInspect(pileType);
        }

        /// <summary>
        /// パイル確認を閉じる
        /// </summary>
        public void ClosePileInspect()
        {
            _state.ClosePileInspect();
        }

        /// <summary>
        /// 画面遷移共通処理
        /// </summary>
        private void SetCurrentPage(BattleScenePage page)
        {
            if (_state.CurrentPage != page)
            {
                _state.ClearOwnedInspections();
            }

            _state.CurrentPage = page;
        }

        /// <summary>
        /// 状態をセーブする
        /// </summary>
        private void RequestSave()
        {
            if (_runSaveService == null || _runDefinition == null || _checkpointService == null)
            {
                return;
            }

            RunSaveData data = _checkpointService.BuildSaveData(
                _state,
                _runDefinition,
                _masterSeed,
                _mapSeed,
                CurrentMapLayoutVersion,
                _randomProvider.Counter);
            _runSaveService.SaveCurrentRunAsync(data).Forget(ex =>
            {
                TLogger.Error($"RunSave request failed: {ex.Message}", "Battle");
            });
        }

        /// <summary>
        /// 初期マップを状態へ反映する
        /// </summary>
        private void ApplyInitialMapNodes(int mapSeed)
        {
            _state.Nodes.Clear();

            if (TryApplyNodes(_mapGenerator?.Generate(_runDefinition, mapSeed)))
            {
                return;
            }

            if (TryApplyNodes(_runDefinition?.Nodes))
            {
                return;
            }

            _state.Nodes.Add(new RuntimeMapNode(1, "default_01", 1, InGameNodeType.Battle, BattleSceneConstants.DefaultBattleNodeLabel, string.Empty, new[] { 1 }));
            _state.Nodes.Add(new RuntimeMapNode(2, "default_02", 2, InGameNodeType.RestShop, BattleSceneConstants.DefaultRestNodeLabel, string.Empty, new[] { 2 }));
            _state.Nodes.Add(new RuntimeMapNode(3, "default_03", 3, InGameNodeType.Battle, BattleSceneConstants.DefaultBattleNodeTwoLabel, string.Empty, new[] { 3 }));
            _state.Nodes.Add(new RuntimeMapNode(4, "default_04", 4, InGameNodeType.EliteBattle, BattleSceneConstants.DefaultEliteNodeLabel, string.Empty, new[] { 4 }));
            _state.Nodes.Add(new RuntimeMapNode(5, "default_05", 5, InGameNodeType.RestShop, BattleSceneConstants.DefaultShopNodeLabel, string.Empty, new[] { 5 }));
            _state.Nodes.Add(new RuntimeMapNode(6, "default_06", 6, InGameNodeType.Boss, BattleSceneConstants.DefaultBossNodeLabel, string.Empty, Array.Empty<int>()));
        }

        /// <summary>
        /// ノード一覧を状態へ適用する
        /// </summary>
        private bool TryApplyNodes(IReadOnlyList<RuntimeMapNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                RuntimeMapNode node = nodes[i];
                if (node != null)
                {
                    _state.Nodes.Add(node);
                }
            }

            return _state.Nodes.Count > 0;
        }

        /// <summary>
        /// 新規Run用シードを生成する
        /// </summary>
        private static int GenerateMasterSeed()
        {
            int seed = Environment.TickCount;
            return seed == 0 ? 1 : seed;
        }

        /// <summary>
        /// Map用シードを派生する
        /// </summary>
        private static int BuildMapSeed(int masterSeed)
        {
            return HashCode.Combine(masterSeed, MapSeedSalt);
        }
    }
}
