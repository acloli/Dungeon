using System;
using System.Collections.Generic;
using System.Threading;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Domain;
using Dungeon.Runtime.InGame.Save.Model;
using Dungeon.Runtime.InGame.Save.Services;
using Game.MasterData.Generated;
using NUnit.Framework;
using Cysharp.Threading.Tasks;
using TFramework.MasterData;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleSceneFlowServiceの編集モード試験クラス
    /// </summary>
    public sealed class BattleSceneFlowServiceTests
    {
        [Test]
        public void Initialize_OpensMapWithRunDefaults()
        {
            BattleSceneFlowService service = CreateService(CreateRunDefinition(), 0);

            service.Initialize(5501);
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Map));
            Assert.That(snapshot.PlayerMaxHp, Is.EqualTo(50));
            Assert.That(snapshot.PlayerHp, Is.EqualTo(50));
            Assert.That(snapshot.Gold, Is.EqualTo(120));
            Assert.That(snapshot.Nodes.Count, Is.EqualTo(2));
            Assert.That(snapshot.AvailableNodeIndices, Is.EqualTo(new[] { 0 }));
            Assert.That(snapshot.MapMessage, Does.Contain("Next 1/2"));
        }

        [Test]
        public void CreateSnapshot_AfterBranchNode_ReturnsAvailableNextNodes()
        {
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[]
                {
                    CreateNode(5301, 1, InGameNodeType.RestShop, "Rest", new[] { 1, 2 }),
                    CreateNode(5302, 2, InGameNodeType.Battle, "Battle", new[] { 3 }),
                    CreateNode(5303, 2, InGameNodeType.EliteBattle, "Elite", new[] { 3 }),
                    CreateNode(5304, 3, InGameNodeType.Boss, "Boss", new int[0])
                });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.ApplyRest();
            service.ContinueFromRestShop();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Map));
            Assert.That(snapshot.AvailableNodeIndices, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void SelectMapNode_UnavailableNode_KeepsCurrentNodeAndShowsMessage()
        {
            BattleSceneFlowService service = CreateService(CreateRunDefinition(), 0);

            service.Initialize(5501);
            service.SelectMapNode(1);
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentNodeIndex, Is.EqualTo(-1));
            Assert.That(snapshot.MapMessage, Is.EqualTo("You can only go to the next node."));
            Assert.That(snapshot.AvailableNodeIndices, Is.EqualTo(new[] { 0 }));
        }

        [Test]
        public void SelectMapNode_BattleNode_OpensBattleAndDrawsHand()
        {
            RuntimeCard strike = CreateCard(1001, "Strike", 1, 6);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { strike, strike, strike },
                rewardCards: new[] { CreateRewardEntry(CreateCard(2001, "Burst", 2, 12), 10, 1, 99) },
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 14, CreateAction(1, 4, RepeatRule.RepeatAfterOpening)), 10) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Battle));
            Assert.That(snapshot.Hand.Count, Is.EqualTo(3));
            Assert.That(snapshot.CurrentEnemy.DisplayName, Is.EqualTo("Slime"));
            Assert.That(snapshot.Enemies.Count, Is.EqualTo(1));
            Assert.That(snapshot.BattleHintMessage, Is.EqualTo("Select target, then use card."));
        }

        [Test]
        public void SelectMapNode_BattleNode_DrawsUniqueCardsFromCombatDrawPile()
        {
            RuntimeCard first = CreateCard(1001, "First", 1, 6);
            RuntimeCard second = CreateCard(1002, "Second", 1, 7);
            RuntimeCard third = CreateCard(1003, "Third", 1, 8);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { first, second, third },
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 14, CreateAction(1, 4, RepeatRule.RepeatAfterOpening)), 10) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            BattleSceneSnapshot snapshot = service.CreateSnapshot();
            List<int> handCardIds = new List<int>();
            for (int i = 0; i < snapshot.Hand.Count; i++)
            {
                handCardIds.Add(snapshot.Hand[i].Id);
            }

            Assert.That(snapshot.Hand.Count, Is.EqualTo(3));
            Assert.That(handCardIds, Is.EquivalentTo(new[] { 1001, 1002, 1003 }));
            Assert.That(service.GetDeckCards().Count, Is.EqualTo(3));
        }

        [Test]
        public void TryPlaySelectedCard_PlayedCardLeavesHand()
        {
            RuntimeCard first = CreateCard(1001, "First", 1, 1);
            RuntimeCard second = CreateCard(1002, "Second", 1, 1);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { first, second },
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 14, CreateAction(1, 0, RepeatRule.RepeatAfterOpening)), 10) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectHandCard(0);
            service.TryPlaySelectedCard();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Battle));
            Assert.That(snapshot.Hand.Count, Is.EqualTo(1));
            Assert.That(service.GetDeckCards().Count, Is.EqualTo(2));
        }

        [Test]
        public void EndTurn_DrawPileEmpty_ShufflesDiscardIntoNextHand()
        {
            RuntimeCard guard = CreateCard(1001, "Guard", 1, 0, new[]
            {
                new RuntimeCardEffect(1, EffectType.GainBlock, 5, 1, StatusType.None, 0, TargetSide.Self)
            });
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { guard },
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 14, CreateAction(1, 0, RepeatRule.RepeatAfterOpening)), 10) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectHandCard(0);
            service.TryPlaySelectedCard();
            service.EndTurn();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Battle));
            Assert.That(snapshot.Hand.Count, Is.EqualTo(1));
            Assert.That(snapshot.Hand[0].Id, Is.EqualTo(1001));
        }

        [Test]
        public void TryPlaySelectedCard_DrawCards_StopsAtHandLimit()
        {
            RuntimeCardEffect drawEffect = new RuntimeCardEffect(1, EffectType.DrawCards, 20, 1, StatusType.None, 0, TargetSide.Self);
            List<RuntimeCard> deck = new List<RuntimeCard>();
            for (int i = 0; i < 12; i++)
            {
                deck.Add(CreateCard(1001 + i, $"Draw {i}", 0, 0, new[] { drawEffect }));
            }

            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: deck,
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 14, CreateAction(1, 0, RepeatRule.RepeatAfterOpening)), 10) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectHandCard(0);
            service.TryPlaySelectedCard();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.Hand.Count, Is.EqualTo(10));
        }

        [Test]
        public void TryPlaySelectedCard_GainEnergy_IncreasesCurrentEnergy()
        {
            RuntimeCard sparkFocus = CreateCard(1001, "Spark Focus", 0, 0, new[]
            {
                new RuntimeCardEffect(1, EffectType.GainEnergy, 1, 1, StatusType.None, 0, TargetSide.Self)
            });
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { sparkFocus },
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 14, CreateAction(1, 0, RepeatRule.RepeatAfterOpening)), 10) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectHandCard(0);
            service.TryPlaySelectedCard();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.PlayerEnergy, Is.EqualTo(4));
        }

        [Test]
        public void BattleFlow_CombatEvents_FiresAtStableTiming()
        {
            RuntimeCard strike = CreateCard(1001, "Strike", 1, 1);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { strike },
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 14, CreateAction(1, 2, RepeatRule.RepeatAfterOpening)), 10) });
            FakeBattleCombatEventService combatEventService = new FakeBattleCombatEventService();
            BattleSceneFlowService service = CreateServiceWithCombatEvents(runDefinition, combatEventService, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectHandCard(0);
            service.TryPlaySelectedCard();
            service.EndTurn();

            Assert.That(combatEventService.Events, Is.EqualTo(new[]
            {
                "CombatStart",
                "PlayerTurnStart",
                "CardPlayed:Strike:1",
                "PlayerTurnEnd",
                "PlayerDamaged:2",
                "PlayerTurnStart"
            }));
        }

        [Test]
        public void SelectMapNode_MultiEnemyFormation_OpensBattleWithAllEnemies()
        {
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[]
                {
                    CreateEncounter(
                        CreateFormation(
                            CreateEnemyEntry(CreateEnemy(3001, "Mite", 8, 8, 5, CreateAction(1, 2, RepeatRule.RepeatAfterOpening)), 0),
                            CreateEnemyEntry(CreateEnemy(3002, "Slime", 12, 12, 7, CreateAction(1, 3, RepeatRule.RepeatAfterOpening)), 1)),
                        10)
                });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.Enemies.Count, Is.EqualTo(2));
            Assert.That(snapshot.Enemies[0].DisplayName, Is.EqualTo("Mite"));
            Assert.That(snapshot.Enemies[1].DisplayName, Is.EqualTo("Slime"));
            Assert.That(snapshot.SelectedEnemyIndex, Is.EqualTo(0));
        }

        [Test]
        public void SelectMapNode_BattleNode_CreatesEnemyIntentPreview()
        {
            RuntimeEnemyAction action = CreateAction(
                1,
                7,
                RepeatRule.OpeningOnly,
                IntentType.AttackDefend,
                2,
                5,
                StatusType.Weak,
                2,
                BuffType.Ritual,
                3);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 14, action), 10) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.EnemyIntent, Is.Not.Null);
            Assert.That(snapshot.EnemyIntent.IntentType, Is.EqualTo(IntentType.AttackDefend));
            Assert.That(snapshot.EnemyIntent.IntentName, Is.EqualTo(nameof(IntentType.AttackDefend)));
            Assert.That(snapshot.EnemyIntent.Damage, Is.EqualTo(7));
            Assert.That(snapshot.EnemyIntent.HitCount, Is.EqualTo(2));
            Assert.That(snapshot.EnemyIntent.Block, Is.EqualTo(5));
            Assert.That(snapshot.EnemyIntent.StatusType, Is.EqualTo(StatusType.Weak));
            Assert.That(snapshot.EnemyIntent.StatusName, Is.EqualTo(nameof(StatusType.Weak)));
            Assert.That(snapshot.EnemyIntent.StatusValue, Is.EqualTo(2));
            Assert.That(snapshot.EnemyIntent.BuffType, Is.EqualTo(BuffType.Ritual));
            Assert.That(snapshot.EnemyIntent.BuffName, Is.EqualTo(nameof(BuffType.Ritual)));
            Assert.That(snapshot.EnemyIntent.BuffValue, Is.EqualTo(3));
        }

        [Test]
        public void CreateSnapshot_BattleState_IncludesStatusAndBuffViews()
        {
            RuntimeCard weakCard = CreateCard(1001, "Weak", 1, 0, new[]
            {
                new RuntimeCardEffect(1, EffectType.ApplyStatus, 0, 1, StatusType.Weak, 2, TargetSide.Enemy)
            });
            RuntimeEnemyAction action = CreateAction(
                1,
                0,
                RepeatRule.OpeningOnly,
                IntentType.Buff,
                1,
                0,
                StatusType.Vulnerable,
                2,
                BuffType.Ritual,
                3);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { weakCard },
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 14, action), 10) });
            BattleSceneFlowService service = CreateServiceWithDisplayText(runDefinition, new FakeBattleDisplayTextService(), 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectHandCard(0);
            service.TryPlaySelectedCard();
            service.EndTurn();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.PlayerStatuses.Count, Is.EqualTo(1));
            Assert.That(snapshot.PlayerStatuses[0].Name, Is.EqualTo("表示Vulnerable"));
            Assert.That(snapshot.PlayerStatuses[0].Value, Is.EqualTo(2));
            Assert.That(snapshot.EnemyStatuses.Count, Is.EqualTo(1));
            Assert.That(snapshot.EnemyStatuses[0].Name, Is.EqualTo("表示Weak"));
            Assert.That(snapshot.EnemyStatuses[0].Value, Is.EqualTo(1));
            Assert.That(snapshot.EnemyBuffs.Count, Is.EqualTo(1));
            Assert.That(snapshot.EnemyBuffs[0].Name, Is.EqualTo("表示Ritual"));
            Assert.That(snapshot.EnemyBuffs[0].Value, Is.EqualTo(3));
        }

        [Test]
        public void TryPlaySelectedCard_KillEnemy_OpensRewardAndAddsGold()
        {
            RuntimeCard finisher = CreateCard(1001, "Finisher", 1, 99);
            RuntimeCard reward = CreateCard(1002, "Reward", 1, 5);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { finisher },
                rewardCards: new[] { CreateRewardEntry(reward, 10, 1, 99) },
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 30, CreateAction(1, 4, RepeatRule.RepeatAfterOpening)), 10) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectHandCard(0);
            service.TryPlaySelectedCard();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Reward));
            Assert.That(snapshot.Gold, Is.EqualTo(120));
            Assert.That(snapshot.BattleGoldReward, Is.EqualTo(30));
            Assert.That(snapshot.RewardChoices.Count, Is.EqualTo(1));
            Assert.That(snapshot.RewardChoices[0].Card.DisplayName, Is.EqualTo("Reward"));
        }

        [Test]
        public void TryPlaySelectedCard_TargetEnemy_DamagesSelectedEnemyOnly()
        {
            RuntimeCard strike = CreateCard(1001, "Strike", 1, 6);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { strike },
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[]
                {
                    CreateEncounter(
                        CreateFormation(
                            CreateEnemyEntry(CreateEnemy(3001, "Mite", 10, 10, 5, CreateAction(1, 2, RepeatRule.RepeatAfterOpening)), 0),
                            CreateEnemyEntry(CreateEnemy(3002, "Slime", 12, 12, 7, CreateAction(1, 3, RepeatRule.RepeatAfterOpening)), 1)),
                        10)
                });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectEnemyTarget(1);
            service.SelectHandCard(0);
            service.TryPlaySelectedCard();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.Enemies[0].Hp, Is.EqualTo(10));
            Assert.That(snapshot.Enemies[1].Hp, Is.EqualTo(6));
            Assert.That(snapshot.SelectedEnemyIndex, Is.EqualTo(1));
        }

        [Test]
        public void DoesSelectedCardRequireEnemyTarget_TargetSideEnemy_ReturnsTrue()
        {
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { CreateCard(1001, "Strike", 1, 6) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectHandCard(0);

            Assert.That(service.DoesSelectedCardRequireEnemyTarget(), Is.True);
        }

        [Test]
        public void DoesSelectedCardRequireEnemyTarget_AllEnemies_ReturnsFalse()
        {
            RuntimeCard sweep = CreateCard(1001, "Sweep", 1, 12, new[]
            {
                new RuntimeCardEffect(1, EffectType.DealDamage, 12, 1, StatusType.None, 0, TargetSide.AllEnemies)
            });
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { sweep });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectHandCard(0);

            Assert.That(service.DoesSelectedCardRequireEnemyTarget(), Is.False);
        }

        [Test]
        public void DoesSelectedCardRequireEnemyTarget_Self_ReturnsFalse()
        {
            RuntimeCard guard = CreateCard(1001, "Guard", 1, 0, new[]
            {
                new RuntimeCardEffect(1, EffectType.GainBlock, 5, 1, StatusType.None, 0, TargetSide.Self)
            });
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { guard });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectHandCard(0);

            Assert.That(service.DoesSelectedCardRequireEnemyTarget(), Is.False);
        }

        [Test]
        public void TryPlaySelectedCard_AllEnemies_DamagesAllAndRewardsAfterAllDefeated()
        {
            RuntimeCard sweep = CreateCard(1001, "Sweep", 1, 12, new[]
            {
                new RuntimeCardEffect(1, EffectType.DealDamage, 12, 1, StatusType.None, 0, TargetSide.AllEnemies)
            });
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { sweep },
                rewardCards: new[] { CreateRewardEntry(CreateCard(1002, "Reward", 1, 5), 10, 1, 99) },
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[]
                {
                    CreateEncounter(
                        CreateFormation(
                            CreateEnemyEntry(CreateEnemy(3001, "Mite", 8, 8, 5, CreateAction(1, 2, RepeatRule.RepeatAfterOpening)), 0),
                            CreateEnemyEntry(CreateEnemy(3002, "Slime", 12, 12, 7, CreateAction(1, 3, RepeatRule.RepeatAfterOpening)), 1)),
                        10)
                });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectHandCard(0);
            service.TryPlaySelectedCard();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Reward));
            Assert.That(snapshot.Gold, Is.EqualTo(120));
            Assert.That(snapshot.BattleGoldReward, Is.EqualTo(12));
        }

        [Test]
        public void EndTurn_WhenPlayerDies_OpensResult()
        {
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                playerMaxHp: 3,
                starterDeck: new[] { CreateCard(1001, "Strike", 1, 1) },
                rewardCards: new[] { CreateRewardEntry(CreateCard(1002, "Reward", 1, 5), 10, 1, 99) },
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 30, CreateAction(1, 5, RepeatRule.RepeatAfterOpening)), 10) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.EndTurn();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Result));
            Assert.That(snapshot.ResultMessage, Is.EqualTo("Run Failed"));
        }

        [Test]
        public void RestShopFlow_RestAndContinue_ReturnsToMap()
        {
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.RestShop, "Rest", new[] { 1 }) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.ApplyRest();
            BattleSceneSnapshot restSnapshot = service.CreateSnapshot();

            Assert.That(restSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.RestShop));
            Assert.That(restSnapshot.IsRestShopContinueEnabled, Is.True);
            Assert.That(restSnapshot.RestShopMessage, Does.Contain("Rest done."));

            service.ContinueFromRestShop();
            BattleSceneSnapshot mapSnapshot = service.CreateSnapshot();

            Assert.That(mapSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.Map));
        }

        [Test]
        public void Initialize_RequestSave_SavesMapCheckpoint()
        {
            FakeRunSaveService runSaveService = new FakeRunSaveService();
            BattleSceneFlowService service = CreateServiceWithRunSave(CreateRunDefinition(), runSaveService, 0);

            service.Initialize(5501);

            Assert.That(runSaveService.SaveCallCount, Is.EqualTo(1));
            Assert.That(runSaveService.LastSavedData.CurrentPage, Is.EqualTo((int)BattleScenePage.Map));
            Assert.That(runSaveService.LastSavedData.CurrentNodeIndex, Is.EqualTo(-1));
        }

        [Test]
        public void SelectMapNode_RestShop_SavesRestShopCheckpoint()
        {
            FakeRunSaveService runSaveService = new FakeRunSaveService();
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.RestShop, "Rest", new[] { 1 }) });
            BattleSceneFlowService service = CreateServiceWithRunSave(runDefinition, runSaveService, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);

            Assert.That(runSaveService.SaveCallCount, Is.EqualTo(2));
            Assert.That(runSaveService.LastSavedData.CurrentPage, Is.EqualTo((int)BattleScenePage.RestShop));
            Assert.That(runSaveService.LastSavedData.CurrentNodeIndex, Is.EqualTo(0));
        }

        [Test]
        public void SelectReward_RequestSave_SavesDeckCheckpoint()
        {
            FakeRunSaveService runSaveService = new FakeRunSaveService();
            RuntimeCard reward = CreateCard(1002, "Reward", 1, 5);
            RuntimeRewardEntry rewardEntry = CreateRewardEntry(reward, 10, 1, 99);
            BattleSceneFlowService service = CreateServiceWithRunSave(CreateRunDefinition(), runSaveService, 0);

            service.Initialize(5501);
            service.SelectReward(rewardEntry);
            service.ContinueFromReward();

            Assert.That(runSaveService.LastSavedData.CurrentPage, Is.EqualTo((int)BattleScenePage.Map));
            Assert.That(runSaveService.LastSavedData.DeckCardIds, Does.Contain(1002));
        }

        [Test]
        public void ClaimRelic_AddsOwnedRelicAndSavesOwnedRelicIds()
        {
            FakeRunSaveService runSaveService = new FakeRunSaveService();
            RuntimeRelic relic = CreateRelic(1, "Burning Core", new[]
            {
                new RuntimeRelicEffect(1, RelicTriggerType.CombatStart, EffectType.GainBlock, 6, 1, StatusType.None, 0, TargetSide.Self)
            });
            RuntimeCard finisher = CreateCard(1001, "Finisher", 1, 99);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                relicDropChance: 100,
                starterDeck: new[] { finisher },
                rewardCards: new[] { CreateRewardEntry(CreateCard(1002, "Reward", 1, 5), 10, 1, 99) },
                relicCatalog: new Dictionary<int, RuntimeRelic> { { relic.Id, relic } });
            BattleSceneFlowService service = CreateServiceWithRunSave(runDefinition, runSaveService, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectHandCard(0);
            service.TryPlaySelectedCard();
            service.ClaimRelic();
            service.ContinueFromReward();

            Assert.That(runSaveService.LastSavedData.OwnedRelicIds, Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void OwnedRelic_CombatStartGainBlock_AppliesAtBattleStart()
        {
            RuntimeRelic relic = CreateRelic(1, "Burning Core", new[]
            {
                new RuntimeRelicEffect(1, RelicTriggerType.CombatStart, EffectType.GainBlock, 6, 1, StatusType.None, 0, TargetSide.Self)
            });
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                relicCatalog: new Dictionary<int, RuntimeRelic> { { relic.Id, relic } });
            BattleSceneFlowService service = CreateService(runDefinition, 0);
            RunSaveData saveData = new RunSaveData
            {
                RunProfileId = 5501,
                PlayerMaxHp = 50,
                PlayerHp = 50,
                PlayerEnergy = 3,
                Gold = 120,
                CurrentNodeIndex = -1,
                CurrentPage = (int)BattleScenePage.Map,
                OwnedRelicIds = new List<int> { 1 }
            };

            service.InitializeFromSave(saveData);
            service.SelectMapNode(0);
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.PlayerBlock, Is.EqualTo(6));
        }

        [Test]
        public void InspectOwnedRelic_TogglesSelectionAndClearsOnPageChange()
        {
            RuntimeRelic relic = CreateRelic(1, "Burning Core", new[]
            {
                new RuntimeRelicEffect(1, RelicTriggerType.CombatStart, EffectType.GainBlock, 6, 1, StatusType.None, 0, TargetSide.Self)
            });
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[]
                {
                    CreateNode(5301, 1, InGameNodeType.RestShop, "Rest", new[] { 1 }),
                    CreateNode(5302, 2, InGameNodeType.Battle, "Battle", Array.Empty<int>())
                },
                relicCatalog: new Dictionary<int, RuntimeRelic> { { relic.Id, relic } },
                shopLineup: new RuntimeShopLineup(
                    1,
                    new[]
                    {
                        new RuntimeShopSlot(7, RewardType.Relic, CardType.Attack, 100)
                    }),
                itemPriceRules: new[]
                {
                    new RuntimeItemPriceRule(RewardType.Relic, relic.Id, 80, 0)
                });
            BattleShopService shopService = new BattleShopService(new FakeMasterDataService());
            BattleSceneFlowService service = CreateServiceWithShop(runDefinition, shopService, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.OpenShop();
            int slotIndex = service.CreateSnapshot().ShopItems[0].SlotIndex;
            service.PurchaseShopItem(slotIndex);

            service.InspectOwnedRelic(0);
            BattleSceneSnapshot inspectedSnapshot = service.CreateSnapshot();
            Assert.That(inspectedSnapshot.SelectedOwnedRelicIndex, Is.EqualTo(0));
            Assert.That(inspectedSnapshot.OwnedRelicHintMessage, Does.Contain("Burning Core"));

            service.InspectOwnedRelic(0);
            BattleSceneSnapshot clearedSnapshot = service.CreateSnapshot();
            Assert.That(clearedSnapshot.SelectedOwnedRelicIndex, Is.EqualTo(-1));
            Assert.That(clearedSnapshot.OwnedRelicHintMessage, Is.Empty);

            service.InspectOwnedRelic(0);
            service.LeaveShop();
            BattleSceneSnapshot pageChangedSnapshot = service.CreateSnapshot();
            Assert.That(pageChangedSnapshot.SelectedOwnedRelicIndex, Is.EqualTo(-1));
            Assert.That(pageChangedSnapshot.OwnedRelicHintMessage, Is.Empty);
        }

        [Test]
        public void OwnedRelic_PlayerTurnStartGainEnergy_AppliesEachTurn()
        {
            RuntimeRelic relic = CreateRelic(2, "Ember Crown", new[]
            {
                new RuntimeRelicEffect(1, RelicTriggerType.PlayerTurnStart, EffectType.GainEnergy, 1, 1, StatusType.None, 0, TargetSide.Self)
            });
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                relicCatalog: new Dictionary<int, RuntimeRelic> { { relic.Id, relic } });
            BattleSceneFlowService service = CreateService(runDefinition, 0);
            RunSaveData saveData = new RunSaveData
            {
                RunProfileId = 5501,
                PlayerMaxHp = 50,
                PlayerHp = 50,
                PlayerEnergy = 3,
                Gold = 120,
                CurrentNodeIndex = -1,
                CurrentPage = (int)BattleScenePage.Map,
                OwnedRelicIds = new List<int> { 2 }
            };

            service.InitializeFromSave(saveData);
            service.SelectMapNode(0);
            BattleSceneSnapshot firstTurnSnapshot = service.CreateSnapshot();
            service.EndTurn();
            BattleSceneSnapshot secondTurnSnapshot = service.CreateSnapshot();

            Assert.That(firstTurnSnapshot.PlayerEnergy, Is.EqualTo(4));
            Assert.That(secondTurnSnapshot.PlayerEnergy, Is.EqualTo(4));
        }

        [Test]
        public void EndTurn_WhenPlayerDies_DeletesSavedRun()
        {
            FakeRunSaveService runSaveService = new FakeRunSaveService();
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                playerMaxHp: 3,
                starterDeck: new[] { CreateCard(1001, "Strike", 1, 1) },
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 30, CreateAction(1, 5, RepeatRule.RepeatAfterOpening)), 10) });
            BattleSceneFlowService service = CreateServiceWithRunSave(runDefinition, runSaveService, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.EndTurn();

            Assert.That(runSaveService.DeleteCallCount, Is.EqualTo(1));
        }

        [Test]
        public void InitializeFromSave_RestoresSavedState()
        {
            RuntimeCard reward = CreateCard(1002, "Reward", 1, 5);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                rewardCards: new[] { CreateRewardEntry(reward, 10, 1, 99) });
            BattleSceneFlowService service = CreateService(runDefinition, 0);
            RunSaveData saveData = new RunSaveData
            {
                RunProfileId = 5501,
                PlayerMaxHp = 50,
                PlayerHp = 23,
                PlayerEnergy = 3,
                Gold = 177,
                CurrentNodeIndex = 0,
                CurrentPage = (int)BattleScenePage.Map,
                DeckCardIds = new List<int> { 1002 },
                OwnedRelicIds = new List<int>(),
                IsCardRemovalSoldOut = true,
                CardRemovalCount = 1,
                ShopItems = new List<SaveShopItem>
                {
                    new SaveShopItem { SlotIndex = 0, RewardType = (int)RewardType.Card, CardId = 1002, ItemId = 0, Price = 50, IsSoldOut = true }
                }
            };

            service.InitializeFromSave(saveData);
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Map));
            Assert.That(snapshot.PlayerHp, Is.EqualTo(23));
            Assert.That(snapshot.Gold, Is.EqualTo(177));
            Assert.That(snapshot.CurrentNodeIndex, Is.EqualTo(0));
            Assert.That(snapshot.ShopItems.Count, Is.EqualTo(1));
            Assert.That(snapshot.ShopItems[0].IsSoldOut, Is.True);
            Assert.That(snapshot.ShopItems[0].Card.Id, Is.EqualTo(1002));
            Assert.That(snapshot.IsCardRemovalSoldOut, Is.True);
            Assert.That(snapshot.CardRemovalPrice, Is.EqualTo(75)); // FakeBattleShopService returns 75

            service.SelectMapNode(1);
            BattleSceneSnapshot battleSnapshot = service.CreateSnapshot();
            Assert.That(battleSnapshot.Hand.Count, Is.EqualTo(1));
            Assert.That(battleSnapshot.Hand[0].Id, Is.EqualTo(1002));
        }

        [Test]
        public void OpenShop_TransitionsToShopPage()
        {
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.RestShop, "Rest", new[] { 1 }) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.OpenShop();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Shop));
        }

        [Test]
        public void LeaveShop_ReturnsToRestShopWithContinueEnabled()
        {
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.RestShop, "Rest", new[] { 1 }) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.OpenShop();
            service.LeaveShop();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.RestShop));
            Assert.That(snapshot.IsRestShopContinueEnabled, Is.True);
        }

        [Test]
        public void PurchaseShopItem_Relic_AddsOwnedRelicToSnapshotAndNextBattle()
        {
            RuntimeRelic relic = CreateRelic(1, "Burning Core", new[]
            {
                new RuntimeRelicEffect(1, RelicTriggerType.CombatStart, EffectType.GainBlock, 6, 1, StatusType.None, 0, TargetSide.Self)
            });
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[]
                {
                    CreateNode(5301, 1, InGameNodeType.RestShop, "Rest", new[] { 1 }),
                    CreateNode(5302, 2, InGameNodeType.Battle, "Battle", Array.Empty<int>())
                },
                relicCatalog: new Dictionary<int, RuntimeRelic> { { relic.Id, relic } },
                shopLineup: new RuntimeShopLineup(
                    1,
                    new[]
                    {
                        new RuntimeShopSlot(7, RewardType.Relic, CardType.Attack, 100)
                    }),
                itemPriceRules: new[]
                {
                    new RuntimeItemPriceRule(RewardType.Relic, relic.Id, 80, 0)
                });
            BattleShopService shopService = new BattleShopService(new FakeMasterDataService());
            BattleSceneFlowService service = CreateServiceWithShop(runDefinition, shopService, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.OpenShop();

            BattleSceneSnapshot shopSnapshot = service.CreateSnapshot();
            Assert.That(shopSnapshot.ShopItems.Count, Is.EqualTo(1));
            Assert.That(shopSnapshot.ShopItems[0].RewardType, Is.EqualTo(RewardType.Relic));
            Assert.That(shopSnapshot.ShopItems[0].Relic, Is.Not.Null);

            service.PurchaseShopItem(shopSnapshot.ShopItems[0].SlotIndex);

            BattleSceneSnapshot purchasedSnapshot = service.CreateSnapshot();
            Assert.That(purchasedSnapshot.OwnedRelics.Count, Is.EqualTo(1));
            Assert.That(purchasedSnapshot.OwnedRelics[0].DisplayName, Is.EqualTo("Burning Core"));

            service.LeaveShop();
            service.ContinueFromRestShop();
            service.SelectMapNode(1);

            BattleSceneSnapshot battleSnapshot = service.CreateSnapshot();
            Assert.That(battleSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.Battle));
            Assert.That(battleSnapshot.OwnedRelics.Count, Is.EqualTo(1));
            Assert.That(battleSnapshot.OwnedRelics[0].DisplayName, Is.EqualTo("Burning Core"));
        }

        [Test]
        public void SelectMapNode_EventNode_OpensEventWithEventSet()
        {
            RuntimeEvent evt = new RuntimeEvent(
                9001, "TestEvent", "event.test_title", "event.test", "img_test",
                new[] { new RuntimeEventChoice(1, "Choice 1", EffectType.GainGold, 50) });
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Event, "Event", new[] { 1 }) },
                events: new[] { evt });
            BattleSceneFlowService service = CreateService(runDefinition, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Event));
            Assert.That(snapshot.CurrentEvent, Is.Not.Null);
            Assert.That(snapshot.CurrentEvent.EventName, Is.EqualTo("TestEvent"));
        }

        [Test]
        public void SelectEventChoice_GainGold_AppliesEffectAndReturnsToMap()
        {
            RuntimeEvent evt = new RuntimeEvent(
                9001, "GoldFountain", "event.fountain_title", "event.fountain", "img_fountain",
                new[] { new RuntimeEventChoice(1, "Take Gold", EffectType.GainGold, 50) });
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                startingGold: 100,
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Event, "Event", new[] { 1 }) },
                events: new[] { evt });
            BattleSceneFlowService service = CreateService(runDefinition, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectEventChoice(1);
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Map));
            Assert.That(snapshot.Gold, Is.EqualTo(150));
            Assert.That(snapshot.CurrentEvent, Is.Null);
        }

        [Test]
        public void TryPlaySelectedCard_GainBlock_ReducesIncomingDamage()
        {
            RuntimeCard guard = CreateCard(1001, "Guard", 1, 0, new[]
            {
                new RuntimeCardEffect(1, EffectType.GainBlock, 5, 1, StatusType.None, 0, TargetSide.Self)
            });
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { guard },
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }) },
                battleEncounters: new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 10, CreateAction(1, 7, RepeatRule.RepeatAfterOpening)), 10) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectHandCard(0);
            service.TryPlaySelectedCard();
            service.EndTurn();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.PlayerHp, Is.EqualTo(48));
        }

        private static BattleSceneFlowService CreateService(RuntimeRunDefinition runDefinition, params int[] values)
        {
            return new BattleSceneFlowService(
                new BattleSceneRules(),
                new SequenceRandomProvider(values),
                new FakeBattleMasterDataFacade(runDefinition),
                new BattleRewardService(),
                new BattleSnapshotFactory(new BattleDisplayTextService(), new FakeBattleShopService()),
                new FakeBattleShopService(),
                new BattleCombatEventService(new BattleRelicService()),
                new BattleRelicService(),
                new BattlePotionService(),
                new BattleEventService());
        }

        private static BattleSceneFlowService CreateServiceWithCombatEvents(
            RuntimeRunDefinition runDefinition,
            IBattleCombatEventService combatEventService,
            params int[] values)
        {
            return new BattleSceneFlowService(
                new BattleSceneRules(),
                new SequenceRandomProvider(values),
                new FakeBattleMasterDataFacade(runDefinition),
                new BattleRewardService(),
                new BattleSnapshotFactory(new BattleDisplayTextService(), new FakeBattleShopService()),
                new FakeBattleShopService(),
                combatEventService,
                new BattleRelicService(),
                new BattlePotionService(),
                new BattleEventService());
        }

        private static BattleSceneFlowService CreateServiceWithRunSave(
            RuntimeRunDefinition runDefinition,
            IRunSaveService runSaveService,
            params int[] values)
        {
            return new BattleSceneFlowService(
                new BattleSceneRules(),
                new SequenceRandomProvider(values),
                new FakeBattleMasterDataFacade(runDefinition),
                new BattleRewardService(),
                new BattleSnapshotFactory(new BattleDisplayTextService(), new FakeBattleShopService()),
                new FakeBattleShopService(),
                new BattleCombatEventService(new BattleRelicService()),
                new BattleRelicService(),
                new BattlePotionService(),
                new BattleEventService(),
                runSaveService);
        }

        private static BattleSceneFlowService CreateServiceWithShop(
            RuntimeRunDefinition runDefinition,
            IBattleShopService shopService,
            params int[] values)
        {
            return new BattleSceneFlowService(
                new BattleSceneRules(),
                new SequenceRandomProvider(values),
                new FakeBattleMasterDataFacade(runDefinition),
                new BattleRewardService(),
                new BattleSnapshotFactory(new BattleDisplayTextService(), shopService),
                shopService,
                new BattleCombatEventService(new BattleRelicService()),
                new BattleRelicService(),
                new BattlePotionService(),
                new BattleEventService());
        }

        private static BattleSceneFlowService CreateServiceWithDisplayText(
            RuntimeRunDefinition runDefinition,
            IBattleDisplayTextService displayTextService,
            params int[] values)
        {
            return new BattleSceneFlowService(
                new BattleSceneRules(),
                new SequenceRandomProvider(values),
                new FakeBattleMasterDataFacade(runDefinition),
                new BattleRewardService(),
                new BattleSnapshotFactory(displayTextService, new FakeBattleShopService()),
                new FakeBattleShopService(),
                new BattleCombatEventService(new BattleRelicService()),
                new BattleRelicService(),
                new BattlePotionService(),
                new BattleEventService());
        }

        private static RuntimeRunDefinition CreateRunDefinition(
            int playerMaxHp = 50,
            int startingGold = 120,
            int relicDropChance = 0,
            IReadOnlyList<RuntimeMapNode> nodes = null,
            IReadOnlyList<RuntimeCard> starterDeck = null,
            IReadOnlyList<RuntimeRewardEntry> rewardCards = null,
            IReadOnlyList<RuntimeEncounterEntry> battleEncounters = null,
            IReadOnlyList<RuntimeEncounterEntry> eliteEncounters = null,
            IReadOnlyList<RuntimeEncounterEntry> bossEncounters = null,
            IReadOnlyList<RuntimeEvent> events = null,
            IReadOnlyDictionary<int, RuntimeRelic> relicCatalog = null,
            RuntimeShopLineup shopLineup = null,
            IReadOnlyList<RuntimeItemPriceRule> itemPriceRules = null)
        {
            Dictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>> encounters =
                new Dictionary<InGameNodeType, IReadOnlyList<RuntimeEncounterEntry>>
                {
                    { InGameNodeType.Battle, battleEncounters ?? new[] { CreateEncounter(CreateEnemy(3001, "Slime", 18, 18, 14, CreateAction(1, 4, RepeatRule.RepeatAfterOpening)), 10) } },
                    { InGameNodeType.EliteBattle, eliteEncounters ?? new[] { CreateEncounter(CreateEnemy(3002, "Guard", 24, 24, 30, CreateAction(1, 6, RepeatRule.RepeatAfterOpening)), 10) } },
                    { InGameNodeType.Boss, bossEncounters ?? new[] { CreateEncounter(CreateEnemy(3003, "Boss", 40, 40, 100, CreateAction(1, 8, RepeatRule.RepeatAfterOpening)), 10) } }
                };

            return new RuntimeRunDefinition(
                5501,
                "run_test",
                CharacterArchetype.CrimsonExile,
                playerMaxHp,
                startingGold,
                3,
                0,
                relicDropChance,
                starterDeck ?? new[] { CreateCard(1001, "Strike", 1, 6) },
                rewardCards ?? new[] { CreateRewardEntry(CreateCard(1002, "Reward", 1, 5), 10, 1, 99) },
                nodes ?? new[]
                {
                    CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }),
                    CreateNode(5302, 2, InGameNodeType.Boss, "Boss", new int[0])
                },
                encounters,
                events ?? null,
                relicCatalog ?? new Dictionary<int, RuntimeRelic>(),
                new Dictionary<int, RuntimePotion>(),
                shopLineup,
                null,
                itemPriceRules);
        }

        private static RuntimeCard CreateCard(int id, string displayName, int cost, int damage, IReadOnlyList<RuntimeCardEffect> effects = null)
        {
            return new RuntimeCard(
                id,
                $"card_{id}",
                displayName,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                cost,
                CardType.Attack,
                CardRarity.Common,
                CharacterArchetype.CrimsonExile,
                effects ?? new[]
                {
                    new RuntimeCardEffect(1, EffectType.DealDamage, damage, 1, StatusType.None, 0, TargetSide.Enemy)
                });
        }

        private static RuntimeEnemy CreateEnemy(int id, string displayName, int hpMin, int hpMax, int goldReward, params RuntimeEnemyAction[] actions)
        {
            return new RuntimeEnemy(
                id,
                $"enemy_{id}",
                displayName,
                string.Empty,
                EnemyTier.Normal,
                hpMin,
                hpMax,
                goldReward,
                actions);
        }

        private static RuntimeEnemyAction CreateAction(
            int order,
            int damage,
            RepeatRule repeatRule,
            IntentType intentType = IntentType.Attack,
            int hitCount = 1,
            int block = 0,
            StatusType statusType = StatusType.None,
            int statusValue = 0,
            BuffType buffType = BuffType.None,
            int buffValue = 0)
        {
            return new RuntimeEnemyAction(
                order,
                intentType,
                damage,
                hitCount,
                block,
                statusType,
                statusValue,
                buffType,
                buffValue,
                repeatRule);
        }

        private static RuntimeMapNode CreateNode(int id, int floor, InGameNodeType nodeType, string displayName, IReadOnlyList<int> nextNodeIndices)
        {
            return new RuntimeMapNode(id, $"node_{id}", floor, nodeType, displayName, string.Empty, nextNodeIndices);
        }

        private static RuntimeEncounterEntry CreateEncounter(RuntimeEnemy enemy, int weight)
        {
            return CreateEncounter(CreateFormation(CreateEnemyEntry(enemy, 0)), weight);
        }

        private static RuntimeEncounterEntry CreateEncounter(RuntimeEncounterFormation formation, int weight)
        {
            return new RuntimeEncounterEntry(formation, weight);
        }

        private static RuntimeEncounterFormation CreateFormation(params RuntimeEncounterEnemyEntry[] enemies)
        {
            return new RuntimeEncounterFormation(7001, "formation_test", "Formation", enemies);
        }

        private static RuntimeEncounterEnemyEntry CreateEnemyEntry(RuntimeEnemy enemy, int slotIndex)
        {
            return new RuntimeEncounterEnemyEntry(enemy, slotIndex);
        }

        private static RuntimeRewardEntry CreateRewardEntry(RuntimeCard card, int weight, int minFloor, int maxFloor)
        {
            return new RuntimeRewardEntry(RewardType.Card, card != null ? card.Id : 0, card, null, null, weight, minFloor, maxFloor);
        }

        private static RuntimeRelic CreateRelic(int id, string displayName, IReadOnlyList<RuntimeRelicEffect> effects)
        {
            return new RuntimeRelic(
                id,
                $"relic_{id}",
                displayName,
                string.Empty,
                $"{displayName} description",
                string.Empty,
                $"relic_icon_{id}",
                CardRarity.Uncommon,
                effects);
        }

        /// <summary>
        /// テスト用MasterDataFacade
        /// </summary>
        private sealed class FakeBattleMasterDataFacade : IBattleMasterDataFacade
        {
            private readonly RuntimeRunDefinition _runDefinition;

            public FakeBattleMasterDataFacade(RuntimeRunDefinition runDefinition)
            {
                _runDefinition = runDefinition;
            }

            public RuntimeRunDefinition BuildRunDefinition(int runProfileId)
            {
                return _runDefinition;
            }

            public IReadOnlyDictionary<int, RuntimeCard> BuildCardCatalog()
            {
                Dictionary<int, RuntimeCard> catalog = new Dictionary<int, RuntimeCard>();
                if (_runDefinition != null)
                {
                    if (_runDefinition.StarterDeck != null)
                    {
                        foreach (RuntimeCard card in _runDefinition.StarterDeck)
                        {
                            if (card != null && !catalog.ContainsKey(card.Id))
                            {
                                catalog.Add(card.Id, card);
                            }
                        }
                    }
                    if (_runDefinition.RewardPool != null)
                    {
                        foreach (RuntimeRewardEntry entry in _runDefinition.RewardPool)
                        {
                            if (entry != null && entry.Card != null && !catalog.ContainsKey(entry.Card.Id))
                            {
                                catalog.Add(entry.Card.Id, entry.Card);
                            }
                        }
                    }
                }
                return catalog;
            }
        }

        private sealed class FakeMasterDataService : IMasterDataService
        {
            public UniTask InitializeAsync(CancellationToken ct) => UniTask.CompletedTask;

            public IReadOnlyList<T> GetAll<T>() where T : class, IMasterDataObject
            {
                return Array.Empty<T>();
            }

            public T Get<T, TKey>(TKey key) where T : class, IMasterDataObject<TKey>
            {
                return null;
            }

            public T GetContainer<T>() where T : class
            {
                return null;
            }

            public UniTask DownloadFromServerAsync(CancellationToken ct) => UniTask.CompletedTask;

            public UniTask ReloadAsync(CancellationToken ct) => UniTask.CompletedTask;
        }

        /// <summary>
        /// テスト用Battle表示名解決クラス
        /// </summary>
        private sealed class FakeBattleDisplayTextService : IBattleDisplayTextService
        {
            public string GetIntentName(IntentType intentType)
            {
                return $"表示{intentType}";
            }

            public string GetStatusName(StatusType statusType)
            {
                return $"表示{statusType}";
            }

            public string GetBuffName(BuffType buffType)
            {
                return $"表示{buffType}";
            }
        }

        /// <summary>
        /// テスト用戦闘イベント通知クラス
        /// </summary>
        private sealed class FakeBattleCombatEventService : IBattleCombatEventService
        {
            private readonly List<string> _events = new List<string>();

            public IReadOnlyList<string> Events => _events;

            public void OnCombatStart(BattleSceneState state)
            {
                _events.Add("CombatStart");
            }

            public void OnPlayerTurnStart(BattleSceneState state)
            {
                _events.Add("PlayerTurnStart");
            }

            public void OnPlayerTurnEnd(BattleSceneState state)
            {
                _events.Add("PlayerTurnEnd");
            }

            public void OnCardPlayed(BattleSceneState state, RuntimeCard card, BattleCardResolutionResult result)
            {
                _events.Add($"CardPlayed:{card.DisplayName}:{result.TotalDamage}");
            }

            public void OnPlayerDamaged(BattleSceneState state, int damage)
            {
                _events.Add($"PlayerDamaged:{damage}");
            }
        }

        private sealed class FakeRunSaveService : IRunSaveService
        {
            public int SaveCallCount { get; private set; }
            public int DeleteCallCount { get; private set; }
            public RunSaveData LastSavedData { get; private set; }

            public UniTask SaveCurrentRunAsync(RunSaveData data, System.Threading.CancellationToken token = default)
            {
                SaveCallCount++;
                LastSavedData = data;
                return UniTask.CompletedTask;
            }

            public UniTask<RunSaveData> LoadCurrentRunAsync(System.Threading.CancellationToken token = default)
            {
                return UniTask.FromResult<RunSaveData>(null);
            }

            public bool HasSavedRun()
            {
                return LastSavedData != null;
            }

            public void DeleteSavedRun()
            {
                DeleteCallCount++;
            }
        }

        /// <summary>
        /// 固定乱数提供クラス
        /// </summary>
        private sealed class SequenceRandomProvider : IBattleRandomProvider
        {
            private readonly Queue<int> _values;

            public SequenceRandomProvider(IEnumerable<int> values)
            {
                _values = new Queue<int>(values);
            }

            public int Range(int minInclusive, int maxExclusive)
            {
                if (_values.Count == 0)
                {
                    return minInclusive;
                }

                int value = _values.Dequeue();
                if (value < minInclusive)
                {
                    return minInclusive;
                }

                if (value >= maxExclusive)
                {
                    return maxExclusive - 1;
                }

                return value;
            }
        }

        private sealed class FakeBattleShopService : IBattleShopService
        {
            public void InitializeShop(BattleSceneState state, RuntimeRunDefinition runDef, IBattleRandomProvider random) {}
            public bool PurchaseShopItem(BattleSceneState state, int slotIndex) => false;
            public int GetCardRemovalPrice(BattleSceneState state) => 75;
            public bool PurchaseCardRemoval(BattleSceneState state, RuntimeCard card) => false;
        }
    }
}
