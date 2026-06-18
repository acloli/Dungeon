using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.View;
using NUnit.Framework;
using TFramework.UI;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleSceneUiCoordinatorのEditモードテストクラス
    /// </summary>
    [TestFixture]
    public sealed class BattleSceneUiCoordinatorTests
    {
        [Test]
        public void ShowRewardAndRestShop_UseCachedDialogs_ResultUsesSingleUseDialog()
        {
            FakeUIService uiService = new FakeUIService();
            BattleSceneUiCoordinator coordinator = new BattleSceneUiCoordinator(uiService);
            FakeBattleSceneHostView hostView = new FakeBattleSceneHostView();
            BattleSceneSnapshot snapshot = CreateSnapshot(BattleScenePage.Map);

            coordinator.InitializeAsync(hostView, CancellationToken.None).GetAwaiter().GetResult();
            coordinator.ShowRewardAsync(snapshot, CancellationToken.None).GetAwaiter().GetResult();
            coordinator.ShowRestShopAsync(snapshot, CancellationToken.None).GetAwaiter().GetResult();
            coordinator.ShowResultAsync(snapshot, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(uiService.ClearStackCallCount, Is.EqualTo(4));
            Assert.That(uiService.LastRewardDialogParam.CacheOnClose, Is.True);
            Assert.That(uiService.LastRestShopDialogParam.CacheOnClose, Is.True);
            Assert.That(uiService.LastResultDialogParam.CacheOnClose, Is.False);
            Assert.That(hostView.IsBattleVisible, Is.False);
        }

        [Test]
        public void ShowShopAsync_DispatchesCorrectDialog()
        {
            FakeUIService uiService = new FakeUIService();
            BattleSceneUiCoordinator coordinator = new BattleSceneUiCoordinator(uiService);
            FakeBattleSceneHostView hostView = new FakeBattleSceneHostView();
            BattleSceneSnapshot snapshot = CreateSnapshot(BattleScenePage.Shop);

            coordinator.InitializeAsync(hostView, CancellationToken.None).GetAwaiter().GetResult();
            coordinator.ShowShopAsync(snapshot, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(uiService.ClearStackCallCount, Is.EqualTo(2));
            Assert.That(uiService.LastShopDialogParam, Is.Not.Null);
        }

        [Test]
        public void ShowEventAsync_DispatchesCorrectDialog()
        {
            FakeUIService uiService = new FakeUIService();
            BattleSceneUiCoordinator coordinator = new BattleSceneUiCoordinator(uiService);
            FakeBattleSceneHostView hostView = new FakeBattleSceneHostView();
            BattleSceneSnapshot snapshot = CreateSnapshot(BattleScenePage.Event);

            coordinator.InitializeAsync(hostView, CancellationToken.None).GetAwaiter().GetResult();
            coordinator.ShowEventAsync(snapshot, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(uiService.ClearStackCallCount, Is.EqualTo(2));
            Assert.That(uiService.LastEventDialogParam, Is.Not.Null);
        }

        [Test]
        public void HostChromeModalDialogs_ToggleHostInteractable()
        {
            FakeUIService uiService = new FakeUIService();
            BattleSceneUiCoordinator coordinator = new BattleSceneUiCoordinator(uiService);
            FakeBattleSceneHostView hostView = new FakeBattleSceneHostView();
            BattleSceneSnapshot snapshot = CreateSnapshot(BattleScenePage.Map);

            coordinator.InitializeAsync(hostView, CancellationToken.None).GetAwaiter().GetResult();
            coordinator.ShowCardPickAsync(snapshot, CancellationToken.None).GetAwaiter().GetResult();
            coordinator.ShowPotionReplaceAsync(snapshot, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(hostView.SetHostChromeInteractableCallCount, Is.EqualTo(5));
            Assert.That(hostView.LastHostChromeInteractable, Is.True);
        }

        private static BattleSceneSnapshot CreateSnapshot(BattleScenePage page)
        {
            return new BattleSceneSnapshot(
                page,
                Array.Empty<RuntimeMapNode>(),
                Array.Empty<RuntimeCard>(),
                Array.Empty<RuntimeRewardEntry>(),
                -1,
                40,
                40,
                3,
                0,
                100,
                null,
                0,
                0,
                false,
                -1,
                false,
                "map",
                "battle",
                "rest",
                "result");
        }

        private sealed class FakeBattleSceneHostView : IBattleSceneHostView
        {
            public IBattlePageView BattlePageView => null;
            public bool IsBattleVisible { get; private set; }
            public bool IsSaveQuitVisible { get; private set; }
            public bool LastHostChromeInteractable { get; private set; } = true;
            public int SetHostChromeInteractableCallCount { get; private set; }

            public void BuildOwnedRelics(System.Collections.Generic.IReadOnlyList<BattleMultiIconViewModel> relics, Action<int> onClicked)
            {
            }

            public void SetOwnedRelicHint(string message, int selectedIndex)
            {
            }

            public void ClearOwnedRelics()
            {
            }

            public void BuildOwnedPotions(System.Collections.Generic.IReadOnlyList<BattleMultiIconViewModel> potions, Action<int> onClicked)
            {
            }

            public void SetOwnedPotionHint(string message, int selectedIndex)
            {
            }

            public void SetOwnedPotionUseVisible(bool visible, Action onClicked)
            {
            }

            public void ClearOwnedPotions()
            {
            }

            public void SetHostChromeInteractable(bool interactable)
            {
                LastHostChromeInteractable = interactable;
                SetHostChromeInteractableCallCount++;
            }

            public void WireHostBackgroundClick(Action onClicked)
            {
            }

            public void UnwireHostBackgroundClick()
            {
            }

            public void SetBattleVisible(bool visible)
            {
                IsBattleVisible = visible;
            }

            public void SetSaveQuitVisible(bool visible)
            {
                IsSaveQuitVisible = visible;
            }

            public void WireSaveQuitButton(Action onSaveQuitClicked)
            {
            }

            public void UnwireSaveQuitButton()
            {
            }
        }

        private sealed class FakeUIService : IUIService
        {
            public int ClearStackCallCount { get; private set; }
            public UIDialogOpenParam LastRewardDialogParam { get; private set; }
            public UIDialogOpenParam LastRestShopDialogParam { get; private set; }
            public UIDialogOpenParam LastResultDialogParam { get; private set; }
            public UIDialogOpenParam LastShopDialogParam { get; private set; }
            public UIDialogOpenParam LastEventDialogParam { get; private set; }
            public UIDialogOpenParam LastPotionReplaceDialogParam { get; private set; }

            public UIPageBase CurrentPage => null;
            public int PageStackCount => 0;
            public bool IsLoading => false;

            public UniTask InitializeAsync(CancellationToken ct) => UniTask.CompletedTask;
            public UniTask ShowPageAsync<TPage>(object param = null, CancellationToken ct = default) where TPage : UIPageBase => UniTask.CompletedTask;
            public UniTask ShowPageAsync(string address, object param = null, CancellationToken ct = default) => UniTask.CompletedTask;
            public UniTask<bool> GoBackAsync(CancellationToken ct = default) => UniTask.FromResult(false);

            public UniTask ClearStackAsync(CancellationToken ct = default)
            {
                ClearStackCallCount++;
                return UniTask.CompletedTask;
            }

            public UniTask ShowDialogAsync<TDialog>(object param = null, CancellationToken ct = default) where TDialog : UIDialogBase
            {
                LastResultDialogParam = param as UIDialogOpenParam;
                return UniTask.CompletedTask;
            }

            public UniTask<TResult> ShowDialogAsync<TDialog, TResult>(object param = null, CancellationToken ct = default) where TDialog : UIDialogBase<TResult>
            {
                // 型ごとの受け取り方だけを検証できれば十分なので、結果は常にdefaultを返す。
                if (typeof(TDialog) == typeof(RewardDialog))
                {
                    LastRewardDialogParam = param as UIDialogOpenParam;
                }
                else if (typeof(TDialog) == typeof(RestShopDialog))
                {
                    LastRestShopDialogParam = param as UIDialogOpenParam;
                }
                else if (typeof(TDialog) == typeof(ShopDialog))
                {
                    LastShopDialogParam = param as UIDialogOpenParam;
                }
                else if (typeof(TDialog) == typeof(EventDialog))
                {
                    LastEventDialogParam = param as UIDialogOpenParam;
                }
                else if (typeof(TDialog) == typeof(PotionReplaceDialog))
                {
                    LastPotionReplaceDialogParam = param as UIDialogOpenParam;
                }

                return UniTask.FromResult(default(TResult));
            }

            public UniTask ShowDialogAsync(string address, object param = null, CancellationToken ct = default) => UniTask.CompletedTask;
            public void ShowToast(string message, float duration = 0f) { }
            public IDisposable ShowLoading(string message = null) => null;
            public void HideLoading() { }

            public void RegisterPageAddress<TPage>(string address) where TPage : UIPageBase { }

            public void RegisterDialogAddress<TDialog>(string address) where TDialog : UIDialogBase { }

            public void Dispose()
            {
            }
        }
    }
}
