using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Game.MasterData.Generated;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleEnemyActionSelectorのEditorモードテストクラス
    /// </summary>
    public sealed class BattleEnemyActionSelectorTests
    {
        [Test]
        public void SelectEnemyAction_FirstTurn_PrioritizesOpeningAction()
        {
            BattleEnemyState enemyState = CreateEnemyState(
                CreateAction(1, RepeatRule.Random),
                CreateAction(2, RepeatRule.OpeningOnly));
            BattleEnemyActionSelector service = new BattleEnemyActionSelector();

            RuntimeEnemyAction selected = service.SelectEnemyAction(enemyState, new FixedRandomProvider(0));

            Assert.That(selected.Order, Is.EqualTo(2));
        }

        [Test]
        public void SelectEnemyAction_AfterOpeningRandom_UsesRandomIndex()
        {
            BattleEnemyState enemyState = CreateEnemyState(
                CreateAction(1, RepeatRule.AfterOpeningRandom),
                CreateAction(2, RepeatRule.AfterOpeningRandom));
            enemyState.TurnCount = 1;
            BattleEnemyActionSelector service = new BattleEnemyActionSelector();

            RuntimeEnemyAction selected = service.SelectEnemyAction(enemyState, new FixedRandomProvider(1));

            Assert.That(selected.Order, Is.EqualTo(2));
        }

        [Test]
        public void SelectEnemyAction_CycleRule_AdvancesCycleIndex()
        {
            BattleEnemyState enemyState = CreateEnemyState(
                CreateAction(1, RepeatRule.Cycle),
                CreateAction(2, RepeatRule.Cycle));
            enemyState.TurnCount = 1;
            enemyState.CycleIndex = 1;
            BattleEnemyActionSelector service = new BattleEnemyActionSelector();

            RuntimeEnemyAction selected = service.SelectEnemyAction(enemyState, new FixedRandomProvider(0));

            Assert.That(selected.Order, Is.EqualTo(2));
            Assert.That(enemyState.CycleIndex, Is.EqualTo(2));
        }

        [Test]
        public void GetSelectedEnemy_ReplacesDefeatedSelectionWithFirstAliveEnemy()
        {
            BattleSceneState state = new BattleSceneState
            {
                SelectedEnemyIndex = 1
            };
            state.Enemies.Add(CreateEnemyState(CreateAction(1, RepeatRule.Random)));
            BattleEnemyState defeatedEnemy = CreateEnemyState(CreateAction(2, RepeatRule.Random));
            defeatedEnemy.IsDefeated = true;
            state.Enemies.Add(defeatedEnemy);
            BattleEnemyActionSelector service = new BattleEnemyActionSelector();

            BattleEnemyState selected = service.GetSelectedEnemy(state);

            Assert.That(selected, Is.SameAs(state.Enemies[0]));
            Assert.That(state.SelectedEnemyIndex, Is.EqualTo(0));
        }

        [Test]
        public void GetTargetEnemies_AllEnemies_ExcludesDefeatedEnemy()
        {
            BattleSceneState state = new BattleSceneState
            {
                SelectedEnemyIndex = 0
            };
            state.Enemies.Add(CreateEnemyState(CreateAction(1, RepeatRule.Random)));
            BattleEnemyState defeatedEnemy = CreateEnemyState(CreateAction(2, RepeatRule.Random));
            defeatedEnemy.IsDefeated = true;
            state.Enemies.Add(defeatedEnemy);
            BattleEnemyActionSelector service = new BattleEnemyActionSelector();

            List<BattleEnemyState> targets = service.GetTargetEnemies(state, TargetSide.AllEnemies).ToList();

            Assert.That(targets.Count, Is.EqualTo(1));
            Assert.That(targets[0], Is.SameAs(state.Enemies[0]));
        }

        private static BattleEnemyState CreateEnemyState(params RuntimeEnemyAction[] actions)
        {
            RuntimeEnemy enemy = new RuntimeEnemy(
                3001,
                "enemy_key",
                "Slime",
                string.Empty,
                EnemyTier.Normal,
                10,
                10,
                10,
                actions);
            return new BattleEnemyState(enemy, 0, 10);
        }

        private static RuntimeEnemyAction CreateAction(int actionId, RepeatRule repeatRule)
        {
            return new RuntimeEnemyAction(
                actionId,
                IntentType.Attack,
                5,
                1,
                0,
                StatusType.None,
                0,
                BuffType.None,
                0,
                repeatRule);
        }

        /// <summary>
        /// 固定値を返す乱数提供クラス
        /// </summary>
        private sealed class FixedRandomProvider : IBattleRandomProvider
        {
            private readonly int _value;

            public int Seed { get; private set; }

            public int Counter { get; private set; }

            public FixedRandomProvider(int value)
            {
                _value = value;
            }

            public void Initialize(int seed)
            {
                Seed = seed;
                Counter = 0;
            }

            public void Restore(int seed, int counter)
            {
                Seed = seed;
                Counter = counter;
            }

            public int Range(int minInclusive, int maxExclusive)
            {
                Counter++;
                return Math.Clamp(_value, minInclusive, maxExclusive - 1);
            }
        }
    }
}
