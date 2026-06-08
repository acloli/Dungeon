using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Save.Model;
using TFramework.Debug;
using TFramework.SaveData;

namespace Dungeon.Runtime.InGame.Save.Services
{
    /// <summary>
    /// ゲーム内状態保存サービス
    /// </summary>
    public class RunSaveService : IRunSaveService
    {
        private const string RunSaveKey = "run_save";
        private readonly ISaveDataService _saveDataService;

        public RunSaveService(ISaveDataService saveDataService)
        {
            _saveDataService = saveDataService;
        }

        /// <summary>
        /// 現在のゲーム内状態保存
        /// </summary>
        public async UniTask SaveCurrentRunAsync(RunSaveData data, CancellationToken token = default)
        {
            if (data == null || !data.IsValid)
            {
                TLogger.Warning("RunSaveData is invalid", "RunSave");
                return;
            }

            try
            {
                await _saveDataService.SaveAsync(RunSaveKey, data, token);
            }
            catch (System.OperationCanceledException)
            {
                throw;
            }
            catch (System.Exception ex)
            {
                TLogger.Error($"RunSave save failed: {ex.Message}", "RunSave");
            }
        }

        /// <summary>
        /// 保存済みゲーム内状態読み込み
        /// </summary>
        public async UniTask<RunSaveData> LoadCurrentRunAsync(CancellationToken token = default)
        {
            try
            {
                RunSaveData data = await _saveDataService.LoadAsync<RunSaveData>(RunSaveKey, null, token);
                if (data == null || !data.IsValid)
                {
                    return null;
                }

                return data;
            }
            catch (System.OperationCanceledException)
            {
                throw;
            }
            catch (System.Exception ex)
            {
                TLogger.Error($"RunSave load failed: {ex.Message}", "RunSave");
                return null;
            }
        }

        /// <summary>
        /// 保存済みゲーム内状態存在判定
        /// </summary>
        public bool HasSavedRun()
        {
            try
            {
                return _saveDataService.Exists(RunSaveKey);
            }
            catch (System.Exception ex)
            {
                TLogger.Error($"RunSave exists failed: {ex.Message}", "RunSave");
                return false;
            }
        }

        /// <summary>
        /// 保存済みゲーム内状態削除
        /// </summary>
        public void DeleteSavedRun()
        {
            try
            {
                if (_saveDataService.Exists(RunSaveKey))
                {
                    _saveDataService.Delete(RunSaveKey);
                }
            }
            catch (System.Exception ex)
            {
                TLogger.Error($"RunSave delete failed: {ex.Message}", "RunSave");
            }
        }
    }
}
