using System;
using System.Collections.Generic;

namespace Dungeon.Runtime.InGame.Save.Model
{
    /// <summary>
    /// 冒険の削除とは独立して保持する図鑑とキャラクターの解放記録
    /// </summary>
    [Serializable]
    public sealed class CollectionProgressData
    {
        public int Version = 1;
        public RunHistoryData LatestRun;
        public List<int> CardIds = new List<int>();
        public List<int> RelicIds = new List<int>();
        public List<int> CharacterIds = new List<int>();
    }

    /// <summary>
    /// 直近の冒険の結果と所持品を保持
    /// </summary>
    [Serializable]
    public sealed class RunHistoryData
    {
        public bool Victory;
        public int ReachedFloor;
        public int EnemyCount;
        public int EliteCount;
        public int BossCount;
        public List<int> CardIds = new List<int>();
        public List<int> RelicIds = new List<int>();
    }
}
