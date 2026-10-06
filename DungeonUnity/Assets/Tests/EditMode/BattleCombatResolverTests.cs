using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Tests.EditMode.Support;
using Game.MasterData.Generated;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleCombatResolverのEditorモードテストクラス
    /// </summary>
    public sealed class BattleCombatResolverTests
    {
        [Test]
        public void CanPlayCard_UsesCurrentEnergyThreshold()
        {
            BattleSceneState state = new BattleSceneState
            {
                PlayerEnergy = 1
            };
            BattleCombatResolver service = CreateService();
            RuntimeCard card = CreateCard(1001, 2, Array.Empty<RuntimeCardEffect>());

            bool canPlay = service.CanPlayCard(state, card);

            Assert.That(canPlay, Is.False);
        }

        [Test]
        public void PlayCard_ResolvesDamageBlockAndDraw()
        {
            BattleSceneState state = new BattleSceneState
            {
                PlayerEnergy = 3,
                SelectedEnemyIndex = 0
            };
            RuntimeCard playedCard = CreateCard(
                1001,
                1,
                new[]
                {
                    new RuntimeCardEffect(1, EffectType.DealDamage, 6, 1, StatusType.None, 0, TargetSide.Enemy),
                    new RuntimeCardEffect(2, EffectType.GainBlock, 5, 1, StatusType.None, 0, TargetSide.Self),
                    new RuntimeCardEffect(3, EffectType.DrawCards, 1, 1, StatusType.None, 0, TargetSide.Self)
                });
            state.Hand.Add(playedCard);
            state.DrawPile.Add(CreateCard(1002, 1, Array.Empty<RuntimeCardEffect>()));
            state.Enemies.Add(CreateEnemyState(3001, hp: 10, CreateEnemyAction(1, 4)));
            BattleCombatResolver service = CreateService();

            BattleCardResolutionResult result = service.PlayCard(state, 0, new FixedRandomProvider(0));

            Assert.That(result.TotalDamage, Is.EqualTo(6));
            Assert.That(result.TotalBlock, Is.EqualTo(5));
            Assert.That(result.TotalDraw, Is.EqualTo(1));
            Assert.That(state.PlayerEnergy, Is.EqualTo(2));
            Assert.That(state.PlayerBlock, Is.EqualTo(5));
            Assert.That(state.Hand.Count, Is.EqualTo(1));
            Assert.That(state.DiscardPile, Has.Count.EqualTo(1));
            Assert.That(state.Enemies[0].Hp, Is.EqualTo(4));
            Assert.That(state.EnemyHp, Is.EqualTo(4));
        }

        [Test]
        public void PlayCard_WhenTargetFalls_SwitchesPrimaryEnemySelection()
        {
            BattleSceneState state = new BattleSceneState
            {
                PlayerEnergy = 3,
                SelectedEnemyIndex = 0
            };
            state.Hand.Add(CreateCard(
                1001,
                1,
                new[]
                {
                    new RuntimeCardEffect(1, EffectType.DealDamage, 6, 1, StatusType.None, 0, TargetSide.Enemy)
                }));
            state.Enemies.Add(CreateEnemyState(3001, hp: 6, CreateEnemyAction(1, 4)));
            state.Enemies.Add(CreateEnemyState(3002, hp: 12, CreateEnemyAction(1, 5), slotIndex: 1));
            BattleCombatResolver service = CreateService();

            service.PlayCard(state, 0, new FixedRandomProvider(0));

            Assert.That(state.SelectedEnemyIndex, Is.EqualTo(1));
            Assert.That(state.CurrentEnemy.DisplayName, Is.EqualTo("Enemy3002"));
            Assert.That(state.EnemyHp, Is.EqualTo(12));
        }

        [Test]
        public void ResolveEnemyTurn_AppliesDamageBlockStatusAndBuff()
        {
            BattleSceneState state = new BattleSceneState
            {
                PlayerHp = 20,
                PlayerBlock = 2,
                SelectedEnemyIndex = 0
            };
            BattleEnemyState enemyState = CreateEnemyState(
                3001,
                10,
                CreateEnemyAction(1, 5, block: 4, statusType: StatusType.Weak, statusValue: 1, buffType: BuffType.Strength, buffValue: 2));
            enemyState.TurnCount = 1;
            state.Enemies.Add(enemyState);
            BattleCombatResolver service = CreateService();
            service.PrepareEnemyActions(state, new FixedRandomProvider(0));

            BattleEnemyTurnResult result = service.ResolveEnemyTurn(state);

            Assert.That(result.DamageDealt, Is.EqualTo(3));
            Assert.That(state.PlayerHp, Is.EqualTo(17));
            Assert.That(state.PlayerBlock, Is.EqualTo(0));
            Assert.That(state.PlayerStatuses[StatusType.Weak], Is.EqualTo(1));
            Assert.That(enemyState.Block, Is.EqualTo(4));
            Assert.That(enemyState.Buffs[BuffType.Strength], Is.EqualTo(2));
            Assert.That(enemyState.TurnCount, Is.EqualTo(2));
            Assert.That(state.EnemyBlock, Is.EqualTo(4));
        }

        [Test]
        public void PrepareEnemyActions_IsIdempotentAndResolutionConsumesPlannedAction()
        {
            BattleSceneState state = new BattleSceneState
            {
                PlayerHp = 20
            };
            RuntimeEnemyAction firstAction = CreateEnemyAction(1, 5, repeatRule: RepeatRule.Random);
            RuntimeEnemyAction secondAction = CreateEnemyAction(2, 9, repeatRule: RepeatRule.Random);
            state.Enemies.Add(CreateEnemyState(3001, 20, firstAction, secondAction));
            FixedRandomProvider randomProvider = new FixedRandomProvider(1);
            BattleCombatResolver service = CreateService();

            service.PrepareEnemyActions(state, randomProvider);
            RuntimeEnemyAction plannedAction = state.Enemies[0].PlannedAction;
            service.PrepareEnemyActions(state, randomProvider);
            BattleEnemyTurnResult result = service.ResolveEnemyTurn(state);

            Assert.That(plannedAction.Order, Is.EqualTo(2));
            Assert.That(result.Action, Is.SameAs(plannedAction));
            Assert.That(result.DamageDealt, Is.EqualTo(9));
            Assert.That(randomProvider.Counter, Is.EqualTo(1));
            Assert.That(state.Enemies[0].TurnCount, Is.EqualTo(1));
            Assert.That(state.Enemies[0].PlannedAction, Is.Null);
        }

        [Test]
        public void PrepareEnemyActions_CycleAdvancesOnlyOnceForThePlannedAction()
        {
            BattleSceneState state = new BattleSceneState();
            RuntimeEnemyAction firstAction = CreateEnemyAction(1, 3, repeatRule: RepeatRule.Cycle);
            RuntimeEnemyAction secondAction = CreateEnemyAction(2, 4, repeatRule: RepeatRule.Cycle);
            BattleEnemyState enemyState = CreateEnemyState(3001, 10, firstAction, secondAction);
            enemyState.TurnCount = 1;
            enemyState.CycleIndex = 1;
            state.Enemies.Add(enemyState);
            BattleCombatResolver service = CreateService();
            FixedRandomProvider randomProvider = new FixedRandomProvider(0);

            service.PrepareEnemyActions(state, randomProvider);
            RuntimeEnemyAction plannedAction = enemyState.PlannedAction;
            service.PrepareEnemyActions(state, randomProvider);

            Assert.That(plannedAction.Order, Is.EqualTo(2));
            Assert.That(enemyState.PlannedAction, Is.SameAs(plannedAction));
            Assert.That(enemyState.CycleIndex, Is.EqualTo(2));
            Assert.That(randomProvider.Counter, Is.Zero);
        }

        [TestCase(TargetSide.Enemy, false)]
        [TestCase(TargetSide.AllEnemies, true)]
        public void PlayCard_KillingPlannedEnemyClearsOnlyDeadPlansWithoutDrawingAgain(
            TargetSide targetSide,
            bool killAllEnemies)
        {
            BattleSceneState state = new BattleSceneState
            {
                PlayerEnergy = 3,
                SelectedEnemyIndex = 0
            };
            RuntimeEnemyAction firstAction = CreateEnemyAction(1, 5, repeatRule: RepeatRule.Random);
            RuntimeEnemyAction secondAction = CreateEnemyAction(2, 8, repeatRule: RepeatRule.Random);
            BattleEnemyState firstEnemy = CreateEnemyState(3001, 1, firstAction, secondAction);
            BattleEnemyState secondEnemy = CreateEnemyState(3002, 1, 1, firstAction, secondAction);
            state.Enemies.Add(firstEnemy);
            state.Enemies.Add(secondEnemy);
            state.Hand.Add(CreateCard(1001, 0, new[]
            {
                new RuntimeCardEffect(1, EffectType.DealDamage, 1, 1, StatusType.None, 0, targetSide)
            }));
            BattleCombatResolver service = CreateService();
            FixedRandomProvider randomProvider = new FixedRandomProvider(1);
            service.PrepareEnemyActions(state, randomProvider);
            RuntimeEnemyAction remainingPlan = secondEnemy.PlannedAction;
            int counterAfterPreparation = randomProvider.Counter;

            service.PlayCard(state, 0, randomProvider);

            Assert.That(randomProvider.Counter, Is.EqualTo(counterAfterPreparation));
            Assert.That(firstEnemy.IsDefeated, Is.True);
            Assert.That(firstEnemy.PlannedAction, Is.Null);
            Assert.That(secondEnemy.IsDefeated, Is.EqualTo(killAllEnemies));
            Assert.That(secondEnemy.PlannedAction, Is.EqualTo(killAllEnemies ? null : remainingPlan));
        }

        [Test]
        public void ResolveEnemyTurn_UnpreparedEnemyActionIsRejectedBeforeStateChanges()
        {
            BattleSceneState state = new BattleSceneState();
            BattleEnemyState enemyState = CreateEnemyState(3001, 10, CreateEnemyAction(1, 5));
            enemyState.Block = 3;
            state.Enemies.Add(enemyState);
            state.PlayerStatuses[StatusType.Weak] = 2;
            BattleCombatResolver service = CreateService();

            Assert.Throws<InvalidOperationException>(() => service.ResolveEnemyTurn(state));

            Assert.That(state.PlayerStatuses[StatusType.Weak], Is.EqualTo(2));
            Assert.That(enemyState.Block, Is.EqualTo(3));
            Assert.That(enemyState.TurnCount, Is.Zero);
        }

        [TestCase(1, 3)]
        [TestCase(3, 13)]
        public void ResolveEnemyTurn_PredictedPerHitDamageMatchesHpLossAfterBlock(int hitCount, int expectedHpLoss)
        {
            BattleSceneState state = new BattleSceneState
            {
                CurrentPage = BattleScenePage.Battle,
                PlayerHp = 20,
                PlayerBlock = 2
            };
            BattleEnemyState enemyState = CreateEnemyState(
                3001,
                10,
                CreateEnemyAction(1, 5, hitCount: hitCount, repeatRule: RepeatRule.OpeningOnly));
            state.Enemies.Add(enemyState);
            BattleCombatResolver service = CreateService();
            service.PrepareEnemyActions(state, new FixedRandomProvider(0));

            int displayedDamagePerHit = BattleEnemyIntentPredictor.PredictDamagePerHit(state)[enemyState];
            BattleEnemyTurnResult result = service.ResolveEnemyTurn(state);

            Assert.That(displayedDamagePerHit, Is.EqualTo(5));
            Assert.That(result.DamageDealt, Is.EqualTo(expectedHpLoss));
            Assert.That(state.PlayerHp, Is.EqualTo(20 - expectedHpLoss));
            Assert.That(state.PlayerBlock, Is.Zero);
        }

        [TestCase(1, 3, 1)]
        [TestCase(2, 5, 3)]
        public void ResolveEnemyTurn_VulnerableDurationPredictionMatchesWeakAndBlockTiming(
            int vulnerableDuration,
            int expectedDamagePerHit,
            int expectedHpLoss)
        {
            BattleSceneState state = new BattleSceneState
            {
                CurrentPage = BattleScenePage.Battle,
                PlayerHp = 20,
                PlayerBlock = 2
            };
            state.PlayerStatuses[StatusType.Vulnerable] = vulnerableDuration;
            BattleEnemyState enemyState = CreateEnemyState(
                3001,
                10,
                CreateEnemyAction(1, 5, repeatRule: RepeatRule.OpeningOnly));
            enemyState.Statuses[StatusType.Weak] = 1;
            state.Enemies.Add(enemyState);
            BattleCombatResolver service = CreateService();
            service.PrepareEnemyActions(state, new FixedRandomProvider(0));

            int displayedDamagePerHit = BattleEnemyIntentPredictor.PredictDamagePerHit(state)[enemyState];
            BattleEnemyTurnResult result = service.ResolveEnemyTurn(state);

            Assert.That(displayedDamagePerHit, Is.EqualTo(expectedDamagePerHit));
            Assert.That(result.DamageDealt, Is.EqualTo(expectedHpLoss));
            Assert.That(state.PlayerHp, Is.EqualTo(20 - expectedHpLoss));
            if (vulnerableDuration == 1)
            {
                Assert.That(state.PlayerStatuses.ContainsKey(StatusType.Vulnerable), Is.False);
            }
            else
            {
                Assert.That(state.PlayerStatuses[StatusType.Vulnerable], Is.EqualTo(vulnerableDuration - 1));
            }

            Assert.That(enemyState.Statuses.ContainsKey(StatusType.Weak), Is.False);
        }

        [Test]
        public void ResolveEnemyTurn_ZeroDamageActionDoesNotApplyAttackBuffToHpButKeepsOtherEffects()
        {
            BattleSceneState state = new BattleSceneState
            {
                PlayerHp = 20,
                PlayerBlock = 4
            };
            RuntimeEnemyAction action = CreateEnemyAction(
                1,
                0,
                block: 3,
                statusType: StatusType.Weak,
                statusValue: 2,
                buffType: BuffType.Strength,
                buffValue: 2);
            BattleEnemyState enemyState = CreateEnemyState(3001, 10, action);
            enemyState.TurnCount = 1;
            enemyState.Buffs[BuffType.Ritual] = 5;
            state.Enemies.Add(enemyState);
            BattleCombatResolver service = CreateService();
            service.PrepareEnemyActions(state, new FixedRandomProvider(0));

            BattleEnemyTurnResult result = service.ResolveEnemyTurn(state);

            Assert.That(result.DamageDealt, Is.Zero);
            Assert.That(state.PlayerHp, Is.EqualTo(20));
            Assert.That(state.PlayerBlock, Is.Zero);
            Assert.That(state.PlayerStatuses[StatusType.Weak], Is.EqualTo(2));
            Assert.That(enemyState.Block, Is.EqualTo(3));
            Assert.That(enemyState.Buffs[BuffType.Ritual], Is.EqualTo(5));
            Assert.That(enemyState.Buffs[BuffType.Strength], Is.EqualTo(2));
        }

        [Test]
        public void PlayCard_NormalCard_GoesToDiscardPile()
        {
            BattleSceneState state = new BattleSceneState
            {
                PlayerEnergy = 3,
                SelectedEnemyIndex = 0
            };
            RuntimeCard card = CreateCard(1001, 1, Array.Empty<RuntimeCardEffect>(), exhaustsOnPlay: false);
            state.Hand.Add(card);
            state.Enemies.Add(CreateEnemyState(3001, hp: 10, CreateEnemyAction(1, 4)));
            BattleCombatResolver service = CreateService();

            service.PlayCard(state, 0, new FixedRandomProvider(0));

            Assert.That(state.DiscardPile, Has.Count.EqualTo(1));
            Assert.That(state.DiscardPile[0].Id, Is.EqualTo(1001));
            Assert.That(state.ExhaustPile, Is.Empty);
        }

        [Test]
        public void PlayCard_ExhaustsOnPlayCard_GoesToExhaustPile()
        {
            BattleSceneState state = new BattleSceneState
            {
                PlayerEnergy = 3,
                SelectedEnemyIndex = 0
            };
            RuntimeCard card = CreateCard(1001, 1, Array.Empty<RuntimeCardEffect>(), exhaustsOnPlay: true);
            state.Hand.Add(card);
            state.Enemies.Add(CreateEnemyState(3001, hp: 10, CreateEnemyAction(1, 4)));
            BattleCombatResolver service = CreateService();

            service.PlayCard(state, 0, new FixedRandomProvider(0));

            Assert.That(state.ExhaustPile, Has.Count.EqualTo(1));
            Assert.That(state.ExhaustPile[0].Id, Is.EqualTo(1001));
            Assert.That(state.DiscardPile, Is.Empty);
        }

        [Test]
        public void PlayCard_ExhaustsOnPlayCard_DoesNotDuplicateToDiscard()
        {
            BattleSceneState state = new BattleSceneState
            {
                PlayerEnergy = 3,
                SelectedEnemyIndex = 0
            };
            RuntimeCard card = CreateCard(1001, 1, Array.Empty<RuntimeCardEffect>(), exhaustsOnPlay: true);
            state.Hand.Add(card);
            state.Enemies.Add(CreateEnemyState(3001, hp: 10, CreateEnemyAction(1, 4)));
            BattleCombatResolver service = CreateService();

            service.PlayCard(state, 0, new FixedRandomProvider(0));

            Assert.That(state.ExhaustPile, Has.Count.EqualTo(1));
            Assert.That(state.DiscardPile, Is.Empty);
            Assert.That(state.Hand, Is.Empty);
        }

        [Test]
        public void PlayCard_DrawEffectCardWithEmptyDrawPile_CanReshufflePlayedCardFromDiscard()
        {
            BattleSceneState state = new BattleSceneState
            {
                PlayerEnergy = 3,
                SelectedEnemyIndex = 0
            };
            RuntimeCard playedCard = CreateCard(
                1001,
                1,
                new[]
                {
                    new RuntimeCardEffect(1, EffectType.DrawCards, 1, 1, StatusType.None, 0, TargetSide.Self)
                });
            state.Hand.Add(playedCard);
            state.DiscardPile.Add(CreateCard(1002, 1, Array.Empty<RuntimeCardEffect>()));
            state.Enemies.Add(CreateEnemyState(3001, hp: 10, CreateEnemyAction(1, 4)));
            BattleCombatResolver service = CreateService();

            BattleCardResolutionResult result = service.PlayCard(state, 0, new FixedRandomProvider(1));

            Assert.That(result.TotalDraw, Is.EqualTo(1));
            Assert.That(state.DrawPile, Has.Count.EqualTo(1));
            Assert.That(state.DrawPile[0].Id, Is.EqualTo(1002));
            Assert.That(state.DiscardPile, Is.Empty);
            Assert.That(state.ExhaustPile, Is.Empty);
            Assert.That(state.Hand, Has.Count.EqualTo(1));
            Assert.That(state.Hand[0].Id, Is.EqualTo(1001));
        }

        [Test]
        public void PlayCard_DrawEffectExhaustCardWithEmptyDrawPile_DoesNotReshufflePlayedCard()
        {
            BattleSceneState state = new BattleSceneState
            {
                PlayerEnergy = 3,
                SelectedEnemyIndex = 0
            };
            RuntimeCard playedCard = CreateCard(
                1001,
                1,
                new[]
                {
                    new RuntimeCardEffect(1, EffectType.DrawCards, 1, 1, StatusType.None, 0, TargetSide.Self)
                },
                exhaustsOnPlay: true);
            state.Hand.Add(playedCard);
            state.DiscardPile.Add(CreateCard(1002, 1, Array.Empty<RuntimeCardEffect>()));
            state.Enemies.Add(CreateEnemyState(3001, hp: 10, CreateEnemyAction(1, 4)));
            BattleCombatResolver service = CreateService();

            BattleCardResolutionResult result = service.PlayCard(state, 0, new FixedRandomProvider(1));

            Assert.That(result.TotalDraw, Is.EqualTo(1));
            Assert.That(state.Hand, Has.Count.EqualTo(1));
            Assert.That(state.Hand[0].Id, Is.EqualTo(1002));
            Assert.That(state.DrawPile, Is.Empty);
            Assert.That(state.DiscardPile, Is.Empty);
            Assert.That(state.ExhaustPile, Has.Count.EqualTo(1));
            Assert.That(state.ExhaustPile[0].Id, Is.EqualTo(1001));
        }

        private static BattleCombatResolver CreateService()
        {
            return new BattleCombatResolver(new BattleDeckService(), new BattleEnemyActionSelector());
        }

        private static RuntimeCard CreateCard(int id, int cost, IReadOnlyList<RuntimeCardEffect> effects, bool exhaustsOnPlay = false)
        {
            RuntimeCardBuilder builder = BattleTestData.Card(id);
            builder.Cost = cost;
            builder.Effects = effects;
            builder.ExhaustsOnPlay = exhaustsOnPlay;
            return builder.Build();
        }

        private static BattleEnemyState CreateEnemyState(int id, int hp, RuntimeEnemyAction action, int slotIndex = 0)
        {
            return CreateEnemyState(id, hp, slotIndex, action);
        }

        private static BattleEnemyState CreateEnemyState(int id, int hp, params RuntimeEnemyAction[] actions)
        {
            return CreateEnemyState(id, hp, 0, actions);
        }

        private static BattleEnemyState CreateEnemyState(int id, int hp, int slotIndex, params RuntimeEnemyAction[] actions)
        {
            RuntimeEnemyBuilder builder = BattleTestData.Enemy(id);
            builder.Actions = actions;
            RuntimeEnemy enemy = builder.Build();
            return new BattleEnemyState(enemy, slotIndex, hp);
        }

        private static RuntimeEnemyAction CreateEnemyAction(
            int order,
            int damage,
            int block = 0,
            StatusType statusType = StatusType.None,
            int statusValue = 0,
            BuffType buffType = BuffType.None,
            int buffValue = 0,
            RepeatRule repeatRule = RepeatRule.RepeatAfterOpening,
            int hitCount = 1)
        {
            RuntimeEnemyActionBuilder builder = BattleTestData.EnemyAction(order);
            builder.Damage = damage;
            builder.HitCount = hitCount;
            builder.Block = block;
            builder.StatusType = statusType;
            builder.StatusValue = statusValue;
            builder.BuffType = buffType;
            builder.BuffValue = buffValue;
            builder.RepeatRule = repeatRule;
            return builder.Build();
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
