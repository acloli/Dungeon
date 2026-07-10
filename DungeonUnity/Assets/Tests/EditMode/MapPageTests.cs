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
        private const float PositionTolerance = 0.01f;

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
            AssertAssignedReference<Transform>(serialized, "_nodeRoot");
            AssertAssignedReference<Transform>(serialized, "_connectionRoot");
            AssertAssignedReference<Image>(serialized, "_connectionLineTemplate");

            Assert.That(serialized.FindProperty("_nodeSpacing").vector2Value, Is.EqualTo(new Vector2(160f, 120f)));
            Assert.That(serialized.FindProperty("_nodeOffset").vector2Value, Is.EqualTo(Vector2.zero));
            Assert.That(serialized.FindProperty("_connectionLineThickness").floatValue, Is.EqualTo(6f));
            Assert.That(serialized.FindProperty("_currentNodeScale").vector3Value, Is.EqualTo(new Vector3(1.08f, 1.08f, 1f)));
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
            AssertTemplateColor(prefab, "TreasureNodeTemplate", new Color32(183, 146, 61, 255));
        }

        [Test]
        public void MapPagePrefab_TreasureTemplateDeclaresButtonAndLabel()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MapPagePrefabPath);
            Transform template = prefab.transform.Find("MapNodeRoot/TreasureNodeTemplate");

            Assert.That(template, Is.Not.Null);
            Assert.That(template.GetComponent<BattleOptionButtonView>(), Is.Not.Null);
            Assert.That(template.GetComponent<Button>(), Is.Not.Null);
            Assert.That(ReadLabel(template), Is.EqualTo("Treasure"));
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
        public void BuildMapGraph_AppliesLayoutCoordinatesAndCreatesUiImageConnectionLines()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MapPagePrefabPath);
            GameObject instance = Object.Instantiate(prefab);

            try
            {
                MapPage page = instance.GetComponent<MapPage>();
                page.BuildMapGraph(
                    new[]
                    {
                        CreateNode(1, InGameNodeType.Battle, "Start", new[] { 1, 2 }),
                        CreateNode(2, InGameNodeType.EliteBattle, "Elite"),
                        CreateNode(2, InGameNodeType.Event, "Event")
                    },
                    new[]
                    {
                        new MapNodeLayout(0, 0f, 0f, 1),
                        new MapNodeLayout(1, 0.5f, 1f, 2),
                        new MapNodeLayout(2, -0.5f, 1f, 2)
                    },
                    new[] { 1 },
                    1,
                    0,
                    null);

                Transform[] activeChildren = GetActiveChildren(page);
                Image[] activeConnectionLines = GetActiveConnectionLines(page);

                AssertVector2(ReadAnchoredPosition(activeChildren[0]), Vector2.zero);
                AssertVector2(ReadAnchoredPosition(activeChildren[1]), new Vector2(80f, 120f));
                AssertVector2(ReadAnchoredPosition(activeChildren[2]), new Vector2(-80f, 120f));

                Assert.That(activeConnectionLines, Has.Length.EqualTo(2));
                Assert.That(activeConnectionLines.All(line => line.GetComponent<Image>() != null), Is.True);
                Assert.That(activeConnectionLines.All(line => line.raycastTarget == false), Is.True);
                AssertVector2(ReadAnchoredPosition(activeConnectionLines[0].transform), new Vector2(40f, 60f));
                AssertVector2(ReadSize(activeConnectionLines[0].transform), new Vector2(Mathf.Sqrt(20800f), 6f));
                Assert.That((Color32)activeConnectionLines[0].color, Is.EqualTo((Color32)new Color(0.95f, 0.74f, 0.28f, 0.8f)));
                Assert.That((Color32)activeConnectionLines[1].color, Is.EqualTo((Color32)new Color(0.44f, 0.47f, 0.55f, 0.25f)));
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void BuildMapGraph_DisablesFogNodeAndHighlightsCurrentNode()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MapPagePrefabPath);
            GameObject instance = Object.Instantiate(prefab);

            try
            {
                MapPage page = instance.GetComponent<MapPage>();
                page.BuildMapGraph(
                    new[]
                    {
                        CreateNode(1, InGameNodeType.Battle, "Start", new[] { 1 }),
                        CreateNode(2, InGameNodeType.Event, "Event"),
                        CreateNode(3, InGameNodeType.Boss, "Boss")
                    },
                    new[]
                    {
                        new MapNodeLayout(0, 0f, 0f, 1),
                        new MapNodeLayout(1, 0f, 1f, 2),
                        new MapNodeLayout(2, 0f, 2f, 3)
                    },
                    new[] { 1 },
                    1,
                    0,
                    null);

                Transform[] activeChildren = GetActiveChildren(page);

                Assert.That(activeChildren[0].GetComponent<Button>().interactable, Is.False);
                Assert.That(activeChildren[0].localScale, Is.EqualTo(new Vector3(1.08f, 1.08f, 1f)));
                Assert.That(activeChildren[0].GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f));

                Assert.That(activeChildren[1].GetComponent<Button>().interactable, Is.True);
                Assert.That(activeChildren[1].localScale, Is.EqualTo(Vector3.one));
                Assert.That(activeChildren[1].GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f));

                Assert.That(activeChildren[2].GetComponent<Button>().interactable, Is.False);
                Assert.That(activeChildren[2].localScale, Is.EqualTo(Vector3.one));
                Assert.That(activeChildren[2].GetComponent<CanvasGroup>().alpha, Is.EqualTo(0.32f));
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

        private static void AssertAssignedReference<T>(SerializedObject serialized, string propertyName) where T : Object
        {
            T component = serialized.FindProperty(propertyName).objectReferenceValue as T;

            Assert.That(component, Is.Not.Null, $"{propertyName} should be assigned.");
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

        private static RuntimeMapNode CreateNode(
            int floor,
            InGameNodeType nodeType,
            string displayName,
            int[] nextNodeIndices)
        {
            return new RuntimeMapNode(
                floor,
                displayName,
                floor,
                nodeType,
                displayName,
                string.Empty,
                nextNodeIndices);
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

        private static Image[] GetActiveConnectionLines(MapPage page)
        {
            Transform connectionRoot = GetSerializedReference<Transform>(page, "_connectionRoot");
            return connectionRoot.Cast<Transform>()
                .Where(child => child.gameObject.activeSelf)
                .Select(child => child.GetComponent<Image>())
                .Where(image => image != null)
                .ToArray();
        }

        private static Vector2 ReadAnchoredPosition(Transform transform)
        {
            RectTransform rectTransform = transform as RectTransform;

            Assert.That(rectTransform, Is.Not.Null);
            return rectTransform.anchoredPosition;
        }

        private static Vector2 ReadSize(Transform transform)
        {
            RectTransform rectTransform = transform as RectTransform;

            Assert.That(rectTransform, Is.Not.Null);
            return rectTransform.sizeDelta;
        }

        private static string ReadLabel(Transform child)
        {
            Component label = child.Find("Label")
                .GetComponents<Component>()
                .First(component => component.GetType().Name == "TFTextUGUI");
            SerializedObject serialized = new SerializedObject(label);
            return serialized.FindProperty("m_text").stringValue;
        }

        private static void AssertVector2(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(PositionTolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(PositionTolerance));
        }
    }
}
