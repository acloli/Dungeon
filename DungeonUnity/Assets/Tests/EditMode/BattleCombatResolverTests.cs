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

            BattleEnemyTurnResult result = service.ResolveEnemyTurn(state, new FixedRandomProvider(0));

            Assert.That(result.DamageDealt, Is.EqualTo(3));
            Assert.That(state.PlayerHp, Is.EqualTo(17));
            Assert.That(state.PlayerBlock, Is.EqualTo(0));
            Assert.That(state.PlayerStatuses[StatusType.Weak], Is.EqualTo(1));
            Assert.That(enemyState.Block, Is.EqualTo(4));
            Assert.That(enemyState.Buffs[BuffType.Strength], Is.EqualTo(2));
            Assert.That(enemyState.TurnCount, Is.EqualTo(2));
            Assert.That(state.EnemyBlock, Is.EqualTo(4));
        }

        private static BattleCombatResolver CreateService()
        {
            return new BattleCombatResolver(new BattleDeckService(), new BattleEnemyActionSelector());
        }

        private static RuntimeCard CreateCard(int id, int cost, IReadOnlyList<RuntimeCardEffect> effects)
        {
            RuntimeCardBuilder builder = BattleTestData.Card(id);
            builder.Cost = cost;
            builder.Effects = effects;
            return builder.Build();
        }

        private static BattleEnemyState CreateEnemyState(int id, int hp, RuntimeEnemyAction action, int slotIndex = 0)
        {
            RuntimeEnemyBuilder builder = BattleTestData.Enemy(id);
            builder.Actions = new[] { action };
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
            int buffValue = 0)
        {
            RuntimeEnemyActionBuilder builder = BattleTestData.EnemyAction(order);
            builder.Damage = damage;
            builder.Block = block;
            builder.StatusType = statusType;
            builder.StatusValue = statusValue;
            builder.BuffType = buffType;
            builder.BuffValue = buffValue;
            builder.RepeatRule = RepeatRule.RepeatAfterOpening;
            return builder.Build();
        }

        /// <summary>
        /// 固定値を返す乱数提供クラス
        /// </summary>
        private sealed class FixedRandomProvider : IBattleRandomProvider
        {
            private readonly int _value;

            public FixedRandomProvider(int value)
            {
                _value = value;
            }

            public int Range(int minInclusive, int maxExclusive)
            {
                return Math.Clamp(_value, minInclusive, maxExclusive - 1);
            }
        }
    }
}
