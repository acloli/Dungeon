using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TFramework.Debug;
using TFramework.Scene;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Title
{
    public class TitleSceneController : SceneControllerBase
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private string _mainSceneName = "MainScene";

        protected override UniTask OnInitializeInternalAsync(ISceneBridgeData bridgeData, CancellationToken ct)
        {
            WireButtons();
            return UniTask.CompletedTask;
        }

        protected override void OnTerminateInternal()
        {
            UnwireButtons();
        }

        private void WireButtons()
        {
            if (_startButton == null)
            {
                return;
            }
            _startButton.onClick.RemoveListener(OnStartClicked);
            _startButton.onClick.AddListener(OnStartClicked);
        }

        private void UnwireButtons()
        {
            if (_startButton == null)
            {
                return;
            }
            _startButton.onClick.RemoveListener(OnStartClicked);
        }

        private void OnStartClicked()
        {
            LoadMainSceneAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>
        /// MainSceneへの遷移処理
        /// </summary>
        private async UniTaskVoid LoadMainSceneAsync(CancellationToken ct)
        {
            try
            {
                await SceneService.LoadSceneAsync(_mainSceneName, null, true, ct);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                TLogger.Error($"MainScene load failed: {ex.Message}", "Title");
            }
        }
    }
}
