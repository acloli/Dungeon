using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneのカード強化を扱うサービス
    /// </summary>
    public sealed class BattleCardUpgradeService : IBattleCardUpgradeService
    {
        /// <inheritdoc />
        public bool TryGetUpgradePreview(
            RuntimeRunDefinition runDefinition,
            RuntimeCard card,
            out RuntimeCard upgradedCard)
        {
            upgradedCard = null;
            return runDefinition != null
                   && card != null
                   && !card.IsUpgraded
                   && card.UpgradeCardId > 0
                   && runDefinition.CardCatalog.TryGetValue(card.UpgradeCardId, out upgradedCard)
                   && upgradedCard != null;
        }

        /// <inheritdoc />
        public bool TryReplaceDeckCard(BattleSceneState state, int deckIndex, RuntimeCard replacementCard)
        {
            if (state == null
                || replacementCard == null
                || deckIndex < 0
                || deckIndex >= state.Deck.Count)
            {
                return false;
            }

            state.Deck[deckIndex] = replacementCard;
            return true;
        }

        /// <inheritdoc />
        public bool TryUpgradeRandomCard(
            BattleSceneState state,
            RuntimeRunDefinition runDefinition,
            CardRarity rarity,
            IBattleRandomProvider randomProvider)
        {
            if (state == null || runDefinition == null || randomProvider == null)
            {
                return false;
            }

            List<int> candidateDeckIndices = new List<int>();
            for (int deckIndex = 0; deckIndex < state.Deck.Count; deckIndex++)
            {
                RuntimeCard card = state.Deck[deckIndex];
                if (card == null
                    || card.Rarity != rarity
                    || !TryGetUpgradePreview(runDefinition, card, out _))
                {
                    continue;
                }

                candidateDeckIndices.Add(deckIndex);
            }

            if (candidateDeckIndices.Count == 0)
            {
                return false;
            }

            int candidateIndex = randomProvider.Range(0, candidateDeckIndices.Count);
            if (candidateIndex < 0 || candidateIndex >= candidateDeckIndices.Count)
            {
                return false;
            }

            int selectedDeckIndex = candidateDeckIndices[candidateIndex];
            RuntimeCard selectedCard = state.Deck[selectedDeckIndex];
            return TryGetUpgradePreview(runDefinition, selectedCard, out RuntimeCard upgradedCard)
                   && TryReplaceDeckCard(state, selectedDeckIndex, upgradedCard);
        }
    }
}
