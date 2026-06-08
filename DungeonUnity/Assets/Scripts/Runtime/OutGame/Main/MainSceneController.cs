using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.SceneFlow;
using TFramework.Debug;
using TFramework.Scene;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Dungeon.Runtime.OutGame.Main
{
    public class MainSceneController : SceneControllerBase
    {
        [SerializeField] private Button _startRunButton;
        [SerializeField] private string _battleSceneName = "BattleScene";
        [SerializeField] private int _defaultRunProfileId = 5501;
        [SerializeField] private TFTextUGUI _selectedRunProfileText;

        private IMainRunProfileService _runProfileService;
        private MainRunProfileViewModel _runProfile;

        [Inject]
        private void Construct(IMainRunProfileService runProfileService)
        {
            _runProfileService = runProfileService;
        }

        protected override UniTask OnInitializeInternalAsync(ISceneBridgeData bridgeData, CancellationToken ct)
        {
            BuildRunProfileEntry();
            WireButtons();
            return UniTask.CompletedTask;
        }

        protected override void OnTerminateInternal()
        {
            UnwireButtons();
        }

        /// <summary>
        /// RunProfile入口表示構築
        /// </summary>
        private void BuildRunProfileEntry()
        {
            _runProfile = _runProfileService != null
                ? _runProfileService.BuildRunProfile(_defaultRunProfileId)
                : null;

            if (_runProfile == null)
            {
                SetStartRunInteractable(false);
                SetSelectedRunProfileText("RunProfile is not found");
                TLogger.Warning($"RunProfileMaster is not found id={_defaultRunProfileId}", "Main");
                return;
            }

            SetStartRunInteractable(true);
            RefreshRunProfileText();
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

        /// <summary>
        /// RunProfile選択文言設定
        /// </summary>
        private void SetSelectedRunProfileText(string text)
        {
            if (_selectedRunProfileText != null)
            {
                _selectedRunProfileText.text = text;
            }
        }

        /// <summary>
        /// Run開始ボタン有効状態設定
        /// </summary>
        private void SetStartRunInteractable(bool interactable)
        {
            if (_startRunButton != null)
            {
                _startRunButton.interactable = interactable;
            }
        }

        /// <summary>
        /// RunProfile概要表示更新
        /// </summary>
        private void RefreshRunProfileText()
        {
            SetSelectedRunProfileText(
                $"{_runProfile.DisplayName}\nHP {_runProfile.PlayerMaxHp}  Gold {_runProfile.StartingGold}  Archetype {_runProfile.CharacterArchetype}");
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
