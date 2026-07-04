using System.Collections.Generic;
using Dungeon.Runtime.InGame.Domain;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Model
{
    /// <summary>
    /// BattleSceneの進行保持クラス
    /// </summary>
    public sealed class BattleSceneState
    {
        #region Card piles

        public List<RuntimeCard> Deck { get; } = new List<RuntimeCard>();
        public List<RuntimeCard> DrawPile { get; } = new List<RuntimeCard>();
        public List<RuntimeCard> DiscardPile { get; } = new List<RuntimeCard>();
        public List<RuntimeCard> ExhaustPile { get; } = new List<RuntimeCard>();
        public List<RuntimeCard> Hand { get; } = new List<RuntimeCard>();

        #endregion

        #region Owned items

        public List<RuntimeRelic> OwnedRelics { get; } = new List<RuntimeRelic>();
        public List<RuntimePotion> OwnedPotions { get; } = new List<RuntimePotion>();
        public int SelectedOwnedRelicIndex { get; set; } = BattleSceneConstants.UnselectedCardIndex;
        public int SelectedOwnedPotionIndex { get; set; } = BattleSceneConstants.UnselectedCardIndex;
        public string OwnedRelicHintMessage { get; set; } = string.Empty;
        public string OwnedPotionHintMessage { get; set; } = string.Empty;

        #endregion

        #region Reward

        public List<RuntimeRewardEntry> RewardChoices { get; } = new List<RuntimeRewardEntry>();
        public int BattleGoldReward { get; set; }
        public bool GoldClaimed { get; set; }
        public bool PotionDropped { get; set; }
        public bool RelicDropped { get; set; }
        public bool PotionClaimed { get; set; }
        public bool RelicClaimed { get; set; }
        public bool CardRewardPicked { get; set; }
        public RuntimeRelic PendingRelicReward { get; set; }
        public RuntimePotion PendingPotionReward { get; set; }
        public PendingPotionOffer PendingPotionOffer { get; set; }

        #endregion

        #region Map

        public List<RuntimeMapNode> Nodes { get; } = new List<RuntimeMapNode>();
        public int CurrentNodeIndex { get; set; } = BattleSceneConstants.DefaultNodeIndex;
        public string MapMessage { get; set; } = string.Empty;

        #endregion

        #region Battle combat

        public Dictionary<StatusType, int> PlayerStatuses { get; } = new Dictionary<StatusType, int>();
        public Dictionary<StatusType, int> EnemyStatuses { get; } = new Dictionary<StatusType, int>();
        public Dictionary<BuffType, int> PlayerBuffs { get; } = new Dictionary<BuffType, int>();
        public Dictionary<BuffType, int> EnemyBuffs { get; } = new Dictionary<BuffType, int>();
        public List<BattleEnemyState> Enemies { get; } = new List<BattleEnemyState>();
        public int PlayerMaxHp { get; set; }
        public int PlayerHp { get; set; }
        public int PlayerEnergy { get; set; }
        public int PlayerBlock { get; set; }
        public RuntimeEnemy CurrentEnemy { get; set; }
        public int EnemyHp { get; set; }
        public int EnemyBlock { get; set; }
        public int EnemyTurnCount { get; set; }
        public int EnemyCycleIndex { get; set; }
        public int SelectedCardIndex { get; set; } = BattleSceneConstants.UnselectedCardIndex;
        public int SelectedEnemyIndex { get; set; } = BattleSceneConstants.DefaultEnemyTargetIndex;
        public bool BattleFinished { get; set; }
        public string BattleHintMessage { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;

        #endregion

        #region Inventory

        public int Gold { get; set; }

        #endregion

        #region RestShop

        public List<BattleShopItemState> ShopItems { get; } = new List<BattleShopItemState>();
        public bool IsCardRemovalSoldOut { get; set; }
        public int CardRemovalCount { get; set; }
        public bool IsRestShopContinueEnabled { get; set; }
        public string RestShopMessage { get; set; } = string.Empty;
        public string CardSelectMessage { get; set; } = string.Empty;
        public CardSelectMode CardSelectMode { get; set; } = CardSelectMode.CardRemoval;

        #endregion

        #region Event

        public RuntimeEvent CurrentEvent { get; set; }
        public string EventMessage { get; set; } = string.Empty;

        #endregion

        #region Page / global

        public BattleScenePage CurrentPage { get; set; } = BattleScenePage.Map;
        public BattlePileType? OpenedPileType { get; set; }

        #endregion

        // ---------------------------------------------------------------
        // Centralized state transitions
        // ---------------------------------------------------------------

        /// <summary>
        /// 選択中敵の旧表示項目を同期する
        /// </summary>
        public void SyncSelectedEnemyDisplay(BattleEnemyState enemyState, int selectedEnemyIndex)
        {
            SelectedEnemyIndex = selectedEnemyIndex;
            if (enemyState == null)
            {
                ClearSelectedEnemyDisplay();
                return;
            }

            CurrentEnemy = enemyState.Enemy;
            EnemyHp = enemyState.Hp;
            EnemyBlock = enemyState.Block;
            EnemyTurnCount = enemyState.TurnCount;
            EnemyCycleIndex = enemyState.CycleIndex;
            CopyDictionary(enemyState.Statuses, EnemyStatuses);
            CopyDictionary(enemyState.Buffs, EnemyBuffs);
        }

        /// <summary>
        /// 選択中敵の旧表示項目をクリアする
        /// </summary>
        public void ClearSelectedEnemyDisplay()
        {
            CurrentEnemy = null;
            EnemyHp = 0;
            EnemyBlock = 0;
            EnemyTurnCount = 0;
            EnemyCycleIndex = 0;
            EnemyStatuses.Clear();
            EnemyBuffs.Clear();
        }

        /// <summary>
        /// 所持レリック選択状態を消去する
        /// </summary>
        public void ClearOwnedRelicInspection()
        {
            SelectedOwnedRelicIndex = BattleSceneConstants.UnselectedCardIndex;
            OwnedRelicHintMessage = string.Empty;
        }

        /// <summary>
        /// 所持ポーション選択状態を消去する
        /// </summary>
        public void ClearOwnedPotionInspection()
        {
            SelectedOwnedPotionIndex = BattleSceneConstants.UnselectedCardIndex;
            OwnedPotionHintMessage = string.Empty;
        }

        /// <summary>
        /// 報酬 pending 状態をクリアする
        /// </summary>
        public void ClearPendingRewards()
        {
            PendingRelicReward = null;
            PendingPotionReward = null;
            PendingPotionOffer = null;
        }

        /// <summary>
        /// 新しい戦闘の開始に備えて戦闘関連の状態をリセットする
        /// </summary>
        public void PrepareForNewBattle()
        {
            BattleFinished = false;
            SelectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
            PlayerEnergy = BattleSceneConstants.DefaultPlayerEnergy;
            PlayerBlock = 0;
            ClearSelectedEnemyDisplay();
            Enemies.Clear();
            SelectedEnemyIndex = BattleSceneConstants.DefaultEnemyTargetIndex;
        }

        /// <summary>
        /// 所持アイテム選択状態を消去する
        /// </summary>
        public void ClearOwnedInspections()
        {
            ClearOwnedRelicInspection();
            ClearOwnedPotionInspection();
        }

        /// <summary>
        /// パイル確認状態を開く
        /// </summary>
        public void OpenPileInspect(BattlePileType pileType)
        {
            OpenedPileType = pileType;
        }

        /// <summary>
        /// パイル確認状態を閉じる
        /// </summary>
        public void ClosePileInspect()
        {
            OpenedPileType = null;
        }

        /// <summary>
        /// 指定インデックスへのノード遷移が可能か判定する
        /// </summary>
        public bool CanMoveToNode(int index)
        {
            if (Nodes == null || index < 0 || index >= Nodes.Count)
            {
                return false;
            }

            if (CurrentNodeIndex < 0)
            {
                return index == 0;
            }

            RuntimeMapNode currentNode = Nodes[CurrentNodeIndex];
            RuntimeMapNode targetNode = Nodes[index];
            if (currentNode == null || targetNode == null)
            {
                return false;
            }

            // 現行MapPageは経路を描画しないため、同一の次フロアに見えているノードはすべて選択可能にする
            return targetNode.Floor == currentNode.Floor + 1;
        }

        /// <summary>
        /// 辞書内容を複写する
        /// </summary>
        private static void CopyDictionary<TKey>(IReadOnlyDictionary<TKey, int> source, IDictionary<TKey, int> destination)
        {
            destination.Clear();
            foreach (KeyValuePair<TKey, int> entry in source)
            {
                destination[entry.Key] = entry.Value;
            }
        }
    }
}
