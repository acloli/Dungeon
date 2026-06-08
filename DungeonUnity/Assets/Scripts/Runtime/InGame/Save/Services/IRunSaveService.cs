using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Save.Model;

namespace Dungeon.Runtime.InGame.Save.Services
{
    /// <summary>
    /// 探索中のゲーム状態（RunSaveData）の保存・読み込みを管理するサービス
    /// </summary>
    public interface IRunSaveService
    {
        /// <summary>
        /// 現在のゲーム内状態を保存する
        /// </summary>
        UniTask SaveCurrentRunAsync(RunSaveData data, CancellationToken token = default);

        /// <summary>
        /// 保存されたゲーム内状態を読み込む
        /// </summary>
        UniTask<RunSaveData> LoadCurrentRunAsync(CancellationToken token = default);

        /// <summary>
        /// セーブデータが存在するかどうかを判定する
        /// </summary>
        bool HasSavedRun();

        /// <summary>
        /// セーブデータを削除する（ゲームオーバー時やクリア時）
        /// </summary>
        void DeleteSavedRun();
    }
}
