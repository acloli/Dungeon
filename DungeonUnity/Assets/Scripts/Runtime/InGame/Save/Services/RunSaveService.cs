using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Save.Model;
using TFramework.SaveData;

namespace Dungeon.Runtime.InGame.Save.Services
{
    public class RunSaveService : IRunSaveService
    {
        private const string RunSaveKey = "run_save";
        private readonly ISaveDataService _saveDataService;

        public RunSaveService(ISaveDataService saveDataService)
        {
            _saveDataService = saveDataService;
        }

        public UniTask SaveCurrentRunAsync(RunSaveData data, CancellationToken token = default)
        {
            return _saveDataService.SaveAsync(RunSaveKey, data, token);
        }

        public UniTask<RunSaveData> LoadCurrentRunAsync(CancellationToken token = default)
        {
            return _saveDataService.LoadAsync<RunSaveData>(RunSaveKey, null, token);
        }

        public bool HasSavedRun()
        {
            return _saveDataService.Exists(RunSaveKey);
        }

        public void DeleteSavedRun()
        {
            if (_saveDataService.Exists(RunSaveKey))
            {
                _saveDataService.Delete(RunSaveKey);
            }
        }
    }
}
