using System;
using System.Collections.Generic;

namespace Dungeon.Runtime.InGame.Battle.Model
{
    /// <summary>
    /// BattleScene表示用状態スナップショットクラス
    /// </summary>
    public sealed class BattleSceneSnapshot
    {
        public BattleSceneSnapshot(
            BattleScenePage currentPage,
            IReadOnlyList<RuntimeMapNode> nodes,
            IReadOnlyList<RuntimeCard> hand,
            IReadOnlyList<RuntimeRewardEntry> rewardChoices,
            int currentNodeIndex,
            int playerMaxHp,
            int playerHp,
            int playerEnergy,
            int playerBlock,
            int gold,
            RuntimeEnemy currentEnemy,
            int enemyHp,
            int enemyBlock,
            bool battleFinished,
            int selectedCardIndex,
            bool isRestShopContinueEnabled,
            string mapMessage,
            string battleHintMessage,
            string restShopMessage,
            string resultMessage,
            BattleIntentViewModel enemyIntent = null,
            IReadOnlyList<BattleHandCardViewModel> handCards = null,
            IReadOnlyList<BattleStatusViewModel> playerStatuses = null,
            IReadOnlyList<BattleStatusViewModel> enemyStatuses = null,
            IReadOnlyList<BattleStatusViewModel> playerBuffs = null,
            IReadOnlyList<BattleStatusViewModel> enemyBuffs = null,
            IReadOnlyList<BattleEnemyViewModel> enemies = null,
            IReadOnlyList<BattleMultiIconViewModel> ownedRelics = null,
            int selectedEnemyIndex = BattleSceneConstants.DefaultEnemyTargetIndex,
            int selectedOwnedRelicIndex = BattleSceneConstants.UnselectedCardIndex,
            int drawPileCount = 0,
            int discardPileCount = 0,
            int handCount = 0,
            int maxHandCount = 0,
            IReadOnlyList<int> availableNodeIndices = null,
            IReadOnlyList<BattleShopItemViewModel> shopItems = null,
            bool isCardRemovalSoldOut = false,
            int cardRemovalPrice = 0,
            RuntimeEvent currentEvent = null,
            RuntimeRelic pendingRelicReward = null,
            string eventMessage = null,
            bool goldClaimed = false,
            bool potionClaimed = false,
            bool relicClaimed = false,
            int battleGoldReward = 0,
            bool potionDropped = false,
            bool relicDropped = false,
            bool cardRewardPicked = false,
            string ownedRelicHintMessage = null)
        {
            CurrentPage = currentPage;
            Nodes = nodes;
            Hand = hand;
            RewardChoices = rewardChoices;
            CurrentNodeIndex = currentNodeIndex;
            PlayerMaxHp = playerMaxHp;
            PlayerHp = playerHp;
            PlayerEnergy = playerEnergy;
            PlayerBlock = playerBlock;
            Gold = gold;
            CurrentEnemy = currentEnemy;
            EnemyHp = enemyHp;
            EnemyBlock = enemyBlock;
            BattleFinished = battleFinished;
            SelectedCardIndex = selectedCardIndex;
            IsRestShopContinueEnabled = isRestShopContinueEnabled;
            MapMessage = mapMessage;
            BattleHintMessage = battleHintMessage;
            RestShopMessage = restShopMessage;
            ResultMessage = resultMessage;
            EnemyIntent = enemyIntent;
            HandCards = handCards ?? Array.Empty<BattleHandCardViewModel>();
            PlayerStatuses = playerStatuses ?? Array.Empty<BattleStatusViewModel>();
            EnemyStatuses = enemyStatuses ?? Array.Empty<BattleStatusViewModel>();
            PlayerBuffs = playerBuffs ?? Array.Empty<BattleStatusViewModel>();
            EnemyBuffs = enemyBuffs ?? Array.Empty<BattleStatusViewModel>();
            Enemies = enemies ?? Array.Empty<BattleEnemyViewModel>();
            OwnedRelics = ownedRelics ?? Array.Empty<BattleMultiIconViewModel>();
            SelectedEnemyIndex = selectedEnemyIndex;
            SelectedOwnedRelicIndex = selectedOwnedRelicIndex;
            DrawPileCount = drawPileCount;
            DiscardPileCount = discardPileCount;
            HandCount = handCount;
            MaxHandCount = maxHandCount;
            AvailableNodeIndices = availableNodeIndices ?? Array.Empty<int>();
            ShopItems = shopItems ?? Array.Empty<BattleShopItemViewModel>();
            IsCardRemovalSoldOut = isCardRemovalSoldOut;
            CardRemovalPrice = cardRemovalPrice;
            CurrentEvent = currentEvent;
            PendingRelicReward = pendingRelicReward;
            GoldClaimed = goldClaimed;
            PotionClaimed = potionClaimed;
            RelicClaimed = relicClaimed;
            EventMessage = eventMessage ?? string.Empty;
            BattleGoldReward = battleGoldReward;
            PotionDropped = potionDropped;
            RelicDropped = relicDropped;
            CardRewardPicked = cardRewardPicked;
            OwnedRelicHintMessage = ownedRelicHintMessage ?? string.Empty;
        }

        public BattleScenePage CurrentPage { get; }
        public IReadOnlyList<RuntimeMapNode> Nodes { get; }
        public IReadOnlyList<RuntimeCard> Hand { get; }
        public IReadOnlyList<RuntimeRewardEntry> RewardChoices { get; }
        public int CurrentNodeIndex { get; }
        public int PlayerMaxHp { get; }
        public int PlayerHp { get; }
        public int PlayerEnergy { get; }
        public int PlayerBlock { get; }
        public int Gold { get; }
        public RuntimeEnemy CurrentEnemy { get; }
        public int EnemyHp { get; }
        public int EnemyBlock { get; }
        public bool BattleFinished { get; }
        public int SelectedCardIndex { get; }
        public bool IsRestShopContinueEnabled { get; }
        public string MapMessage { get; }
        public string BattleHintMessage { get; }
        public string RestShopMessage { get; }
        public string ResultMessage { get; }
        public BattleIntentViewModel EnemyIntent { get; }
        public IReadOnlyList<BattleHandCardViewModel> HandCards { get; }
        public IReadOnlyList<BattleStatusViewModel> PlayerStatuses { get; }
        public IReadOnlyList<BattleStatusViewModel> EnemyStatuses { get; }
        public IReadOnlyList<BattleStatusViewModel> PlayerBuffs { get; }
        public IReadOnlyList<BattleStatusViewModel> EnemyBuffs { get; }
        public IReadOnlyList<BattleEnemyViewModel> Enemies { get; }
        public IReadOnlyList<BattleMultiIconViewModel> OwnedRelics { get; }
        public int SelectedEnemyIndex { get; }
        public int SelectedOwnedRelicIndex { get; }
        public int DrawPileCount { get; }
        public int DiscardPileCount { get; }
        public int HandCount { get; }
        public int MaxHandCount { get; }
        public IReadOnlyList<int> AvailableNodeIndices { get; }
        public IReadOnlyList<BattleShopItemViewModel> ShopItems { get; }
        public bool IsCardRemovalSoldOut { get; }
        public int CardRemovalPrice { get; }
        public RuntimeEvent CurrentEvent { get; }
        public RuntimeRelic PendingRelicReward { get; }
        public bool GoldClaimed { get; }
        public bool PotionClaimed { get; }
        public bool RelicClaimed { get; }
        public int BattleGoldReward { get; }
        public bool PotionDropped { get; }
        public bool RelicDropped { get; }
        public bool CardRewardPicked { get; }
        public string EventMessage { get; }
        public string OwnedRelicHintMessage { get; }
    }
}
