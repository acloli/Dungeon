using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// パイル表示順序解決インターフェース
    /// </summary>
    public interface IBattlePileOrderService
    {
        /// <summary>
        /// パイル種別に応じた表示順を返す
        /// </summary>
        IReadOnlyList<RuntimeCard> Order(BattlePileType pileType, IReadOnlyList<RuntimeCard> cards, BattleSceneState state);
    }
}