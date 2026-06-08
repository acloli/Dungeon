using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// Battle用MasterData問い合わせインターフェース
    /// </summary>
    public interface IBattleMasterDataFacade
    {
        /// <summary>
        /// RunProfileからBattle実行定義を組み立てる
        /// </summary>
        RuntimeRunDefinition BuildRunDefinition(int runProfileId);

        /// <summary>
        /// 全てのカードの定義カタログを構築する
        /// </summary>
        IReadOnlyDictionary<int, RuntimeCard> BuildCardCatalog();
    }
}
