using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Game.MasterData.Generated;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleCombatEventServiceのEditorモードテストクラス
    /// </summary>
    public sealed class BattleCombatEventServiceTests
    {
        [TestCaseSource(nameof(AdditionalRelicHookCases))]
        public void AdditionalHook_AppliesCorrespondingRelicTrigger(
            RelicTriggerType expectedTrigger,
            Action<BattleCombatEventService, BattleSceneState> notify)
        {
            BattleSceneState state = new BattleSceneState();
            state.OwnedRelics.Add(CreateRelic(1, expectedTrigger));
            FakeBattleRelicService relicService = new FakeBattleRelicService();
            BattleCombatEventService service = new BattleCombatEventService(relicService);

            notify(service, state);

            Assert.That(relicService.AppliedStates, Is.EqualTo(new[] { state }));
            Assert.That(relicService.AppliedTriggers, Is.EqualTo(new[] { expectedTrigger }));
        }

        [Test]
        public void OnCardPlayed_PassesPlayedCardContextAndState()
        {
            BattleSceneState state = new BattleSceneState();
            RuntimeCard playedCard = CreateCard();
            FakeBattleRelicService relicService = new FakeBattleRelicService();
            BattleCombatEventService service = new BattleCombatEventService(relicService);

            service.OnCardPlayed(state, playedCard, new BattleCardResolutionResult(0, 0, 0));

            Assert.That(relicService.AppliedStates, Is.EqualTo(new[] { state }));
            Assert.That(relicService.AppliedContexts, Has.Count.EqualTo(1));
            Assert.That(relicService.AppliedContexts[0].TriggerType, Is.EqualTo(RelicTriggerType.CardPlayed));
            Assert.That(relicService.AppliedContexts[0].PlayedCard, Is.SameAs(playedCard));
        }

        [Test]
        public void OnPlayerTurnStart_ClearsTurnActivationsBeforeApplyingTrigger()
        {
            BattleSceneState state = new BattleSceneState();
            state.TryReserveTurnRelicEffectActivation(101);
            state.TryReserveTurnRelicEffectActivation(102);
            FakeBattleRelicService relicService = new FakeBattleRelicService();
            relicService.BeforeApply = (appliedState, triggerType) =>
            {
                Assert.That(appliedState, Is.SameAs(state));
                Assert.That(triggerType, Is.EqualTo(RelicTriggerType.PlayerTurnStart));
                Assert.That(appliedState.ActivatedRelicEffectIdsThisTurn, Is.Empty);
                relicService.InvocationSequence.Add("TurnActivationsCleared");
            };
            BattleCombatEventService service = new BattleCombatEventService(relicService);

            service.OnPlayerTurnStart(state);

            Assert.That(state.ActivatedRelicEffectIdsThisTurn, Is.Empty);
            Assert.That(
                relicService.InvocationSequence,
                Is.EqualTo(new[] { "TurnActivationsCleared", "ApplyEffects:PlayerTurnStart" }));
        }

        private static IEnumerable<TestCaseData> AdditionalRelicHookCases
        {
            get
            {
                yield return new TestCaseData(
                    RelicTriggerType.PlayerTurnEnd,
                    new Action<BattleCombatEventService, BattleSceneState>((service, state) => service.OnPlayerTurnEnd(state)))
                    .SetName("OnPlayerTurnEnd_AppliesPlayerTurnEndRelicTrigger");
                yield return new TestCaseData(
                    RelicTriggerType.CardPlayed,
                    new Action<BattleCombatEventService, BattleSceneState>((service, state) => service.OnCardPlayed(state, CreateCard(), new BattleCardResolutionResult(0, 0, 0))))
                    .SetName("OnCardPlayed_AppliesCardPlayedRelicTrigger");
                yield return new TestCaseData(
                    RelicTriggerType.PlayerDamaged,
                    new Action<BattleCombatEventService, BattleSceneState>((service, state) => service.OnPlayerDamaged(state, 3)))
                    .SetName("OnPlayerDamaged_AppliesPlayerDamagedRelicTrigger");
                yield return new TestCaseData(
                    RelicTriggerType.Shuffle,
                    new Action<BattleCombatEventService, BattleSceneState>((service, state) => service.OnShuffle(state)))
                    .SetName("OnShuffle_AppliesShuffleRelicTrigger");
                yield return new TestCaseData(
                    RelicTriggerType.CardExhausted,
                    new Action<BattleCombatEventService, BattleSceneState>((service, state) => service.OnCardExhausted(state, CreateCard())))
                    .SetName("OnCardExhausted_AppliesCardExhaustedRelicTrigger");
                yield return new TestCaseData(
                    RelicTriggerType.LoseHp,
                    new Action<BattleCombatEventService, BattleSceneState>((service, state) => service.OnLoseHp(state, 2)))
                    .SetName("OnLoseHp_AppliesLoseHpRelicTrigger");
            }
        }

        private static RuntimeCard CreateCard()
        {
            return new RuntimeCard(
                1,
                "strike",
                "Strike",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                1,
                CardType.Attack,
                CardRarity.Common,
                CharacterArchetype.CrimsonExile,
                Array.Empty<RuntimeCardEffect>());
        }

        private static RuntimeRelic CreateRelic(int id, RelicTriggerType triggerType)
        {
            return new RuntimeRelic(
                id,
                $"relic_{id}",
                $"Relic{id}",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                CardRarity.Uncommon,
                new[]
                {
                    new RuntimeRelicEffect(1, triggerType, EffectType.GainBlock, 1, 1, StatusType.None, 0, TargetSide.Self)
                });
        }

        private sealed class FakeBattleRelicService : IBattleRelicService
        {
            public List<BattleSceneState> AppliedStates { get; } = new List<BattleSceneState>();

            public List<RelicTriggerType> AppliedTriggers { get; } = new List<RelicTriggerType>();

            public List<RelicTriggerContext> AppliedContexts { get; } = new List<RelicTriggerContext>();

            public List<string> InvocationSequence { get; } = new List<string>();

            public Action<BattleSceneState, RelicTriggerType> BeforeApply { get; set; }

            public void RestoreOwnedRelics(BattleSceneState state, RuntimeRunDefinition runDefinition, IReadOnlyList<int> ownedRelicIds)
            {
            }

            public bool AddOwnedRelic(BattleSceneState state, RuntimeRelic relic)
            {
                return true;
            }

            public RuntimeRelic RollBattleRewardRelic(BattleSceneState state, RuntimeRunDefinition runDefinition, IBattleRandomProvider randomProvider)
            {
                return null;
            }

            public void ApplyEffects(BattleSceneState state, RelicTriggerContext context)
            {
                if (context == null)
                {
                    return;
                }

                AppliedContexts.Add(context);
                RecordApplication(state, context.TriggerType);
            }

            private void RecordApplication(BattleSceneState state, RelicTriggerType triggerType)
            {
                BeforeApply?.Invoke(state, triggerType);
                InvocationSequence.Add($"ApplyEffects:{triggerType}");
                AppliedStates.Add(state);
                AppliedTriggers.Add(triggerType);
            }
        }
    }
}
