using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.View;
using TFramework.Debug;
using TFramework.Scene;
using UnityEngine;
using VContainer;

namespace Dungeon.Runtime.InGame.Battle
{
    /// <summary>
    /// BattleSceneのシーン入口制御クラス
    /// </summary>
    public sealed class BattleSceneController : SceneControllerBase
    {
        [Header("Config")]
        [SerializeField] private int _runProfileId = 5501;
        [SerializeField] private string _mainSceneName = BattleSceneConstants.MainSceneName;

        [Header("View")]
        [SerializeField] private BattleSceneView _view;

        private BattleScenePresenter _presenter;

        [Inject]
        private void Construct(BattleScenePresenter presenter)
        {
            _presenter = presenter;
        }

        /// <summary>
        /// シーン初期化処理
        /// </summary>
        protected override async UniTask OnInitializeInternalAsync(ISceneBridgeData bridgeData, CancellationToken ct)
        {
            int runProfileId = BattleRunProfileResolver.ResolveRunProfileId(bridgeData, _runProfileId);
            if (!ValidateConfiguration(runProfileId))
            {
                return;
            }

            if (_view == null)
            {
                TLogger.Error("BattleSceneView is missing.", "Battle");
                return;
            }

            await _presenter.InitializeAsync(_view, runProfileId, OnResultBackClicked, ct, OnSaveQuitClicked);
        }

        /// <summary>
        /// シーン終了処理
        /// </summary>
        protected override void OnTerminateInternal()
        {
            if (_view == null)
            {
                return;
            }

            if (_presenter != null)
            {
                _presenter.Dispose();
            }
        }

        /// <summary>
        /// 設定参照検証
        /// </summary>
        private bool ValidateConfiguration(int runProfileId)
        {
            if (runProfileId <= 0)
            {
                TLogger.Error(BattleSceneConstants.MissingRunProfile, "Battle");
                return false;
            }

            return true;
        }

        /// <summary>
        /// MainSceneに戻り処理
        /// </summary>
        private void OnResultBackClicked()
        {
            LoadMainSceneAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>
        /// 中断戻り処理
        /// </summary>
        private void OnSaveQuitClicked()
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
                TLogger.Error($"MainScene load failed: {ex.Message}", "Battle");
            }
        }
    }
}
