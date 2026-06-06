using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Model
{
    /// <summary>
    /// 敵意図表示用モデル
    /// </summary>
    public sealed class BattleIntentViewModel
    {
        public BattleIntentViewModel(
            IntentType intentType,
            int damage,
            int hitCount,
            int block,
            StatusType statusType,
            int statusValue,
            BuffType buffType,
            int buffValue)
        {
            IntentType = intentType;
            Damage = damage;
            HitCount = hitCount;
            Block = block;
            StatusType = statusType;
            StatusValue = statusValue;
            BuffType = buffType;
            BuffValue = buffValue;
        }

        public IntentType IntentType { get; }
        public int Damage { get; }
        public int HitCount { get; }
        public int Block { get; }
        public StatusType StatusType { get; }
        public int StatusValue { get; }
        public BuffType BuffType { get; }
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
