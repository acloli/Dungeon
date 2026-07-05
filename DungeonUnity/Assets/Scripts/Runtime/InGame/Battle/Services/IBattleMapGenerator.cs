using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// Battle用マップ生成インターフェース
    /// </summary>
    public interface IBattleMapGenerator
    {
        /// <summary>
        /// Run定義とシードからマップを生成する
        /// </summary>
        IReadOnlyList<RuntimeMapNode> Generate(RuntimeRunDefinition runDefinition, int mapSeed);
    }
}
