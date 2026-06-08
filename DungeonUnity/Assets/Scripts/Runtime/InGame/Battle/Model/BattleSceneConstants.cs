namespace Dungeon.Runtime.InGame.Battle.Model
{
    internal static class BattleSceneConstants
    {
        public const string MainSceneName = "MainScene";

        public const int DefaultPlayerMaxHp = 40;
        public const int DefaultStartingGold = 100;
        public const int DefaultPlayerEnergy = 3;
        public const int DefaultHandSize = 5;
        public const int DefaultEnemyHp = 20;
        public const int DefaultNodeIndex = -1;
        public const int UnselectedCardIndex = -1;
        public const int DefaultEnemyTargetIndex = 0;
        public const int DefaultRewardChoiceCount = 3;
        public const int RestHealAmount = 12;
        public const int ShopPurchaseCost = 50;
        public const int DefaultStatusDuration = 1;

        public const string DefaultBattleNodeLabel = "B1";
        public const string DefaultRestNodeLabel = "Rest";
        public const string DefaultBattleNodeTwoLabel = "B2";
        public const string DefaultEliteNodeLabel = "Elite";
        public const string DefaultShopNodeLabel = "Shop";
        public const string DefaultBossNodeLabel = "Boss";

        public const string MapNodeLabelFormat = "{0}.{1}";
        public const string CardLabelFormat = "{0} C{1} D{2}";
        public const string RewardLabelFormat = "Select {0} C{1} D{2}";
        public const string MapStateFormat = "HP {0}/{1}  Gold {2}  Next {3}/{4}";
        public const string PlayerStateFormat = "Player HP {0}/{1}  Block {2}  Energy {3}  Gold {4}";
        public const string EnemyStateFormat = "{0} HP {1}  Block {2}";
        public const string EnemyTargetButtonFormat = "{0}[{1}] {2}\nHP {3}  Block {4}";
        public const string EnemyTargetButtonIntentFormat = "{0}\nIntent {1}";
        public const string DefeatedEnemyLabel = "Defeated";
        public const string SelectedEnemyMarker = ">";
        public const string IntentLabelFormat = "Intent: {0}";
        public const string IntentDamageFormat = " D{0}x{1}";
        public const string IntentBlockFormat = " B{0}";
        public const string IntentStatusFormat = " Status {0}:{1}";
        public const string IntentBuffFormat = " Buff {0}:{1}";
        public const string StatusLabel = "Status";
        public const string BuffLabel = "Buff";
        public const string LabelSeparator = ": ";
        public const string ValueSeparator = "  ";
        public const string EmptyValueLabel = "-";
        public const string StatusValueFormat = "{0}:{1}";
        public const string RestShopStateFormat = "Rest Shop: HP {0}/{1}  Gold {2}";
        public const string ResultVictoryFormat = "Run Clear!\nHP {0}/{1}\nGold {2}";
        public const string EnemyTurnFormat = "Enemy turn: {0} damage.";
        public const string DealDamageFormat = "Dealt {0} damage to enemy.";
        public const string GainBlockFormat = "Gained {0} block.";
        public const string CardResolvedFormat = "Used {0}.";
        public const string CardSelectedFormat = "Selected {0}, click enemy target.";
        public const string EnemyTargetSelectedFormat = "Target {0}.";
        public const string SelectCardFirst = "Select a card first.";
        public const string SelectCardAndTarget = "Select a card, then click enemy target.";
        public const string NotEnoughEnergy = "Not enough energy.";
        public const string NextNodeOnly = "You can only go to the next node.";
        public const string RestDoneFormat = "Rest done. HP {0}/{1}";
        public const string UpgradeDone = "Upgrade done (M1 mock).";
        public const string PurchaseSuccessFormat = "Purchase success. Gold {0}";
        public const string NotEnoughGold = "Not enough gold.";
        public const string RunFailed = "Run Failed";
        public const string RunFailedMessage = "Run Failed";
        public const string MissingRunProfile = "RunProfileId is invalid.";
        public const string UnknownEnemyName = "Enemy";
    }
}
