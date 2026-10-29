#if UNITY_EDITOR
using Dungeon.Runtime.Common.UI.Dialog;
using System.Collections;
using System.Linq;
using Dungeon.Runtime.InGame.Battle.View;
using Dungeon.Runtime.OutGame.Main;
using Dungeon.Runtime.OutGame.Main.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Dungeon.Tests.PlayMode
{
    public sealed class MainMenuNavigationPlayModeTests
    {
        private MainSceneView _lobby;
        private bool _loadedScene;
        private MainMenuPreviewContent _content;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Scene main = SceneManager.GetSceneByPath("Assets/Scenes/MainScene.unity");
            _loadedScene = !main.isLoaded;
            if (_loadedScene) yield return SceneManager.LoadSceneAsync("Assets/Scenes/MainScene.unity", LoadSceneMode.Additive);
            for (int i = 0; i < 900; i++)
            {
                MainSceneController controller = Object.FindFirstObjectByType<MainSceneController>();
                if (controller != null && controller.IsMenuReady) break;
                if (i == 899) Assert.Fail("MainScene menu initialization timed out");
                yield return null;
            }
            _lobby = Object.FindFirstObjectByType<MainSceneView>();
            _content = AssetDatabase.LoadAssetAtPath<MainMenuPreviewContent>("Assets/Settings/OutGame/MainMenuPreviewContent.asset");
            Assert.That(_lobby.LobbyGroup, Is.Not.Null);
            Assert.That(_lobby.LobbyGroup.interactable, Is.True);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (BoundDialogBase dialog in Object.FindObjectsByType<BoundDialogBase>(FindObjectsSortMode.None)) dialog.Close();
            for (int i = 0; i < 25; i++) yield return null;
            if (_lobby != null)
            {
                _lobby.HomeToggle.isOn = false;
                _lobby.HomeToggle.isOn = true;
                for (int i = 0; i < 10; i++) yield return null;
            }
            if (_loadedScene) yield return SceneManager.UnloadSceneAsync("Assets/Scenes/MainScene.unity");
        }

        [UnityTest]
        public IEnumerator Collections_OpenNestedDetails_AndRestoreInput()
        {
            _lobby.CardCollectionButton.onClick.Invoke();
            _lobby.CardCollectionButton.onClick.Invoke();
            yield return WaitFor<CardCollectionDialog>();
            CardCollectionDialog cards = FindReady<CardCollectionDialog>();
            Assert.That(Object.FindObjectsByType<CardCollectionDialog>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(_lobby.LobbyGroup.interactable, Is.False);
            cards.ScrollRect.content.GetChild(0).GetComponentInChildren<BattleCardIconView>().GetComponent<Button>().onClick.Invoke();
            yield return WaitFor<CardDetailDialog>();
            CardDetailDialog detail = FindReady<CardDetailDialog>();
            Assert.That(cards.CanvasGroup.interactable, Is.False);
            Assert.That(CardName(detail), Is.EqualTo(_content.Data.Cards[0].Name));
            detail.UpgradedToggle.isOn = true;
            Assert.That(CardName(detail), Is.EqualTo(_content.Data.Cards[0].Name + "+"));
            detail.CloseButton.onClick.Invoke();
            yield return WaitFor<CardCollectionDialog>();
            cards.ScrollRect.content.GetChild(3).GetComponentInChildren<BattleCardIconView>().GetComponent<Button>().onClick.Invoke();
            yield return WaitFor<CardDetailDialog>();
            detail = FindReady<CardDetailDialog>();
            Assert.That(detail.NormalToggle.isOn, Is.True);
            Assert.That(detail.transform.Find("Content/VersionSwitch/NormalToggle/OnState").gameObject.activeSelf, Is.True);
            Assert.That(detail.KeywordRoot.GetComponentsInChildren<KeywordTooltip>().Length, Is.EqualTo(3));
            detail.CloseButton.onClick.Invoke();
            yield return WaitFor<CardCollectionDialog>();
            cards.CloseButton.onClick.Invoke();
            yield return WaitForLobby();

            _lobby.RelicCollectionButton.onClick.Invoke();
            yield return WaitFor<RelicCollectionDialog>();
            RelicCollectionDialog relics = FindReady<RelicCollectionDialog>();
            relics.ScrollRect.content.GetChild(2).GetComponent<RelicCollectionItem>().Button.onClick.Invoke();
            yield return WaitFor<RelicDetailDialog>();
            RelicDetailDialog relic = FindReady<RelicDetailDialog>();
            Assert.That(relic.TitleText.text, Is.EqualTo(_content.Data.Relics[2].Name));
            Assert.That(relic.EffectText.text, Is.EqualTo(_content.Data.Relics[2].EffectText));
            relic.CloseButton.onClick.Invoke();
            yield return WaitFor<RelicCollectionDialog>();
            relics.CloseButton.onClick.Invoke();
            yield return WaitForLobby();

            _lobby.CharacterCollectionButton.onClick.Invoke();
            yield return WaitFor<CharacterCollectionDialog>();
            CharacterCollectionDialog characters = FindReady<CharacterCollectionDialog>();
            characters.NextButton.onClick.Invoke();
            Assert.That(characters.LockedProfile.activeSelf, Is.True);
            Assert.That(characters.UnlockedProfile.activeSelf, Is.False);
            characters.PreviousButton.onClick.Invoke();
            Assert.That(characters.UnlockedProfile.activeSelf, Is.True);
            characters.InitialDeckButton.onClick.Invoke();
            yield return WaitFor<InitialDeckDialog>();
            InitialDeckDialog deck = FindReady<InitialDeckDialog>();
            deck.ScrollRect.content.GetChild(1).GetComponentInChildren<BattleCardIconView>().GetComponent<Button>().onClick.Invoke();
            yield return WaitFor<CardDetailDialog>();
            Assert.That(CardName(FindReady<CardDetailDialog>()), Is.EqualTo(_content.Data.Cards[1].Name));
            FindReady<CardDetailDialog>().CloseButton.onClick.Invoke();
            yield return WaitFor<InitialDeckDialog>();
            deck.CloseButton.onClick.Invoke();
            yield return WaitFor<CharacterCollectionDialog>();
            characters.CloseButton.onClick.Invoke();
            yield return WaitForLobby();
        }

        [UnityTest]
        public IEnumerator AdventureAndHistory_ReturnToLobby_WithoutStartingGame()
        {
            _lobby.AdventureButton.onClick.Invoke();
            yield return WaitFor<ResumeDialog>();
            FindReady<ResumeDialog>().NewAdventureButton.onClick.Invoke();
            yield return WaitFor<CommonConfirmDialog>();
            FindReady<CommonConfirmDialog>().CancelButton.onClick.Invoke();
            yield return WaitForLobby();
            _lobby.AdventureButton.onClick.Invoke();
            yield return WaitFor<ResumeDialog>();
            FindReady<ResumeDialog>().NewAdventureButton.onClick.Invoke();
            yield return WaitFor<CommonConfirmDialog>();
            FindReady<CommonConfirmDialog>().ConfirmButton.onClick.Invoke();
            yield return WaitFor<CharacterSelectPage>();
            CharacterSelectPage selection = FindReady<CharacterSelectPage>();
            Assert.That(_lobby.LobbyGroup.alpha, Is.Zero);
            selection.InitialDeckButton.onClick.Invoke();
            yield return WaitFor<InitialDeckDialog>();
            FindReady<InitialDeckDialog>().CloseButton.onClick.Invoke();
            yield return WaitFor<CharacterSelectPage>();
            selection.DepartButton.onClick.Invoke();
            yield return WaitFor<CommonConfirmDialog>();
            Assert.That(SceneManager.GetSceneByName("BattleScene").isLoaded, Is.False);
            FindReady<CommonConfirmDialog>().ConfirmButton.onClick.Invoke();
            yield return WaitFor<CharacterSelectPage>();
            selection.BackButton.onClick.Invoke();
            yield return WaitForLobby();
            _lobby.HistoryToggle.isOn = true;
            yield return WaitFor<AdventureResultPage>();
            AdventureResultPage result = FindReady<AdventureResultPage>();
            result.InventoryButton.onClick.Invoke();
            yield return WaitFor<CommonConfirmDialog>();
            FindReady<CommonConfirmDialog>().ConfirmButton.onClick.Invoke();
            yield return WaitFor<RelicCollectionDialog>();
            FindReady<RelicCollectionDialog>().CloseButton.onClick.Invoke();
            yield return WaitFor<AdventureResultPage>();
            result.HomeButton.onClick.Invoke();
            yield return WaitForLobby();
            Assert.That(_lobby.HomeToggle.isOn, Is.True);
            _lobby.HistoryToggle.isOn = true;
            yield return WaitFor<AdventureResultPage>();
            FindReady<AdventureResultPage>().HomeButton.onClick.Invoke();
            yield return WaitForLobby();
        }

        [UnityTest]
        public IEnumerator Settings_RetainSessionToggles_AndRestoreTheParentDialog()
        {
            _lobby.SettingsButton.onClick.Invoke();
            yield return WaitFor<SettingsDialog>();
            SettingsDialog settings = FindReady<SettingsDialog>();
            settings.BgmToggle.Toggle.isOn = false;
            settings.SoundEffectToggle.Toggle.isOn = true;
            settings.CloseButton.onClick.Invoke();
            yield return WaitForLobby();
            _lobby.SettingsButton.onClick.Invoke();
            yield return WaitFor<SettingsDialog>();
            settings = FindReady<SettingsDialog>();
            Assert.That(settings.BgmToggle.Toggle.isOn, Is.False);
            Assert.That(settings.SoundEffectToggle.Toggle.isOn, Is.True);
            settings.AccountButton.onClick.Invoke();
            yield return WaitFor<CommonConfirmDialog>();
            Assert.That(settings.CanvasGroup.interactable, Is.False);
            Assert.That(FindReady<CommonConfirmDialog>().CancelButton.gameObject.activeSelf, Is.False);
            var confirmRect = (RectTransform)FindReady<CommonConfirmDialog>().ConfirmButton.transform;
            float buttonCenter = confirmRect.TransformPoint(confirmRect.rect.center).x;
            var actionsRect = (RectTransform)confirmRect.parent;
            Assert.That(buttonCenter, Is.EqualTo(actionsRect.TransformPoint(actionsRect.rect.center).x).Within(0.1f));
            FindReady<CommonConfirmDialog>().ConfirmButton.onClick.Invoke();
            yield return WaitFor<SettingsDialog>();
            settings.ClearCacheButton.onClick.Invoke();
            yield return WaitFor<CommonConfirmDialog>();
            FindReady<CommonConfirmDialog>().CancelButton.onClick.Invoke();
            yield return WaitFor<SettingsDialog>();
            settings.LogoutButton.onClick.Invoke();
            yield return WaitFor<CommonConfirmDialog>();
            FindReady<CommonConfirmDialog>().ConfirmButton.onClick.Invoke();
            yield return WaitFor<CommonConfirmDialog>();
            FindReady<CommonConfirmDialog>().ConfirmButton.onClick.Invoke();
            yield return WaitFor<SettingsDialog>();
            settings.CloseButton.onClick.Invoke();
            yield return WaitForLobby();
        }

        private static string CardName(CardDetailDialog detail)
        {
            var view = new SerializedObject(detail.CardIcon);
            return ((TFramework.UI.TFTextUGUI)view.FindProperty("_nameText").objectReferenceValue).text;
        }

        private IEnumerator WaitForLobby()
        {
            for (int i = 0; i < 600; i++)
            {
                if (_lobby.LobbyGroup.alpha > 0.99f && _lobby.LobbyGroup.interactable) yield break;
                yield return null;
            }
            Assert.Fail("Lobby did not become interactable");
        }

        private static T FindReady<T>() where T : Component
        {
            return Object.FindObjectsByType<T>(FindObjectsSortMode.None)
                .FirstOrDefault(view => view.GetComponent<CanvasGroup>().alpha > 0.99f && view.GetComponent<CanvasGroup>().interactable);
        }

        private static IEnumerator WaitFor<T>() where T : Component
        {
            for (int i = 0; i < 600; i++)
            {
                if (FindReady<T>() != null) yield break;
                yield return null;
            }
            Assert.Fail("Menu screen did not become ready: " + typeof(T).Name);
        }
    }
}
#endif
