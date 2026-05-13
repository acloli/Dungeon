namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// Battle用乱数提供インターフェース
    /// </summary>
    public interface IBattleRandomProvider
    {
        /// <summary>
        /// 範囲乱数取得
        /// </summary>
        int Range(int minInclusive, int maxExclusive);
    }
}
