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

            public void ApplyEffects(BattleSceneState state, RelicTriggerType triggerType)
            {
                AppliedStates.Add(state);
                AppliedTriggers.Add(triggerType);
            }
        }
    }
}
