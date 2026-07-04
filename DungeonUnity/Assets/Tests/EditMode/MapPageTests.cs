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
    }
}
