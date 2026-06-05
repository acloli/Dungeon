using System.Collections.Generic;

namespace Dungeon.Runtime.InGame.Battle.Model
{
    /// <summary>
    /// BattleSceneの進行保持クラス
    /// </summary>
    public sealed class BattleSceneState
    {
        public List<RuntimeCard> Deck { get; } = new List<RuntimeCard>();
        public List<RuntimeCard> Hand { get; } = new List<RuntimeCard>();
        public List<RuntimeCard> RewardChoices { get; } = new List<RuntimeCard>();
        public List<RuntimeMapNode> Nodes { get; } = new List<RuntimeMapNode>();
        public Dictionary<BattleStatusType, int> PlayerStatuses { get; } = new Dictionary<BattleStatusType, int>();
        public Dictionary<BattleStatusType, int> EnemyStatuses { get; } = new Dictionary<BattleStatusType, int>();

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
        public bool IsRestShopContinueEnabled { get; set; }
        public int EnemyTurnCount { get; set; }
        public int EnemyCycleIndex { get; set; }
        public string MapMessage { get; set; } = string.Empty;
        public string BattleHintMessage { get; set; } = string.Empty;
        public string RestShopMessage { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
    }
}
