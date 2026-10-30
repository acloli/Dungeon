#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using Dungeon.Runtime.OutGame.Collection.Character;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Dungeon.Tests.PlayMode
{
    public sealed class OutGameUiPlayModeTests
    {
        private GameObject _canvasObject;

        [SetUp]
        public void SetUp()
        {
            _canvasObject = new GameObject("OutGameUiTestCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            _canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = _canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(_canvasObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AudioToggle_ChangesOnlyItsVisibleState()
        {
            GameObject instance = Instantiate("SettingsDialog");
            yield return null;
            Transform audio = instance.transform.Find("Sheet/Audio/BgmToggle");
            Toggle toggle = audio.GetComponent<Toggle>();

            toggle.isOn = false;
            yield return null;
            Assert.That(audio.Find("OnState").gameObject.activeSelf, Is.False);
            Assert.That(audio.Find("OffState").gameObject.activeSelf, Is.True);

            toggle.isOn = true;
            yield return null;
            Assert.That(audio.Find("OnState").gameObject.activeSelf, Is.True);
            Assert.That(audio.Find("OffState").gameObject.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator CardVersionSwitch_SelectsOneVisibleState()
        {
            GameObject instance = Instantiate("CardDetailDialog");
            yield return null;
            Transform normal = instance.transform.Find("Content/VersionSwitch/NormalToggle");
            Transform upgraded = instance.transform.Find("Content/VersionSwitch/UpgradedToggle");
            Assert.That(normal.GetComponent<Toggle>().isOn, Is.True);
            Assert.That(upgraded.GetComponent<Toggle>().isOn, Is.False);

            upgraded.GetComponent<Toggle>().isOn = true;
            yield return null;
            Assert.That(normal.GetComponent<Toggle>().isOn, Is.False);
            Assert.That(upgraded.GetComponent<Toggle>().isOn, Is.True);
            Assert.That(normal.Find("OnState").gameObject.activeSelf, Is.False);
            Assert.That(upgraded.Find("OnState").gameObject.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator CharacterCollection_SwitchesProfileObjects()
        {
            GameObject instance = Instantiate("CharacterCollectionDialog");
            yield return null;
            CharacterCollectionDialog view = instance.GetComponent<CharacterCollectionDialog>();
            view.SetLocked(true);
            Assert.That(view.UnlockedProfile.activeSelf, Is.False);
            Assert.That(view.LockedProfile.activeSelf, Is.True);
            view.SetLocked(false);
            Assert.That(view.UnlockedProfile.activeSelf, Is.True);
            Assert.That(view.LockedProfile.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator CharacterRoster_SelectsTheAvailableCharacter()
        {
            GameObject instance = Instantiate("CharacterSelectPage");
            yield return null;
            var page = instance.GetComponent<Dungeon.Runtime.OutGame.CharacterSelect.CharacterSelectPage>();
            Assert.That(page.Roster.Root.childCount, Is.Zero);
            var data = new[] { "Available", "Locked", "Another" };
            page.Roster.Bind(data, (cell, name, index) =>
            {
                var toggle = cell.GetComponent<Toggle>();
                toggle.group = null;
                toggle.isOn = false;
                toggle.group = page.CharacterToggleGroup;
                toggle.interactable = index == 0;
                toggle.isOn = index == 0;
            });
            yield return null;
            Toggle[] toggles = page.CharacterToggleGroup.GetComponentsInChildren<Toggle>();
            Assert.That(toggles.Length, Is.EqualTo(data.Length));
            Assert.That(toggles[0].isOn, Is.True);
            Assert.That(toggles[0].interactable, Is.True);
            Assert.That(toggles[1].interactable, Is.False);
            page.Roster.Bind(new[] { "Only" }, (cell, name, index) => { });
            Assert.That(page.CharacterToggleGroup.GetComponentsInChildren<Toggle>().Length, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator CardCollection_RecyclesClippedCells_AndClearsStaleInput()
        {
            GameObject instance = Instantiate("CardCollectionDialog");
            yield return null;
            Canvas.ForceUpdateCanvases();
            var grid = instance.GetComponentInChildren<Dungeon.Runtime.Common.UI.List.RecyclingGridView>();
            ScrollRect scroll = grid.ScrollRect;
            Assert.That(scroll.content.childCount, Is.Zero);
            Assert.That(scroll.viewport.GetComponent<RectMask2D>(), Is.Not.Null);
            int clicked = -1;
            using var binding = grid.Bind(Enumerable.Range(0, 1000).ToArray(), (cell, value, index) =>
            {
                var input = new Dungeon.Runtime.Common.UI.Binding.ScreenInputBindings();
                input.Add(cell.GetComponentInChildren<Button>(), () => clicked = value);
                return input;
            });
            yield return null;
            Assert.That(grid.ActiveCellCount, Is.GreaterThan(0));
            Assert.That(scroll.content.childCount, Is.LessThan(30));
            int capacity = scroll.content.childCount;
            scroll.content.GetChild(0).GetComponentInChildren<Button>().onClick.Invoke();
            Assert.That(clicked, Is.Zero);
            scroll.verticalNormalizedPosition = 0;
            yield return null;
            Assert.That(scroll.content.anchoredPosition.y, Is.GreaterThan(1000));
            scroll.content.GetChild(0).GetComponentInChildren<Button>().onClick.Invoke();
            Assert.That(clicked, Is.GreaterThan(900));
            Assert.That(scroll.content.childCount, Is.EqualTo(capacity));
            using var smaller = grid.Bind(new[] { 23 }, (cell, value, index) =>
            {
                var input = new Dungeon.Runtime.Common.UI.Binding.ScreenInputBindings();
                input.Add(cell.GetComponentInChildren<Button>(), () => clicked = value);
                return input;
            });
            binding.Dispose();
            Assert.That(grid.ActiveCellCount, Is.EqualTo(1));
            scroll.content.GetChild(0).GetComponentInChildren<Button>().onClick.Invoke();
            Assert.That(clicked, Is.EqualTo(23));
            smaller.Dispose();
            Assert.That(grid.ActiveCellCount, Is.Zero);
            Assert.That(scroll.content.GetComponentsInChildren<Button>().Length, Is.Zero);
        }

        private GameObject Instantiate(string name)
        {
            string[] paths = AssetDatabase.FindAssets(name + " t:Prefab", new[] { "Assets/Prefabs/OutGame", "Assets/Prefabs/Common" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => System.IO.Path.GetFileNameWithoutExtension(path) == name).ToArray();
            Assert.That(paths.Length, Is.EqualTo(1), name);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(paths[0]);
            Assert.That(prefab, Is.Not.Null);
            GameObject instance = Object.Instantiate(prefab, _canvasObject.transform, false);
            RectTransform rect = instance.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return instance;
        }
    }
}
#endif
