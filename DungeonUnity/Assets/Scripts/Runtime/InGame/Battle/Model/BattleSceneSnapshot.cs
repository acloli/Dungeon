using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Domain;

namespace Dungeon.Runtime.InGame.Battle.Model
{
    /// <summary>
    /// BattleScene表示用状態スナップショットクラス
    /// </summary>
    public sealed class BattleSceneSnapshot
    {
        public BattleSceneSnapshot(
            BattleScenePage currentPage,
            BattleHostChromeSnapshot hostChrome = null,
            BattleMapSnapshot map = null,
            BattleCombatSnapshot combat = null,
            BattleRewardSnapshot reward = null,
            BattleRestShopSnapshot restShop = null,
            BattleShopSnapshot shop = null,
            BattleEventSnapshot battleEvent = null,
            BattleResultSnapshot result = null,
            BattlePotionReplaceSnapshot potionReplace = null,
            BattlePileInspectSnapshot pileInspect = null)
        {
            CurrentPage = currentPage;
            HostChrome = hostChrome ?? new BattleHostChromeSnapshot();
            Map = map ?? new BattleMapSnapshot();
            Combat = combat ?? new BattleCombatSnapshot();
            Reward = reward ?? new BattleRewardSnapshot();
            RestShop = restShop ?? new BattleRestShopSnapshot();
            Shop = shop ?? new BattleShopSnapshot();
            Event = battleEvent ?? new BattleEventSnapshot();
            Result = result ?? new BattleResultSnapshot();
            PotionReplace = potionReplace ?? new BattlePotionReplaceSnapshot();
            PileInspect = pileInspect ?? new BattlePileInspectSnapshot();
        }

        public BattleScenePage CurrentPage { get; }
        public BattleHostChromeSnapshot HostChrome { get; }
        public BattleMapSnapshot Map { get; }
        public BattleCombatSnapshot Combat { get; }
        public BattleRewardSnapshot Reward { get; }
        public BattleRestShopSnapshot RestShop { get; }
        public BattleShopSnapshot Shop { get; }
        public BattleEventSnapshot Event { get; }
        public BattleResultSnapshot Result { get; }
        public BattlePotionReplaceSnapshot PotionReplace { get; }
        public BattlePileInspectSnapshot PileInspect { get; }
    }

    public sealed class BattleHostChromeSnapshot
    {
        public BattleHostChromeSnapshot(
            IReadOnlyList<BattleMultiIconViewModel> ownedRelics = null,
            IReadOnlyList<BattleMultiIconViewModel> ownedPotions = null,
            int selectedOwnedRelicIndex = BattleSceneConstants.UnselectedCardIndex,
            int selectedOwnedPotionIndex = BattleSceneConstants.UnselectedCardIndex,
            string ownedRelicHintMessage = null,
            string ownedPotionHintMessage = null,
            bool canUseSelectedPotion = false)
        {
            OwnedRelics = ownedRelics ?? Array.Empty<BattleMultiIconViewModel>();
            OwnedPotions = ownedPotions ?? Array.Empty<BattleMultiIconViewModel>();
            SelectedOwnedRelicIndex = selectedOwnedRelicIndex;
            SelectedOwnedPotionIndex = selectedOwnedPotionIndex;
            OwnedRelicHintMessage = ownedRelicHintMessage ?? string.Empty;
            OwnedPotionHintMessage = ownedPotionHintMessage ?? string.Empty;
            CanUseSelectedPotion = canUseSelectedPotion;
        }

        public IReadOnlyList<BattleMultiIconViewModel> OwnedRelics { get; }
        public IReadOnlyList<BattleMultiIconViewModel> OwnedPotions { get; }
        public int SelectedOwnedRelicIndex { get; }
        public int SelectedOwnedPotionIndex { get; }
        public string OwnedRelicHintMessage { get; }
        public string OwnedPotionHintMessage { get; }
        public bool CanUseSelectedPotion { get; }
    }

    public sealed class BattleMapSnapshot
    {
        public BattleMapSnapshot(
            IReadOnlyList<RuntimeMapNode> nodes = null,
            IReadOnlyList<int> availableNodeIndices = null,
            string mapMessage = null)
        {
            Nodes = nodes ?? Array.Empty<RuntimeMapNode>();
            AvailableNodeIndices = availableNodeIndices ?? Array.Empty<int>();
            MapMessage = mapMessage ?? string.Empty;
        }

        public IReadOnlyList<RuntimeMapNode> Nodes { get; }
        public IReadOnlyList<int> AvailableNodeIndices { get; }
        public string MapMessage { get; }
    }

    public sealed class BattleCombatSnapshot
    {
        public BattleCombatSnapshot(
            int playerMaxHp = 40,
            int playerHp = 40,
            int playerEnergy = 3,
            int playerBlock = 0,
            int gold = 100,
            string battleHintMessage = null,
            IReadOnlyList<BattleHandCardViewModel> handCards = null,
            IReadOnlyList<BattleEnemyViewModel> enemies = null,
            int selectedEnemyIndex = BattleSceneConstants.DefaultEnemyTargetIndex,
            BattleIntentViewModel enemyIntent = null,
            IReadOnlyList<BattleStatusViewModel> playerStatuses = null,
            IReadOnlyList<BattleStatusViewModel> enemyStatuses = null,
            IReadOnlyList<BattleStatusViewModel> playerBuffs = null,
            IReadOnlyList<BattleStatusViewModel> enemyBuffs = null,
            int drawPileCount = 0,
            int discardPileCount = 0,
            int exhaustPileCount = 0,
            int handCount = 0,
            int maxHandCount = 0,
            RuntimeEnemy currentEnemy = null,
            int enemyHp = 0,
            int enemyBlock = 0)
        {
            PlayerMaxHp = playerMaxHp;
            PlayerHp = playerHp;
            PlayerEnergy = playerEnergy;
            PlayerBlock = playerBlock;
            Gold = gold;
            BattleHintMessage = battleHintMessage ?? string.Empty;
            HandCards = handCards ?? Array.Empty<BattleHandCardViewModel>();
            Enemies = enemies ?? Array.Empty<BattleEnemyViewModel>();
            SelectedEnemyIndex = selectedEnemyIndex;
            EnemyIntent = enemyIntent;
            PlayerStatuses = playerStatuses ?? Array.Empty<BattleStatusViewModel>();
            EnemyStatuses = enemyStatuses ?? Array.Empty<BattleStatusViewModel>();
            PlayerBuffs = playerBuffs ?? Array.Empty<BattleStatusViewModel>();
            EnemyBuffs = enemyBuffs ?? Array.Empty<BattleStatusViewModel>();
            DrawPileCount = drawPileCount;
            DiscardPileCount = discardPileCount;
            ExhaustPileCount = exhaustPileCount;
            HandCount = handCount;
            MaxHandCount = maxHandCount;
            CurrentEnemy = currentEnemy;
            EnemyHp = enemyHp;
            EnemyBlock = enemyBlock;
        }

        public int PlayerMaxHp { get; }
        public int PlayerHp { get; }
        public int PlayerEnergy { get; }
        public int PlayerBlock { get; }
        public int Gold { get; }
        public string BattleHintMessage { get; }
        public IReadOnlyList<BattleHandCardViewModel> HandCards { get; }
        public IReadOnlyList<BattleEnemyViewModel> Enemies { get; }
        public int SelectedEnemyIndex { get; }
        public BattleIntentViewModel EnemyIntent { get; }
        public IReadOnlyList<BattleStatusViewModel> PlayerStatuses { get; }
        public IReadOnlyList<BattleStatusViewModel> EnemyStatuses { get; }
        public IReadOnlyList<BattleStatusViewModel> PlayerBuffs { get; }
        public IReadOnlyList<BattleStatusViewModel> EnemyBuffs { get; }
        public int DrawPileCount { get; }
        public int DiscardPileCount { get; }
        public int ExhaustPileCount { get; }
        public int HandCount { get; }
        public int MaxHandCount { get; }
        public RuntimeEnemy CurrentEnemy { get; }
        public int EnemyHp { get; }
        public int EnemyBlock { get; }
    }

    public sealed class BattleRewardSnapshot
    {
        public BattleRewardSnapshot(
            IReadOnlyList<RuntimeRewardEntry> rewardChoices = null,
            bool goldClaimed = false,
            bool potionClaimed = false,
            bool relicClaimed = false,
            bool cardRewardPicked = false,
            int battleGoldReward = 0,
            bool potionDropped = false,
            bool relicDropped = false,
            RuntimePotion pendingPotionReward = null,
            RuntimeRelic pendingRelicReward = null)
        {
            RewardChoices = rewardChoices ?? Array.Empty<RuntimeRewardEntry>();
            GoldClaimed = goldClaimed;
            PotionClaimed = potionClaimed;
            RelicClaimed = relicClaimed;
            CardRewardPicked = cardRewardPicked;
            BattleGoldReward = battleGoldReward;
            PotionDropped = potionDropped;
            RelicDropped = relicDropped;
            PendingPotionReward = pendingPotionReward;
            PendingRelicReward = pendingRelicReward;
        }

        public IReadOnlyList<RuntimeRewardEntry> RewardChoices { get; }
        public bool GoldClaimed { get; }
        public bool PotionClaimed { get; }
        public bool RelicClaimed { get; }
        public bool CardRewardPicked { get; }
        public int BattleGoldReward { get; }
        public bool PotionDropped { get; }
        public bool RelicDropped { get; }
        public RuntimePotion PendingPotionReward { get; }
        public RuntimeRelic PendingRelicReward { get; }
    }

    public sealed class BattleRestShopSnapshot
    {
        public BattleRestShopSnapshot(
            string restShopMessage = null,
            bool isRestShopContinueEnabled = false)
        {
            RestShopMessage = restShopMessage ?? string.Empty;
            IsRestShopContinueEnabled = isRestShopContinueEnabled;
        }

        public string RestShopMessage { get; }
        public bool IsRestShopContinueEnabled { get; }
    }

    public sealed class BattleShopSnapshot
    {
        public BattleShopSnapshot(
            int gold = 100,
            IReadOnlyList<BattleShopItemViewModel> shopItems = null,
            bool isCardRemovalSoldOut = false,
            int cardRemovalPrice = 0)
        {
            Gold = gold;
            ShopItems = shopItems ?? Array.Empty<BattleShopItemViewModel>();
            IsCardRemovalSoldOut = isCardRemovalSoldOut;
            CardRemovalPrice = cardRemovalPrice;
        }

        public int Gold { get; }
        public IReadOnlyList<BattleShopItemViewModel> ShopItems { get; }
        public bool IsCardRemovalSoldOut { get; }
        public int CardRemovalPrice { get; }
    }

    public sealed class BattleEventSnapshot
    {
        public BattleEventSnapshot(RuntimeEvent currentEvent = null)
        {
            CurrentEvent = currentEvent;
        }

        public RuntimeEvent CurrentEvent { get; }
    }

    public sealed class BattleResultSnapshot
    {
        public BattleResultSnapshot(string resultMessage = null)
        {
            ResultMessage = resultMessage ?? string.Empty;
        }

        public string ResultMessage { get; }
    }

    public sealed class BattlePotionReplaceSnapshot
    {
        public BattlePotionReplaceSnapshot(
            IReadOnlyList<BattleMultiIconViewModel> ownedPotions = null,
            PendingPotionOffer pendingPotionOffer = null)
        {
            OwnedPotions = ownedPotions ?? Array.Empty<BattleMultiIconViewModel>();
            PendingPotionOffer = pendingPotionOffer;
        }

        public IReadOnlyList<BattleMultiIconViewModel> OwnedPotions { get; }
        public PendingPotionOffer PendingPotionOffer { get; }
    }

    /// <summary>
    /// パイル確認用スナップショットクラス
    /// </summary>
    public sealed class BattlePileInspectSnapshot
    {
        public BattlePileInspectSnapshot(
            BattlePileType pileType = BattlePileType.Draw,
            string title = null,
            IReadOnlyList<BattleMultiIconViewModel> cards = null,
            bool isOpen = false)
        {
            PileType = pileType;
            Title = title ?? string.Empty;
            Cards = cards ?? Array.Empty<BattleMultiIconViewModel>();
            IsOpen = isOpen;
        }

        public BattlePileType PileType { get; }
        public string Title { get; }
        public IReadOnlyList<BattleMultiIconViewModel> Cards { get; }
        public bool IsOpen { get; }
    }
}
