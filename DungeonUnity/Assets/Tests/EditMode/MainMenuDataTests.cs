using System;
using System.Linq;
using System.Threading;
using Dungeon.Runtime.OutGame.Main;
using Dungeon.Runtime.OutGame.Main.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Dungeon.Tests.EditMode
{
    public sealed class MainMenuDataTests
    {
        private const string ContentPath = "Assets/Settings/OutGame/MainMenuPreviewContent.asset";

        [Test]
        public void PreviewService_ExposesContentWithoutExecutingGameActions()
        {
            MainMenuPreviewContent content = AssetDatabase.LoadAssetAtPath<MainMenuPreviewContent>(ContentPath);
            string before = EditorJsonUtility.ToJson(content);
            var service = new MainMenuPreviewService(content);
            Assert.That(service.LoadAsync(CancellationToken.None).GetAwaiter().GetResult(), Is.SameAs(content.Data));
            Assert.That(content.Data.Cards.All(c => c.Artwork != null && !string.IsNullOrEmpty(c.Id)), Is.True);
            foreach (MainMenuAction action in Enum.GetValues(typeof(MainMenuAction)))
                Assert.That(service.ExecuteAsync(action, CancellationToken.None).GetAwaiter().GetResult(), Is.False);
            Assert.That(EditorJsonUtility.ToJson(content), Is.EqualTo(before));
        }

        [Test]
        public void PreviewService_HonorsCancellation()
        {
            var service = new MainMenuPreviewService(AssetDatabase.LoadAssetAtPath<MainMenuPreviewContent>(ContentPath));
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() => service.LoadAsync(cancellation.Token));
            Assert.Throws<OperationCanceledException>(() => service.ExecuteAsync(MainMenuAction.StartAdventure, cancellation.Token));
        }

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
                Assert.That(new SerializedObject(scope).FindProperty("_previewContent").objectReferenceValue, Is.Not.Null);
                foreach (Transform transform in roots.SelectMany(r => r.GetComponentsInChildren<Transform>(true)))
                    Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject), Is.Zero, transform.name);
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }
    }
}
