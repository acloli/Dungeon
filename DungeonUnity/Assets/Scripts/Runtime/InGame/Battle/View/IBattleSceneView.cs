using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Domain;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// BattleScene表示操作インターフェース
    /// </summary>
    public interface IBattleSceneView
    {
        /// <summary>
        /// 画面表示切り替え
        /// </summary>
        void SetPanels(bool map, bool battle, bool reward, bool restShop, bool result);

        /// <summary>
        /// マップ文言反映
        /// </summary>
        void SetMapStateText(string message);

        /// <summary>
        /// 戦闘文言反映
        /// </summary>
        void SetBattleStateText(string playerText, string enemyText, string hintText);

        /// <summary>
        /// 補給文言反映
        /// </summary>
        void SetRestShopText(string message);

        /// <summary>
        /// 補給継続の切り替え
        /// </summary>
        void SetRestShopContinueInteractable(bool interactable);

        /// <summary>
        /// 結果文言反映
        /// </summary>
        void SetResultText(string message);

        /// <summary>
        /// マップボタン構築
        /// </summary>
        void BuildMapButtons(IReadOnlyList<MapTemplate.Node> nodes, Action<int> onClicked);

        /// <summary>
        /// マップボタン活性切り替え
        /// </summary>
        void SetMapButtonInteractable(int allowedIndex);

        /// <summary>
        /// 手札ボタン構築
        /// </summary>
        void BuildHandButtons(IReadOnlyList<CardDefinition> hand, Action<int> onClicked);

        /// <summary>
        /// 報酬ボタン構築
        /// </summary>
        void BuildRewardButtons(IReadOnlyList<CardDefinition> cards, Action<CardDefinition> onClicked);

        /// <summary>
        /// 動的ボタン消去
        /// </summary>
        void ClearDynamicButtons();
    }
}
