using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Services;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleRandomProviderのEditorモードテストクラス
    /// </summary>
    [TestFixture]
    public sealed class BattleRandomProviderTests
    {
        [Test]
        public void Range_SameSeed_ReturnsSameSequence()
        {
            BattleRandomProvider provider1 = new BattleRandomProvider();
            BattleRandomProvider provider2 = new BattleRandomProvider();
            provider1.Initialize(12345);
            provider2.Initialize(12345);

            List<int> sequence1 = CreateSequence(provider1, 6);
            List<int> sequence2 = CreateSequence(provider2, 6);

            Assert.That(sequence1, Is.EqualTo(sequence2));
        }

        [Test]
        public void Restore_ReplaysToSavedCounter()
        {
            BattleRandomProvider original = new BattleRandomProvider();
            original.Initialize(54321);
            List<int> sequence = CreateSequence(original, 6);

            BattleRandomProvider restored = new BattleRandomProvider();
            restored.Restore(54321, 4);
            int restoredValue = restored.Range(0, 100);

            Assert.That(restoredValue, Is.EqualTo(sequence[4]));
            Assert.That(restored.Counter, Is.EqualTo(5));
        }

        [Test]
        public void Range_IncrementsCounter()
        {
            BattleRandomProvider provider = new BattleRandomProvider();
            provider.Initialize(123);

            provider.Range(0, 10);
            provider.Range(0, 10);
            provider.Range(0, 10);

            Assert.That(provider.Counter, Is.EqualTo(3));
        }

        [Test]
        public void Range_DifferentSeed_ReturnsDifferentSequence()
        {
            BattleRandomProvider provider1 = new BattleRandomProvider();
            BattleRandomProvider provider2 = new BattleRandomProvider();
            provider1.Initialize(100);
            provider2.Initialize(200);

            List<int> sequence1 = CreateSequence(provider1, 6);
            List<int> sequence2 = CreateSequence(provider2, 6);

            Assert.That(sequence1, Is.Not.EqualTo(sequence2));
        }

        private static List<int> CreateSequence(IBattleRandomProvider provider, int count)
        {
            List<int> values = new List<int>();
            for (int i = 0; i < count; i++)
            {
                values.Add(provider.Range(0, 100));
            }

            return values;
        }
    }
}
