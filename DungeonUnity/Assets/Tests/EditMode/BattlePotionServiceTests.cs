using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Tests.EditMode.Support;
using Game.MasterData.Generated;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattlePotionServiceのEditorモードテストクラス
    /// </summary>
    public sealed class BattlePotionServiceTests
    {
        [Test]
        public void UsePotion_TargetEnemyDamage_DamagesSelectedEnemyAndConsumesPotion()
        {
            BattleSceneState state = CreateBattleState();
            BattleEnemyState firstEnemy = CreateEnemyState(3001, 20, 0);
            BattleEnemyState secondEnemy = CreateEnemyState(3002, 18, 4);
            RuntimePotion potion = CreatePotion(
                5001,
                PotionTargetMode.AnyEnemy,
                new RuntimePotionEffect(1, EffectType.DealDamage, 10, 1, StatusType.None, 0, TargetSide.Enemy));
            state.Enemies.Add(firstEnemy);
            state.Enemies.Add(secondEnemy);
            state.OwnedPotions.Add(potion);

            bool used = new BattlePotionService().UsePotion(state, 0, new BattlePotionUseTarget(1), null, null);

            Assert.That(used, Is.True);
            Assert.That(state.OwnedPotions, Is.Empty);
            Assert.That(firstEnemy.Hp, Is.EqualTo(20));
            Assert.That(firstEnemy.Block, Is.EqualTo(0));
            Assert.That(secondEnemy.Hp, Is.EqualTo(12));
            Assert.That(secondEnemy.Block, Is.EqualTo(0));
        }

        [Test]
        public void UsePotion_TargetEnemyStatus_AppliesStatusToSelectedEnemy()
        {
            BattleSceneState state = CreateBattleState();
            BattleEnemyState firstEnemy = CreateEnemyState(3001, 20, 0);
            BattleEnemyState secondEnemy = CreateEnemyState(3002, 18, 0);
            RuntimePotion potion = CreatePotion(
                5002,
                PotionTargetMode.AnyEnemy,
                new RuntimePotionEffect(1, EffectType.ApplyStatus, 0, 1, StatusType.Weak, 2, TargetSide.Enemy));
            state.Enemies.Add(firstEnemy);
            state.Enemies.Add(secondEnemy);
            state.OwnedPotions.Add(potion);

            bool used = new BattlePotionService().UsePotion(state, 0, new BattlePotionUseTarget(1), null, null);

            Assert.That(used, Is.True);
            Assert.That(state.OwnedPotions, Is.Empty);
            Assert.That(firstEnemy.Statuses.ContainsKey(StatusType.Weak), Is.False);
            Assert.That(secondEnemy.Statuses[StatusType.Weak], Is.EqualTo(2));
            Assert.That(state.PlayerStatuses.ContainsKey(StatusType.Weak), Is.False);
        }

        [Test]
        public void UsePotion_AllEnemiesDamage_DamagesAllAliveEnemies()
        {
            BattleSceneState state = CreateBattleState();
            BattleEnemyState firstEnemy = CreateEnemyState(3001, 20, 2);
            BattleEnemyState defeatedEnemy = CreateEnemyState(3002, 0, 0, true);
            BattleEnemyState thirdEnemy = CreateEnemyState(3003, 8, 0);
            RuntimePotion potion = CreatePotion(
                5003,
                PotionTargetMode.AllEnemies,
                new RuntimePotionEffect(1, EffectType.DealDamage, 6, 1, StatusType.None, 0, TargetSide.AllEnemies));
            state.Enemies.Add(firstEnemy);
            state.Enemies.Add(defeatedEnemy);
            state.Enemies.Add(thirdEnemy);
            state.OwnedPotions.Add(potion);

            bool used = new BattlePotionService().UsePotion(state, 0, new BattlePotionUseTarget(-1), null, null);

            Assert.That(used, Is.True);
            Assert.That(state.OwnedPotions, Is.Empty);
            Assert.That(firstEnemy.Hp, Is.EqualTo(16));
            Assert.That(firstEnemy.Block, Is.EqualTo(0));
            Assert.That(defeatedEnemy.Hp, Is.EqualTo(0));
            Assert.That(defeatedEnemy.IsDefeated, Is.True);
            Assert.That(thirdEnemy.Hp, Is.EqualTo(2));
        }

        [Test]
        public void UsePotion_InvalidEnemyTarget_DoesNotConsumePotion()
        {
            BattleSceneState state = CreateBattleState();
            BattleEnemyState firstEnemy = CreateEnemyState(3001, 0, 0, true);
            RuntimePotion potion = CreatePotion(
                5004,
                PotionTargetMode.AnyEnemy,
                new RuntimePotionEffect(1, EffectType.DealDamage, 6, 1, StatusType.None, 0, TargetSide.Enemy));
            state.Enemies.Add(firstEnemy);
            state.OwnedPotions.Add(potion);

            bool used = new BattlePotionService().UsePotion(state, 0, new BattlePotionUseTarget(0), null, null);

            Assert.That(used, Is.False);
            Assert.That(state.OwnedPotions.Count, Is.EqualTo(1));
            Assert.That(state.OwnedPotions[0], Is.SameAs(potion));
            Assert.That(firstEnemy.Hp, Is.EqualTo(0));
            Assert.That(firstEnemy.IsDefeated, Is.True);
        }

        [Test]
        public void UsePotion_SelfPotion_KeepsExistingGainBlockAndDrawCardsBehavior()
        {
            BattleSceneState state = CreateBattleState();
            state.DrawPile.Add(CreateCard(1001));
            state.DrawPile.Add(CreateCard(1002));
            RuntimePotion potion = CreatePotion(
                5005,
                PotionTargetMode.Self,
                new RuntimePotionEffect(1, EffectType.GainBlock, 5, 1, StatusType.None, 0, TargetSide.Self),
                new RuntimePotionEffect(2, EffectType.DrawCards, 2, 1, StatusType.None, 0, TargetSide.Self));
            state.OwnedPotions.Add(potion);
            BattleSceneRules rules = new BattleSceneRules(new BattleDeckService(), null, null, null);

            bool used = new BattlePotionService().UsePotion(state, 0, new BattlePotionUseTarget(-1), rules, new FixedRandomProvider());

            Assert.That(used, Is.True);
            Assert.That(state.OwnedPotions, Is.Empty);
            Assert.That(state.PlayerBlock, Is.EqualTo(5));
            Assert.That(state.Hand.Count, Is.EqualTo(2));
            Assert.That(state.DrawPile, Is.Empty);
        }

        [Test]
        public void HasCapacity_UsesRuntimeMaxPotionCount()
        {
            BattleSceneState state = CreateBattleState();
            BattlePotionService service = new BattlePotionService();
            state.OwnedPotions.Add(CreatePotion(5006, PotionTargetMode.Self));
            state.OwnedPotions.Add(CreatePotion(5007, PotionTargetMode.Self));
            state.OwnedPotions.Add(CreatePotion(5008, PotionTargetMode.Self));

            state.MaxPotionCount = 3;
            Assert.That(service.HasCapacity(state), Is.False);

            state.MaxPotionCount = 4;
            Assert.That(service.HasCapacity(state), Is.True);

            state.MaxPotionCount = 2;
            Assert.That(service.HasCapacity(state), Is.False);
        }

        private static BattleSceneState CreateBattleState()
        {
            return new BattleSceneState
            {
                CurrentPage = BattleScenePage.Battle,
                PlayerMaxHp = 50,
                PlayerHp = 50
            };
        }

        private static BattleEnemyState CreateEnemyState(int id, int hp, int block, bool isDefeated = false)
        {
            RuntimeEnemy enemy = BattleTestData.Enemy(id).Build();
            return new BattleEnemyState(enemy, id - 3001, hp)
            {
                Block = block,
                IsDefeated = isDefeated
            };
        }

        private static RuntimePotion CreatePotion(int id, PotionTargetMode targetMode, params RuntimePotionEffect[] effects)
        {
            RuntimePotionBuilder builder = BattleTestData.Potion(id);
            builder.UseContext = PotionUseContext.BattleOnly;
            builder.TargetMode = targetMode;
            builder.Effects = effects;
            return builder.Build();
        }

        private static RuntimeCard CreateCard(int id)
        {
            RuntimeCardBuilder builder = BattleTestData.Card(id);
            builder.Effects = new List<RuntimeCardEffect>();
            return builder.Build();
        }

        /// <summary>
        /// 固定値を返す乱数提供クラス
        /// </summary>
        private sealed class FixedRandomProvider : IBattleRandomProvider
        {
            public int Seed { get; private set; }

            public int Counter { get; private set; }

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
                return minInclusive;
            }
        }
    }
}
