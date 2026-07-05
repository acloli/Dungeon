using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Game.MasterData.Generated;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleDeckServiceのEditorモードテストクラス
    /// </summary>
    public sealed class BattleDeckServiceTests
    {
        private const int UnselectedCardIndex = -1;
        private const int MaxHandSize = 10;

        [Test]
        public void PrepareBattleDeck_MovesDeckIntoDrawPile()
        {
            BattleSceneState state = new BattleSceneState();
            state.Deck.Add(CreateCard(1001, "Strike"));
            state.Deck.Add(CreateCard(1002, "Guard"));
            BattleDeckService service = new BattleDeckService();

            service.PrepareBattleDeck(state, new FixedRandomProvider());

            Assert.That(state.DrawPile.Count, Is.EqualTo(2));
            Assert.That(state.DiscardPile, Is.Empty);
            Assert.That(state.Hand, Is.Empty);
        }

        [Test]
        public void DrawCards_WhenDrawPileEmpty_RefillsFromDiscard()
        {
            BattleSceneState state = new BattleSceneState();
            state.DiscardPile.Add(CreateCard(1001, "Strike"));
            state.DiscardPile.Add(CreateCard(1002, "Guard"));
            BattleDeckService service = new BattleDeckService();

            int drawn = service.DrawCards(state, new FixedRandomProvider(), 2);

            Assert.That(drawn, Is.EqualTo(2));
            Assert.That(state.Hand.Count, Is.EqualTo(2));
            Assert.That(state.DrawPile, Is.Empty);
            Assert.That(state.DiscardPile, Is.Empty);
        }

        [Test]
        public void DiscardHand_ClearsSelectionAndMovesCards()
        {
            BattleSceneState state = new BattleSceneState
            {
                SelectedCardIndex = 1
            };
            state.Hand.Add(CreateCard(1001, "Strike"));
            state.Hand.Add(CreateCard(1002, "Guard"));
            BattleDeckService service = new BattleDeckService();

            service.DiscardHand(state);

            Assert.That(state.Hand, Is.Empty);
            Assert.That(state.DiscardPile.Count, Is.EqualTo(2));
            Assert.That(state.SelectedCardIndex, Is.EqualTo(UnselectedCardIndex));
        }

        [Test]
        public void DrawCards_StopsAtMaxHandSize()
        {
            BattleSceneState state = new BattleSceneState();
            for (int i = 0; i < MaxHandSize + 2; i++)
            {
                state.DrawPile.Add(CreateCard(1000 + i, $"Card{i}"));
            }

            BattleDeckService service = new BattleDeckService();
            int drawn = service.DrawCards(state, new FixedRandomProvider(), MaxHandSize + 2);

            Assert.That(drawn, Is.EqualTo(MaxHandSize));
            Assert.That(state.Hand.Count, Is.EqualTo(MaxHandSize));
            Assert.That(state.DrawPile.Count, Is.EqualTo(2));
        }

        private static RuntimeCard CreateCard(int id, string name)
        {
            var builder = Support.BattleTestData.Card(id);
            builder.DisplayName = name;
            builder.Cost = 1;
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
