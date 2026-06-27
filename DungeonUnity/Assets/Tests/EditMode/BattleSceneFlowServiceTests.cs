using System;
using System.Collections.Generic;
using System.Threading;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Domain;
using Dungeon.Runtime.InGame.Save.Model;
using Dungeon.Runtime.InGame.Save.Services;
using Dungeon.Tests.EditMode.Support;
using Game.MasterData.Generated;
using NUnit.Framework;
using Cysharp.Threading.Tasks;
using TFramework.MasterData;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleSceneFlowServiceのEditorモードテストクラス
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
            Assert.That(Combat(snapshot).PlayerMaxHp, Is.EqualTo(50));
            Assert.That(Combat(snapshot).PlayerHp, Is.EqualTo(50));
            Assert.That(Shop(snapshot).Gold, Is.EqualTo(120));
            Assert.That(Map(snapshot).Nodes.Count, Is.EqualTo(2));
            Assert.That(Map(snapshot).AvailableNodeIndices, Is.EqualTo(new[] { 0 }));
            Assert.That(Map(snapshot).MapMessage, Does.Contain("Next 1/2"));
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
            Assert.That(Map(snapshot).AvailableNodeIndices, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void SelectMapNode_UnavailableNode_KeepsCurrentNodeAndShowsMessage()
        {
            BattleSceneFlowService service = CreateService(CreateRunDefinition(), 0);

            service.Initialize(5501);
            service.SelectMapNode(1);
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(Map(snapshot).MapMessage, Is.EqualTo("You can only go to the next node."));
            Assert.That(Map(snapshot).AvailableNodeIndices, Is.EqualTo(new[] { 0 }));
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
            Assert.That(Combat(snapshot).HandCards.Count, Is.EqualTo(3));
            Assert.That(Combat(snapshot).CurrentEnemy.DisplayName, Is.EqualTo("Slime"));
            Assert.That(Combat(snapshot).Enemies.Count, Is.EqualTo(1));
            Assert.That(Combat(snapshot).BattleHintMessage, Is.EqualTo("Select target, then use card."));
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
            for (int i = 0; i < Combat(snapshot).HandCards.Count; i++)
            {
                handCardIds.Add(Combat(snapshot).HandCards[i].Card.Id);
            }

            Assert.That(Combat(snapshot).HandCards.Count, Is.EqualTo(3));
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
            Assert.That(Combat(snapshot).HandCards.Count, Is.EqualTo(1));
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
            Assert.That(Combat(snapshot).HandCards.Count, Is.EqualTo(1));
            Assert.That(Combat(snapshot).HandCards[0].Card.Id, Is.EqualTo(1001));
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

            Assert.That(Combat(snapshot).HandCards.Count, Is.EqualTo(10));
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

            Assert.That(Combat(snapshot).PlayerEnergy, Is.EqualTo(4));
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

            Assert.That(Combat(snapshot).Enemies.Count, Is.EqualTo(2));
            Assert.That(Combat(snapshot).Enemies[0].DisplayName, Is.EqualTo("Mite"));
            Assert.That(Combat(snapshot).Enemies[1].DisplayName, Is.EqualTo("Slime"));
            Assert.That(Combat(snapshot).SelectedEnemyIndex, Is.EqualTo(0));
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

            Assert.That(Combat(snapshot).EnemyIntent, Is.Not.Null);
            Assert.That(Combat(snapshot).EnemyIntent.IntentType, Is.EqualTo(IntentType.AttackDefend));
            Assert.That(Combat(snapshot).EnemyIntent.IntentName, Is.EqualTo(nameof(IntentType.AttackDefend)));
            Assert.That(Combat(snapshot).EnemyIntent.Damage, Is.EqualTo(7));
            Assert.That(Combat(snapshot).EnemyIntent.HitCount, Is.EqualTo(2));
            Assert.That(Combat(snapshot).EnemyIntent.Block, Is.EqualTo(5));
            Assert.That(Combat(snapshot).EnemyIntent.StatusType, Is.EqualTo(StatusType.Weak));
            Assert.That(Combat(snapshot).EnemyIntent.StatusName, Is.EqualTo(nameof(StatusType.Weak)));
            Assert.That(Combat(snapshot).EnemyIntent.StatusValue, Is.EqualTo(2));
            Assert.That(Combat(snapshot).EnemyIntent.BuffType, Is.EqualTo(BuffType.Ritual));
            Assert.That(Combat(snapshot).EnemyIntent.BuffName, Is.EqualTo(nameof(BuffType.Ritual)));
            Assert.That(Combat(snapshot).EnemyIntent.BuffValue, Is.EqualTo(3));
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

            Assert.That(Combat(snapshot).PlayerStatuses.Count, Is.EqualTo(1));
            Assert.That(Combat(snapshot).PlayerStatuses[0].Name, Is.EqualTo("表示Vulnerable"));
            Assert.That(Combat(snapshot).PlayerStatuses[0].Value, Is.EqualTo(2));
            Assert.That(Combat(snapshot).EnemyStatuses.Count, Is.EqualTo(1));
            Assert.That(Combat(snapshot).EnemyStatuses[0].Name, Is.EqualTo("表示Weak"));
            Assert.That(Combat(snapshot).EnemyStatuses[0].Value, Is.EqualTo(1));
            Assert.That(Combat(snapshot).EnemyBuffs.Count, Is.EqualTo(1));
            Assert.That(Combat(snapshot).EnemyBuffs[0].Name, Is.EqualTo("表示Ritual"));
            Assert.That(Combat(snapshot).EnemyBuffs[0].Value, Is.EqualTo(3));
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
            Assert.That(Shop(snapshot).Gold, Is.EqualTo(120));
            Assert.That(Reward(snapshot).BattleGoldReward, Is.EqualTo(30));
            Assert.That(Reward(snapshot).RewardChoices.Count, Is.EqualTo(1));
            Assert.That(Reward(snapshot).RewardChoices[0].Card.DisplayName, Is.EqualTo("Reward"));
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

            Assert.That(Combat(snapshot).Enemies[0].Hp, Is.EqualTo(10));
            Assert.That(Combat(snapshot).Enemies[1].Hp, Is.EqualTo(6));
            Assert.That(Combat(snapshot).SelectedEnemyIndex, Is.EqualTo(1));
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
            Assert.That(Shop(snapshot).Gold, Is.EqualTo(120));
            Assert.That(Reward(snapshot).BattleGoldReward, Is.EqualTo(12));
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
            Assert.That(Result(snapshot).ResultMessage, Is.EqualTo("Run Failed"));
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
            Assert.That(RestShop(restSnapshot).IsRestShopContinueEnabled, Is.True);
            Assert.That(RestShop(restSnapshot).RestShopMessage, Does.Contain("Rest done."));

            service.ContinueFromRestShop();
            BattleSceneSnapshot mapSnapshot = service.CreateSnapshot();

            Assert.That(mapSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.Map));
        }

        [Test]
        public void ApplyUpgrade_WithNoUpgradeableCards_StaysInRestShop()
        {
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.RestShop, "Rest", new[] { 1 }) },
                starterDeck: new[] { CreateCard(1001, "Strike", 1, 6) });
            BattleSceneFlowService service = CreateService(runDefinition, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.ApplyUpgrade();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.RestShop));
            Assert.That(RestShop(snapshot).RestShopMessage, Is.EqualTo("No cards can be upgraded."));
            Assert.That(RestShop(snapshot).IsRestShopContinueEnabled, Is.False);
        }

        [Test]
        public void ApplyUpgrade_WithUpgradeableCards_OpensCardSelectInUpgradeMode()
        {
            RuntimeCard strike = CreateCard(1001, "Strike", 1, 6, upgradeCardId: 1002);
            RuntimeCard strikePlus = CreateCard(1002, "Strike+", 1, 9, isUpgraded: true);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.RestShop, "Rest", new[] { 1 }) },
                starterDeck: new[] { strike, strikePlus });
            BattleSceneFlowService service = CreateService(runDefinition, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.ApplyUpgrade();

            Assert.That(service.CreateSnapshot().CurrentPage, Is.EqualTo(BattleScenePage.CardSelect));
            Assert.That(service.GetCardSelectMode(), Is.EqualTo(CardSelectMode.Upgrade));
            Assert.That(service.GetCardSelectCards(), Has.Count.EqualTo(1));
            Assert.That(service.GetCardSelectCards()[0].Id, Is.EqualTo(1001));
            Assert.That(service.GetCardSelectPrices()[1001], Is.EqualTo(25));
        }

        [Test]
        public void ApplyUpgrade_WithInsufficientGold_OpensCardSelectWithCards()
        {
            RuntimeCard strike = CreateCard(1001, "Strike", 1, 6, upgradeCardId: 1002);
            RuntimeCard strikePlus = CreateCard(1002, "Strike+", 1, 9, isUpgraded: true);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                startingGold: 20,
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.RestShop, "Rest", new[] { 1 }) },
                starterDeck: new[] { strike },
                additionalCards: new[] { strikePlus });
            BattleSceneFlowService service = CreateService(runDefinition, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.ApplyUpgrade();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.CardSelect));
            Assert.That(service.GetCardSelectCards(), Has.Count.EqualTo(1));
            Assert.That(service.GetCardSelectCards()[0].Id, Is.EqualTo(1001));
            Assert.That(service.GetCardSelectPrices()[1001], Is.EqualTo(25));
            Assert.That(service.GetCardSelectUpgradedCards()[1001].Id, Is.EqualTo(1002));
            Assert.That(Shop(snapshot).Gold, Is.EqualTo(20));
        }

        [Test]
        public void ConfirmCardSelect_UpgradeMode_WithInsufficientGold_DoesNotUpgrade()
        {
            RuntimeCard strike = CreateCard(1001, "Strike", 1, 6, upgradeCardId: 1002);
            RuntimeCard strikePlus = CreateCard(1002, "Strike+", 1, 9, isUpgraded: true);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                startingGold: 20,
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.RestShop, "Rest", new[] { 1 }) },
                starterDeck: new[] { strike },
                additionalCards: new[] { strikePlus });
            BattleSceneFlowService service = CreateService(runDefinition, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.ApplyUpgrade();
            service.ConfirmCardSelect(strike);
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.CardSelect));
            Assert.That(Shop(snapshot).Gold, Is.EqualTo(20));
            Assert.That(service.GetDeckCards()[0].Id, Is.EqualTo(1001));
            Assert.That(service.GetCardSelectMessage(), Is.EqualTo("Not enough gold."));
        }

        [Test]
        public void ConfirmCardSelect_UpgradeMode_StaysOpenAndHidesUpgradedCard()
        {
            FakeRunSaveService runSaveService = new FakeRunSaveService();
            RuntimeCard strike = CreateCard(1001, "Strike", 1, 6, upgradeCardId: 1002);
            RuntimeCard strikePlus = CreateCard(1002, "Strike+", 1, 9, isUpgraded: true);
            RuntimeCard guard = CreateCard(1003, "Guard", 1, 0, upgradeCardId: 1004);
            RuntimeCard guardPlus = CreateCard(1004, "Guard+", 1, 0, isUpgraded: true);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.RestShop, "Rest", new[] { 1 }) },
                starterDeck: new[] { strike, guard },
                additionalCards: new[] { strikePlus, guardPlus });
            BattleSceneFlowService service = CreateServiceWithRunSave(runDefinition, runSaveService, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.ApplyUpgrade();
            service.ConfirmCardSelect(strike);
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.CardSelect));
            Assert.That(service.GetCardSelectMessage(), Does.Contain("Strike"));
            Assert.That(service.GetCardSelectCards(), Has.Count.EqualTo(1));
            Assert.That(service.GetCardSelectCards()[0].Id, Is.EqualTo(1003));
            Assert.That(RestShop(snapshot).IsRestShopContinueEnabled, Is.True);
            Assert.That(Shop(snapshot).Gold, Is.EqualTo(95));
            Assert.That(service.GetDeckCards()[0].Id, Is.EqualTo(1002));
            Assert.That(runSaveService.LastSavedData.DeckCardIds[0], Is.EqualTo(1002));
            Assert.That(runSaveService.LastSavedData.Gold, Is.EqualTo(95));
        }

        [Test]
        public void ConfirmCardSelect_UpgradeMode_ChargesEachUpgrade()
        {
            RuntimeCard strike = CreateCard(1001, "Strike", 1, 6, upgradeCardId: 1101);
            RuntimeCard guard = CreateCard(1002, "Guard", 1, 0, upgradeCardId: 1102);
            RuntimeCard strikePlus = CreateCard(1101, "Strike+", 1, 9, isUpgraded: true);
            RuntimeCard guardPlus = CreateCard(1102, "Guard+", 1, 0, isUpgraded: true);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.RestShop, "Rest", new[] { 1 }) },
                starterDeck: new[] { strike, guard },
                additionalCards: new[] { strikePlus, guardPlus });
            BattleSceneFlowService service = CreateService(runDefinition, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.ApplyUpgrade();
            service.ConfirmCardSelect(strike);
            service.ConfirmCardSelect(guard);
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.CardSelect));
            Assert.That(service.GetCardSelectCards(), Is.Empty);
            Assert.That(Shop(snapshot).Gold, Is.EqualTo(70));
            Assert.That(service.GetDeckCards()[0].Id, Is.EqualTo(1101));
            Assert.That(service.GetDeckCards()[1].Id, Is.EqualTo(1102));
        }

        [Test]
        public void CancelCardSelect_AfterUpgrade_ReturnsToRestShopWithContinueEnabled()
        {
            RuntimeCard strike = CreateCard(1001, "Strike", 1, 6, upgradeCardId: 1002);
            RuntimeCard strikePlus = CreateCard(1002, "Strike+", 1, 9, isUpgraded: true);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.RestShop, "Rest", new[] { 1 }) },
                starterDeck: new[] { strike },
                additionalCards: new[] { strikePlus });
            BattleSceneFlowService service = CreateService(runDefinition, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.ApplyUpgrade();
            service.ConfirmCardSelect(strike);
            service.CancelCardSelect();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.RestShop));
            Assert.That(RestShop(snapshot).RestShopMessage, Does.Contain("Strike -> Strike+"));
            Assert.That(RestShop(snapshot).IsRestShopContinueEnabled, Is.True);
        }

        [Test]
        public void InitializeFromSave_WithUpgradedCardId_RestoresUpgradedDeckCard()
        {
            RuntimeCard strike = CreateCard(1001, "Strike", 1, 6, upgradeCardId: 1101);
            RuntimeCard strikePlus = CreateCard(1101, "Strike+", 1, 9, isUpgraded: true);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { strike },
                additionalCards: new[] { strikePlus });
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
                DeckCardIds = new List<int> { 1101 }
            };

            service.InitializeFromSave(saveData);

            Assert.That(service.GetDeckCards(), Has.Count.EqualTo(1));
            Assert.That(service.GetDeckCards()[0].Id, Is.EqualTo(1101));
            Assert.That(service.GetDeckCards()[0].IsUpgraded, Is.True);
        }

        [Test]
        public void CancelCardSelect_UpgradeMode_ReturnsToRestShopWithoutChangingDeck()
        {
            RuntimeCard strike = CreateCard(1001, "Strike", 1, 6, upgradeCardId: 1002);
            RuntimeCard strikePlus = CreateCard(1002, "Strike+", 1, 9, isUpgraded: true);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.RestShop, "Rest", new[] { 1 }) },
                starterDeck: new[] { strike },
                additionalCards: new[] { strikePlus });
            BattleSceneFlowService service = CreateService(runDefinition, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.ApplyUpgrade();
            service.CancelCardSelect();
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.RestShop));
            Assert.That(service.GetDeckCards()[0].Id, Is.EqualTo(1001));
            Assert.That(RestShop(snapshot).IsRestShopContinueEnabled, Is.False);
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
        public void ConfirmCardSelect_UpgradeMode_RequestSave_NormalizesCheckpointToRestShop()
        {
            FakeRunSaveService runSaveService = new FakeRunSaveService();
            RuntimeCard strike = CreateCard(1001, "Strike", 1, 6, upgradeCardId: 1002);
            RuntimeCard strikePlus = CreateCard(1002, "Strike+", 1, 9, isUpgraded: true);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.RestShop, "Rest", new[] { 1 }) },
                starterDeck: new[] { strike },
                additionalCards: new[] { strikePlus });
            BattleSceneFlowService service = CreateServiceWithRunSave(runDefinition, runSaveService, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.ApplyUpgrade();
            service.ConfirmCardSelect(strike);

            Assert.That(runSaveService.LastSavedData.CurrentPage, Is.EqualTo((int)BattleScenePage.RestShop));
            Assert.That(runSaveService.LastSavedData.DeckCardIds, Is.EqualTo(new[] { 1002 }));
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
        public void ClaimGold_ThenContinueFromReward_UpdatesGoldAndClearsRewardFlags()
        {
            RuntimeCard finisher = CreateCard(1001, "Finisher", 1, 99);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                starterDeck: new[] { finisher },
                rewardCards: new[] { CreateRewardEntry(CreateCard(1002, "Reward", 1, 5), 10, 1, 99) });
            BattleSceneFlowService service = CreateService(runDefinition, 0, 0, 0, 0, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            service.SelectHandCard(0);
            service.TryPlaySelectedCard();
            BattleSceneSnapshot beforeClaimSnapshot = service.CreateSnapshot();
            int startingGold = Shop(beforeClaimSnapshot).Gold;
            int rewardGold = Reward(beforeClaimSnapshot).BattleGoldReward;
            service.ClaimGold();
            BattleSceneSnapshot rewardSnapshot = service.CreateSnapshot();

            Assert.That(Reward(rewardSnapshot).GoldClaimed, Is.True);
            Assert.That(Shop(rewardSnapshot).Gold, Is.EqualTo(startingGold + rewardGold));
            Assert.That(Reward(rewardSnapshot).BattleGoldReward, Is.EqualTo(0));

            service.ContinueFromReward();
            BattleSceneSnapshot mapSnapshot = service.CreateSnapshot();

            Assert.That(mapSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.Map));
            Assert.That(Reward(mapSnapshot).GoldClaimed, Is.False);
            Assert.That(Shop(mapSnapshot).Gold, Is.EqualTo(startingGold + rewardGold));
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

            Assert.That(Combat(snapshot).PlayerBlock, Is.EqualTo(6));
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
            int slotIndex = Shop(service.CreateSnapshot()).ShopItems[0].SlotIndex;
            service.PurchaseShopItem(slotIndex);

            service.InspectOwnedRelic(0);
            BattleSceneSnapshot inspectedSnapshot = service.CreateSnapshot();
            Assert.That(HostChrome(inspectedSnapshot).SelectedOwnedRelicIndex, Is.EqualTo(0));
            Assert.That(HostChrome(inspectedSnapshot).OwnedRelicHintMessage, Does.Contain("Burning Core"));

            service.InspectOwnedRelic(0);
            BattleSceneSnapshot clearedSnapshot = service.CreateSnapshot();
            Assert.That(HostChrome(clearedSnapshot).SelectedOwnedRelicIndex, Is.EqualTo(-1));
            Assert.That(HostChrome(clearedSnapshot).OwnedRelicHintMessage, Is.Empty);

            service.InspectOwnedRelic(0);
            service.LeaveShop();
            BattleSceneSnapshot pageChangedSnapshot = service.CreateSnapshot();
            Assert.That(HostChrome(pageChangedSnapshot).SelectedOwnedRelicIndex, Is.EqualTo(-1));
            Assert.That(HostChrome(pageChangedSnapshot).OwnedRelicHintMessage, Is.Empty);
        }

        [Test]
        public void OwnedInspects_AreMutuallyExclusive_AndPotionUseClearsSelection()
        {
            RuntimeRelic relic = CreateRelic(1, "Burning Core", new[]
            {
                new RuntimeRelicEffect(1, RelicTriggerType.CombatStart, EffectType.GainBlock, 6, 1, StatusType.None, 0, TargetSide.Self)
            });
            RuntimePotion potion = CreatePotion(2, "Fruit Juice", PotionUseContext.Both, PotionTargetMode.Self, new[]
            {
                new RuntimePotionEffect(1, EffectType.GainMaxHp, 5, 1, StatusType.None, 0, TargetSide.Self)
            });
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                relicCatalog: new Dictionary<int, RuntimeRelic> { { relic.Id, relic } },
                potionCatalog: new Dictionary<int, RuntimePotion> { { potion.Id, potion } });
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
                OwnedRelicIds = new List<int> { relic.Id },
                OwnedPotionIds = new List<int> { potion.Id }
            };

            service.InitializeFromSave(saveData);
            service.InspectOwnedRelic(0);
            service.InspectOwnedPotion(0);
            BattleSceneSnapshot potionSnapshot = service.CreateSnapshot();
            Assert.That(HostChrome(potionSnapshot).SelectedOwnedRelicIndex, Is.EqualTo(-1));
            Assert.That(HostChrome(potionSnapshot).SelectedOwnedPotionIndex, Is.EqualTo(0));

            service.UsePotion(0);
            BattleSceneSnapshot usedPotionSnapshot = service.CreateSnapshot();
            Assert.That(HostChrome(usedPotionSnapshot).SelectedOwnedPotionIndex, Is.EqualTo(-1));
            Assert.That(HostChrome(usedPotionSnapshot).OwnedPotionHintMessage, Is.Empty);

            service.InspectOwnedRelic(0);
            service.InspectOwnedPotion(0);
            service.ClearOwnedInspections();
            BattleSceneSnapshot clearedSnapshot = service.CreateSnapshot();
            Assert.That(HostChrome(clearedSnapshot).SelectedOwnedRelicIndex, Is.EqualTo(-1));
            Assert.That(HostChrome(clearedSnapshot).SelectedOwnedPotionIndex, Is.EqualTo(-1));
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

            Assert.That(Combat(firstTurnSnapshot).PlayerEnergy, Is.EqualTo(4));
            Assert.That(Combat(secondTurnSnapshot).PlayerEnergy, Is.EqualTo(4));
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
            Assert.That(Combat(snapshot).PlayerHp, Is.EqualTo(23));
            Assert.That(Shop(snapshot).Gold, Is.EqualTo(177));
            Assert.That(Shop(snapshot).ShopItems.Count, Is.EqualTo(1));
            Assert.That(Shop(snapshot).ShopItems[0].IsSoldOut, Is.True);
            Assert.That(Shop(snapshot).ShopItems[0].Card.Id, Is.EqualTo(1002));
            Assert.That(Shop(snapshot).IsCardRemovalSoldOut, Is.True);
            Assert.That(Shop(snapshot).CardRemovalPrice, Is.EqualTo(75)); // FakeBattleShopService returns 75

            service.SelectMapNode(1);
            BattleSceneSnapshot battleSnapshot = service.CreateSnapshot();
            Assert.That(Combat(battleSnapshot).HandCards.Count, Is.EqualTo(1));
            Assert.That(Combat(battleSnapshot).HandCards[0].Card.Id, Is.EqualTo(1002));
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
            Assert.That(RestShop(snapshot).IsRestShopContinueEnabled, Is.True);
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
            Assert.That(Shop(shopSnapshot).ShopItems.Count, Is.EqualTo(1));
            Assert.That(Shop(shopSnapshot).ShopItems[0].RewardType, Is.EqualTo(RewardType.Relic));
            Assert.That(Shop(shopSnapshot).ShopItems[0].Relic, Is.Not.Null);

            service.PurchaseShopItem(Shop(shopSnapshot).ShopItems[0].SlotIndex);

            BattleSceneSnapshot purchasedSnapshot = service.CreateSnapshot();
            Assert.That(HostChrome(purchasedSnapshot).OwnedRelics.Count, Is.EqualTo(1));
            Assert.That(HostChrome(purchasedSnapshot).OwnedRelics[0].DisplayName, Is.EqualTo("Burning Core"));

            service.LeaveShop();
            service.ContinueFromRestShop();
            service.SelectMapNode(1);

            BattleSceneSnapshot battleSnapshot = service.CreateSnapshot();
            Assert.That(battleSnapshot.CurrentPage, Is.EqualTo(BattleScenePage.Battle));
            Assert.That(HostChrome(battleSnapshot).OwnedRelics.Count, Is.EqualTo(1));
            Assert.That(HostChrome(battleSnapshot).OwnedRelics[0].DisplayName, Is.EqualTo("Burning Core"));
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
            Assert.That(Event(snapshot).CurrentEvent, Is.Not.Null);
            Assert.That(Event(snapshot).CurrentEvent.EventName, Is.EqualTo("TestEvent"));
        }

        [Test]
        public void SelectMapNode_EventNode_WithNoConfiguredEvents_ReturnsToMap()
        {
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                nodes: new[] { CreateNode(5301, 1, InGameNodeType.Event, "Event", new[] { 1 }) },
                events: Array.Empty<RuntimeEvent>());
            BattleSceneFlowService service = CreateService(runDefinition, 0);

            service.Initialize(5501);
            service.SelectMapNode(0);
            BattleSceneSnapshot snapshot = service.CreateSnapshot();

            Assert.That(snapshot.CurrentPage, Is.EqualTo(BattleScenePage.Map));
            Assert.That(Event(snapshot).CurrentEvent, Is.Null);
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
            Assert.That(Shop(snapshot).Gold, Is.EqualTo(150));
            Assert.That(Event(snapshot).CurrentEvent, Is.Null);
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

            Assert.That(Combat(snapshot).PlayerHp, Is.EqualTo(48));
        }

        private static BattleMapSnapshot Map(BattleSceneSnapshot snapshot)
        {
            return snapshot.Map;
        }

        private static BattleCombatSnapshot Combat(BattleSceneSnapshot snapshot)
        {
            return snapshot.Combat;
        }

        private static BattleRewardSnapshot Reward(BattleSceneSnapshot snapshot)
        {
            return snapshot.Reward;
        }

        private static BattleRestShopSnapshot RestShop(BattleSceneSnapshot snapshot)
        {
            return snapshot.RestShop;
        }

        private static BattleShopSnapshot Shop(BattleSceneSnapshot snapshot)
        {
            return snapshot.Shop;
        }

        private static BattleHostChromeSnapshot HostChrome(BattleSceneSnapshot snapshot)
        {
            return snapshot.HostChrome;
        }

        private static BattleEventSnapshot Event(BattleSceneSnapshot snapshot)
        {
            return snapshot.Event;
        }

        private static BattleResultSnapshot Result(BattleSceneSnapshot snapshot)
        {
            return snapshot.Result;
        }

        private static BattleSceneFlowService CreateService(RuntimeRunDefinition runDefinition, params int[] values)
        {
            BattleSceneRules rules = new BattleSceneRules(new BattleDeckService(), new BattleEnemyActionSelector(), new BattleEncounterSelector(), new BattleRewardRollService());
            SequenceRandomProvider randomProvider = new SequenceRandomProvider(values);
            BattleRelicService relicService = new BattleRelicService();
            BattlePotionService potionService = new BattlePotionService();
            BattleEventService eventService = new BattleEventService();
            FakeBattleShopService shopService = new FakeBattleShopService();
            return new BattleSceneFlowService(
                rules,
                randomProvider,
                new FakeBattleMasterDataFacade(runDefinition),
                new BattleRewardFlowService(new BattleRewardService(), rules, randomProvider, potionService, relicService),
                new BattleSnapshotFactory(new BattleDisplayTextService(), shopService),
                shopService,
                new BattleCombatEventService(relicService),
                relicService,
                potionService,
                new BattleEventFlowService(randomProvider, eventService),
                new BattleRestShopFlowService(rules, randomProvider, shopService, potionService, relicService),
                null,
                new BattleCheckpointService());
        }

        private static BattleSceneFlowService CreateServiceWithCombatEvents(
            RuntimeRunDefinition runDefinition,
            IBattleCombatEventService combatEventService,
            params int[] values)
        {
            BattleSceneRules rules = new BattleSceneRules(new BattleDeckService(), new BattleEnemyActionSelector(), new BattleEncounterSelector(), new BattleRewardRollService());
            SequenceRandomProvider randomProvider = new SequenceRandomProvider(values);
            BattleRelicService relicService = new BattleRelicService();
            BattlePotionService potionService = new BattlePotionService();
            BattleEventService eventService = new BattleEventService();
            FakeBattleShopService shopService = new FakeBattleShopService();
            return new BattleSceneFlowService(
                rules,
                randomProvider,
                new FakeBattleMasterDataFacade(runDefinition),
                new BattleRewardFlowService(new BattleRewardService(), rules, randomProvider, potionService, relicService),
                new BattleSnapshotFactory(new BattleDisplayTextService(), shopService),
                shopService,
                combatEventService,
                relicService,
                potionService,
                new BattleEventFlowService(randomProvider, eventService),
                new BattleRestShopFlowService(rules, randomProvider, shopService, potionService, relicService),
                null,
                new BattleCheckpointService());
        }

        private static BattleSceneFlowService CreateServiceWithRunSave(
            RuntimeRunDefinition runDefinition,
            IRunSaveService runSaveService,
            params int[] values)
        {
            BattleSceneRules rules = new BattleSceneRules(new BattleDeckService(), new BattleEnemyActionSelector(), new BattleEncounterSelector(), new BattleRewardRollService());
            SequenceRandomProvider randomProvider = new SequenceRandomProvider(values);
            BattleRelicService relicService = new BattleRelicService();
            BattlePotionService potionService = new BattlePotionService();
            BattleEventService eventService = new BattleEventService();
            FakeBattleShopService shopService = new FakeBattleShopService();
            return new BattleSceneFlowService(
                rules,
                randomProvider,
                new FakeBattleMasterDataFacade(runDefinition),
                new BattleRewardFlowService(new BattleRewardService(), rules, randomProvider, potionService, relicService),
                new BattleSnapshotFactory(new BattleDisplayTextService(), shopService),
                shopService,
                new BattleCombatEventService(relicService),
                relicService,
                potionService,
                new BattleEventFlowService(randomProvider, eventService),
                new BattleRestShopFlowService(rules, randomProvider, shopService, potionService, relicService),
                runSaveService,
                new BattleCheckpointService());
        }

        private static BattleSceneFlowService CreateServiceWithShop(
            RuntimeRunDefinition runDefinition,
            IBattleShopService shopService,
            params int[] values)
        {
            BattleSceneRules rules = new BattleSceneRules(new BattleDeckService(), new BattleEnemyActionSelector(), new BattleEncounterSelector(), new BattleRewardRollService());
            SequenceRandomProvider randomProvider = new SequenceRandomProvider(values);
            BattleRelicService relicService = new BattleRelicService();
            BattlePotionService potionService = new BattlePotionService();
            BattleEventService eventService = new BattleEventService();
            return new BattleSceneFlowService(
                rules,
                randomProvider,
                new FakeBattleMasterDataFacade(runDefinition),
                new BattleRewardFlowService(new BattleRewardService(), rules, randomProvider, potionService, relicService),
                new BattleSnapshotFactory(new BattleDisplayTextService(), shopService),
                shopService,
                new BattleCombatEventService(relicService),
                relicService,
                potionService,
                new BattleEventFlowService(randomProvider, eventService),
                new BattleRestShopFlowService(rules, randomProvider, shopService, potionService, relicService),
                null,
                new BattleCheckpointService());
        }

        private static BattleSceneFlowService CreateServiceWithDisplayText(
            RuntimeRunDefinition runDefinition,
            IBattleDisplayTextService displayTextService,
            params int[] values)
        {
            BattleSceneRules rules = new BattleSceneRules(new BattleDeckService(), new BattleEnemyActionSelector(), new BattleEncounterSelector(), new BattleRewardRollService());
            SequenceRandomProvider randomProvider = new SequenceRandomProvider(values);
            BattleRelicService relicService = new BattleRelicService();
            BattlePotionService potionService = new BattlePotionService();
            BattleEventService eventService = new BattleEventService();
            FakeBattleShopService shopService = new FakeBattleShopService();
            return new BattleSceneFlowService(
                rules,
                randomProvider,
                new FakeBattleMasterDataFacade(runDefinition),
                new BattleRewardFlowService(new BattleRewardService(), rules, randomProvider, potionService, relicService),
                new BattleSnapshotFactory(displayTextService, shopService),
                shopService,
                new BattleCombatEventService(relicService),
                relicService,
                potionService,
                new BattleEventFlowService(randomProvider, eventService),
                new BattleRestShopFlowService(rules, randomProvider, shopService, potionService, relicService),
                null,
                new BattleCheckpointService());
        }

        private static RuntimeRunDefinition CreateRunDefinition(
            int playerMaxHp = 50,
            int startingGold = 120,
            int relicDropChance = 0,
            IReadOnlyList<RuntimeMapNode> nodes = null,
            IReadOnlyList<RuntimeCard> starterDeck = null,
            IReadOnlyList<RuntimeCard> additionalCards = null,
            IReadOnlyList<RuntimeRewardEntry> rewardCards = null,
            IReadOnlyList<RuntimeEncounterEntry> battleEncounters = null,
            IReadOnlyList<RuntimeEncounterEntry> eliteEncounters = null,
            IReadOnlyList<RuntimeEncounterEntry> bossEncounters = null,
            IReadOnlyList<RuntimeEvent> events = null,
            IReadOnlyDictionary<int, RuntimeRelic> relicCatalog = null,
            IReadOnlyDictionary<int, RuntimePotion> potionCatalog = null,
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

            IReadOnlyList<RuntimeCard> resolvedStarterDeck = starterDeck ?? new[] { CreateCard(1001, "Strike", 1, 6) };
            IReadOnlyList<RuntimeRewardEntry> resolvedRewardCards = rewardCards ?? new[] { CreateRewardEntry(CreateCard(1002, "Reward", 1, 5), 10, 1, 99) };
            Dictionary<int, RuntimeCard> cardCatalog = new Dictionary<int, RuntimeCard>();
            for (int i = 0; i < resolvedStarterDeck.Count; i++)
            {
                RuntimeCard card = resolvedStarterDeck[i];
                if (card != null)
                {
                    cardCatalog[card.Id] = card;
                }
            }

            for (int i = 0; i < resolvedRewardCards.Count; i++)
            {
                RuntimeCard card = resolvedRewardCards[i]?.Card;
                if (card != null)
                {
                    cardCatalog[card.Id] = card;
                }
            }

            if (additionalCards != null)
            {
                for (int i = 0; i < additionalCards.Count; i++)
                {
                    RuntimeCard card = additionalCards[i];
                    if (card != null)
                    {
                        cardCatalog[card.Id] = card;
                    }
                }
            }

            RuntimeRunDefinitionBuilder builder = BattleTestData.RunDefinition();
            builder.RunProfileId = 5501;
            builder.Key = "run_test";
            builder.PlayerMaxHp = playerMaxHp;
            builder.StartingGold = startingGold;
            builder.RelicDropChance = relicDropChance;
            builder.StarterDeck = resolvedStarterDeck;
            builder.CardCatalog = cardCatalog;
            builder.RewardPool = resolvedRewardCards;
            builder.Nodes = nodes ?? new[]
            {
                CreateNode(5301, 1, InGameNodeType.Battle, "B1", new[] { 1 }),
                CreateNode(5302, 2, InGameNodeType.Boss, "Boss", new int[0])
            };
            builder.EncountersByNodeType = encounters;
            builder.PossibleEvents = events ?? Array.Empty<RuntimeEvent>();
            builder.RelicCatalog = relicCatalog ?? new Dictionary<int, RuntimeRelic>();
            builder.PotionCatalog = potionCatalog ?? new Dictionary<int, RuntimePotion>();
            builder.ShopLineup = shopLineup;
            builder.ItemPriceRules = itemPriceRules ?? Array.Empty<RuntimeItemPriceRule>();
            return builder.Build();
        }

        private static RuntimeCard CreateCard(int id, string displayName, int cost, int damage, IReadOnlyList<RuntimeCardEffect> effects = null, int upgradeCardId = 0, bool isUpgraded = false)
        {
            RuntimeCardBuilder builder = BattleTestData.Card(id);
            builder.DisplayName = displayName;
            builder.Cost = cost;
            builder.Effects = effects ?? new[]
            {
                new RuntimeCardEffect(1, EffectType.DealDamage, damage, 1, StatusType.None, 0, TargetSide.Enemy)
            };
            builder.UpgradeCardId = upgradeCardId;
            builder.IsUpgraded = isUpgraded;
            return builder.Build();
        }

        private static RuntimeEnemy CreateEnemy(int id, string displayName, int hpMin, int hpMax, int goldReward, params RuntimeEnemyAction[] actions)
        {
            RuntimeEnemyBuilder builder = BattleTestData.Enemy(id);
            builder.DisplayName = displayName;
            builder.HpMin = hpMin;
            builder.HpMax = hpMax;
            builder.GoldReward = goldReward;
            builder.Actions = actions;
            return builder.Build();
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
            RuntimeEnemyActionBuilder builder = BattleTestData.EnemyAction(order);
            builder.Damage = damage;
            builder.RepeatRule = repeatRule;
            builder.IntentType = intentType;
            builder.HitCount = hitCount;
            builder.Block = block;
            builder.StatusType = statusType;
            builder.StatusValue = statusValue;
            builder.BuffType = buffType;
            builder.BuffValue = buffValue;
            return builder.Build();
        }

        private static RuntimeMapNode CreateNode(int id, int floor, InGameNodeType nodeType, string displayName, IReadOnlyList<int> nextNodeIndices)
        {
            RuntimeMapNodeBuilder builder = BattleTestData.MapNode(id);
            builder.Floor = floor;
            builder.NodeType = nodeType;
            builder.DisplayName = displayName;
            builder.NextNodeIndices = nextNodeIndices;
            return builder.Build();
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
            RuntimeRewardEntryBuilder builder = BattleTestData.RewardEntry();
            builder.RewardType = RewardType.Card;
            builder.Card = card;
            builder.Weight = weight;
            builder.MinFloor = minFloor;
            builder.MaxFloor = maxFloor;
            return builder.Build();
        }

        private static RuntimeRelic CreateRelic(int id, string displayName, IReadOnlyList<RuntimeRelicEffect> effects)
        {
            RuntimeRelicBuilder builder = BattleTestData.Relic(id);
            builder.DisplayName = displayName;
            builder.Description = $"{displayName} description";
            builder.Effects = effects;
            return builder.Build();
        }

        private static RuntimePotion CreatePotion(int id, string displayName, PotionUseContext useContext, PotionTargetMode targetMode, IReadOnlyList<RuntimePotionEffect> effects)
        {
            RuntimePotionBuilder builder = BattleTestData.Potion(id);
            builder.DisplayName = displayName;
            builder.Description = $"{displayName} description";
            builder.UseContext = useContext;
            builder.TargetMode = targetMode;
            builder.Effects = effects;
            return builder.Build();
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
                return _runDefinition?.CardCatalog ?? new Dictionary<int, RuntimeCard>();
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
            public int GetCardUpgradePrice(RuntimeRunDefinition runDefinition, RuntimeCard card) => 25;
        }
    }
}
