using System;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// 乱数用クラス
    /// </summary>
    public sealed class BattleRandomProvider : IBattleRandomProvider
    {
        private Random _random = new Random(1);

        public int Seed { get; private set; }

        public int Counter { get; private set; }

        /// <summary>
        /// シードで初期化する
        /// </summary>
        public void Initialize(int seed)
        {
            Seed = seed;
            Counter = 0;
            _random = new Random(seed);
        }

        /// <summary>
        /// シードと消費回数から復元する
        /// </summary>
        public void Restore(int seed, int counter)
        {
            Initialize(seed);
            for (int i = 0; i < counter; i++)
            {
                _random.Next();
            }

            Counter = counter;
        }

        /// <summary>
        /// 範囲乱数取得
        /// </summary>
        public int Range(int minInclusive, int maxExclusive)
        {
            Counter++;
            return _random.Next(minInclusive, maxExclusive);
        }
    }
}
