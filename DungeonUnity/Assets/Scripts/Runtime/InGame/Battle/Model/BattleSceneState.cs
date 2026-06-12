using System.Collections.Generic;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Model
{
    /// <summary>
    /// BattleSceneの進行保持クラス
    /// </summary>
    public sealed class BattleSceneState
    {
        public List<RuntimeCard> Deck { get; } = new List<RuntimeCard>();
        public List<RuntimeCard> DrawPile { get; } = new List<RuntimeCard>();
        public List<RuntimeCard> DiscardPile { get; } = new List<RuntimeCard>();
        public List<RuntimeCard> ExhaustPile { get; } = new List<RuntimeCard>();
        public List<RuntimeCard> Hand { get; } = new List<RuntimeCard>();
        public List<RuntimeRewardEntry> RewardChoices { get; } = new List<RuntimeRewardEntry>();
        public List<RuntimeMapNode> Nodes { get; } = new List<RuntimeMapNode>();
        public List<BattleEnemyState> Enemies { get; } = new List<BattleEnemyState>();
        public List<BattleShopItemState> ShopItems { get; } = new List<BattleShopItemState>();
        public bool IsCardRemovalSoldOut { get; set; }
        public int CardRemovalCount { get; set; }
        public int BattleGoldReward { get; set; }
        public bool PotionDropped { get; set; }
        public bool RelicDropped { get; set; }
        public bool CardRewardPicked { get; set; }
        public bool GoldClaimed { get; set; }
        public bool PotionClaimed { get; set; }
        public bool RelicClaimed { get; set; }
        public Dictionary<StatusType, int> PlayerStatuses { get; } = new Dictionary<StatusType, int>();
        public Dictionary<StatusType, int> EnemyStatuses { get; } = new Dictionary<StatusType, int>();
        public Dictionary<BuffType, int> PlayerBuffs { get; } = new Dictionary<BuffType, int>();
        public Dictionary<BuffType, int> EnemyBuffs { get; } = new Dictionary<BuffType, int>();

        public BattleScenePage CurrentPage { get; set; } = BattleScenePage.Map;
        public int CurrentNodeIndex { get; set; } = BattleSceneConstants.DefaultNodeIndex;
        public int PlayerMaxHp { get; set; }
        public int PlayerHp { get; set; }
        public int PlayerEnergy { get; set; }
        public int PlayerBlock { get; set; }
        public int Gold { get; set; }
        public RuntimeEnemy CurrentEnemy { get; set; }
        public int EnemyHp { get; set; }
        public int EnemyBlock { get; set; }
        public bool BattleFinished { get; set; }
        public int SelectedCardIndex { get; set; } = BattleSceneConstants.UnselectedCardIndex;
        public int SelectedEnemyIndex { get; set; } = BattleSceneConstants.DefaultEnemyTargetIndex;
        public bool IsRestShopContinueEnabled { get; set; }
        public int EnemyTurnCount { get; set; }
        public int EnemyCycleIndex { get; set; }
        public string MapMessage { get; set; } = string.Empty;
        public string BattleHintMessage { get; set; } = string.Empty;
        public string RestShopMessage { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
        public RuntimeEvent CurrentEvent { get; set; }
        public string EventMessage { get; set; } = string.Empty;
    }
}
