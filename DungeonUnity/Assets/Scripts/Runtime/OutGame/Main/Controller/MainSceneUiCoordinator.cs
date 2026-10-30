using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.Common.UI.Binding;
using Dungeon.Runtime.Common.UI.Dialog;
using Dungeon.Runtime.Common.UI.MultiIcon;
using Dungeon.Runtime.Common.UI.Page;
using Dungeon.Runtime.OutGame.CardDetail;
using Dungeon.Runtime.OutGame.CardDetail.UI;
using Dungeon.Runtime.OutGame.CharacterSelect;
using Dungeon.Runtime.OutGame.Collection.Card;
using Dungeon.Runtime.OutGame.Collection.Character;
using Dungeon.Runtime.OutGame.Collection.Relic;
using Dungeon.Runtime.OutGame.Collection.Relic.UI;
using Dungeon.Runtime.OutGame.InitialDeck;
using Dungeon.Runtime.OutGame.Main.Dialog;
using Dungeon.Runtime.OutGame.Main.Presentation;
using Dungeon.Runtime.OutGame.Main.View;
using Dungeon.Runtime.OutGame.RelicDetail;
using Dungeon.Runtime.OutGame.Result;
using Dungeon.Runtime.OutGame.Settings;
using TFramework.Debug;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Main
{
    /// <summary>
    /// ロビーから各メニュー画面への遷移と表示接続を調整
    /// </summary>
    public sealed class MainSceneUiCoordinator : IDisposable
    {
        private readonly IUIService _ui;
        private readonly IMainMenuDataSource _dataSource;
        private readonly IMainMenuActionHandler _actions;
        private readonly HashSet<Type> _openDialogs = new HashSet<Type>();
        private readonly List<CanvasGroup> _dialogLayers = new List<CanvasGroup>();
        private CancellationTokenSource _lifetime;
        private ScreenInputBindings _lobbyBindings;
        private MainSceneView _view;
        private MainMenuData _data;
        private MainMenuCharacterData _selectedCharacter;
        private bool _pageTransition;
        private bool _bgmEnabled;
        private bool _soundEffectEnabled;

        public bool IsInitialized => _lobbyBindings != null;

        public MainSceneUiCoordinator(IUIService ui, IMainMenuDataSource dataSource, IMainMenuActionHandler actions)
        {
            _ui = ui;
            _dataSource = dataSource;
            _actions = actions;
        }

        public async UniTask InitializeAsync(MainSceneView view, CancellationToken ct)
        {
            Dispose();
            _view = view != null ? view : throw new ArgumentNullException(nameof(view));
            _lifetime = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _data = await _dataSource.LoadAsync(_lifetime.Token);
            _bgmEnabled = _data.BgmEnabled;
            _soundEffectEnabled = _data.SoundEffectEnabled;
            await _ui.ClearStackAsync(_lifetime.Token);
            _view.SetVisible(true);
            _view.SelectHome();
            _lobbyBindings = new ScreenInputBindings();
            _lobbyBindings.Add(view.AdventureButton, () => Navigate(ShowAdventureEntryAsync));
            _lobbyBindings.Add(view.SettingsButton, () => Navigate(ShowSettingsAsync));
            _lobbyBindings.Add(view.CardCollectionButton, () => Navigate(ShowCardsAsync));
            _lobbyBindings.Add(view.RelicCollectionButton, () => Navigate(ShowRelicsAsync));
            _lobbyBindings.Add(view.CharacterCollectionButton, () => Navigate(ShowCharactersAsync));
            _lobbyBindings.Add(view.ProfileButton, () => Navigate(ShowCharactersAsync));
            _lobbyBindings.Add(view.HomeToggle, selected =>
            {
                if (selected) Navigate(ShowHomeAsync);
            });
            _lobbyBindings.Add(view.HistoryToggle, selected =>
            {
                if (selected) Navigate(ShowResultAsync);
            });
            _lobbyBindings.Add(view.ShopToggle, selected =>
            {
                if (selected) Navigate(ShowShopAsync);
            });
        }

        private void Navigate(Func<CancellationToken, UniTask> navigation)
        {
            if (_lifetime == null || _lifetime.IsCancellationRequested) return;
            NavigateAsync(navigation, _lifetime.Token).Forget(ex =>
            {
                if (ex is OperationCanceledException)
                    TLogger.Debug("Menu navigation cancelled", "Main");
                else
                    TLogger.Error(ex.ToString(), "Main");
            });
        }

        private async UniTask NavigateAsync(Func<CancellationToken, UniTask> navigation, CancellationToken ct)
        {
            int selectedId = _selectedCharacter?.Id ?? 0;
            _data = await _dataSource.LoadAsync(ct);
            _selectedCharacter = _data.Characters.FirstOrDefault(character => character.Id == selectedId);
            await navigation(ct);
        }

        private async UniTask ShowDialogAsync<T>(Func<T, ScreenInputBindings> configure, CancellationToken ct)
            where T : BoundDialogBase
        {
            if (!_openDialogs.Add(typeof(T))) return;
            CanvasGroup previous = _dialogLayers.LastOrDefault() ?? _ui.CurrentPage?.CanvasGroup ?? _view.LobbyGroup;
            SetInteractable(previous, false);
            bool opened = false;
            var binding = new ScreenBinding<T>(dialog =>
            {
                opened = true;
                _dialogLayers.Add(dialog.CanvasGroup);
                ScreenInputBindings controls = configure(dialog);
                controls.OnDispose(() => _dialogLayers.Remove(dialog.CanvasGroup));
                return controls;
            });
            try
            {
                await _ui.ShowDialogAsync<T>(new UIDialogOpenParam(binding, true), ct);
                if (!opened) throw new InvalidOperationException("Could not open " + typeof(T).Name);
            }
            finally
            {
                binding.Dispose();
                _openDialogs.Remove(typeof(T));
                if (!ct.IsCancellationRequested) SetInteractable(previous, true);
            }
        }

        private async UniTask ShowPageAsync<T>(Func<T, ScreenInputBindings> configure, CancellationToken ct)
            where T : BoundPageBase
        {
            if (_pageTransition) return;
            _pageTransition = true;
            _view.SetVisible(false);
            try
            {
                await _ui.ShowPageAsync<T>(new ScreenBinding<T>(configure), ct);
                if (!(_ui.CurrentPage is T)) throw new InvalidOperationException("Could not open " + typeof(T).Name);
            }
            finally
            {
                _pageTransition = false;
                if (_ui.CurrentPage == null && !ct.IsCancellationRequested) _view.SetVisible(true);
            }
        }

        private async UniTask ShowHomeAsync(CancellationToken ct)
        {
            if (_pageTransition) return;
            _pageTransition = true;
            try
            {
                await _ui.ClearStackAsync(ct);
                _view.SetVisible(true);
                _view.SelectHome();
            }
            finally
            {
                _pageTransition = false;
            }
        }

        private async UniTask ShowAdventureEntryAsync(CancellationToken ct)
        {
            if (!_data.HasResumableAdventure)
            {
                await ShowCharacterSelectAsync(ct);
                return;
            }

            int choice = 0;
            await ShowDialogAsync<ResumeDialog>(dialog =>
            {
                var controls = CloseControls(dialog.CloseButton, dialog.Close);
                controls.Add(dialog.ContinueButton, () =>
                {
                    choice = 1;
                    dialog.Close();
                });
                controls.Add(dialog.NewAdventureButton, () =>
                {
                    choice = 2;
                    dialog.Close();
                });
                return controls;
            }, ct);
            if (choice == 1) await RequestActionAsync(MainMenuAction.ResumeAdventure, ct);
            if (choice == 2 && await ConfirmAsync("新しい冒険を始めますか？\n途中の冒険は再開できなくなります。", "やめる", "新しくはじめる", ct))
                await ShowCharacterSelectAsync(ct);
        }

        private UniTask ShowCharacterSelectAsync(CancellationToken ct)
        {
            return ShowPageAsync<CharacterSelectPage>(page =>
            {
                var controls = new ScreenInputBindings();
                controls.Add(page.BackButton, () => Navigate(ShowHomeAsync));
                controls.Add(page.InitialDeckButton, () => Navigate(ShowDeckAsync));
                controls.Add(page.DepartButton,
                    () => Navigate(token => RequestActionAsync(MainMenuAction.StartAdventure, token)));
                MainMenuCharacterData first = _data.Characters.FirstOrDefault(c => c.IsUnlocked);
                _selectedCharacter = first;
                page.Roster.Bind(_data.Characters, (cell, character, index) =>
                {
                    Toggle toggle = cell.GetComponent<Toggle>();
                    toggle.group = null;
                    toggle.isOn = false;
                    toggle.group = page.CharacterToggleGroup;
                    toggle.interactable = character.IsUnlocked;
                    Image portrait = cell.Find("Image").GetComponent<Image>();
                    portrait.sprite = character.Artwork;
                    cell.Find("Label").GetComponent<TFTextUGUI>().text = character.Name;
                    toggle.isOn = character == first;
                    controls.Add(toggle, selected =>
                    {
                        if (selected) { _selectedCharacter = character; ApplyCharacter(page, character); }
                    });
                });
                controls.OnDispose(page.Roster.Clear);
                page.DepartButton.interactable = first != null;
                page.InitialDeckButton.interactable = first != null;
                if (first != null) ApplyCharacter(page, first);
                else
                {
                    page.NameText.text = "選択できるキャラクターがありません";
                    page.CharacterImage.sprite = null;
                    page.DescriptionText.text = string.Empty;
                    page.HealthText.text = page.GoldText.text = page.DeckCountText.text = string.Empty;
                }
                return controls;
            }, ct);
        }

        private static void ApplyCharacter(CharacterSelectPage page, MainMenuCharacterData character)
        {
            page.CharacterImage.sprite = character.Artwork;
            page.NameText.text = character.Name;
            page.DescriptionText.text = character.Description;
            page.HealthText.text = character.Health.ToString();
            page.GoldText.text = character.Gold.ToString();
            page.DeckCountText.text = character.Deck.Sum(entry => entry.Quantity) + "枚";
        }

        private UniTask ShowCardsAsync(CancellationToken ct)
        {
            return ShowDialogAsync<CardCollectionDialog>(dialog =>
            {
                var controls = CloseControls(dialog.CloseButton, dialog.Close);
                dialog.DiscoveryCountText.text = _data.Cards.Count(card => card.IsDiscovered) + " / " + _data.Cards.Length;
                controls.OnDispose(dialog.Grid.Bind(_data.Cards, (cell, card, index) =>
                {
                    var item = cell.GetComponent<CardListItem>();
                    item.SetQuantity(0);
                    MainMenuUiPresentation.Card(item.Card, card);
                    var input = new ScreenInputBindings();
                    if (card.IsDiscovered) input.Add(item.Card.GetComponent<Button>(), () => Navigate(token => ShowCardAsync(card, token)));
                    return input;
                }).Dispose);
                return controls;
            }, ct);
        }

        private UniTask ShowDeckAsync(CancellationToken ct)
        {
            return ShowDialogAsync<InitialDeckDialog>(dialog =>
            {
                var controls = CloseControls(dialog.CloseButton, dialog.Close);
                MainMenuCharacterData character = _selectedCharacter ?? _data.Characters.FirstOrDefault(c => c.IsUnlocked);
                MainMenuDeckEntry[] cards = character?.Deck ?? Array.Empty<MainMenuDeckEntry>();
                dialog.SummaryText.text = (character?.Name ?? string.Empty) + "　合計" + cards.Sum(entry => entry.Quantity) + "枚";
                controls.OnDispose(dialog.Grid.Bind(cards, (cell, entry, index) =>
                {
                    var item = cell.GetComponent<CardListItem>();
                    MainMenuUiPresentation.Card(item.Card, entry.Card);
                    item.SetQuantity(entry.Quantity);
                    var input = new ScreenInputBindings();
                    input.Add(item.Card.GetComponent<Button>(), () => Navigate(token => ShowCardAsync(entry.Card, token)));
                    return input;
                }).Dispose);
                return controls;
            }, ct);
        }

        private UniTask ShowCardAsync(MainMenuCardData card, CancellationToken ct)
        {
            return ShowDialogAsync<CardDetailDialog>(dialog =>
            {
                var controls = CloseControls(dialog.CloseButton, dialog.Close);

                void Apply(bool upgraded)
                {
                    MainMenuUiPresentation.Card(dialog.CardIcon, card, upgraded);
                    MainMenuKeywordData[] keywords = upgraded ? card.UpgradedKeywords : card.Keywords;
                    dialog.Keywords.Bind(keywords, (cell, keyword, index) =>
                    {
                        var tooltip = cell.GetComponent<KeywordTooltip>();
                        tooltip.TitleText.text = keyword.Title;
                        tooltip.DescriptionText.text = keyword.Description;
                    });
                }

                dialog.UpgradedToggle.interactable = card.HasUpgrade;
                controls.OnDispose(dialog.Keywords.Clear);
                dialog.NormalToggle.isOn = true;
                dialog.UpgradedToggle.isOn = false;
                controls.Add(dialog.NormalToggle, selected =>
                {
                    if (selected) Apply(false);
                });
                controls.Add(dialog.UpgradedToggle, selected =>
                {
                    if (selected) Apply(true);
                });
                Apply(false);
                return controls;
            }, ct);
        }

        private UniTask ShowRelicsAsync(CancellationToken ct)
        {
            return ShowDialogAsync<RelicCollectionDialog>(dialog =>
            {
                var controls = CloseControls(dialog.CloseButton, dialog.Close);
                dialog.DiscoveryCountText.text = _data.Relics.Count(r => r.IsDiscovered) + " / " + _data.Relics.Length;
                controls.OnDispose(dialog.Grid.Bind(_data.Relics, (cell, relic, index) =>
                {
                    var item = cell.GetComponent<RelicCollectionItem>();
                    item.NameText.text = relic.IsDiscovered ? relic.Name : "？？？";
                    item.ArtImage.sprite = relic.Artwork;
                    item.ArtImage.color = relic.IsDiscovered ? Color.white : new Color(0.14f, 0.11f, 0.28f);
                    item.RarityFrame.color = relic.FrameColor;
                    item.Button.interactable = relic.IsDiscovered;
                    var input = new ScreenInputBindings();
                    if (relic.IsDiscovered) input.Add(item.Button, () => Navigate(token => ShowRelicAsync(relic, token)));
                    return input;
                }).Dispose);
                return controls;
            }, ct);
        }

        private UniTask ShowRelicAsync(MainMenuRelicData relic, CancellationToken ct)
        {
            return ShowDialogAsync<RelicDetailDialog>(dialog =>
            {
                dialog.TitleText.text = relic.Name;
                dialog.IconView.ArtImage.sprite = relic.Artwork;
                dialog.IconView.RarityFrame.color = relic.FrameColor;
                dialog.ConditionText.text = relic.Description;
                dialog.EffectText.text = relic.EffectText;
                return CloseControls(dialog.CloseButton, dialog.Close);
            }, ct);
        }

        private UniTask ShowCharactersAsync(CancellationToken ct)
        {
            return ShowDialogAsync<CharacterCollectionDialog>(dialog =>
            {
                var controls = CloseControls(dialog.CloseButton, dialog.Close);
                int index = 0;

                void Apply()
                {
                    MainMenuCharacterData character = _data.Characters[index];
                    dialog.SetLocked(!character.IsUnlocked);
                    dialog.NameText.text = character.Name;
                    dialog.DescriptionText.text = string.IsNullOrEmpty(character.Biography) ? character.Description : character.Biography;
                    dialog.CombatText.text = character.CombatDescription;
                    dialog.PortraitImage.sprite = character.Artwork;
                    dialog.InitialDeckButton.interactable = character.IsUnlocked;
                    dialog.PreviousButton.interactable = index > 0;
                    dialog.NextButton.interactable = index + 1 < _data.Characters.Length;
                }

                controls.Add(dialog.PreviousButton, () =>
                {
                    index = Math.Max(0, index - 1);
                    Apply();
                });
                controls.Add(dialog.NextButton, () =>
                {
                    index = Math.Min(_data.Characters.Length - 1, index + 1);
                    Apply();
                });
                controls.Add(dialog.InitialDeckButton, () => { _selectedCharacter = _data.Characters[index]; Navigate(ShowDeckAsync); });
                if (_data.Characters.Length > 0) Apply();
                else { dialog.PreviousButton.interactable = false; dialog.NextButton.interactable = false; dialog.InitialDeckButton.interactable = false; dialog.SetLocked(true); dialog.NameText.text = "キャラクターがありません"; }
                return controls;
            }, ct);
        }

        private UniTask ShowResultAsync(CancellationToken ct)
        {
            return ShowPageAsync<AdventureResultPage>(page =>
            {
                page.HeadlineText.text = _data.ResultHeadline;
                page.FloorText.text = _data.ReachedFloor.ToString();
                page.NormalEnemyCountText.text = _data.EnemyCount.ToString();
                page.EliteEnemyCountText.text = _data.EliteCount.ToString();
                page.BossCountText.text = _data.BossCount.ToString();
                var controls = new ScreenInputBindings();
                controls.Add(page.HomeButton, () => Navigate(ShowHomeAsync));
                controls.Add(page.InventoryButton, () => Navigate(ShowInventoryAsync));
                return controls;
            }, ct);
        }

        private async UniTask ShowInventoryAsync(CancellationToken ct)
        {
            bool relics = await ConfirmAsync("カード・レリック一覧", "カード", "レリック", ct);
            if (relics) await ShowRelicsAsync(ct);
            else await ShowCardsAsync(ct);
        }

        private UniTask ShowSettingsAsync(CancellationToken ct)
        {
            return ShowDialogAsync<SettingsDialog>(dialog =>
            {
                var controls = CloseControls(dialog.CloseButton, dialog.Close);
                dialog.BgmToggle.Toggle.isOn = _bgmEnabled;
                dialog.SoundEffectToggle.Toggle.isOn = _soundEffectEnabled;
                controls.Add(dialog.BgmToggle.Toggle, value => _bgmEnabled = value);
                controls.Add(dialog.SoundEffectToggle.Toggle, value => _soundEffectEnabled = value);
                dialog.VersionText.text = "Ver. " + _data.Version;
                dialog.GameIdText.text = "ゲームID " + _data.GameId;

                void Bind(Button button, MainMenuAction action) => controls.Add(button,
                    () => Navigate(token => RequestActionAsync(action, token)));

                Bind(dialog.LanguageButton, MainMenuAction.Language);
                Bind(dialog.AccountButton, MainMenuAction.Account);
                Bind(dialog.ClearCacheButton, MainMenuAction.ClearCache);
                Bind(dialog.SupportButton, MainMenuAction.Support);
                Bind(dialog.CreditsButton, MainMenuAction.Credits);
                Bind(dialog.LogoutButton, MainMenuAction.Logout);
                Bind(dialog.TermsButton, MainMenuAction.Terms);
                Bind(dialog.PrivacyButton, MainMenuAction.Privacy);
                return controls;
            }, ct);
        }

        private async UniTask ShowShopAsync(CancellationToken ct)
        {
            await RequestActionAsync(MainMenuAction.Shop, ct);
            _view.SelectHome();
        }

        private async UniTask RequestActionAsync(MainMenuAction action, CancellationToken ct)
        {
            if (action == MainMenuAction.ClearCache || action == MainMenuAction.Logout)
            {
                string message = action == MainMenuAction.ClearCache ? "キャッシュを削除しますか？" : "ログアウトしますか？";
                if (!await ConfirmAsync(message, "やめる", "確認", ct)) return;
            }

            if (!await _actions.ExecuteAsync(action, ct))
                await ConfirmAsync("ただいま準備中です。", null, "OK", ct);
        }

        private async UniTask<bool> ConfirmAsync(string body, string cancel, string confirm, CancellationToken ct)
        {
            bool accepted = false;
            await ShowDialogAsync<CommonConfirmDialog>(dialog =>
            {
                dialog.ApplyText(body, cancel, confirm);
                var controls = new ScreenInputBindings();
                controls.Add(dialog.CancelButton, dialog.Close);
                controls.Add(dialog.ConfirmButton, () =>
                {
                    accepted = true;
                    dialog.Close();
                });
                return controls;
            }, ct);
            return accepted;
        }

        private static ScreenInputBindings CloseControls(Button close, Action onClose)
        {
            var controls = new ScreenInputBindings();
            controls.Add(close, onClose);
            return controls;
        }

        private static void SetInteractable(CanvasGroup group, bool enabled)
        {
            if (group == null) return;
            group.interactable = enabled;
            group.blocksRaycasts = enabled;
        }

        public void Dispose()
        {
            _lifetime?.Cancel();
            _lifetime?.Dispose();
            _lifetime = null;
            _lobbyBindings?.Dispose();
            _lobbyBindings = null;
            _dialogLayers.Clear();
            _openDialogs.Clear();
        }
    }
}
