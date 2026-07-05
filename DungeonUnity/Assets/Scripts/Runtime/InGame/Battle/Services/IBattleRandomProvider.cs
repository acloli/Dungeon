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

        /// <summary>
        /// 現在のシード値
        /// </summary>
        int Seed { get; }

        /// <summary>
        /// 現在の消費回数
        /// </summary>
        int Counter { get; }

        /// <summary>
        /// シードで初期化する
        /// </summary>
        void Initialize(int seed);

        /// <summary>
        /// シードと消費回数から復元する
        /// </summary>
        void Restore(int seed, int counter);
    }
}
