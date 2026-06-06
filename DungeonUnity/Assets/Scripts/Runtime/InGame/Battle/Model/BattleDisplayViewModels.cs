using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Model
{
    /// <summary>
    /// 敵意図表示用モデル
    /// </summary>
    public sealed class BattleIntentViewModel
    {
        public BattleIntentViewModel(IntentType intentType, string intentName, int damage, int hitCount, int block, StatusType statusType, string statusName, int statusValue, BuffType buffType, string buffName, int buffValue)
        {
            IntentType = intentType;
            IntentName = intentName;
            Damage = damage;
            HitCount = hitCount;
            Block = block;
            StatusType = statusType;
            StatusName = statusName;
            StatusValue = statusValue;
            BuffType = buffType;
            BuffName = buffName;
            BuffValue = buffValue;
        }

        public IntentType IntentType { get; }
        public string IntentName { get; }
        public int Damage { get; }
        public int HitCount { get; }
        public int Block { get; }
        public StatusType StatusType { get; }
        public string StatusName { get; }
        public int StatusValue { get; }
        public BuffType BuffType { get; }
        public string BuffName { get; }
        public int BuffValue { get; }
    }

    /// <summary>
    /// 状態表示用モデル
    /// </summary>
    public sealed class BattleStatusViewModel
    {
        public BattleStatusViewModel(string name, int value, bool isBuff)
        {
            Name = name;
            Value = value;
            IsBuff = isBuff;
        }

        public string Name { get; }
        public int Value { get; }
        public bool IsBuff { get; }
    }
}
