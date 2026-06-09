using Dungeon.Runtime.InGame.Battle;
using Dungeon.Runtime.InGame.Battle.View;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleUIprefab命名・継承・Addressables整合性テストクラス
    /// </summary>
    [TestFixture]
    public sealed class BattleUiPrefabConsistencyTests
    {
        private const string UiFolder = "Assets/Prefabs/InGame/UI";
        private const string AddressGroupPath = "Assets/AddressableAssetsData/AssetGroups/Default Local Group.asset";

        [TestCase("CommonPage", false, null)]
        [TestCase("CommonDialog", false, null)]
        [TestCase("MapPage", true, "CommonPage")]
        [TestCase("RewardDialog", true, "CommonDialog")]
        [TestCase("RestShopDialog", true, "CommonDialog")]
        [TestCase("ResultDialog", true, "CommonDialog")]
        public void Prefabs_FollowNamingAndVariantRules(string prefabName, bool shouldBeVariant, string parentPrefabName)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UiFolder}/{prefabName}.prefab");

            Assert.That(prefab, Is.Not.Null, $"{prefabName}.prefab is missing.");
            Assert.That(prefab.name, Is.EqualTo(prefabName));
            Assert.That(prefab.name.EndsWith("View"), Is.False);
            Assert.That(PrefabUtility.IsPartOfPrefabAsset(prefab), Is.True);
            Assert.That(PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Variant, Is.EqualTo(shouldBeVariant));

            if (!shouldBeVariant)
            {
                return;
            }

            Object parentPrefab = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
            Assert.That(parentPrefab, Is.Not.Null, $"{prefabName} should inherit from {parentPrefabName}.");
            Assert.That(AssetDatabase.GetAssetPath(parentPrefab), Is.EqualTo($"{UiFolder}/{parentPrefabName}.prefab"));
        }

        [Test]
        public void BattleUiPrefabs_HaveExpectedComponentsAndAddresses()
        {
            AssertPrefabContainsComponent<MapPage>("MapPage");
            AssertPrefabContainsComponent<RewardDialog>("RewardDialog");
            AssertPrefabContainsComponent<RestShopDialog>("RestShopDialog");
            AssertPrefabContainsComponent<ResultDialog>("ResultDialog");

            GameObject mapPagePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UiFolder}/MapPage.prefab");
            MapPage mapPageComponent = mapPagePrefab.GetComponent<MapPage>();
            SerializedObject serialized = new SerializedObject(mapPageComponent);
            Assert.That(serialized.FindProperty("_pageAddress").stringValue, Is.EqualTo(BattleUiAddressCatalog.MapPage));
        }

        [Test]
        public void RewardDialog_HasGoldRewardIconReference()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UiFolder}/RewardDialog.prefab");
            RewardDialog rewardDialog = prefab.GetComponent<RewardDialog>();

            SerializedObject serialized = new SerializedObject(rewardDialog);
            Assert.That(serialized.FindProperty("_goldRewardIcon").objectReferenceValue, Is.Not.Null);
        }

        [Test]
        public void AddressableGroup_UsesPrefabNamesAsKeys()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string yaml = File.ReadAllText(Path.Combine(projectRoot, AddressGroupPath));

            StringAssert.Contains($"m_Address: {BattleUiAddressCatalog.MapPage}", yaml);
            StringAssert.Contains($"m_Address: {BattleUiAddressCatalog.RewardDialog}", yaml);
            StringAssert.Contains($"m_Address: {BattleUiAddressCatalog.RestShopDialog}", yaml);
            StringAssert.Contains($"m_Address: {BattleUiAddressCatalog.ResultDialog}", yaml);
            StringAssert.DoesNotContain("m_Address: MapPageView", yaml);
            StringAssert.DoesNotContain("m_Address: RewardDialogView", yaml);
            StringAssert.DoesNotContain("m_Address: RestShopDialogView", yaml);
            StringAssert.DoesNotContain("m_Address: ResultDialogView", yaml);
        }

        [Test]
        public void AddressableGroup_UsesSceneNamesAsKeys()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string yaml = File.ReadAllText(Path.Combine(projectRoot, AddressGroupPath));

            StringAssert.Contains("m_Address: TitleScene", yaml);
            StringAssert.Contains("m_Address: MainScene", yaml);
            StringAssert.Contains("m_Address: BattleScene", yaml);
        }

        [Test]
        public void BattleScene_HasDedicatedHudReferences()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string yaml = File.ReadAllText(Path.Combine(projectRoot, "Assets/Scenes/BattleScene.unity"));

            StringAssert.Contains("_playerSummaryText: {fileID:", yaml);
            StringAssert.Contains("_enemySummaryText: {fileID:", yaml);
            StringAssert.Contains("_intentText: {fileID:", yaml);
            StringAssert.Contains("_playerStatusText: {fileID:", yaml);
            StringAssert.Contains("_playerBuffText: {fileID:", yaml);
            StringAssert.Contains("_enemyStatusText: {fileID:", yaml);
            StringAssert.Contains("_enemyBuffText: {fileID:", yaml);
            StringAssert.Contains("m_Name: IntentPanel", yaml);
            StringAssert.Contains("m_Name: PlayerStatusPanel", yaml);
            StringAssert.Contains("m_Name: EnemyBuffPanel", yaml);
        }

        [Test]
        public void RunSaveScenes_HaveEntryAndQuitReferences()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string mainSceneYaml = File.ReadAllText(Path.Combine(projectRoot, "Assets/Scenes/MainScene.unity"));
            string battleSceneYaml = File.ReadAllText(Path.Combine(projectRoot, "Assets/Scenes/BattleScene.unity"));

            StringAssert.Contains("_newRunButton: {fileID:", mainSceneYaml);
            StringAssert.Contains("_continueRunButton: {fileID:", mainSceneYaml);
            StringAssert.Contains("value: NewRunButton", mainSceneYaml);
            StringAssert.Contains("m_Name: ContinueRunButton", mainSceneYaml);
            StringAssert.Contains("_saveQuitButton: {fileID:", battleSceneYaml);
            StringAssert.Contains("m_Name: SaveQuitButton", battleSceneYaml);
        }

        private static void AssertPrefabContainsComponent<T>(string prefabName) where T : Component
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UiFolder}/{prefabName}.prefab");
            Assert.That(prefab.GetComponent<T>(), Is.Not.Null, $"{prefabName}.prefab is missing {typeof(T).Name}.");
        }
    }
}
