using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneの戦闘用デッキサイクルを扱うインターフェース
    /// </summary>
    public interface IBattleDeckService
    {
        /// <summary>
        /// 手札補充を行う
        /// </summary>
        void DrawHand(BattleSceneState state, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 戦闘用山札を初期化する
        /// </summary>
        void PrepareBattleDeck(BattleSceneState state, IBattleRandomProvider randomProvider);

        /// <summary>
        /// 手札を捨て札へ送る
        /// </summary>
        void DiscardHand(BattleSceneState state);

        /// <summary>
        /// 指定枚数だけカードを引く
        /// </summary>
        int DrawCards(BattleSceneState state, IBattleRandomProvider randomProvider, int drawCount);

        /// <summary>
        /// 指定枚数を引く前に山札補充が発生するかを判定する
        /// </summary>
        bool WillRefillDrawPile(BattleSceneState state, int drawCount);
    }
}
