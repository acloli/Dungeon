using Dungeon.Runtime.InGame.Battle;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.View;
using Dungeon.Tests.EditMode.Support;
using Game.MasterData.Generated;
using NUnit.Framework;
using TFramework.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
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
        private const string CommonUiFolder = "Assets/Prefabs/Common/UI";
        private const string AddressGroupPath = "Assets/AddressableAssetsData/AssetGroups/Default Local Group.asset";

        [TestCase("CommonPage", false, null)]
        [TestCase("CommonDialog", false, null)]
        [TestCase("MapPage", true, "CommonPage")]
        [TestCase("RewardDialog", true, "CommonDialog")]
        [TestCase("RestShopDialog", true, "CommonDialog")]
        [TestCase("ResultDialog", true, "CommonDialog")]
        [TestCase("ShopDialog", true, "CommonDialog")]
        [TestCase("CardSelectDialog", true, "CommonDialog")]
        [TestCase("PotionReplaceDialog", true, "CommonDialog")]
        [TestCase("MultiIcon", false, null)]
        [TestCase("CardIcon", true, "MultiIcon")]
        [TestCase("RelicIcon", true, "MultiIcon")]
        [TestCase("PotionIcon", true, "MultiIcon")]
        [TestCase("ResourceIcon", true, "MultiIcon")]
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
            AssertPrefabContainsComponent<ShopDialog>("ShopDialog");
            AssertPrefabContainsComponent<CardSelectDialog>("CardSelectDialog");
            AssertPrefabContainsComponent<PotionReplaceDialog>("PotionReplaceDialog");

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
            Assert.That(serialized.FindProperty("_rewardRoot").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_rewardButtonTemplate").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_continueButton").objectReferenceValue, Is.Not.Null);
        }

        [Test]
        public void ShopDialog_HasSerializedViewReferences()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UiFolder}/ShopDialog.prefab");
            ShopDialog shopDialog = prefab.GetComponent<ShopDialog>();

            SerializedObject serialized = new SerializedObject(shopDialog);
            Assert.That(serialized.FindProperty("_leaveButton").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_goldText").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_cardRemovalButton").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_cardRemovalPriceText").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_shopItemsContainer").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_shopItemTemplate").objectReferenceValue, Is.Not.Null);
        }

        [Test]
        public void BattleShopItemViewPrefab_HasSerializedViewReferences()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UiFolder}/BattleShopItemView.prefab");
            BattleShopItemView shopItemView = prefab.GetComponent<BattleShopItemView>();

            Assert.That(shopItemView, Is.Not.Null);

            SerializedObject serialized = new SerializedObject(shopItemView);
            Assert.That(serialized.FindProperty("_button").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_iconView").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_priceText").objectReferenceValue, Is.Not.Null);
        }

        [Test]
        public void CardSelectDialog_HasSerializedViewReferences()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UiFolder}/CardSelectDialog.prefab");
            CardSelectDialog cardSelectDialog = prefab.GetComponent<CardSelectDialog>();

            SerializedObject serialized = new SerializedObject(cardSelectDialog);
            Assert.That(serialized.FindProperty("_cancelButton").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_previewCancelButton").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_confirmButton").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_messageText").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_cardContainer").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_previewContainer").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_cardTemplate").objectReferenceValue, Is.Not.Null);
        }

        [Test]
        public void PotionReplaceDialog_HasSerializedViewReferences()
        {
            GameObject replacePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UiFolder}/PotionReplaceDialog.prefab");
            PotionReplaceDialog replaceDialog = replacePrefab.GetComponent<PotionReplaceDialog>();
            SerializedObject replaceSerialized = new SerializedObject(replaceDialog);
            Assert.That(replaceSerialized.FindProperty("_ownedPotionRoot").objectReferenceValue, Is.Not.Null);
            Assert.That(replaceSerialized.FindProperty("_ownedPotionTemplate").objectReferenceValue, Is.Not.Null);
            Assert.That(replaceSerialized.FindProperty("_offeredPotionView").objectReferenceValue, Is.Not.Null);
            Assert.That(replaceSerialized.FindProperty("_cancelButton").objectReferenceValue, Is.Not.Null);
        }

        [Test]
        public void CardIconPrefabs_HaveExpectedComponents()
        {
            AssertPrefabContainsComponent<BattleMultiIconView>("MultiIcon");
            AssertPrefabContainsComponent<BattleCardIconView>("CardIcon");
            AssertPrefabContainsComponent<BattleMultiIconView>("RelicIcon");
            AssertPrefabContainsComponent<BattleMultiIconView>("PotionIcon");
            AssertPrefabContainsComponent<BattleMultiIconView>("ResourceIcon");
        }

        [Test]
        public void CardIcon_PriceFooter_RespondsToVisibilityFlag()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UiFolder}/CardIcon.prefab");
            GameObject instance = Object.Instantiate(prefab);

            try
            {
                BattleCardIconView cardIcon = instance.GetComponent<BattleCardIconView>();
                SerializedObject serialized = new SerializedObject(cardIcon);
                Component footerText = serialized.FindProperty("_quantityText").objectReferenceValue as Component;
                RuntimeCard card = new RuntimeCard(
                    1001,
                    "strike",
                    "Strike",
                    string.Empty,
                    "Deal damage.",
                    string.Empty,
                    string.Empty,
                    1,
                    CardType.Attack,
                    CardRarity.Common,
                    CharacterArchetype.CrimsonExile,
                    System.Array.Empty<RuntimeCardEffect>());

                Assert.That(footerText, Is.Not.Null);

                cardIcon.Bind(
                    BattleMultiIconViewModel.CreateCard(card, true, true, false, 25, true),
                    null);
                SerializedObject footerSerialized = new SerializedObject(footerText);
                footerSerialized.Update();
                Assert.That(footerText.gameObject.activeSelf, Is.True);
                Assert.That(footerSerialized.FindProperty("m_text").stringValue, Is.EqualTo("25"));

                cardIcon.Bind(
                    BattleMultiIconViewModel.CreateCard(card, true, true, false, 0, false),
                    null);
                footerSerialized.Update();
                Assert.That(footerText.gameObject.activeSelf, Is.False);
                Assert.That(footerSerialized.FindProperty("m_text").stringValue, Is.Empty);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void CardSelectDialog_UpgradeSelectionPreviewsBeforeConfirming()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UiFolder}/CardSelectDialog.prefab");
            GameObject instance = Object.Instantiate(prefab);

            try
            {
                RuntimeCard strike = CreateCard(1001, "Strike", 1, 1002);
                RuntimeCard strikePlus = CreateCard(1002, "Strike+", 1);
                RuntimeCard guard = CreateCard(1003, "Guard", 1, 1004);
                RuntimeCard guardPlus = CreateCard(1004, "Guard+", 1);
                CardSelectDialog dialog = instance.GetComponent<CardSelectDialog>();
                int confirmCount = 0;
                BattleCardSelectDialogParam param = new BattleCardSelectDialogParam(
                    100,
                    new[] { strike, guard },
                    CardSelectMode.Upgrade,
                    true,
                    new System.Collections.Generic.Dictionary<int, int> { { strike.Id, 25 }, { guard.Id, 25 } },
                    new System.Collections.Generic.Dictionary<int, RuntimeCard> { { strike.Id, strikePlus }, { guard.Id, guardPlus } },
                    string.Empty,
                    card =>
                    {
                        confirmCount++;
                        Assert.That(card.Id, Is.EqualTo(strike.Id));
                        return new BattleCardSelectDialogRefreshData(
                            new[] { guard },
                            new System.Collections.Generic.Dictionary<int, int> { { guard.Id, 25 } },
                            new System.Collections.Generic.Dictionary<int, RuntimeCard> { { guard.Id, guardPlus } },
                            75,
                            "Upgrade done. Strike -> Strike+ (-25 Gold). Gold 75");
                    });

                IUIDialog lifecycle = dialog;
                lifecycle.OnPreOpenAsync(param, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
                lifecycle.OnOpened();

                System.Collections.IList beforeViews = GetCardViews(dialog);
                Button cancelButton = GetSerializedReference<Button>(dialog, "_cancelButton");
                Button previewCancelButton = GetSerializedReference<Button>(dialog, "_previewCancelButton");
                Button confirmButton = GetSerializedReference<Button>(dialog, "_confirmButton");
                Assert.That(beforeViews.Count, Is.EqualTo(2));
                Assert.That(confirmButton.interactable, Is.False);
                Assert.That(confirmButton.gameObject.activeSelf, Is.False);
                Assert.That(previewCancelButton.gameObject.activeSelf, Is.False);
                Assert.That(cancelButton.interactable, Is.True);

                InvokeButton((Component)beforeViews[0]);

                Assert.That(confirmCount, Is.EqualTo(0));
                Assert.That(GetCardViews(dialog).Count, Is.EqualTo(2));
                Assert.That(GetPreviewCardViews(dialog).Count, Is.EqualTo(2));
                Assert.That(cancelButton.interactable, Is.False);
                Assert.That(previewCancelButton.gameObject.activeSelf, Is.True);
                Assert.That(previewCancelButton.interactable, Is.True);
                Assert.That(confirmButton.gameObject.activeSelf, Is.True);
                Assert.That(confirmButton.interactable, Is.True);

                InvokeButton(confirmButton);

                System.Collections.IList afterViews = GetCardViews(dialog);
                Component messageText = GetSerializedReference<Component>(dialog, "_messageText");

                Assert.That(confirmCount, Is.EqualTo(1));
                Assert.That(afterViews.Count, Is.EqualTo(1));
                Assert.That(GetPreviewCardViews(dialog).Count, Is.EqualTo(0));
                Assert.That(cancelButton.interactable, Is.True);
                Assert.That(previewCancelButton.gameObject.activeSelf, Is.False);
                Assert.That(confirmButton.gameObject.activeSelf, Is.False);
                Assert.That(confirmButton.interactable, Is.False);
                Assert.That(ReadText(messageText), Does.Contain("Strike -> Strike+"));
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void CardSelectDialog_UpgradePreviewLocksBackgroundSelection()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UiFolder}/CardSelectDialog.prefab");
            GameObject instance = Object.Instantiate(prefab);

            try
            {
                RuntimeCard firstStrike = CreateCard(1001, "Strike", 1, 1002);
                RuntimeCard secondStrike = CreateCard(1001, "Strike", 1, 1002);
                RuntimeCard guard = CreateCard(1003, "Guard", 1, 1004);
                RuntimeCard strikePlus = CreateCard(1002, "Strike+", 1);
                RuntimeCard guardPlus = CreateCard(1004, "Guard+", 1);
                CardSelectDialog dialog = instance.GetComponent<CardSelectDialog>();
                BattleCardSelectDialogParam param = new BattleCardSelectDialogParam(
                    100,
                    new[] { firstStrike, secondStrike, guard },
                    CardSelectMode.Upgrade,
                    true,
                    new System.Collections.Generic.Dictionary<int, int> { { firstStrike.Id, 25 }, { guard.Id, 25 } },
                    new System.Collections.Generic.Dictionary<int, RuntimeCard> { { firstStrike.Id, strikePlus }, { guard.Id, guardPlus } },
                    "Select a card to upgrade.",
                    _ => null);

                IUIDialog lifecycle = dialog;
                lifecycle.OnPreOpenAsync(param, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
                lifecycle.OnOpened();

                System.Collections.IList beforeViews = GetCardViews(dialog);
                Assert.That(beforeViews.Count, Is.EqualTo(3));

                InvokeButton((Component)beforeViews[1]);

                System.Collections.IList previewViews = GetCardViews(dialog);
                Assert.That(IsCardSelected((Component)previewViews[0]), Is.False);
                Assert.That(IsCardSelected((Component)previewViews[1]), Is.True);
                Assert.That(IsCardSelected((Component)previewViews[2]), Is.False);
                Assert.That(IsButtonInteractable((Component)previewViews[0]), Is.False);
                Assert.That(IsButtonInteractable((Component)previewViews[1]), Is.False);
                Assert.That(IsButtonInteractable((Component)previewViews[2]), Is.False);

                InvokeButton((Component)previewViews[2]);

                System.Collections.IList lockedViews = GetCardViews(dialog);
                Assert.That(IsCardSelected((Component)lockedViews[0]), Is.False);
                Assert.That(IsCardSelected((Component)lockedViews[1]), Is.True);
                Assert.That(IsCardSelected((Component)lockedViews[2]), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void CardSelectDialog_PreviewCancelClosesPreviewBeforeDialog()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UiFolder}/CardSelectDialog.prefab");
            GameObject instance = Object.Instantiate(prefab);

            try
            {
                RuntimeCard strike = CreateCard(1001, "Strike", 1, 1002);
                RuntimeCard strikePlus = CreateCard(1002, "Strike+", 1);
                CardSelectDialog dialog = instance.GetComponent<CardSelectDialog>();
                BattleCardSelectDialogParam param = new BattleCardSelectDialogParam(
                    100,
                    new[] { strike },
                    CardSelectMode.Upgrade,
                    true,
                    new System.Collections.Generic.Dictionary<int, int> { { strike.Id, 25 } },
                    new System.Collections.Generic.Dictionary<int, RuntimeCard> { { strike.Id, strikePlus } },
                    "Select a card to upgrade.",
                    _ => null);

                IUIDialog lifecycle = dialog;
                lifecycle.OnPreOpenAsync(param, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
                lifecycle.OnOpened();

                System.Collections.IList beforeViews = GetCardViews(dialog);
                Button cancelButton = GetSerializedReference<Button>(dialog, "_cancelButton");
                Button previewCancelButton = GetSerializedReference<Button>(dialog, "_previewCancelButton");
                Button confirmButton = GetSerializedReference<Button>(dialog, "_confirmButton");
                Component messageText = GetSerializedReference<Component>(dialog, "_messageText");

                InvokeButton((Component)beforeViews[0]);
                Assert.That(GetPreviewCardViews(dialog).Count, Is.EqualTo(2));
                Assert.That(IsButtonInteractable((Component)GetCardViews(dialog)[0]), Is.False);
                Assert.That(cancelButton.interactable, Is.False);
                Assert.That(previewCancelButton.gameObject.activeSelf, Is.True);

                InvokeButton(previewCancelButton);

                System.Collections.IList afterCancelViews = GetCardViews(dialog);
                Assert.That(GetPreviewCardViews(dialog).Count, Is.EqualTo(0));
                Assert.That(afterCancelViews.Count, Is.EqualTo(1));
                Assert.That(IsCardSelected((Component)afterCancelViews[0]), Is.False);
                Assert.That(IsButtonInteractable((Component)afterCancelViews[0]), Is.True);
                Assert.That(cancelButton.interactable, Is.True);
                Assert.That(previewCancelButton.gameObject.activeSelf, Is.False);
                Assert.That(confirmButton.gameObject.activeSelf, Is.False);
                Assert.That(confirmButton.interactable, Is.False);
                Assert.That(ReadText(messageText), Is.EqualTo("Select a card to upgrade."));
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void CardSelectDialog_InsufficientGoldShowsPreviewButDisablesConfirm()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UiFolder}/CardSelectDialog.prefab");
            GameObject instance = Object.Instantiate(prefab);

            try
            {
                RuntimeCard strike = CreateCard(1001, "Strike", 1, 1002);
                RuntimeCard strikePlus = CreateCard(1002, "Strike+", 1);
                CardSelectDialog dialog = instance.GetComponent<CardSelectDialog>();
                int confirmCount = 0;
                BattleCardSelectDialogParam param = new BattleCardSelectDialogParam(
                    20,
                    new[] { strike },
                    CardSelectMode.Upgrade,
                    true,
                    new System.Collections.Generic.Dictionary<int, int> { { strike.Id, 25 } },
                    new System.Collections.Generic.Dictionary<int, RuntimeCard> { { strike.Id, strikePlus } },
                    "Select a card to upgrade.",
                    _ =>
                    {
                        confirmCount++;
                        return null;
                    });

                IUIDialog lifecycle = dialog;
                lifecycle.OnPreOpenAsync(param, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
                lifecycle.OnOpened();

                System.Collections.IList beforeViews = GetCardViews(dialog);
                Button confirmButton = GetSerializedReference<Button>(dialog, "_confirmButton");
                Button previewCancelButton = GetSerializedReference<Button>(dialog, "_previewCancelButton");
                Component messageText = GetSerializedReference<Component>(dialog, "_messageText");
                Assert.That(beforeViews.Count, Is.EqualTo(1));

                InvokeButton((Component)beforeViews[0]);

                Assert.That(GetCardViews(dialog).Count, Is.EqualTo(1));
                Assert.That(GetPreviewCardViews(dialog).Count, Is.EqualTo(2));
                Assert.That(previewCancelButton.gameObject.activeSelf, Is.True);
                Assert.That(confirmButton.gameObject.activeSelf, Is.False);
                Assert.That(confirmButton.interactable, Is.False);
                Assert.That(ReadText(messageText), Is.EqualTo("Not enough gold."));

                InvokeButton(confirmButton);

                Assert.That(confirmCount, Is.EqualTo(0));
                Assert.That(GetCardViews(dialog).Count, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
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
            StringAssert.Contains($"m_Address: {BattleUiAddressCatalog.ShopDialog}", yaml);
            StringAssert.Contains($"m_Address: {BattleUiAddressCatalog.CardSelectDialog}", yaml);
            StringAssert.Contains($"m_Address: {BattleUiAddressCatalog.PotionReplaceDialog}", yaml);
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

            StringAssert.Contains("_battlePanelBackgroundImage: {fileID:", yaml);
            StringAssert.Contains("_ownedRelicRoot: {fileID:", yaml);
            StringAssert.Contains("_ownedRelicTemplate: {fileID:", yaml);
            StringAssert.Contains("_ownedRelicHintRoot: {fileID:", yaml);
            StringAssert.Contains("_ownedRelicHintText: {fileID:", yaml);
            StringAssert.Contains("_ownedRelicCanvasGroup: {fileID:", yaml);
            StringAssert.Contains("_ownedRelicHintCanvasGroup: {fileID:", yaml);
            StringAssert.Contains("_ownedPotionRoot: {fileID:", yaml);
            StringAssert.Contains("_ownedPotionTemplate: {fileID:", yaml);
            StringAssert.Contains("_ownedPotionHintRoot: {fileID:", yaml);
            StringAssert.Contains("_ownedPotionHintText: {fileID:", yaml);
            StringAssert.Contains("_ownedPotionUseButton: {fileID:", yaml);
            StringAssert.Contains("_ownedPotionCanvasGroup: {fileID:", yaml);
            StringAssert.Contains("_ownedPotionHintCanvasGroup: {fileID:", yaml);
            StringAssert.Contains("_ownedPotionUseCanvasGroup: {fileID:", yaml);
            StringAssert.Contains("_hostBackgroundButton: {fileID:", yaml);
            StringAssert.Contains("_playerSummaryText: {fileID:", yaml);
            StringAssert.Contains("_enemySummaryText: {fileID:", yaml);
            StringAssert.Contains("_intentText: {fileID:", yaml);
            StringAssert.Contains("_playerStatusText: {fileID:", yaml);
            StringAssert.Contains("_playerBuffText: {fileID:", yaml);
            StringAssert.Contains("_enemyStatusText: {fileID:", yaml);
            StringAssert.Contains("_enemyBuffText: {fileID:", yaml);
            StringAssert.Contains("_drawPileCountText: {fileID:", yaml);
            StringAssert.Contains("_discardPileCountText: {fileID:", yaml);
            StringAssert.Contains("_handCountText: {fileID:", yaml);
            StringAssert.Contains("_handCardTemplate: {fileID:", yaml);
            StringAssert.Contains("m_Name: IntentPanel", yaml);
            StringAssert.Contains("m_Name: PlayerStatusPanel", yaml);
            StringAssert.Contains("m_Name: EnemyBuffPanel", yaml);
            StringAssert.Contains("m_Name: DrawPilePanel", yaml);
            StringAssert.Contains("m_Name: DiscardPilePanel", yaml);
            StringAssert.Contains("m_Name: HandCountPanel", yaml);
            StringAssert.Contains("m_Name: RelicStrip", yaml);
            StringAssert.Contains("m_Name: OwnedRelicHintPanel", yaml);
            StringAssert.Contains("m_Name: PotionStrip", yaml);
            StringAssert.Contains("m_Name: OwnedPotionHintPanel", yaml);
            StringAssert.Contains("m_Name: UsePotionButton", yaml);
        }

        [Test]
        public void BattleScene_BattlePageView_DoesNotSerializeRelicStripFields()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string yaml = File.ReadAllText(Path.Combine(projectRoot, "Assets/Scenes/BattleScene.unity"));

            StringAssert.DoesNotContain("_relicRoot: {fileID:", yaml);
            StringAssert.DoesNotContain("_relicTemplate: {fileID:", yaml);
        }

        [Test]
        public void BattleScene_RelicStrip_IsChildOfBattlePanel()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/BattleScene.unity", OpenSceneMode.Single);
            Assert.That(scene.IsValid(), Is.True);

            GameObject relicStrip = null;
            foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (gameObject.scene == scene && gameObject.name == "RelicStrip")
                {
                    relicStrip = gameObject;
                    break;
                }
            }

            Assert.That(relicStrip, Is.Not.Null);
            Assert.That(relicStrip.transform.parent, Is.Not.Null);
            Assert.That(relicStrip.transform.parent.name, Is.EqualTo("BattlePanel"));
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

        [Test]
        public void SceneUiRoot_UsesLowerSortingThanFrameworkUiRoot()
        {
            GameObject sceneUiRootPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{CommonUiFolder}/SceneUIRoot.prefab");
            GameObject uiRootPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{CommonUiFolder}/UIRoot.prefab");

            Assert.That(sceneUiRootPrefab, Is.Not.Null);
            Assert.That(uiRootPrefab, Is.Not.Null);

            Canvas sceneCanvas = sceneUiRootPrefab.GetComponent<Canvas>();
            Canvas uiRootCanvas = uiRootPrefab.GetComponent<Canvas>();

            Assert.That(sceneCanvas, Is.Not.Null);
            Assert.That(uiRootCanvas, Is.Not.Null);
            Assert.That(sceneCanvas.overrideSorting, Is.True);
            Assert.That(sceneCanvas.sortingOrder, Is.LessThan(0));
            Assert.That(sceneCanvas.sortingOrder, Is.LessThan(uiRootCanvas.sortingOrder));
            Assert.That(uiRootCanvas.sortingOrder, Is.EqualTo(0));
        }

        private static void AssertPrefabContainsComponent<T>(string prefabName) where T : Component
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UiFolder}/{prefabName}.prefab");
            Assert.That(prefab.GetComponent<T>(), Is.Not.Null, $"{prefabName}.prefab is missing {typeof(T).Name}.");
        }

        private static RuntimeCard CreateCard(int id, string displayName, int cost, int upgradeCardId = 0)
        {
            RuntimeCardBuilder builder = BattleTestData.Card(id);
            builder.DisplayName = displayName;
            builder.Description = "Deal damage.";
            builder.Cost = cost;
            builder.UpgradeCardId = upgradeCardId;
            return builder.Build();
        }

        private static BattleSceneSnapshot CreateSnapshot(BattleScenePage page, int gold = 100)
        {
            BattleSceneSnapshotBuilder builder = BattleTestData.Snapshot(page);
            builder.Combat = new BattleCombatSnapshot(
                playerMaxHp: 40,
                playerHp: 40,
                playerEnergy: 3,
                playerBlock: 0,
                gold: gold,
                battleHintMessage: "battle");
            builder.Shop = new BattleShopSnapshot(gold: gold);
            return builder.Build();
        }

        private static System.Collections.IList GetCardViews(CardSelectDialog dialog)
        {
            System.Reflection.FieldInfo field = typeof(CardSelectDialog).GetField(
                "_cardViews",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return field?.GetValue(dialog) as System.Collections.IList;
        }

        private static System.Collections.IList GetPreviewCardViews(CardSelectDialog dialog)
        {
            System.Reflection.FieldInfo field = typeof(CardSelectDialog).GetField(
                "_previewCardViews",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return field?.GetValue(dialog) as System.Collections.IList;
        }

        private static T GetSerializedReference<T>(UnityEngine.Object target, string propertyName) where T : UnityEngine.Object
        {
            SerializedObject serialized = new SerializedObject(target);
            return serialized.FindProperty(propertyName).objectReferenceValue as T;
        }

        private static string ReadText(Component textComponent)
        {
            SerializedObject serialized = new SerializedObject(textComponent);
            serialized.Update();
            return serialized.FindProperty("m_text").stringValue;
        }

        private static bool IsCardSelected(Component cardView)
        {
            SerializedObject serialized = new SerializedObject(cardView);
            Component selectionHighlight = serialized.FindProperty("_selectionHighlight").objectReferenceValue as Component;
            return selectionHighlight != null && selectionHighlight.gameObject.activeSelf;
        }

        private static bool IsButtonInteractable(Component component)
        {
            Button button = FindButton(component);
            return button != null && button.interactable;
        }

        private static void InvokeButton(Component component)
        {
            Button button = FindButton(component);
            if (button != null)
            {
                button.onClick.Invoke();
                return;
            }

            Assert.Fail("Card view button is missing.");
        }

        private static Button FindButton(Component component)
        {
            return component.GetComponentInChildren<Button>(true);
        }
    }
}
