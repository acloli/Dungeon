using Dungeon.Runtime.InGame.Battle.View;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// 報酬ボタン表示の単体テスト
    /// </summary>
    [TestFixture]
    public sealed class BattleOptionButtonViewTests
    {
        private const int TextureWidth = 1;
        private const int TextureHeight = 1;
        private static readonly Rect SpriteRect = new Rect(0f, 0f, TextureWidth, TextureHeight);
        private static readonly Vector2 SpritePivot = new Vector2(0.5f, 0.5f);

        [Test]
        public void RewardButtonTemplate_HasEditorAttachedIcon()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/InGame/UI/RewardDialog.prefab");
            Transform template = prefab.transform.Find("RewardRoot/RewardButtonTemplate");
            Assert.That(template, Is.Not.Null);

            BattleOptionButtonView view = template.GetComponent<BattleOptionButtonView>();
            SerializedObject serialized = new SerializedObject(view);
            Image icon = serialized.FindProperty("_icon").objectReferenceValue as Image;

            Assert.That(icon, Is.Not.Null);
            Assert.That(template.Find("Icon"), Is.Not.Null);
            Assert.That(icon.gameObject.activeSelf, Is.False);
            Assert.That(icon.raycastTarget, Is.False);
        }

        [Test]
        public void SetIcon_UsesAttachedPrefabIcon()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/InGame/UI/RewardDialog.prefab");
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Texture2D texture = null;
            Sprite sprite = null;

            try
            {
                BattleOptionButtonView view = instance.transform.Find("RewardRoot/RewardButtonTemplate").GetComponent<BattleOptionButtonView>();
                texture = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false);
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
                sprite = Sprite.Create(texture, SpriteRect, SpritePivot);

                view.SetIcon(sprite);

                Transform iconTransform = instance.transform.Find("RewardRoot/RewardButtonTemplate/Icon");
                Image image = iconTransform.GetComponent<Image>();
                Assert.That(image.sprite, Is.SameAs(sprite));
                Assert.That(image.gameObject.activeSelf, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(instance);

                if (sprite != null)
                {
                    Object.DestroyImmediate(sprite);
                }

                if (texture != null)
                {
                    Object.DestroyImmediate(texture);
                }
            }
        }
    }
}
