using System.Collections.Generic;

namespace Dungeon.Runtime.OutGame.Main
{
    /// <summary>
    /// MainSceneRunProfile取得インターフェース
    /// </summary>
    public interface IMainRunProfileService
    {
        /// <summary>
        /// RunProfile一覧取得
        /// </summary>
        IReadOnlyList<MainRunProfileViewModel> BuildRunProfiles();
    }
}
