#if UNITY_EDITOR
using System.Collections;
using Dungeon.Runtime.OutGame.Main.UI;
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
            Toggle[] toggles = instance.transform.Find("CharacterRoster").GetComponentsInChildren<Toggle>();
            Assert.That(toggles.Length, Is.EqualTo(3));
            Assert.That(toggles[0].isOn, Is.True);
            Assert.That(toggles[0].interactable, Is.True);
            for (int i = 1; i < toggles.Length; i++)
            {
                Assert.That(toggles[i].isOn, Is.False);
                Assert.That(toggles[i].interactable, Is.False);
                Assert.That(toggles[i].graphic.canvasRenderer.GetAlpha(), Is.Zero.Within(0.01f));
            }
        }

        [UnityTest]
        public IEnumerator CardCollection_ScrollsItsClippedContent()
        {
            GameObject instance = Instantiate("CardCollectionDialog");
            yield return null;
            Canvas.ForceUpdateCanvases();
            ScrollRect scroll = instance.GetComponentInChildren<ScrollRect>();
            Assert.That(scroll.viewport.GetComponent<RectMask2D>(), Is.Not.Null);
            Assert.That(scroll.content.childCount, Is.EqualTo(11));
            Assert.That(scroll.content.rect.height, Is.GreaterThan(scroll.viewport.rect.height * 2));

            scroll.verticalNormalizedPosition = 0;
            yield return null;
            Assert.That(scroll.content.anchoredPosition.y, Is.GreaterThan(1000));
            Assert.That(scroll.verticalNormalizedPosition, Is.Zero.Within(0.01f));
        }

        private GameObject Instantiate(string name)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/OutGame/UI/{name}.prefab");
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
