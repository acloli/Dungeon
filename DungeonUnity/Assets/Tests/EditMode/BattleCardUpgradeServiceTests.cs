using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Tests.EditMode.Support;
using Game.MasterData.Generated;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleCardUpgradeServiceのEditorモードテストクラス
    /// </summary>
    [TestFixture]
    public sealed class BattleCardUpgradeServiceTests
    {
        [Test]
        public void TryGetUpgradePreview_WhenUpgradeTargetExists_ReturnsCatalogCard()
        {
            RuntimeCard sourceCard = CreateCard(1001, 1101);
            RuntimeCard upgradedCard = CreateCard(1101, isUpgraded: true);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(upgradedCard);
            BattleCardUpgradeService service = new BattleCardUpgradeService();

            bool succeeded = service.TryGetUpgradePreview(runDefinition, sourceCard, out RuntimeCard previewCard);

            Assert.That(succeeded, Is.True);
            Assert.That(previewCard, Is.SameAs(upgradedCard));
        }

        [Test]
        public void TryGetUpgradePreview_WhenCardCannotUpgrade_ReturnsFalse()
        {
            RuntimeCard noUpgradeCard = CreateCard(1001);
            RuntimeCard upgradedSourceCard = CreateCard(1002, 1102, isUpgraded: true);
            RuntimeCard missingTargetCard = CreateCard(1003, 1103);
            RuntimeCard catalogCard = CreateCard(1102, isUpgraded: true);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(catalogCard);
            BattleCardUpgradeService service = new BattleCardUpgradeService();

            bool noUpgradeSucceeded = service.TryGetUpgradePreview(
                runDefinition,
                noUpgradeCard,
                out RuntimeCard noUpgradePreview);
            bool upgradedSourceSucceeded = service.TryGetUpgradePreview(
                runDefinition,
                upgradedSourceCard,
                out RuntimeCard upgradedSourcePreview);
            bool missingTargetSucceeded = service.TryGetUpgradePreview(
                runDefinition,
                missingTargetCard,
                out RuntimeCard missingTargetPreview);

            Assert.That(noUpgradeSucceeded, Is.False);
            Assert.That(noUpgradePreview, Is.Null);
            Assert.That(upgradedSourceSucceeded, Is.False);
            Assert.That(upgradedSourcePreview, Is.Null);
            Assert.That(missingTargetSucceeded, Is.False);
            Assert.That(missingTargetPreview, Is.Null);
        }

        [Test]
        public void TryReplaceDeckCard_ReplacesOnlySpecifiedDeckIndex()
        {
            RuntimeCard firstCard = CreateCard(1001);
            RuntimeCard selectedCard = CreateCard(1002);
            RuntimeCard lastCard = CreateCard(1003);
            RuntimeCard replacementCard = CreateCard(1102, isUpgraded: true);
            BattleSceneState state = CreateState(firstCard, selectedCard, lastCard);
            BattleCardUpgradeService service = new BattleCardUpgradeService();

            bool succeeded = service.TryReplaceDeckCard(state, 1, replacementCard);

            Assert.That(succeeded, Is.True);
            Assert.That(state.Deck[0], Is.SameAs(firstCard));
            Assert.That(state.Deck[1], Is.SameAs(replacementCard));
            Assert.That(state.Deck[2], Is.SameAs(lastCard));
        }

        [Test]
        public void TryUpgradeRandomCard_FiltersCommonCandidatesAndSelectsDuplicateByDeckIndex()
        {
            RuntimeCard firstDuplicate = CreateCard(1001, 1101);
            RuntimeCard secondDuplicate = CreateCard(1001, 1101);
            RuntimeCard alreadyUpgraded = CreateCard(1002, 1102, isUpgraded: true);
            RuntimeCard uncommonCard = CreateCard(1003, 1103, CardRarity.Uncommon);
            RuntimeCard missingCatalogTarget = CreateCard(1004, 1104);
            RuntimeCard noUpgradeTarget = CreateCard(1005);
            RuntimeCard duplicateUpgrade = CreateCard(1101, isUpgraded: true);
            RuntimeCard otherUpgrade = CreateCard(1103, isUpgraded: true);
            BattleSceneState state = CreateState(
                firstDuplicate,
                secondDuplicate,
                alreadyUpgraded,
                uncommonCard,
                missingCatalogTarget,
                noUpgradeTarget);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(duplicateUpgrade, otherUpgrade);
            RecordingRandomProvider randomProvider = new RecordingRandomProvider(1);
            BattleCardUpgradeService service = new BattleCardUpgradeService();

            bool succeeded = service.TryUpgradeRandomCard(
                state,
                runDefinition,
                CardRarity.Common,
                randomProvider);

            Assert.That(succeeded, Is.True);
            Assert.That(randomProvider.CallCount, Is.EqualTo(1));
            Assert.That(randomProvider.LastMinInclusive, Is.EqualTo(0));
            Assert.That(randomProvider.LastMaxExclusive, Is.EqualTo(2));
            Assert.That(state.Deck[0], Is.SameAs(firstDuplicate));
            Assert.That(state.Deck[1], Is.SameAs(duplicateUpgrade));
            Assert.That(state.Deck[2], Is.SameAs(alreadyUpgraded));
            Assert.That(state.Deck[3], Is.SameAs(uncommonCard));
            Assert.That(state.Deck[4], Is.SameAs(missingCatalogTarget));
            Assert.That(state.Deck[5], Is.SameAs(noUpgradeTarget));
        }

        [Test]
        public void TryUpgradeRandomCard_WhenNoCandidate_DoesNotChangeDeckOrConsumeRandom()
        {
            RuntimeCard noUpgradeTarget = CreateCard(1001);
            RuntimeCard alreadyUpgraded = CreateCard(1002, 1102, isUpgraded: true);
            RuntimeCard uncommonCard = CreateCard(1003, 1103, CardRarity.Uncommon);
            RuntimeCard missingCatalogTarget = CreateCard(1004, 1104);
            BattleSceneState state = CreateState(
                noUpgradeTarget,
                alreadyUpgraded,
                uncommonCard,
                missingCatalogTarget);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                CreateCard(1102, isUpgraded: true),
                CreateCard(1103, isUpgraded: true));
            RecordingRandomProvider randomProvider = new RecordingRandomProvider(0);
            randomProvider.Restore(1234, 7);
            BattleCardUpgradeService service = new BattleCardUpgradeService();

            bool succeeded = service.TryUpgradeRandomCard(
                state,
                runDefinition,
                CardRarity.Common,
                randomProvider);

            Assert.That(succeeded, Is.False);
            Assert.That(randomProvider.CallCount, Is.Zero);
            Assert.That(randomProvider.Counter, Is.EqualTo(7));
            Assert.That(state.Deck[0], Is.SameAs(noUpgradeTarget));
            Assert.That(state.Deck[1], Is.SameAs(alreadyUpgraded));
            Assert.That(state.Deck[2], Is.SameAs(uncommonCard));
            Assert.That(state.Deck[3], Is.SameAs(missingCatalogTarget));
        }

        [Test]
        public void TryUpgradeRandomCard_WithSameRandomStateAndDeckOrder_SelectsSameDeckIndex()
        {
            RuntimeCard firstSource = CreateCard(1001, 1101);
            RuntimeCard secondSource = CreateCard(1002, 1102);
            RuntimeCard firstUpgrade = CreateCard(1101, isUpgraded: true);
            RuntimeCard secondUpgrade = CreateCard(1102, isUpgraded: true);
            RuntimeRunDefinition runDefinition = CreateRunDefinition(firstUpgrade, secondUpgrade);
            BattleSceneState firstState = CreateState(firstSource, secondSource);
            BattleSceneState secondState = CreateState(firstSource, secondSource);
            RecordingRandomProvider firstRandom = new RecordingRandomProvider(1);
            RecordingRandomProvider secondRandom = new RecordingRandomProvider(1);
            firstRandom.Restore(2468, 3);
            secondRandom.Restore(2468, 3);
            BattleCardUpgradeService service = new BattleCardUpgradeService();

            bool firstSucceeded = service.TryUpgradeRandomCard(
                firstState,
                runDefinition,
                CardRarity.Common,
                firstRandom);
            bool secondSucceeded = service.TryUpgradeRandomCard(
                secondState,
                runDefinition,
                CardRarity.Common,
                secondRandom);

            Assert.That(firstSucceeded, Is.True);
            Assert.That(secondSucceeded, Is.True);
            Assert.That(firstRandom.CallCount, Is.EqualTo(1));
            Assert.That(secondRandom.CallCount, Is.EqualTo(1));
            Assert.That(firstRandom.Counter, Is.EqualTo(4));
            Assert.That(secondRandom.Counter, Is.EqualTo(4));
            Assert.That(firstState.Deck[0], Is.SameAs(secondState.Deck[0]));
            Assert.That(firstState.Deck[1], Is.SameAs(secondUpgrade));
            Assert.That(secondState.Deck[1], Is.SameAs(secondUpgrade));
        }

        private static RuntimeCard CreateCard(
            int id,
            int upgradeCardId = 0,
            CardRarity rarity = CardRarity.Common,
            bool isUpgraded = false)
        {
            RuntimeCardBuilder builder = BattleTestData.Card(id);
            builder.UpgradeCardId = upgradeCardId;
            builder.Rarity = rarity;
            builder.IsUpgraded = isUpgraded;
            return builder.Build();
        }

        private static RuntimeRunDefinition CreateRunDefinition(params RuntimeCard[] catalogCards)
        {
            Dictionary<int, RuntimeCard> cardCatalog = new Dictionary<int, RuntimeCard>();
            foreach (RuntimeCard card in catalogCards)
            {
                cardCatalog.Add(card.Id, card);
            }

            RuntimeRunDefinitionBuilder builder = BattleTestData.RunDefinition();
            builder.CardCatalog = cardCatalog;
            return builder.Build();
        }

        private static BattleSceneState CreateState(params RuntimeCard[] cards)
        {
            BattleSceneState state = new BattleSceneState();
            state.Deck.AddRange(cards);
            return state;
        }

        /// <summary>
        /// 呼び出し内容を記録して固定値を返す乱数提供クラス
        /// </summary>
        private sealed class RecordingRandomProvider : IBattleRandomProvider
        {
            private readonly int _returnedValue;

            public RecordingRandomProvider(int returnedValue)
            {
                _returnedValue = returnedValue;
            }

            public int Seed { get; private set; }

            public int Counter { get; private set; }

            public int CallCount { get; private set; }

            public int LastMinInclusive { get; private set; }

            public int LastMaxExclusive { get; private set; }

            public void Initialize(int seed)
            {
                Seed = seed;
                Counter = 0;
                CallCount = 0;
            }

            public void Restore(int seed, int counter)
            {
                Seed = seed;
                Counter = counter;
                CallCount = 0;
            }

            public int Range(int minInclusive, int maxExclusive)
            {
                LastMinInclusive = minInclusive;
                LastMaxExclusive = maxExclusive;
                CallCount++;
                Counter++;
                return _returnedValue;
            }
        }
    }
}
