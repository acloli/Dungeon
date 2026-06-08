using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.SceneFlow;
using TFramework.Debug;
using TFramework.Scene;
using TFramework.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;
using Dungeon.Runtime.InGame.Save.Services;

namespace Dungeon.Runtime.OutGame.Main
{
    public class MainSceneController : SceneControllerBase
    {
        [FormerlySerializedAs("_startRunButton")]
        [SerializeField] private Button _newRunButton;
        [SerializeField] private Button _continueRunButton;
        [SerializeField] private string _battleSceneName = "BattleScene";
        [SerializeField] private int _defaultRunProfileId = 5501;
        [SerializeField] private TFTextUGUI _selectedRunProfileText;

        private IMainRunProfileService _runProfileService;
        private MainRunProfileViewModel _runProfile;
        private IRunSaveService _runSaveService;

        [Inject]
        private void Construct(IMainRunProfileService runProfileService, IRunSaveService runSaveService = null)
        {
            _runProfileService = runProfileService;
            _runSaveService = runSaveService;
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
                SetNewRunInteractable(false);
                SetContinueRunActive(false);
                SetSelectedRunProfileText("RunProfile is not found");
                TLogger.Warning($"RunProfileMaster is not found id={_defaultRunProfileId}", "Main");
                return;
            }

            SetNewRunInteractable(true);
            RefreshRunProfileText();
        }

        private void WireButtons()
        {
            if (_newRunButton != null)
            {
                _newRunButton.onClick.RemoveListener(OnNewRunClicked);
                _newRunButton.onClick.AddListener(OnNewRunClicked);
            }

            if (_continueRunButton != null)
            {
                _continueRunButton.onClick.RemoveListener(OnContinueRunClicked);
                _continueRunButton.onClick.AddListener(OnContinueRunClicked);
            }
        }

        private void UnwireButtons()
        {
            if (_newRunButton != null)
            {
                _newRunButton.onClick.RemoveListener(OnNewRunClicked);
            }

            if (_continueRunButton != null)
            {
                _continueRunButton.onClick.RemoveListener(OnContinueRunClicked);
            }
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
        /// NewRunボタン有効状態設定
        /// </summary>
        private void SetNewRunInteractable(bool interactable)
        {
            if (_newRunButton != null)
            {
                _newRunButton.interactable = interactable;
            }
        }

        /// <summary>
        /// Continueボタン有効状態設定
        /// </summary>
        private void SetContinueRunActive(bool isActive)
        {
            if (_continueRunButton != null)
            {
                _continueRunButton.gameObject.SetActive(isActive);
            }
        }

        /// <summary>
        /// RunProfile概要表示更新
        /// </summary>
        private void RefreshRunProfileText()
        {
            bool hasSavedRun = _runSaveService != null && _runSaveService.HasSavedRun();
            SetContinueRunActive(hasSavedRun);
            if (hasSavedRun)
            {
                SetSelectedRunProfileText(
                    $"<color=yellow>Continue available</color>\n{_runProfile.DisplayName}\nHP {_runProfile.PlayerMaxHp}  Gold {_runProfile.StartingGold}  Archetype {_runProfile.CharacterArchetype}");
            }
            else
            {
                SetSelectedRunProfileText(
                    $"{_runProfile.DisplayName}\nHP {_runProfile.PlayerMaxHp}  Gold {_runProfile.StartingGold}  Archetype {_runProfile.CharacterArchetype}");
            }
        }

        private void OnNewRunClicked()
        {
            _runSaveService?.DeleteSavedRun();
            LoadBattleSceneAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private void OnContinueRunClicked()
        {
            if (_runSaveService == null || !_runSaveService.HasSavedRun())
            {
                RefreshRunProfileText();
                return;
            }

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
