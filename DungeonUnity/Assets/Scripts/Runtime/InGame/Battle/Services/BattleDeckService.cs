using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneの戦闘用デッキサイクルを扱うクラス
    /// </summary>
    public sealed class BattleDeckService : IBattleDeckService
    {
        /// <summary>
        /// 手札補充を行う
        /// </summary>
        public void DrawHand(BattleSceneState state, IBattleRandomProvider randomProvider)
        {
            if (state == null)
            {
                return;
            }

            DiscardHand(state);
            DrawCards(state, randomProvider, BattleSceneConstants.DefaultHandSize);
        }

        /// <summary>
        /// 戦闘用山札を初期化する
        /// </summary>
        public void PrepareBattleDeck(BattleSceneState state, IBattleRandomProvider randomProvider)
        {
            if (state == null)
            {
                return;
            }

            state.DrawPile.Clear();
            state.DiscardPile.Clear();
            state.ExhaustPile.Clear();
            state.Hand.Clear();

            for (int i = 0; i < state.Deck.Count; i++)
            {
                RuntimeCard card = state.Deck[i];
                if (card != null)
                {
                    state.DrawPile.Add(card);
                }
            }

            ShufflePile(state.DrawPile, randomProvider);
        }

        /// <summary>
        /// 手札を捨て札へ送る
        /// </summary>
        public void DiscardHand(BattleSceneState state)
        {
            if (state == null || state.Hand.Count == 0)
            {
                return;
            }

            for (int i = 0; i < state.Hand.Count; i++)
            {
                RuntimeCard card = state.Hand[i];
                if (card != null)
                {
                    state.DiscardPile.Add(card);
                }
            }

            state.Hand.Clear();
            state.SelectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
        }

        /// <summary>
        /// 指定枚数だけカードを引く
        /// </summary>
        public int DrawCards(BattleSceneState state, IBattleRandomProvider randomProvider, int drawCount)
        {
            if (state == null || drawCount <= 0)
            {
                return 0;
            }

            int drawnCount = 0;
            while (drawnCount < drawCount && state.Hand.Count < BattleSceneConstants.MaxHandSize)
            {
                RefillDrawPileIfNeeded(state, randomProvider);
                if (state.DrawPile.Count == 0)
                {
                    break;
                }

                int topIndex = state.DrawPile.Count - 1;
                RuntimeCard card = state.DrawPile[topIndex];
                state.DrawPile.RemoveAt(topIndex);
                if (card == null)
                {
                    continue;
                }

                state.Hand.Add(card);
                drawnCount++;
            }

            return drawnCount;
        }

        /// <summary>
        /// 山札不足時に捨て札を戻す
        /// </summary>
        private static void RefillDrawPileIfNeeded(BattleSceneState state, IBattleRandomProvider randomProvider)
        {
            if (state.DrawPile.Count > 0 || state.DiscardPile.Count == 0)
            {
                return;
            }

            for (int i = 0; i < state.DiscardPile.Count; i++)
            {
                RuntimeCard card = state.DiscardPile[i];
                if (card != null)
                {
                    state.DrawPile.Add(card);
                }
            }

            state.DiscardPile.Clear();
            ShufflePile(state.DrawPile, randomProvider);
        }

        /// <summary>
        /// 山札を乱数順に並べ替える
        /// </summary>
        private static void ShufflePile(IList<RuntimeCard> cards, IBattleRandomProvider randomProvider)
        {
            if (cards == null || cards.Count <= 1)
            {
                return;
            }

            for (int i = cards.Count - 1; i > 0; i--)
            {
                int index = randomProvider != null ? randomProvider.Range(0, i + 1) : 0;
                RuntimeCard current = cards[i];
                cards[i] = cards[index];
                cards[index] = current;
            }
        }
    }
}
