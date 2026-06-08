using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Save.Model
{
    /// <summary>
    /// 探索中の状態を保存するデータモデル
    /// </summary>
    [Serializable]
    public class RunSaveData
    {
        public int RunProfileId;
        public int PlayerMaxHp;
        public int PlayerHp;
        public int PlayerEnergy;
        public int Gold;
        public int CurrentNodeIndex;
        public BattleScenePage CurrentPage;

        // MasterDataのCardIDリスト
        public List<int> DeckCardIds = new List<int>();

        /// <summary>
        /// 有効なデータを持っているかどうかの簡易判定
        /// </summary>
        public bool IsValid => RunProfileId > 0;
    }
}
