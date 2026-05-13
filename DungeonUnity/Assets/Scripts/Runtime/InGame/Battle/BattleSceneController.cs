using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.View;
using Dungeon.Runtime.InGame.Domain;
using TFramework.Scene;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using VContainer;

namespace Dungeon.Runtime.InGame.Battle
{
    /// <summary>
    /// BattleSceneのシーン入口制御クラス
    /// </summary>
    public sealed class BattleSceneController : SceneControllerBase
    {
        [Header("Config")]
        [SerializeField] private RunStartConfig _runStartConfig;
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
        protected override UniTask OnInitializeInternalAsync(ISceneBridgeData bridgeData, CancellationToken ct)
        {
            ValidateConfiguration();

            if (_view == null)
            {
                Debug.LogError("BattleSceneView is missing.");
                return UniTask.CompletedTask;
            }

            _view.WireStaticButtons(
                _presenter.OnEnemyTargetClicked,
                _presenter.OnEndTurnClicked,
                _presenter.OnRestClicked,
                _presenter.OnUpgradeClicked,
                _presenter.OnShopClicked,
                _presenter.OnRestShopContinueClicked,
                OnResultBackClicked);
            _presenter.Initialize(_view, _runStartConfig);
            return UniTask.CompletedTask;
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

            _view.UnwireStaticButtons();
            _view.ClearDynamicButtons();
            if (_presenter != null)
            {
                _presenter.Dispose();
            }
        }

        /// <summary>
        /// 設定参照検証
        /// </summary>
        private void ValidateConfiguration()
        {
            if (_runStartConfig == null)
            {
                Debug.LogError(BattleSceneConstants.MissingRunConfig);
            }
        }

        /// <summary>
        /// MainSceneに戻り処理
        /// </summary>
        private void OnResultBackClicked()
        {
            UnitySceneManager.LoadScene(_mainSceneName);
        }
    }
}
