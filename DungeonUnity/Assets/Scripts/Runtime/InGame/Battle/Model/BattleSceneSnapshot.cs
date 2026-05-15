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
            IReadOnlyList<MapTemplate.Node> nodes,
            IReadOnlyList<CardDefinition> hand,
            IReadOnlyList<CardDefinition> rewardChoices,
            int currentNodeIndex,
            int playerMaxHp,
            int playerHp,
            int playerEnergy,
            int gold,
            EnemyDefinition currentEnemy,
            int enemyHp,
            bool battleFinished,
            int selectedCardIndex,
            bool isRestShopContinueEnabled,
            string mapMessage,
            string battleHintMessage,
            string restShopMessage,
            string resultMessage)
        {
            CurrentPage = currentPage;
            Nodes = nodes;
            Hand = hand;
            RewardChoices = rewardChoices;
            CurrentNodeIndex = currentNodeIndex;
            PlayerMaxHp = playerMaxHp;
            PlayerHp = playerHp;
            PlayerEnergy = playerEnergy;
            Gold = gold;
            CurrentEnemy = currentEnemy;
            EnemyHp = enemyHp;
            BattleFinished = battleFinished;
            SelectedCardIndex = selectedCardIndex;
            IsRestShopContinueEnabled = isRestShopContinueEnabled;
            MapMessage = mapMessage;
            BattleHintMessage = battleHintMessage;
            RestShopMessage = restShopMessage;
            ResultMessage = resultMessage;
        }

        public BattleScenePage CurrentPage { get; }
        public IReadOnlyList<MapTemplate.Node> Nodes { get; }
        public IReadOnlyList<CardDefinition> Hand { get; }
        public IReadOnlyList<CardDefinition> RewardChoices { get; }
        public int CurrentNodeIndex { get; }
        public int PlayerMaxHp { get; }
        public int PlayerHp { get; }
        public int PlayerEnergy { get; }
        public int Gold { get; }
        public EnemyDefinition CurrentEnemy { get; }
        public int EnemyHp { get; }
        public bool BattleFinished { get; }
        public int SelectedCardIndex { get; }
        public bool IsRestShopContinueEnabled { get; }
        public string MapMessage { get; }
        public string BattleHintMessage { get; }
        public string RestShopMessage { get; }
        public string ResultMessage { get; }
    }
}
