using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Tests.EditMode.Support;
using Game.MasterData.Generated;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    [TestFixture]
    public sealed class BattleRestShopFlowServiceTests
    {
        private const int StartingGold = 100;
        private const int UpgradePrice = 25;

        [Test]
        public void GetCardSelectPrices_WithFreeToken_ReturnsZero()
        {
            RuntimeCard sourceCard = CreateCard(1001, 1101);
            RuntimeCard upgradedCard = CreateCard(1101, 0, true);
            FakeCardUpgradeService cardUpgradeService = CreateCardUpgradeService((sourceCard, upgradedCard));
            BattleRestShopFlowService service = CreateService(cardUpgradeService);
            BattleSceneState state = CreateUpgradeState(sourceCard);
            state.GrantRestShopFreeUpgrade();

            IReadOnlyDictionary<int, int> prices = service.GetCardSelectPrices(
                state,
                CreateRunDefinition(sourceCard, upgradedCard),
                state.RestShopFreeUpgradeCount > 0);

            Assert.That(prices[sourceCard.Id], Is.Zero);
            Assert.That(state.RestShopFreeUpgradeCount, Is.EqualTo(1));
            Assert.That(state.Gold, Is.EqualTo(StartingGold));
        }

        [Test]
        public void ConfirmCardSelect_WithFreeToken_ConsumesTokenAndKeepsGold()
        {
            RuntimeCard sourceCard = CreateCard(1001, 1101);
            RuntimeCard upgradedCard = CreateCard(1101, 0, true);
            FakeCardUpgradeService cardUpgradeService = CreateCardUpgradeService((sourceCard, upgradedCard));
            BattleRestShopFlowService service = CreateService(cardUpgradeService);
            BattleSceneState state = CreateUpgradeState(sourceCard);
            state.GrantRestShopFreeUpgrade();

            bool result = service.ConfirmCardSelect(
                state,
                CreateRunDefinition(sourceCard, upgradedCard),
                sourceCard,
                _ => { });

            Assert.That(result, Is.True);
            Assert.That(state.RestShopFreeUpgradeCount, Is.Zero);
            Assert.That(state.Gold, Is.EqualTo(StartingGold));
            Assert.That(state.Deck[0], Is.SameAs(upgradedCard));
            Assert.That(state.IsRestShopContinueEnabled, Is.True);
        }

        [Test]
        public void CancelCardSelect_WithFreeToken_DoesNotConsumeOrCharge()
        {
            RuntimeCard sourceCard = CreateCard(1001, 1101);
            RuntimeCard upgradedCard = CreateCard(1101, 0, true);
            BattleRestShopFlowService service = CreateService(CreateCardUpgradeService((sourceCard, upgradedCard)));
            BattleSceneState state = CreateUpgradeState(sourceCard);
            state.GrantRestShopFreeUpgrade();
            bool reopened = false;

            service.CancelCardSelect(state, _ => { }, () => reopened = true);

            Assert.That(reopened, Is.True);
            Assert.That(state.RestShopFreeUpgradeCount, Is.EqualTo(1));
            Assert.That(state.Gold, Is.EqualTo(StartingGold));
            Assert.That(state.Deck[0], Is.SameAs(sourceCard));
        }

        [Test]
        public void ConfirmCardSelect_WithInvalidCard_DoesNotConsumeOrCharge()
        {
            RuntimeCard sourceCard = CreateCard(1001, 1101);
            RuntimeCard upgradedCard = CreateCard(1101, 0, true);
            RuntimeCard invalidCard = CreateCard(2001, 0);
            BattleRestShopFlowService service = CreateService(CreateCardUpgradeService((sourceCard, upgradedCard)));
            BattleSceneState state = CreateUpgradeState(sourceCard);
            state.GrantRestShopFreeUpgrade();

            bool result = service.ConfirmCardSelect(
                state,
                CreateRunDefinition(sourceCard, upgradedCard, invalidCard),
                invalidCard,
                _ => { });

            Assert.That(result, Is.False);
            Assert.That(state.RestShopFreeUpgradeCount, Is.EqualTo(1));
            Assert.That(state.Gold, Is.EqualTo(StartingGold));
            Assert.That(state.Deck[0], Is.SameAs(sourceCard));
        }

        [Test]
        public void ApplyUpgrade_WithNoCandidate_DoesNotConsumeOrCharge()
        {
            RuntimeCard sourceCard = CreateCard(1001, 1101);
            BattleRestShopFlowService service = CreateService(new FakeCardUpgradeService());
            BattleSceneState state = CreateUpgradeState(sourceCard);
            state.CardSelectMode = CardSelectMode.CardRemoval;
            state.GrantRestShopFreeUpgrade();
            bool pageChanged = false;

            service.ApplyUpgrade(state, CreateRunDefinition(sourceCard), _ => pageChanged = true);

            Assert.That(pageChanged, Is.False);
            Assert.That(state.RestShopFreeUpgradeCount, Is.EqualTo(1));
            Assert.That(state.Gold, Is.EqualTo(StartingGold));
            Assert.That(state.RestShopMessage, Is.Not.Empty);
        }

        [Test]
        public void ConfirmCardSelect_WhenReplacementFails_DoesNotConsumeOrCharge()
        {
            RuntimeCard sourceCard = CreateCard(1001, 1101);
            RuntimeCard upgradedCard = CreateCard(1101, 0, true);
            FakeCardUpgradeService cardUpgradeService = CreateCardUpgradeService((sourceCard, upgradedCard));
            cardUpgradeService.ShouldReplace = false;
            BattleRestShopFlowService service = CreateService(cardUpgradeService);
            BattleSceneState state = CreateUpgradeState(sourceCard);
            state.GrantRestShopFreeUpgrade();

            bool result = service.ConfirmCardSelect(
                state,
                CreateRunDefinition(sourceCard, upgradedCard),
                sourceCard,
                _ => { });

            Assert.That(result, Is.False);
            Assert.That(state.RestShopFreeUpgradeCount, Is.EqualTo(1));
            Assert.That(state.Gold, Is.EqualTo(StartingGold));
            Assert.That(state.Deck[0], Is.SameAs(sourceCard));
        }

        [Test]
        public void ConfirmCardSelect_AfterFreeUpgrade_ChargesSecondUpgrade()
        {
            RuntimeCard firstCard = CreateCard(1001, 1101);
            RuntimeCard firstUpgradedCard = CreateCard(1101, 0, true);
            RuntimeCard secondCard = CreateCard(1002, 1102);
            RuntimeCard secondUpgradedCard = CreateCard(1102, 0, true);
            FakeCardUpgradeService cardUpgradeService = CreateCardUpgradeService(
                (firstCard, firstUpgradedCard),
                (secondCard, secondUpgradedCard));
            BattleRestShopFlowService service = CreateService(cardUpgradeService);
            BattleSceneState state = CreateUpgradeState(firstCard, secondCard);
            state.GrantRestShopFreeUpgrade();
            RuntimeRunDefinition runDefinition = CreateRunDefinition(
                firstCard,
                firstUpgradedCard,
                secondCard,
                secondUpgradedCard);

            bool firstResult = service.ConfirmCardSelect(state, runDefinition, firstCard, _ => { });
            bool secondResult = service.ConfirmCardSelect(state, runDefinition, secondCard, _ => { });

            Assert.That(firstResult, Is.True);
            Assert.That(secondResult, Is.True);
            Assert.That(state.RestShopFreeUpgradeCount, Is.Zero);
            Assert.That(state.Gold, Is.EqualTo(StartingGold - UpgradePrice));
            Assert.That(state.Deck[0], Is.SameAs(firstUpgradedCard));
            Assert.That(state.Deck[1], Is.SameAs(secondUpgradedCard));
        }

        [Test]
        public void ContinueFromRestShop_ClearsVisitStateAndInvokesOpenMap()
        {
            RuntimeCard card = CreateCard(1001, 1101);
            BattleRestShopFlowService service = CreateService(new FakeCardUpgradeService());
            BattleSceneState state = CreateUpgradeState(card);
            state.GrantRestShopFreeUpgrade();
            state.ShopItems.Add(new BattleShopItemState(1, RewardType.Card, card, null, null, 0, 50));
            state.SelectedCardIndex = 0;
            state.CardSelectMessage = "selected";
            state.RestShopMessage = "rest";
            state.IsRestShopContinueEnabled = true;
            int unselectedCardIndex = new BattleSceneState().SelectedCardIndex;
            bool openedMap = false;

            service.ContinueFromRestShop(state, () => openedMap = true);

            Assert.That(openedMap, Is.True);
            Assert.That(state.RestShopFreeUpgradeCount, Is.Zero);
            Assert.That(state.ShopItems, Is.Empty);
            Assert.That(state.CardSelectMode, Is.EqualTo(CardSelectMode.CardRemoval));
            Assert.That(state.SelectedCardIndex, Is.EqualTo(unselectedCardIndex));
            Assert.That(state.CardSelectMessage, Is.Empty);
            Assert.That(state.RestShopMessage, Is.Empty);
            Assert.That(state.IsRestShopContinueEnabled, Is.False);
        }

        private static BattleRestShopFlowService CreateService(IBattleCardUpgradeService cardUpgradeService)
        {
            return new BattleRestShopFlowService(
                null,
                null,
                new FakeShopService(),
                null,
                null,
                cardUpgradeService);
        }

        private static BattleSceneState CreateUpgradeState(params RuntimeCard[] cards)
        {
            BattleSceneState state = new BattleSceneState
            {
                Gold = StartingGold,
                CardSelectMode = CardSelectMode.Upgrade
            };
            state.Deck.AddRange(cards);
            return state;
        }

        private static RuntimeCard CreateCard(int id, int upgradeCardId, bool isUpgraded = false)
        {
            RuntimeCardBuilder builder = BattleTestData.Card(id);
            builder.DisplayName = $"Card{id}";
            builder.UpgradeCardId = upgradeCardId;
            builder.IsUpgraded = isUpgraded;
            return builder.Build();
        }

        private static RuntimeRunDefinition CreateRunDefinition(params RuntimeCard[] cards)
        {
            Dictionary<int, RuntimeCard> cardCatalog = new Dictionary<int, RuntimeCard>();
            for (int i = 0; i < cards.Length; i++)
            {
                cardCatalog[cards[i].Id] = cards[i];
            }

            RuntimeRunDefinitionBuilder builder = BattleTestData.RunDefinition();
            builder.CardCatalog = cardCatalog;
            return builder.Build();
        }

        private static FakeCardUpgradeService CreateCardUpgradeService(
            params (RuntimeCard Source, RuntimeCard Upgraded)[] upgrades)
        {
            FakeCardUpgradeService service = new FakeCardUpgradeService();
            for (int i = 0; i < upgrades.Length; i++)
            {
                service.AddUpgrade(upgrades[i].Source, upgrades[i].Upgraded);
            }

            return service;
        }

        /// <summary>
        /// カード強化結果を固定するテスト用サービスクラス
        /// </summary>
        private sealed class FakeCardUpgradeService : IBattleCardUpgradeService
        {
            private readonly Dictionary<int, RuntimeCard> _upgrades = new Dictionary<int, RuntimeCard>();

            public bool ShouldReplace { get; set; } = true;

            public void AddUpgrade(RuntimeCard sourceCard, RuntimeCard upgradedCard)
            {
                _upgrades[sourceCard.Id] = upgradedCard;
            }

            public bool TryGetUpgradePreview(
                RuntimeRunDefinition runDefinition,
                RuntimeCard card,
                out RuntimeCard upgradedCard)
            {
                upgradedCard = null;
                return card != null
                       && _upgrades.TryGetValue(card.Id, out upgradedCard)
                       && upgradedCard != null;
            }

            public bool TryReplaceDeckCard(BattleSceneState state, int deckIndex, RuntimeCard replacementCard)
            {
                if (!ShouldReplace
                    || state == null
                    || replacementCard == null
                    || deckIndex < 0
                    || deckIndex >= state.Deck.Count)
                {
                    return false;
                }

                state.Deck[deckIndex] = replacementCard;
                return true;
            }

            public bool TryUpgradeRandomCard(
                BattleSceneState state,
                RuntimeRunDefinition runDefinition,
                CardRarity rarity,
                IBattleRandomProvider randomProvider)
            {
                return false;
            }
        }

        /// <summary>
        /// 強化価格を固定するテスト用ショップサービスクラス
        /// </summary>
        private sealed class FakeShopService : IBattleShopService
        {
            public void InitializeShop(
                BattleSceneState state,
                RuntimeRunDefinition runDef,
                IBattleRandomProvider random)
            {
            }

            public bool PurchaseShopItem(BattleSceneState state, int slotIndex)
            {
                return false;
            }

            public int GetCardRemovalPrice(BattleSceneState state)
            {
                return 0;
            }

            public bool PurchaseCardRemoval(BattleSceneState state, RuntimeCard card)
            {
                return false;
            }

            public int GetCardUpgradePrice(RuntimeRunDefinition runDefinition, RuntimeCard card)
            {
                return UpgradePrice;
            }
        }
    }
}
