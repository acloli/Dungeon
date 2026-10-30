using System;
using System.Linq;
using System.Threading;
using Dungeon.Runtime.OutGame.Main;
using Dungeon.Runtime.OutGame.Main.View;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Dungeon.Tests.EditMode
{
    public sealed class MainMenuDataTests
    {
        [Test]
        public void SavedMainScene_HasCompleteMenuAndCharacterReferences()
        {
            var scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/MainScene.unity");
            try
            {
                GameObject[] roots = scene.GetRootGameObjects();
                MainSceneView view = roots.SelectMany(r => r.GetComponentsInChildren<MainSceneView>(true)).Single();
                var serialized = new SerializedObject(view);
                var field = serialized.GetIterator();
                while (field.NextVisible(true))
                    if (field.propertyType == SerializedPropertyType.ObjectReference)
                        Assert.That(field.objectReferenceValue, Is.Not.Null, field.propertyPath);

                MainSceneController controller = roots.SelectMany(r => r.GetComponentsInChildren<MainSceneController>(true)).Single();
                Assert.That(new SerializedObject(controller).FindProperty("_view").objectReferenceValue, Is.EqualTo(view));
                var scope = controller.GetComponent<MainSceneLifetimeScope>();
                Assert.That(new SerializedObject(scope).FindProperty("_presentationCatalog").objectReferenceValue, Is.Not.Null);
                foreach (Transform transform in roots.SelectMany(r => r.GetComponentsInChildren<Transform>(true)))
                    Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject), Is.Zero, transform.name);
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }
    }
}
