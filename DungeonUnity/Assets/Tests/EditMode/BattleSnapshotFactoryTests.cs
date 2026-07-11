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
