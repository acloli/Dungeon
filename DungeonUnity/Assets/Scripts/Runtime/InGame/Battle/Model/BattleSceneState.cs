using System.Collections.Generic;
using Dungeon.Runtime.InGame.Domain;

namespace Dungeon.Runtime.InGame.Battle.Model
{
    /// <summary>
    /// BattleSceneの進行保持クラス
    /// </summary>
    public sealed class BattleSceneState
    {
        public List<CardDefinition> Deck { get; } = new List<CardDefinition>();
        public List<CardDefinition> Hand { get; } = new List<CardDefinition>();
        public List<CardDefinition> RewardChoices { get; } = new List<CardDefinition>();
        public List<MapTemplate.Node> Nodes { get; } = new List<MapTemplate.Node>();

        public BattleScenePage CurrentPage { get; set; } = BattleScenePage.Map;
        public int CurrentNodeIndex { get; set; } = BattleSceneConstants.DefaultNodeIndex;
        public int PlayerMaxHp { get; set; }
        public int PlayerHp { get; set; }
        public int PlayerEnergy { get; set; }
        public int Gold { get; set; }
        public EnemyDefinition CurrentEnemy { get; set; }
        public int EnemyHp { get; set; }
        public bool BattleFinished { get; set; }
        public int SelectedCardIndex { get; set; } = BattleSceneConstants.UnselectedCardIndex;
        public bool IsRestShopContinueEnabled { get; set; }
        public string MapMessage { get; set; } = string.Empty;
        public string BattleHintMessage { get; set; } = string.Empty;
        public string RestShopMessage { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
    }
}
