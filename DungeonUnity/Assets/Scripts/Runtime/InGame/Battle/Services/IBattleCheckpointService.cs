using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Save.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneのcheckpoint保存と復元を扱うインターフェース
    /// </summary>
    public interface IBattleCheckpointService
    {
        /// <summary>
        /// セーブデータからBattleScene状態を復元する
        /// </summary>
        void RestoreFromSave(BattleSceneState state, RuntimeRunDefinition runDefinition, RunSaveData saveData, IReadOnlyDictionary<int, RuntimeCard> cardCatalog, IBattleRelicService relicService, IBattlePotionService potionService);

        /// <summary>
        /// 現在状態からcheckpoint保存データを構築する
        /// </summary>
        RunSaveData BuildSaveData(BattleSceneState state, RuntimeRunDefinition runDefinition, int masterSeed, int mapSeed, int mapLayoutVersion, int randomCounter);
    }
}
