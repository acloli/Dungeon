using UnityEngine;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// 乱数用クラス
    /// </summary>
    public sealed class BattleRandomProvider : IBattleRandomProvider
    {
        /// <summary>
        /// 範囲乱数取得
        /// </summary>
        public int Range(int minInclusive, int maxExclusive)
        {
            return Random.Range(minInclusive, maxExclusive);
        }
    }
}
