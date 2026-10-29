using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.OutGame.Main.UI;
using TFramework.Scene;
using UnityEngine;
using VContainer;

namespace Dungeon.Runtime.OutGame.Main
{
    /// <summary>
    /// MainSceneのメニュー表示を開始
    /// </summary>
    public sealed class MainSceneController : SceneControllerBase
    {
        [SerializeField] private MainSceneView _view;
        private MainSceneUiCoordinator _ui;

        public bool IsMenuReady => _ui?.IsInitialized == true;

        [Inject]
        private void Construct(MainSceneUiCoordinator ui) => _ui = ui;

        protected override UniTask OnInitializeInternalAsync(ISceneBridgeData bridgeData, CancellationToken ct)
        {
            return _ui.InitializeAsync(_view, ct);
        }

        protected override void OnTerminateInternal() => _ui?.Dispose();
        private void OnDestroy() => _ui?.Dispose();
    }
}
