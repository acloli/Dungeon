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
