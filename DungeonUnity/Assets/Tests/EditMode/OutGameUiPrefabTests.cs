using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace Dungeon.Tests.EditMode
{
    public sealed class OutGameUiPrefabTests
    {
        private static readonly Dictionary<string, string> PrefabPaths = new Dictionary<string, string>
        {
            { "CommonConfirmDialog", "Assets/Prefabs/Common/UI/Dialog/CommonConfirmDialog.prefab" },
            { "ResumeDialog", "Assets/Prefabs/OutGame/UI/ResumeDialog.prefab" },
            { "SettingsDialog", "Assets/Prefabs/OutGame/UI/SettingsDialog.prefab" },
            { "CommonBookDialog", "Assets/Prefabs/OutGame/UI/CommonBookDialog.prefab" },
            { "CardCollectionDialog", "Assets/Prefabs/OutGame/UI/CardCollectionDialog.prefab" },
            { "RelicCollectionDialog", "Assets/Prefabs/OutGame/UI/RelicCollectionDialog.prefab" },
            { "CharacterCollectionDialog", "Assets/Prefabs/OutGame/UI/CharacterCollectionDialog.prefab" },
            { "InitialDeckDialog", "Assets/Prefabs/OutGame/UI/InitialDeckDialog.prefab" },
            { "CardDetailDialog", "Assets/Prefabs/OutGame/UI/CardDetailDialog.prefab" },
            { "RelicDetailDialog", "Assets/Prefabs/OutGame/UI/RelicDetailDialog.prefab" },
            { "CharacterSelectPage", "Assets/Prefabs/OutGame/UI/CharacterSelectPage.prefab" },
            { "AdventureResultPage", "Assets/Prefabs/OutGame/UI/AdventureResultPage.prefab" },
            { "CollectionItemIcon", "Assets/Prefabs/OutGame/UI/CollectionItemIcon.prefab" },
            { "RelicCollectionItem", "Assets/Prefabs/OutGame/UI/RelicCollectionItem.prefab" },
            { "SettingsAudioToggle", "Assets/Prefabs/OutGame/UI/SettingsAudioToggle.prefab" },
            { "CardVersionToggle", "Assets/Prefabs/OutGame/UI/CardVersionToggle.prefab" },
            { "CharacterPortraitToggle", "Assets/Prefabs/OutGame/UI/CharacterPortraitToggle.prefab" },
            { "SettingsMenuButton", "Assets/Prefabs/OutGame/UI/SettingsMenuButton.prefab" },
            { "KeywordTooltip", "Assets/Prefabs/OutGame/UI/KeywordTooltip.prefab" },
        };

        [TestCase("CommonConfirmDialog", "CommonDialog")]
        [TestCase("ResumeDialog", "CommonDialog")]
        [TestCase("SettingsDialog", "CommonDialog")]
        [TestCase("CommonBookDialog", "CommonDialog")]
        [TestCase("CardCollectionDialog", "CommonBookDialog")]
        [TestCase("RelicCollectionDialog", "CommonBookDialog")]
        [TestCase("CharacterCollectionDialog", "CommonBookDialog")]
        [TestCase("InitialDeckDialog", "CommonDialog")]
        [TestCase("CardDetailDialog", "CommonDialog")]
        [TestCase("RelicDetailDialog", "CommonDialog")]
        [TestCase("CharacterSelectPage", "CommonPage")]
        [TestCase("AdventureResultPage", "CommonPage")]
        [TestCase("CollectionItemIcon", "MultiIcon")]
        [TestCase("RelicCollectionItem", "CollectionItemIcon")]
        [TestCase("SettingsAudioToggle", "CommonToggle")]
        [TestCase("CardVersionToggle", "CommonToggle")]
        [TestCase("CharacterPortraitToggle", "CommonToggle")]
        [TestCase("SettingsMenuButton", "CommonButton")]
        public void Prefab_InheritsExpectedBase(string name, string parentName)
        {
            GameObject prefab = Load(name);
            Assert.That(PrefabUtility.GetPrefabAssetType(prefab), Is.EqualTo(PrefabAssetType.Variant));
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
            Assert.That(source.name, Is.EqualTo(parentName));
        }

        [TestCase("CommonConfirmDialog")]
        [TestCase("ResumeDialog")]
        [TestCase("SettingsDialog")]
        [TestCase("CardCollectionDialog")]
        [TestCase("RelicCollectionDialog")]
        [TestCase("CharacterCollectionDialog")]
        [TestCase("InitialDeckDialog")]
        [TestCase("CardDetailDialog")]
        [TestCase("RelicDetailDialog")]
        [TestCase("CharacterSelectPage")]
        [TestCase("AdventureResultPage")]
        public void Screen_ClassPrefabAndAddressMatch(string name)
        {
            GameObject prefab = Load(name);
            Assert.That(prefab.name, Is.EqualTo(name));
            Assert.That(prefab.GetComponents<Component>().Any(c => c != null && c.GetType().Name == name), Is.True);
            string guid = AssetDatabase.AssetPathToGUID(PrefabPaths[name]);
            var entry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(guid);
            Assert.That(entry, Is.Not.Null);
            Assert.That(entry.address, Is.EqualTo(name));
        }

        [Test]
        public void Prefabs_HaveCompleteReferencesAndPortableDependencies()
        {
            string[] paths = PrefabPaths.Values.ToArray();
            Assert.That(paths, Is.Not.Empty);

            foreach (string path in paths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (Transform transform in prefab.GetComponentsInChildren<Transform>(true))
                {
                    Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject),
                        Is.Zero, $"{path}: {transform.name}");
                }

                foreach (Component component in prefab.GetComponentsInChildren<Component>(true))
                {
                    Type type = component.GetType();
                    Assert.That(type.Name, Is.Not.EqualTo("TextMeshProUGUI"), path);
                    Assert.That(type.FullName, Is.Not.EqualTo("UnityEngine.UI.Text"), path);
                    while (type != null && type.Namespace != null && type.Namespace.StartsWith("Dungeon.Runtime.", StringComparison.Ordinal))
                    {
                        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                        {
                            if (field.GetCustomAttribute<SerializeField>() != null &&
                                typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
                            {
                                Assert.That(field.GetValue(component), Is.Not.Null, $"{path}: {field.Name}");
                            }
                        }

                        type = type.BaseType;
                    }
                }

                foreach (string dependency in AssetDatabase.GetDependencies(path, true))
                {
                    Assert.That(dependency.StartsWith("Assets/Layer Lab/", StringComparison.Ordinal), Is.False, path);
                    Assert.That(dependency.StartsWith("Assets/Art/", StringComparison.Ordinal), Is.False, path);
                }
            }
        }

        [TestCase("CardCollectionDialog", 11)]
        [TestCase("InitialDeckDialog", 3)]
        [TestCase("CardDetailDialog", 1)]
        public void CardScreens_UseTheSharedCardIcon(string name, int expectedCount)
        {
            GameObject prefab = Load(name);
            var cards = prefab.GetComponentsInChildren<Dungeon.Runtime.InGame.Battle.View.BattleCardIconView>(true);
            Assert.That(cards.Length, Is.EqualTo(expectedCount));
            foreach (var card in cards)
            {
                string source = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(card.gameObject);
                Assert.That(source, Is.EqualTo("Assets/Prefabs/Common/UI/MultiIcon/CardIcon.prefab"));
                SerializedObject serialized = new SerializedObject(card);
                foreach (string key in new[] { "_button", "_frameImage", "_artImage", "_nameText", "_descriptionText", "_costText", "_quantityText" })
                {
                    Assert.That(serialized.FindProperty(key).objectReferenceValue, Is.Not.Null, key);
                }
            }
        }

        [Test]
        public void CardIcon_HidingCostKeepsTheCardFaceVisible()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Common/UI/MultiIcon/CardIcon.prefab");
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            try
            {
                var view = instance.GetComponent<Dungeon.Runtime.InGame.Battle.View.BattleCardIconView>();
                var model = new Dungeon.Runtime.InGame.Battle.Model.BattleMultiIconViewModel(
                    Dungeon.Runtime.InGame.Battle.Model.BattleIconKind.Card,
                    "検証用カード", "検証用の説明", string.Empty,
                    Game.MasterData.Generated.CardRarity.Common,
                    showCost: false, isSelected: true);
                int clicks = 0;
                view.Bind(model, () => clicks++);
                SerializedObject serialized = new SerializedObject(view);
                Component costText = serialized.FindProperty("_costText").objectReferenceValue as Component;
                Assert.That(costText.transform.parent.name, Is.EqualTo("CostBadge"));
                Assert.That(costText.transform.parent.gameObject.activeSelf, Is.False);
                Assert.That(instance.transform.Find("CardFace").gameObject.activeSelf, Is.True);
                Assert.That(instance.transform.Find("UndiscoveredState").gameObject.activeSelf, Is.False);
                Component nameText = serialized.FindProperty("_nameText").objectReferenceValue as Component;
                Assert.That(new SerializedObject(nameText).FindProperty("m_text").stringValue, Is.EqualTo("検証用カード"));
                var button = serialized.FindProperty("_button").objectReferenceValue as UnityEngine.UI.Button;
                button.onClick.Invoke();
                Assert.That(clicks, Is.EqualTo(1));
                var highlight = serialized.FindProperty("_selectionHighlight").objectReferenceValue as Component;
                Assert.That(highlight.gameObject.activeSelf, Is.True);
                Assert.That(instance.GetComponent<RectTransform>().sizeDelta, Is.EqualTo(new Vector2(220, 320)));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static GameObject Load(string name)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPaths[name]);
            Assert.That(prefab, Is.Not.Null, name);
            return prefab;
        }
    }
}
