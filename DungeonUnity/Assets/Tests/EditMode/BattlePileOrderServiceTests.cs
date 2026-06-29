using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Domain;
using Dungeon.Tests.EditMode.Support;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattlePileOrderServiceのEditorモードテストクラス
    /// </summary>
    public sealed class BattlePileOrderServiceTests
    {
        [Test]
        public void Order_SortsByIdAscending()
        {
            BattlePileOrderService service = new BattlePileOrderService();
            BattleSceneState state = new BattleSceneState();
            List<RuntimeCard> cards = new List<RuntimeCard>
            {
                BattleTestData.Card(3003).Build(),
                BattleTestData.Card(1001).Build(),
                BattleTestData.Card(2002).Build()
            };

            IReadOnlyList<RuntimeCard> result = service.Order(BattlePileType.Draw, cards, state);

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].Id, Is.EqualTo(1001));
            Assert.That(result[1].Id, Is.EqualTo(2002));
            Assert.That(result[2].Id, Is.EqualTo(3003));
        }

        [Test]
        public void Order_SameId_StableBySourceIndex()
        {
            BattlePileOrderService service = new BattlePileOrderService();
            BattleSceneState state = new BattleSceneState();
            RuntimeCard cardA = BattleTestData.Card(1001).Build();
            RuntimeCard cardB = BattleTestData.Card(1001).Build();
            
            List<RuntimeCard> cards = new List<RuntimeCard> { cardA, cardB };

            IReadOnlyList<RuntimeCard> result = service.Order(BattlePileType.Exhaust, cards, state);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0], Is.SameAs(cardA));
            Assert.That(result[1], Is.SameAs(cardB));
        }

        [Test]
        public void Order_EmptyList_ReturnsEmpty()
        {
            BattlePileOrderService service = new BattlePileOrderService();
            BattleSceneState state = new BattleSceneState();

            IReadOnlyList<RuntimeCard> result = service.Order(BattlePileType.Discard, Array.Empty<RuntimeCard>(), state);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Order_NullList_ReturnsEmpty()
        {
            BattlePileOrderService service = new BattlePileOrderService();
            BattleSceneState state = new BattleSceneState();

            IReadOnlyList<RuntimeCard> result = service.Order(BattlePileType.Discard, null, state);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Order_AllPileTypes_ProduceSameSort()
        {
            BattlePileOrderService service = new BattlePileOrderService();
            BattleSceneState state = new BattleSceneState();
            List<RuntimeCard> cards = new List<RuntimeCard>
            {
                BattleTestData.Card(2002).Build(),
                BattleTestData.Card(1001).Build()
            };

            IReadOnlyList<RuntimeCard> drawResult = service.Order(BattlePileType.Draw, cards, state);
            IReadOnlyList<RuntimeCard> discardResult = service.Order(BattlePileType.Discard, cards, state);
            IReadOnlyList<RuntimeCard> exhaustResult = service.Order(BattlePileType.Exhaust, cards, state);

            Assert.That(drawResult[0].Id, Is.EqualTo(1001));
            Assert.That(discardResult[0].Id, Is.EqualTo(1001));
            Assert.That(exhaustResult[0].Id, Is.EqualTo(1001));
        }
    }
}