using System.Linq;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.View;
using Dungeon.Runtime.InGame.Domain;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// MapPageのEditorモードテストクラス
    /// </summary>
    [TestFixture]
    public sealed class MapPageTests
    {
        private const string MapPagePrefabPath = "Assets/Prefabs/InGame/UI/MapPage.prefab";

        [TestCase(InGameNodeType.Battle, "BattleNodeTemplate")]
        [TestCase(InGameNodeType.EliteBattle, "EliteNodeTemplate")]
        [TestCase(InGameNodeType.Event, "EventNodeTemplate")]
        [TestCase(InGameNodeType.RestShop, "RestNodeTemplate")]
        [TestCase(InGameNodeType.Boss, "BossNodeTemplate")]
        public void BuildMapButtons_UsesTemplateForNodeType(InGameNodeType nodeType, string expectedTemplateName)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MapPagePrefabPath);
            GameObject instance = Object.Instantiate(prefab);

            try
            {
                MapPage page = instance.GetComponent<MapPage>();
                RuntimeMapNode node = new RuntimeMapNode(
                    1,
                    "node",
                    1,
                    nodeType,
                    "Node",
                    string.Empty,
                    System.Array.Empty<int>());

                page.BuildMapButtons(new[] { node }, null);

                Transform nodeRoot = GetSerializedReference<Transform>(page, "_nodeRoot");
                Transform activeChild = nodeRoot.Cast<Transform>().Single(child => child.gameObject.activeSelf);

                Assert.That(activeChild.name, Is.EqualTo($"{expectedTemplateName}(Clone)"));
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void MapPagePrefab_DeclaresDistinctNodeTemplates()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MapPagePrefabPath);
            MapPage page = prefab.GetComponent<MapPage>();
            SerializedObject serialized = new SerializedObject(page);

            AssertDistinctTemplate(serialized, "_battleNodeButtonTemplate");
            AssertDistinctTemplate(serialized, "_eliteNodeButtonTemplate");
            AssertDistinctTemplate(serialized, "_eventNodeButtonTemplate");
            AssertDistinctTemplate(serialized, "_restNodeButtonTemplate");
            AssertDistinctTemplate(serialized, "_bossNodeButtonTemplate");
        }

        [Test]
        public void MapPagePrefab_NodeTemplatesHaveDifferentBackgroundStyles()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MapPagePrefabPath);

            AssertTemplateColor(prefab, "BattleNodeTemplate", new Color32(76, 89, 112, 255));
            AssertTemplateColor(prefab, "EliteNodeTemplate", new Color32(153, 63, 63, 255));
            AssertTemplateColor(prefab, "EventNodeTemplate", new Color32(63, 97, 153, 255));
            AssertTemplateColor(prefab, "RestNodeTemplate", new Color32(70, 127, 87, 255));
            AssertTemplateColor(prefab, "BossNodeTemplate", new Color32(178, 123, 54, 255));
        }

        [Test]
        public void BuildMapButtons_WithSameTypeNodes_PreservesVisualOrder()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MapPagePrefabPath);
            GameObject instance = Object.Instantiate(prefab);

            try
            {
                MapPage page = instance.GetComponent<MapPage>();
                page.BuildMapButtons(
                    new[]
                    {
                        CreateNode(1, InGameNodeType.Battle, "Top"),
                        CreateNode(1, InGameNodeType.Battle, "Bottom")
                    },
                    null);

                string[] labels = GetActiveButtonLabels(page);

                Assert.That(labels, Is.EqualTo(new[] { "1.Top", "1.Bottom" }));
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void BuildMapButtons_WithMixedTypeNodes_PreservesVisualOrder()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MapPagePrefabPath);
            GameObject instance = Object.Instantiate(prefab);

            try
            {
                MapPage page = instance.GetComponent<MapPage>();
                page.BuildMapButtons(
                    new[]
                    {
                        CreateNode(2, InGameNodeType.RestShop, "Rest"),
                        CreateNode(2, InGameNodeType.Event, "Event")
                    },
                    null);

                string[] labels = GetActiveButtonLabels(page);

                Assert.That(labels, Is.EqualTo(new[] { "2.Rest", "2.Event" }));
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void SetMapButtonInteractable_MatchesDisplayedButtonOrder()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MapPagePrefabPath);
            GameObject instance = Object.Instantiate(prefab);

            try
            {
                MapPage page = instance.GetComponent<MapPage>();
                page.BuildMapButtons(
                    new[]
                    {
                        CreateNode(1, InGameNodeType.RestShop, "Rest"),
                        CreateNode(1, InGameNodeType.Event, "Event")
                    },
                    null);
                page.SetMapButtonInteractable(new[] { 0 });

                Transform[] activeChildren = GetActiveChildren(page);

                Assert.That(ReadLabel(activeChildren[0]), Is.EqualTo("1.Rest"));
                Assert.That(activeChildren[0].GetComponent<Button>().interactable, Is.True);
                Assert.That(ReadLabel(activeChildren[1]), Is.EqualTo("1.Event"));
                Assert.That(activeChildren[1].GetComponent<Button>().interactable, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static T GetSerializedReference<T>(Object target, string propertyName) where T : Object
        {
            SerializedObject serialized = new SerializedObject(target);
            return serialized.FindProperty(propertyName).objectReferenceValue as T;
        }

        private static void AssertDistinctTemplate(SerializedObject serialized, string propertyName)
        {
            Component component = serialized.FindProperty(propertyName).objectReferenceValue as Component;

            Assert.That(component, Is.Not.Null, $"{propertyName} should be assigned.");
            Assert.That(component.gameObject.name, Does.EndWith("NodeTemplate"));
        }

        private static void AssertTemplateColor(GameObject prefab, string templateName, Color32 expectedColor)
        {
            Transform template = prefab.transform.Find($"MapNodeRoot/{templateName}");

            Assert.That(template, Is.Not.Null, $"{templateName} should exist.");

            Image image = template.GetComponent<Image>();
            Assert.That(image, Is.Not.Null, $"{templateName} should have an Image.");
            Assert.That((Color32)image.color, Is.EqualTo(expectedColor));
        }

        private static RuntimeMapNode CreateNode(int floor, InGameNodeType nodeType, string displayName)
        {
            return new RuntimeMapNode(
                floor,
                displayName,
                floor,
                nodeType,
                displayName,
                string.Empty,
                System.Array.Empty<int>());
        }

        private static string[] GetActiveButtonLabels(MapPage page)
        {
            Transform[] activeChildren = GetActiveChildren(page);
            return activeChildren.Select(ReadLabel).ToArray();
        }

        private static Transform[] GetActiveChildren(MapPage page)
        {
            Transform nodeRoot = GetSerializedReference<Transform>(page, "_nodeRoot");
            return nodeRoot.Cast<Transform>().Where(child => child.gameObject.activeSelf).ToArray();
        }

        private static string ReadLabel(Transform child)
        {
            Component label = child.Find("Label")
                .GetComponents<Component>()
                .First(component => component.GetType().Name == "TFTextUGUI");
            SerializedObject serialized = new SerializedObject(label);
            return serialized.FindProperty("m_text").stringValue;
        }
    }
}
