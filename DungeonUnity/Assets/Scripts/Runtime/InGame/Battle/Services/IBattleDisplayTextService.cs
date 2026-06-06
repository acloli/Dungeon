using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// Battle表示名解決インターフェース
    /// </summary>
    public interface IBattleDisplayTextService
    {
        string GetIntentName(IntentType intentType);
        string GetStatusName(StatusType statusType);
        string GetBuffName(BuffType buffType);
    }
}
