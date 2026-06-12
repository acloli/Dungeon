using System;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Game.MasterData.Generated;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// 報酬付与処理の単体テスト
    /// </summary>
    [TestFixture]
    public sealed class BattleRewardServiceTests
    {
        private const int RewardCardId = 1001;
        private const int InitialGold = 17;
        private const int GoldRewardValue = 50;
        private const int RewardWeight = 10;
        private const int RewardMinFloor = 1;
        private const int RewardMaxFloor = 99;
        private const int PotionOrRelicRewardValue = 9001;

        [Test]
        public void ApplyReward_Card_AddsCardToDeck()
        {
            BattleSceneState state = new BattleSceneState();
            RuntimeCard card = CreateCard(RewardCardId, "strike");
            RuntimeRewardEntry entry = new RuntimeRewardEntry(RewardType.Card, card.Id, card, null, null, RewardWeight, RewardMinFloor, RewardMaxFloor);

            new BattleRewardService().ApplyReward(state, entry);

            Assert.That(state.Deck.Count, Is.EqualTo(1));
            Assert.That(state.Deck[0], Is.SameAs(card));
        }

        [Test]
        public void ApplyReward_Gold_IncreasesGold()
        {
            BattleSceneState state = new BattleSceneState
            {
                Gold = InitialGold
            };
            RuntimeRewardEntry entry = new RuntimeRewardEntry(RewardType.Gold, GoldRewardValue, null, null, null, RewardWeight, RewardMinFloor, RewardMaxFloor);

            new BattleRewardService().ApplyReward(state, entry);

            Assert.That(state.Gold, Is.EqualTo(InitialGold + GoldRewardValue));
            Assert.That(state.Deck.Count, Is.EqualTo(0));
        }

        [TestCase(RewardType.Potion)]
        [TestCase(RewardType.Relic)]
        public void ApplyReward_PotionAndRelic_DoNotChangeState(RewardType rewardType)
        {
            BattleSceneState state = new BattleSceneState
            {
                Gold = InitialGold
            };
            RuntimeRewardEntry entry = new RuntimeRewardEntry(rewardType, PotionOrRelicRewardValue, null, null, null, RewardWeight, RewardMinFloor, RewardMaxFloor);

            new BattleRewardService().ApplyReward(state, entry);

            Assert.That(state.Gold, Is.EqualTo(InitialGold));
            Assert.That(state.Deck.Count, Is.EqualTo(0));
        }

        private static RuntimeCard CreateCard(int id, string key)
        {
            return new RuntimeCard(
                id,
                key,
                $"Card{id}",
                $"Card{id}",
                string.Empty,
                $"Card{id}.desc",
                string.Empty,
                1,
                CardType.Attack,
                CardRarity.Common,
                CharacterArchetype.CrimsonExile,
                Array.Empty<RuntimeCardEffect>());
        }
    }
}
