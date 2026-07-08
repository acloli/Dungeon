namespace Dungeon.Runtime.InGame.Battle.Model
{
    /// <summary>
    /// ポーション使用可能文脈
    /// </summary>
    public enum PotionUseContext
    {
        BattleOnly = 0,
        OutOfBattleOnly = 1,
        Both = 2
    }

    /// <summary>
    /// ポーション対象モード
    /// </summary>
    public enum PotionTargetMode
    {
        None = 0,
        Self = 1,
        Enemy = 2,
        AnyEnemy = 3,
        AllEnemies = 4
    }

    /// <summary>
    /// ポーション使用時の敵対象 index
    /// </summary>
    public readonly struct BattlePotionUseTarget
    {
        public BattlePotionUseTarget(int enemyIndex)
        {
            EnemyIndex = enemyIndex;
        }

        public int EnemyIndex { get; }
    }

    /// <summary>
    /// ポーション提示元
    /// </summary>
    public enum PotionOfferSource
    {
        Reward = 0,
        Shop = 1
    }

    /// <summary>
    /// 入れ替え待ちポーション情報
    /// </summary>
    public sealed class PendingPotionOffer
    {
        public PendingPotionOffer(RuntimePotion potion, PotionOfferSource source, int shopSlotIndex = BattleSceneConstants.UnselectedCardIndex)
        {
            Potion = potion;
            Source = source;
            ShopSlotIndex = shopSlotIndex;
        }

        public RuntimePotion Potion { get; }
        public PotionOfferSource Source { get; }
        public int ShopSlotIndex { get; }
    }

}
