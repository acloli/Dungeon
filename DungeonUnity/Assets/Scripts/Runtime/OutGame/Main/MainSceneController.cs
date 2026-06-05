using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.SceneFlow;
using TFramework.Debug;
using TFramework.Scene;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Main
{
    public class MainSceneController : SceneControllerBase
    {
        [SerializeField] private Button _startRunButton;
        [SerializeField] private string _battleSceneName = "BattleScene";
        [SerializeField] private int _defaultRunProfileId = 5501;

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
            if (_startRunButton == null)
            {
                return;
            }
            _startRunButton.onClick.RemoveListener(OnStartRunClicked);
            _startRunButton.onClick.AddListener(OnStartRunClicked);
        }

        private void UnwireButtons()
        {
            if (_startRunButton == null)
            {
                return;
            }
            _startRunButton.onClick.RemoveListener(OnStartRunClicked);
        }

        private void OnStartRunClicked()
        {
            LoadBattleSceneAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>
        /// BattleSceneへの遷移処理
        /// </summary>
        private async UniTaskVoid LoadBattleSceneAsync(CancellationToken ct)
        {
            try
            {
                BattleRunBridgeData bridgeData = new BattleRunBridgeData(_defaultRunProfileId);
                await SceneService.LoadSceneAsync(_battleSceneName, bridgeData, true, ct);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                TLogger.Error($"BattleScene load failed: {ex.Message}", "Main");
            }
        }
    }
}
