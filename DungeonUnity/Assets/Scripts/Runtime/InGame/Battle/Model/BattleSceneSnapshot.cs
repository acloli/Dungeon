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
            IReadOnlyList<RuntimeCard> rewardChoices,
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
            IReadOnlyList<BattleStatusViewModel> playerStatuses = null,
            IReadOnlyList<BattleStatusViewModel> enemyStatuses = null,
            IReadOnlyList<BattleStatusViewModel> playerBuffs = null,
            IReadOnlyList<BattleStatusViewModel> enemyBuffs = null,
            IReadOnlyList<BattleEnemyViewModel> enemies = null,
            int selectedEnemyIndex = BattleSceneConstants.DefaultEnemyTargetIndex)
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
            PlayerStatuses = playerStatuses ?? Array.Empty<BattleStatusViewModel>();
            EnemyStatuses = enemyStatuses ?? Array.Empty<BattleStatusViewModel>();
            PlayerBuffs = playerBuffs ?? Array.Empty<BattleStatusViewModel>();
            EnemyBuffs = enemyBuffs ?? Array.Empty<BattleStatusViewModel>();
            Enemies = enemies ?? Array.Empty<BattleEnemyViewModel>();
            SelectedEnemyIndex = selectedEnemyIndex;
        }

        public BattleScenePage CurrentPage { get; }
        public IReadOnlyList<RuntimeMapNode> Nodes { get; }
        public IReadOnlyList<RuntimeCard> Hand { get; }
        public IReadOnlyList<RuntimeCard> RewardChoices { get; }
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
        public IReadOnlyList<BattleStatusViewModel> PlayerStatuses { get; }
        public IReadOnlyList<BattleStatusViewModel> EnemyStatuses { get; }
        public IReadOnlyList<BattleStatusViewModel> PlayerBuffs { get; }
        public IReadOnlyList<BattleStatusViewModel> EnemyBuffs { get; }
        public IReadOnlyList<BattleEnemyViewModel> Enemies { get; }
        public int SelectedEnemyIndex { get; }
    }
}
