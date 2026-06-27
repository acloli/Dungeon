using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Tests.EditMode.Support;
using Game.MasterData.Generated;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleSceneStateのEditorモードテストクラス
    /// </summary>
    public sealed class BattleSceneStateTests
    {
        [Test]
        public void SyncSelectedEnemyDisplay_CopiesEnemyMirrorFieldsTogether()
        {
            BattleSceneState state = new BattleSceneState();
            BattleEnemyState enemyState = CreateEnemyState();
            enemyState.Block = 4;
            enemyState.TurnCount = 2;
            enemyState.CycleIndex = 3;
            enemyState.Statuses[StatusType.Weak] = 1;
            enemyState.Buffs[BuffType.Ritual] = 2;

            state.SyncSelectedEnemyDisplay(enemyState, 1);

            Assert.That(state.SelectedEnemyIndex, Is.EqualTo(1));
            Assert.That(state.CurrentEnemy, Is.SameAs(enemyState.Enemy));
            Assert.That(state.EnemyHp, Is.EqualTo(12));
            Assert.That(state.EnemyBlock, Is.EqualTo(4));
            Assert.That(state.EnemyTurnCount, Is.EqualTo(2));
            Assert.That(state.EnemyCycleIndex, Is.EqualTo(3));
            Assert.That(state.EnemyStatuses[StatusType.Weak], Is.EqualTo(1));
            Assert.That(state.EnemyBuffs[BuffType.Ritual], Is.EqualTo(2));
        }

        [Test]
        public void ClearSelectedEnemyDisplay_ResetsMirrorFieldsTogether()
        {
            BattleSceneState state = new BattleSceneState();
            BattleEnemyState enemyState = CreateEnemyState();
            enemyState.Statuses[StatusType.Weak] = 1;
            enemyState.Buffs[BuffType.Ritual] = 2;
            state.SyncSelectedEnemyDisplay(enemyState, 0);

            state.ClearSelectedEnemyDisplay();

            Assert.That(state.CurrentEnemy, Is.Null);
            Assert.That(state.EnemyHp, Is.EqualTo(0));
            Assert.That(state.EnemyBlock, Is.EqualTo(0));
            Assert.That(state.EnemyTurnCount, Is.EqualTo(0));
            Assert.That(state.EnemyCycleIndex, Is.EqualTo(0));
            Assert.That(state.EnemyStatuses, Is.Empty);
            Assert.That(state.EnemyBuffs, Is.Empty);
            Assert.That(state.SelectedEnemyIndex, Is.EqualTo(0));
        }

        private static BattleEnemyState CreateEnemyState()
        {
            RuntimeEnemyBuilder builder = BattleTestData.Enemy(3001);
            RuntimeEnemy enemy = builder.Build();
            return new BattleEnemyState(enemy, 0, 12);
        }
    }
}
