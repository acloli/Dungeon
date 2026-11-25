using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Save.Services;
using Dungeon.Runtime.SceneFlow;
using TFramework.Scene;

namespace Dungeon.Runtime.OutGame.Main
{
    /// <summary>
    /// 選択キャラクターと保存済み冒険を検証してシーンへ引き渡す
    /// </summary>
    public sealed class MainMenuActionHandler : IMainMenuActionHandler
    {
        private readonly ISceneService _scenes;
        private readonly IMainMenuDataSource _data;
        private readonly IRunSaveService _run;

        public MainMenuActionHandler(ISceneService scenes, IMainMenuDataSource data, IRunSaveService run)
        {
            _scenes = scenes;
            _data = data;
            _run = run;
        }

        public async UniTask<bool> ExecuteAsync(MainMenuAction action, CancellationToken ct,
            MainMenuCharacterData character = null)
        {
            ct.ThrowIfCancellationRequested();
            BattleRunBridgeData bridge;
            if (action == MainMenuAction.StartAdventure)
            {
                var data = await _data.LoadAsync(ct);
                var selected = data.Characters.FirstOrDefault(entry => entry.Id == character?.Id);
                if (selected == null || !selected.IsUnlocked || selected.RunProfileId <= 0)
                    throw new InvalidOperationException("出発できるキャラクターを選択してください");
                bridge = new BattleRunBridgeData(selected.RunProfileId, BattleRunStartMode.NewAdventure);
            }
            else if (action == MainMenuAction.ResumeAdventure)
            {
                var save = await _run.LoadCurrentRunAsync(ct);
                if (save == null || !save.IsValid)
                    throw new InvalidOperationException("再開できる冒険が見つかりません");
                bridge = new BattleRunBridgeData(save.RunProfileId, BattleRunStartMode.ContinueAdventure);
            }
            else return false;

            ct.ThrowIfCancellationRequested();
            await _scenes.LoadSceneAsync("BattleScene", bridge, true, ct);
            return true;
        }
    }
}
