using Dungeon.Runtime.InGame.Battle.Model;
using System.Collections.Generic;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleScene読取専用クエリインターフェース
    /// </summary>
    public interface IBattleSceneQueryService
    {
        /// <summary>
        /// 現在状態取得
        /// </summary>
        BattleSceneSnapshot CreateSnapshot();

        /// <summary>
        /// 現在のカード選択候補取得
        /// </summary>
        IReadOnlyList<RuntimeCard> GetCardSelectCards();

        /// <summary>
        /// 現在のカード選択価格取得
        /// </summary>
        IReadOnlyDictionary<int, int> GetCardSelectPrices();

        /// <summary>
        /// 現在のカード選択強化後カード取得
        /// </summary>
        IReadOnlyDictionary<int, RuntimeCard> GetCardSelectUpgradedCards();

        /// <summary>
        /// 現在のカード選択メッセージ取得
        /// </summary>
        string GetCardSelectMessage();

        /// <summary>
        /// 現在のカード選択用途取得
        /// </summary>
        CardSelectMode GetCardSelectMode();
    }
}
