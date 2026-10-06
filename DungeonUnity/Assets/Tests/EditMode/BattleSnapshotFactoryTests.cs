using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Tests.EditMode.Support;
using Game.MasterData.Generated;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleSnapshotFactoryのEditorモードテストクラス
    /// </summary>
    public sealed class BattleSnapshotFactoryTests
    {
        [Test]
        public void HostChrome_CanUseSelectedPotion_TargetPotionRequiresBattle()
        {
            BattleSceneState state = CreateState(PotionTargetMode.AnyEnemy, BattleScenePage.Map);
            state.Enemies.Add(CreateEnemyState(10, false));
            BattleSnapshotFactory factory = CreateFactory();

            Assert.That(factory.CreateSnapshot(state).HostChrome.CanUseSelectedPotion, Is.False);

            state.CurrentPage = BattleScenePage.Battle;

            Assert.That(factory.CreateSnapshot(state).HostChrome.CanUseSelectedPotion, Is.True);
        }

        [Test]
        public void HostChrome_CanUseSelectedPotion_AllEnemiesRequiresAliveEnemy()
        {
            BattleSceneState state = CreateState(PotionTargetMode.AllEnemies, BattleScenePage.Battle);
            BattleSnapshotFactory factory = CreateFactory();

            Assert.That(factory.CreateSnapshot(state).HostChrome.CanUseSelectedPotion, Is.False);

            state.Enemies.Add(CreateEnemyState(0, true));

            Assert.That(factory.CreateSnapshot(state).HostChrome.CanUseSelectedPotion, Is.False);

            state.Enemies.Add(CreateEnemyState(12, false));

            Assert.That(factory.CreateSnapshot(state).HostChrome.CanUseSelectedPotion, Is.True);
        }

        [Test]
        public void CreateSnapshot_MapSnapshot_ContainsFloorProgressAndCenteredLayouts()
        {
            BattleSceneState state = new BattleSceneState
            {
                CurrentPage = BattleScenePage.Map,
                CurrentNodeIndex = 4
            };
            state.Nodes.Add(CreateMapNode(5301, 1));
            state.Nodes.Add(CreateMapNode(5302, 2));
            state.Nodes.Add(CreateMapNode(5303, 2));
            state.Nodes.Add(CreateMapNode(5304, 3));
            state.Nodes.Add(CreateMapNode(5305, 3));
            state.Nodes.Add(CreateMapNode(5306, 3));
            state.Nodes.Add(CreateMapNode(5307, 4));
            state.MapRouteNodeIndices.Add(0);
            state.MapRouteNodeIndices.Add(2);
            state.MapRouteNodeIndices.Add(4);
            BattleSnapshotFactory factory = CreateFactory();

            BattleMapSnapshot snapshot = factory.CreateSnapshot(state).Map;

            Assert.That(snapshot.CurrentFloor, Is.EqualTo(3));
            Assert.That(snapshot.TotalFloors, Is.EqualTo(4));
            Assert.That(snapshot.CurrentNodeIndex, Is.EqualTo(4));
            Assert.That(snapshot.MapRouteNodeIndices, Is.EqualTo(new[] { 0, 2, 4 }));
            Assert.That(snapshot.NodeLayouts.Count, Is.EqualTo(state.Nodes.Count));
            AssertMapNodeLayout(snapshot.NodeLayouts[0], 0, 0f, 0f, 1);
            AssertMapNodeLayout(snapshot.NodeLayouts[1], 1, -0.5f, 1f, 2);
            AssertMapNodeLayout(snapshot.NodeLayouts[2], 2, 0.5f, 1f, 2);
            AssertMapNodeLayout(snapshot.NodeLayouts[3], 3, -1f, 2f, 3);
            AssertMapNodeLayout(snapshot.NodeLayouts[4], 4, 0f, 2f, 3);
            AssertMapNodeLayout(snapshot.NodeLayouts[5], 5, 1f, 2f, 3);
            AssertMapNodeLayout(snapshot.NodeLayouts[6], 6, 0f, 3f, 4);
        }

        [Test]
        public void CreateSnapshot_UsesPlannedRandomActionWithoutAdvancingSelectionState()
        {
            BattleSceneState state = new BattleSceneState
            {
                CurrentPage = BattleScenePage.Battle,
                SelectedEnemyIndex = 0
            };
            RuntimeEnemyAction firstAction = CreateAction(1, 5, RepeatRule.Random);
            RuntimeEnemyAction secondAction = CreateAction(2, 8, RepeatRule.Random);
            BattleEnemyState enemyState = CreateEnemyState(firstAction, secondAction);
            enemyState.PlannedAction = secondAction;
            enemyState.TurnCount = 1;
            state.Enemies.Add(enemyState);
            BattleSnapshotFactory factory = CreateFactory();

            BattleCombatSnapshot firstSnapshot = factory.CreateSnapshot(state).Combat;
            BattleCombatSnapshot secondSnapshot = factory.CreateSnapshot(state).Combat;

            Assert.That(firstSnapshot.EnemyIntent.Damage, Is.EqualTo(8));
            Assert.That(firstSnapshot.Enemies[0].Intent.Damage, Is.EqualTo(8));
            Assert.That(secondSnapshot.EnemyIntent.Damage, Is.EqualTo(8));
            Assert.That(enemyState.PlannedAction, Is.SameAs(secondAction));
            Assert.That(enemyState.TurnCount, Is.EqualTo(1));
            Assert.That(enemyState.CycleIndex, Is.Zero);
        }

        [Test]
        public void CreateSnapshot_PredictsSharedDamageAndLaterEnemyStatusWithoutMutatingState()
        {
            BattleSceneState state = new BattleSceneState
            {
                CurrentPage = BattleScenePage.Battle,
                SelectedEnemyIndex = 0,
                PlayerHp = 20,
                PlayerBlock = 2
            };
            BattleEnemyState laterEnemy = CreateEnemyState(1, CreateAction(1, 5, RepeatRule.RepeatAfterOpening));
            laterEnemy.PlannedAction = laterEnemy.Enemy.Actions[0];
            BattleEnemyState earlierEnemy = CreateEnemyState(0, CreateAction(
                2,
                0,
                RepeatRule.RepeatAfterOpening,
                statusType: StatusType.Vulnerable,
                statusValue: 2));
            earlierEnemy.Hp = 1;
            earlierEnemy.PlannedAction = earlierEnemy.Enemy.Actions[0];
            state.Enemies.Add(laterEnemy);
            state.Enemies.Add(earlierEnemy);
            laterEnemy.Statuses[StatusType.Weak] = 1;
            laterEnemy.Buffs[BuffType.Strength] = 3;
            BattleSnapshotFactory factory = CreateFactory();

            RuntimeEnemyAction laterEnemyPlan = laterEnemy.PlannedAction;
            BattleCombatSnapshot snapshotBeforeEarlierEnemyDies = factory.CreateSnapshot(state).Combat;

            Assert.That(snapshotBeforeEarlierEnemyDies.Enemies[0].Intent.Damage, Is.EqualTo(9));
            Assert.That(snapshotBeforeEarlierEnemyDies.Enemies[1].Intent.Damage, Is.Zero);
            Assert.That(snapshotBeforeEarlierEnemyDies.EnemyIntent.Damage, Is.EqualTo(9));
            Assert.That(state.PlayerStatuses, Is.Empty);
            Assert.That(laterEnemy.Statuses[StatusType.Weak], Is.EqualTo(1));
            Assert.That(laterEnemy.Buffs[BuffType.Strength], Is.EqualTo(3));
            Assert.That(state.PlayerHp, Is.EqualTo(20));
            Assert.That(state.PlayerBlock, Is.EqualTo(2));

            RuntimeCardBuilder finisherBuilder = BattleTestData.Card(1001);
            finisherBuilder.Cost = 0;
            finisherBuilder.Effects = new[]
            {
                new RuntimeCardEffect(1, EffectType.DealDamage, 1, 1, StatusType.None, 0, TargetSide.Enemy)
            };
            state.Hand.Add(finisherBuilder.Build());
            state.SelectedEnemyIndex = 1;
            BattleCombatResolver resolver = new BattleCombatResolver(
                new BattleDeckService(),
                new BattleEnemyActionSelector());
            resolver.PlayCard(state, 0, new BattleRandomProvider());

            BattleCombatSnapshot snapshotAfterEarlierEnemyDies = factory.CreateSnapshot(state).Combat;

            Assert.That(earlierEnemy.IsDefeated, Is.True);
            Assert.That(earlierEnemy.PlannedAction, Is.Null);
            Assert.That(laterEnemy.PlannedAction, Is.SameAs(laterEnemyPlan));
            Assert.That(snapshotAfterEarlierEnemyDies.Enemies[0].Intent.Damage, Is.EqualTo(6));
            Assert.That(snapshotAfterEarlierEnemyDies.EnemyIntent.Damage, Is.EqualTo(6));
            Assert.That(snapshotAfterEarlierEnemyDies.Enemies[1].Intent, Is.Null);
            Assert.That(state.PlayerStatuses, Is.Empty);
        }

        [Test]
        public void CreateSnapshot_RefreshesDamageAfterPlayerStatusChangesAndDoesNotPreapplyActionBuff()
        {
            BattleSceneState state = new BattleSceneState
            {
                CurrentPage = BattleScenePage.Battle,
                SelectedEnemyIndex = 0
            };
            RuntimeEnemyAction action = CreateAction(
                1,
                6,
                RepeatRule.RepeatAfterOpening,
                buffType: BuffType.Strength,
                buffValue: 3);
            BattleEnemyState enemyState = CreateEnemyState(action);
            enemyState.PlannedAction = action;
            enemyState.Statuses[StatusType.Weak] = 1;
            enemyState.Buffs[BuffType.Strength] = 3;
            state.Enemies.Add(enemyState);
            BattleSnapshotFactory factory = CreateFactory();

            int initialDamage = factory.CreateSnapshot(state).Combat.EnemyIntent.Damage;
            state.PlayerStatuses[StatusType.Vulnerable] = 2;
            int updatedDamage = factory.CreateSnapshot(state).Combat.EnemyIntent.Damage;

            Assert.That(initialDamage, Is.EqualTo(7));
            Assert.That(updatedDamage, Is.EqualTo(11));
            Assert.That(enemyState.Buffs[BuffType.Strength], Is.EqualTo(3));
        }

        private static BattleSceneState CreateState(PotionTargetMode targetMode, BattleScenePage page)
        {
            BattleSceneState state = new BattleSceneState
            {
                CurrentPage = page,
                SelectedOwnedPotionIndex = 0
            };
            RuntimePotionBuilder builder = BattleTestData.Potion(1);
            builder.UseContext = PotionUseContext.Both;
            builder.TargetMode = targetMode;
            state.OwnedPotions.Add(builder.Build());
            return state;
        }

        private static BattleEnemyState CreateEnemyState(int hp, bool isDefeated)
        {
            RuntimeEnemy enemy = BattleTestData.Enemy(3001).Build();
            return new BattleEnemyState(enemy, 0, hp)
            {
                IsDefeated = isDefeated
            };
        }

        private static RuntimeMapNode CreateMapNode(int id, int floor)
        {
            RuntimeMapNodeBuilder builder = BattleTestData.MapNode(id);
            builder.Floor = floor;
            return builder.Build();
        }

        private static BattleEnemyState CreateEnemyState(params RuntimeEnemyAction[] actions)
        {
            return CreateEnemyState(0, actions);
        }

        private static BattleEnemyState CreateEnemyState(int slotIndex, params RuntimeEnemyAction[] actions)
        {
            RuntimeEnemyBuilder builder = BattleTestData.Enemy(3001);
            builder.Actions = actions;
            return new BattleEnemyState(builder.Build(), slotIndex, 10);
        }

        private static RuntimeEnemyAction CreateAction(
            int order,
            int damage,
            RepeatRule repeatRule,
            StatusType statusType = StatusType.None,
            int statusValue = 0,
            BuffType buffType = BuffType.None,
            int buffValue = 0)
        {
            RuntimeEnemyActionBuilder builder = BattleTestData.EnemyAction(order);
            builder.Damage = damage;
            builder.HitCount = 1;
            builder.StatusType = statusType;
            builder.StatusValue = statusValue;
            builder.BuffType = buffType;
            builder.BuffValue = buffValue;
            builder.RepeatRule = repeatRule;
            return builder.Build();
        }

        private static void AssertMapNodeLayout(MapNodeLayout layout, int nodeIndex, float x, float y, int floor)
        {
            Assert.That(layout.NodeIndex, Is.EqualTo(nodeIndex));
            Assert.That(layout.X, Is.EqualTo(x));
            Assert.That(layout.Y, Is.EqualTo(y));
            Assert.That(layout.Floor, Is.EqualTo(floor));
        }

        private static BattleSnapshotFactory CreateFactory()
        {
            return new BattleSnapshotFactory(
                new BattleDisplayTextService(),
                new FakeBattleShopService(),
                new BattleEnemyActionSelector(),
                new BattlePileOrderService());
        }

        private sealed class FakeBattleShopService : IBattleShopService
        {
            public void InitializeShop(BattleSceneState state, RuntimeRunDefinition runDef, IBattleRandomProvider random) {}

            public bool PurchaseShopItem(BattleSceneState state, int slotIndex) => false;

            public int GetCardRemovalPrice(BattleSceneState state) => 0;

            public bool PurchaseCardRemoval(BattleSceneState state, RuntimeCard card) => false;

            public int GetCardUpgradePrice(RuntimeRunDefinition runDefinition, RuntimeCard card) => 0;
        }
    }
}
