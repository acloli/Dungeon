using System;
using System.Collections.Generic;
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
        [SerializeField] private Transform _runProfileRoot;
        [SerializeField] private MainRunProfileButtonView _runProfileButtonTemplate;
        [SerializeField] private TFTextUGUI _selectedRunProfileText;

        private readonly List<MainRunProfileButtonView> _runProfileButtons = new List<MainRunProfileButtonView>();
        private IMainRunProfileService _runProfileService;
        private IReadOnlyList<MainRunProfileViewModel> _runProfiles = Array.Empty<MainRunProfileViewModel>();
        private int _selectedRunProfileId;

        [Inject]
        private void Construct(IMainRunProfileService runProfileService)
        {
            _runProfileService = runProfileService;
        }

        protected override UniTask OnInitializeInternalAsync(ISceneBridgeData bridgeData, CancellationToken ct)
        {
            BuildRunProfileViews();
            WireButtons();
            return UniTask.CompletedTask;
        }

        protected override void OnTerminateInternal()
        {
            UnwireButtons();
            ClearRunProfileButtons();
        }

        /// <summary>
        /// RunProfile一覧表示構築
        /// </summary>
        private void BuildRunProfileViews()
        {
            ClearRunProfileButtons();
            _runProfiles = _runProfileService != null
                ? _runProfileService.BuildRunProfiles()
                : Array.Empty<MainRunProfileViewModel>();

            if (_runProfiles.Count == 0)
            {
                _selectedRunProfileId = 0;
                SetStartRunInteractable(false);
                SetSelectedRunProfileText("RunProfile is not found");
                TLogger.Warning("RunProfileMaster is empty", "Main");
                return;
            }

            _selectedRunProfileId = _runProfiles[0].Id;
            SetStartRunInteractable(true);
            RebuildRunProfileButtons();
            RefreshSelectedRunProfileText();
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
        /// RunProfile選択ボタン再構築
        /// </summary>
        private void RebuildRunProfileButtons()
        {
            if (_runProfileRoot == null || _runProfileButtonTemplate == null)
            {
                return;
            }

            _runProfileButtonTemplate.gameObject.SetActive(false);
            for (int i = 0; i < _runProfiles.Count; i++)
            {
                MainRunProfileViewModel runProfile = _runProfiles[i];
                MainRunProfileButtonView button = Instantiate(_runProfileButtonTemplate, _runProfileRoot);
                button.gameObject.SetActive(true);
                button.Configure(
                    BuildRunProfileButtonLabel(runProfile),
                    runProfile.Id == _selectedRunProfileId,
                    () => SelectRunProfile(runProfile.Id));
                _runProfileButtons.Add(button);
            }
        }

        /// <summary>
        /// RunProfile選択
        /// </summary>
        private void SelectRunProfile(int runProfileId)
        {
            _selectedRunProfileId = runProfileId;
            RefreshRunProfileSelection();
            RefreshSelectedRunProfileText();
        }

        /// <summary>
        /// RunProfile選択表示更新
        /// </summary>
        private void RefreshRunProfileSelection()
        {
            for (int i = 0; i < _runProfiles.Count && i < _runProfileButtons.Count; i++)
            {
                _runProfileButtons[i].ApplySelectedState(_runProfiles[i].Id == _selectedRunProfileId);
            }
        }

        /// <summary>
        /// 選択中RunProfile文言更新
        /// </summary>
        private void RefreshSelectedRunProfileText()
        {
            MainRunProfileViewModel runProfile = FindSelectedRunProfile();
            if (runProfile == null)
            {
                SetSelectedRunProfileText("RunProfile is not selected");
                return;
            }

            SetSelectedRunProfileText(
                $"Selected {runProfile.Key}\nHP {runProfile.PlayerMaxHp}  Gold {runProfile.StartingGold}  Archetype {runProfile.CharacterArchetype}");
        }

        /// <summary>
        /// 選択中RunProfile取得
        /// </summary>
        private MainRunProfileViewModel FindSelectedRunProfile()
        {
            for (int i = 0; i < _runProfiles.Count; i++)
            {
                MainRunProfileViewModel runProfile = _runProfiles[i];
                if (runProfile.Id == _selectedRunProfileId)
                {
                    return runProfile;
                }
            }

            return null;
        }

        /// <summary>
        /// RunProfileボタン文言構築
        /// </summary>
        private static string BuildRunProfileButtonLabel(MainRunProfileViewModel runProfile)
        {
            return $"{runProfile.Key}\nHP {runProfile.PlayerMaxHp}  Gold {runProfile.StartingGold}";
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
        /// RunProfile選択ボタン消去
        /// </summary>
        private void ClearRunProfileButtons()
        {
            for (int i = 0; i < _runProfileButtons.Count; i++)
            {
                MainRunProfileButtonView button = _runProfileButtons[i];
                if (button == null)
                {
                    continue;
                }

                button.Clear();
                Destroy(button.gameObject);
            }

            _runProfileButtons.Clear();
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
                int runProfileId = _selectedRunProfileId > 0 ? _selectedRunProfileId : _defaultRunProfileId;
                BattleRunBridgeData bridgeData = new BattleRunBridgeData(runProfileId);
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
