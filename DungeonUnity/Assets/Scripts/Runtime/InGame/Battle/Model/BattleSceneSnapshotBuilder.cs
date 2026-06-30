namespace Dungeon.Runtime.InGame.Battle.Model
{
    /// <summary>
    /// BattleSceneSnapshot構築補助クラス
    /// </summary>
    public sealed class BattleSceneSnapshotBuilder
    {
        public BattleSceneSnapshotBuilder(BattleScenePage currentPage)
        {
            CurrentPage = currentPage;
        }

        public BattleScenePage CurrentPage { get; set; }
        public BattleHostChromeSnapshot HostChrome { get; set; }
        public BattleMapSnapshot Map { get; set; }
        public BattleCombatSnapshot Combat { get; set; }
        public BattleRewardSnapshot Reward { get; set; }
        public BattleRestShopSnapshot RestShop { get; set; }
        public BattleShopSnapshot Shop { get; set; }
        public BattleEventSnapshot Event { get; set; }
        public BattleResultSnapshot Result { get; set; }
        public BattlePotionReplaceSnapshot PotionReplace { get; set; }
        public BattlePileInspectSnapshot PileInspect { get; set; }

        public BattleSceneSnapshot Build()
        {
            return new BattleSceneSnapshot(
                CurrentPage,
                HostChrome,
                Map,
                Combat,
                Reward,
                RestShop,
                Shop,
                Event,
                Result,
                PotionReplace,
                PileInspect);
        }
    }
}
