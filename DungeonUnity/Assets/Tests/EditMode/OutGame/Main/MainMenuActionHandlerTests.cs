using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Save.Model;
using Dungeon.Runtime.InGame.Save.Services;
using Dungeon.Runtime.OutGame.Main;
using Dungeon.Runtime.SceneFlow;
using NUnit.Framework;
using R3;
using TFramework.Scene;

namespace Dungeon.Tests.EditMode
{
    public sealed class MainMenuActionHandlerTests
    {
        [Test]
        public void Start_UsesFreshUnlockedCharacterAndKeepsSaveUntilBattleInitializes()
        {
            var source = new Source(); var save = new Save(); var scenes = new Scenes();
            var handler = new MainMenuActionHandler(scenes, source, save);
            Assert.That(handler.ExecuteAsync(MainMenuAction.StartAdventure, CancellationToken.None,
                new MainMenuCharacterData { Id = 2, RunProfileId = 999 }).GetAwaiter().GetResult(), Is.True);
            Assert.That(scenes.Bridge.RunProfileId, Is.EqualTo(6601));
            Assert.That(scenes.Bridge.StartMode, Is.EqualTo(BattleRunStartMode.NewAdventure));
            Assert.That(save.Deleted, Is.False);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        public void Start_RejectsUnavailableCharacters(int id)
        {
            var scenes = new Scenes(); var save = new Save();
            var handler = new MainMenuActionHandler(scenes, new Source(), save);
            Assert.Throws<InvalidOperationException>(() => handler.ExecuteAsync(MainMenuAction.StartAdventure,
                CancellationToken.None, new MainMenuCharacterData { Id = id }).GetAwaiter().GetResult());
            Assert.That(scenes.Calls, Is.Zero); Assert.That(save.Deleted, Is.False);
        }

        [Test]
        public void Continue_UsesSavedProfileAndExplicitResumeMode()
        {
            var scenes = new Scenes(); var save = new Save();
            var handler = new MainMenuActionHandler(scenes, new Source(), save);
            handler.ExecuteAsync(MainMenuAction.ResumeAdventure, CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(scenes.Bridge.RunProfileId, Is.EqualTo(5501));
            Assert.That(scenes.Bridge.StartMode, Is.EqualTo(BattleRunStartMode.ContinueAdventure));
            Assert.That(save.Deleted, Is.False);
            save.Data = null;
            Assert.Throws<InvalidOperationException>(() => handler.ExecuteAsync(MainMenuAction.ResumeAdventure,
                CancellationToken.None).GetAwaiter().GetResult());
            Assert.That(scenes.Calls, Is.EqualTo(1));
        }

        [Test]
        public void CancellationAndLoadFailure_DoNotDeleteCheckpoint()
        {
            var scenes = new Scenes { Fail = true }; var save = new Save();
            var handler = new MainMenuActionHandler(scenes, new Source(), save);
            Assert.Throws<InvalidOperationException>(() => handler.ExecuteAsync(MainMenuAction.ResumeAdventure,
                CancellationToken.None).GetAwaiter().GetResult());
            using var cts = new CancellationTokenSource(); cts.Cancel();
            Assert.Throws<OperationCanceledException>(() => handler.ExecuteAsync(MainMenuAction.ResumeAdventure,
                cts.Token).GetAwaiter().GetResult());
            Assert.That(scenes.Calls, Is.EqualTo(1)); Assert.That(save.Deleted, Is.False);
        }

        private sealed class Source : IMainMenuDataSource
        {
            public UniTask<MainMenuData> LoadAsync(CancellationToken ct) => UniTask.FromResult(new MainMenuData
            {
                Characters = new[] { new MainMenuCharacterData { Id = 1, RunProfileId = 5501 },
                    new MainMenuCharacterData { Id = 2, RunProfileId = 6601, IsUnlocked = true } }
            });
        }
        private sealed class Save : IRunSaveService
        {
            public bool Deleted;
            public RunSaveData Data = new RunSaveData { RunProfileId = 5501, PlayerMaxHp = 80, PlayerHp = 50, MasterSeed = 1 };
            public UniTask<RunSaveData> LoadCurrentRunAsync(CancellationToken token = default) => UniTask.FromResult(Data);
            public UniTask SaveCurrentRunAsync(RunSaveData data, CancellationToken token = default) => UniTask.CompletedTask;
            public bool HasSavedRun() => Data != null;
            public void DeleteSavedRun() => Deleted = true;
        }
        private sealed class Scenes : ISceneService
        {
            public int Calls; public bool Fail; public BattleRunBridgeData Bridge;
            public ReadOnlyReactiveProperty<string> CurrentScene => null;
            public UniTask InitializeAsync(CancellationToken ct) => UniTask.CompletedTask;
            public UniTask LoadSceneAsync(string sceneName, ISceneBridgeData bridgeData = null, bool useLoading = true, CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested(); Calls++;
                Assert.That(sceneName, Is.EqualTo("BattleScene"));
                Bridge = (BattleRunBridgeData)bridgeData;
                if (Fail) throw new InvalidOperationException("検証用ロード失敗");
                return UniTask.CompletedTask;
            }
            public UniTask LoadSceneAdditiveAsync(string sceneName, ISceneBridgeData bridgeData = null, CancellationToken ct = default) => throw new NotSupportedException();
            public ISceneBridgeData GetSceneBridgeData(string sceneName) => Bridge;
            public UniTask UnloadSceneAsync(string sceneName, CancellationToken ct = default) => throw new NotSupportedException();
        }
    }
}
