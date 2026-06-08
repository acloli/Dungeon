namespace Dungeon.Runtime.OutGame.Main
{
    /// <summary>
    /// MainSceneRunProfile概要取得インターフェース
    /// </summary>
    public interface IMainRunProfileService
    {
        /// <summary>
        /// RunProfile概要取得
        /// </summary>
        MainRunProfileViewModel BuildRunProfile(int runProfileId);
    }
}
