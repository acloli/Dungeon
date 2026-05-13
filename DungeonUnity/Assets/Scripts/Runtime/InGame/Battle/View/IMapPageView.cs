using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Domain;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// マップ画面表示インターフェース
    /// </summary>
    public interface IMapPageView
    {
        void SetMapStateText(string message);
        void BuildMapButtons(IReadOnlyList<MapTemplate.Node> nodes, Action<int> onClicked);
        void SetMapButtonInteractable(int allowedIndex);
        void ClearDynamicButtons();
    }
}
