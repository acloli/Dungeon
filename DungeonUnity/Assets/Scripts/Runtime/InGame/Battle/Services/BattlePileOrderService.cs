using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// パイル表示順序既定実装クラス
    /// </summary>
    public sealed class BattlePileOrderService : IBattlePileOrderService
    {
        /// <summary>
        /// パイル種別に応じた表示順を返す
        /// </summary>
        public IReadOnlyList<RuntimeCard> Order(BattlePileType pileType, IReadOnlyList<RuntimeCard> cards, BattleSceneState state)
        {
            if (cards == null || cards.Count == 0)
            {
                return Array.Empty<RuntimeCard>();
            }

            List<(RuntimeCard card, int sourceIndex)> indexed = new List<(RuntimeCard, int)>(cards.Count);
            for (int i = 0; i < cards.Count; i++)
            {
                indexed.Add((cards[i], i));
            }

            indexed.Sort((a, b) =>
            {
                int idCompare = a.card.Id.CompareTo(b.card.Id);
                if (idCompare != 0)
                {
                    return idCompare;
                }

                return a.sourceIndex.CompareTo(b.sourceIndex);
            });

            List<RuntimeCard> ordered = new List<RuntimeCard>(indexed.Count);
            for (int i = 0; i < indexed.Count; i++)
            {
                ordered.Add(indexed[i].card);
            }

            return ordered;
        }
    }
}