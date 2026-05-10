using System.Threading;
using Cysharp.Threading.Tasks;
using TFramework.Scene;
using UnityEngine;
using UnityEngine.UI;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Dungeon.Runtime.OutGame.Main
{
    public class MainSceneController : SceneControllerBase
    {
        [SerializeField] private Button _startRunButton;
        [SerializeField] private string _battleSceneName = "BattleScene";

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
            UnitySceneManager.LoadScene(_battleSceneName);
        }
    }
}
