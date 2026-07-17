using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Save.Model;
using Dungeon.Runtime.InGame.Save.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Dungeon.Tests.PlayMode.Support
{
    /// <summary>
    /// BattleSceneのPlayMode検証用ハーネスクラス
    /// </summary>
    public sealed class BattleScenePlayModeHarness : IDisposable
    {
        private const string BattleScenePath = "Assets/Scenes/BattleScene.unity";
        private const int ScopeReadyFrameLimit = 300;

        private readonly DeterministicRandomProvider _randomProvider;
        private readonly DeterministicMapGenerator _mapGenerator;
        private readonly InMemoryRunSaveService _runSaveService = new InMemoryRunSaveService();

        private IDisposable _overrideInstallation;
        private Scene _battleScene;

        public BattleScenePlayModeHarness(
            IReadOnlyList<RuntimeMapNode> mapNodes = null,
            IReadOnlyList<int> randomValues = null)
        {
            _randomProvider = new DeterministicRandomProvider(randomValues);
            _mapGenerator = new DeterministicMapGenerator(mapNodes);
        }

        public IBattleSceneFlowService FlowService { get; private set; }
        public IBattleSceneQueryService QueryService { get; private set; }
        public RunSaveData SavedRun => _runSaveService.SavedRun;
        public bool IsLoaded => _battleScene.IsValid() && _battleScene.isLoaded;

        /// <summary>
        /// BattleSceneのロードとサービス解決
        /// </summary>
        public IEnumerator LoadAsync()
        {
            if (IsLoaded)
            {
                throw new InvalidOperationException("BattleScene is already loaded by this harness.");
            }

            Scene existingScene = SceneManager.GetSceneByPath(BattleScenePath);
            if (existingScene.IsValid() && existingScene.isLoaded)
            {
                throw new InvalidOperationException("BattleScene is already loaded.");
            }

            _overrideInstallation = LifetimeScope.Enqueue(builder =>
            {
                builder.RegisterInstance(_randomProvider).As<IBattleRandomProvider>();
                builder.RegisterInstance(_mapGenerator).As<IBattleMapGenerator>();
                builder.RegisterInstance(_runSaveService).As<IRunSaveService>();
            });

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(BattleScenePath, LoadSceneMode.Additive);
            if (loadOperation == null)
            {
                Dispose();
                throw new InvalidOperationException("BattleScene load operation could not be created.");
            }

            yield return loadOperation;
            _overrideInstallation.Dispose();
            _overrideInstallation = null;
            _battleScene = SceneManager.GetSceneByPath(BattleScenePath);

            BattleSceneLifetimeScope lifetimeScope = null;
            int waitedFrames = 0;
            while (lifetimeScope == null || lifetimeScope.Container == null)
            {
                lifetimeScope = LifetimeScope.Find<BattleSceneLifetimeScope>(_battleScene) as BattleSceneLifetimeScope;
                if (++waitedFrames > ScopeReadyFrameLimit)
                {
                    throw new TimeoutException("BattleSceneLifetimeScope did not become ready.");
                }

                yield return null;
            }

            FlowService = lifetimeScope.Container.Resolve<IBattleSceneFlowService>();
            QueryService = lifetimeScope.Container.Resolve<IBattleSceneQueryService>();
        }

        /// <summary>
        /// BattleSceneのアンロード
        /// </summary>
        public IEnumerator UnloadAsync()
        {
            FlowService = null;
            QueryService = null;

            if (IsLoaded)
            {
                AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(_battleScene);
                if (unloadOperation != null)
                {
                    yield return unloadOperation;
                }
            }

            _battleScene = default;
            Dispose();
        }

        public void Dispose()
        {
            _overrideInstallation?.Dispose();
            _overrideInstallation = null;
        }

        private sealed class DeterministicRandomProvider : IBattleRandomProvider
        {
            private readonly IReadOnlyList<int> _values;
            private int _index;

            public DeterministicRandomProvider(IReadOnlyList<int> values)
            {
                _values = values ?? Array.Empty<int>();
            }

            public int Seed { get; private set; }
            public int Counter { get; private set; }

            public int Range(int minInclusive, int maxExclusive)
            {
                if (maxExclusive <= minInclusive)
                {
                    return minInclusive;
                }

                int value = _values.Count == 0 ? minInclusive : _values[_index++ % _values.Count];
                int width = maxExclusive - minInclusive;
                int offset = value % width;
                if (offset < 0)
                {
                    offset += width;
                }

                Counter++;
                return minInclusive + offset;
            }

            public void Initialize(int seed)
            {
                Seed = seed;
                Counter = 0;
                _index = 0;
            }

            public void Restore(int seed, int counter)
            {
                Seed = seed;
                Counter = Math.Max(0, counter);
                _index = _values.Count == 0 ? 0 : Counter % _values.Count;
            }
        }

        private sealed class DeterministicMapGenerator : IBattleMapGenerator
        {
            private readonly IReadOnlyList<RuntimeMapNode> _nodes;

            public DeterministicMapGenerator(IReadOnlyList<RuntimeMapNode> nodes)
            {
                _nodes = nodes;
            }

            public IReadOnlyList<RuntimeMapNode> Generate(RuntimeRunDefinition runDefinition, int mapSeed)
            {
                return _nodes ?? runDefinition?.Nodes ?? Array.Empty<RuntimeMapNode>();
            }
        }

        private sealed class InMemoryRunSaveService : IRunSaveService
        {
            public RunSaveData SavedRun { get; private set; }

            public UniTask SaveCurrentRunAsync(RunSaveData data, CancellationToken token = default)
            {
                SavedRun = data;
                return UniTask.CompletedTask;
            }

            public UniTask<RunSaveData> LoadCurrentRunAsync(CancellationToken token = default)
            {
                return UniTask.FromResult(SavedRun);
            }

            public bool HasSavedRun()
            {
                return SavedRun != null;
            }

            public void DeleteSavedRun()
            {
                SavedRun = null;
            }
        }
    }
}
