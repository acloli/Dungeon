using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// マップ画面表示インターフェース
    /// </summary>
    public interface IMapPageView
    {
        void SetMapStateText(string message);
        void BuildMapButtons(IReadOnlyList<RuntimeMapNode> nodes, Action<int> onClicked);
        void SetMapButtonInteractable(IReadOnlyList<int> allowedIndices);
        void ClearDynamicButtons();
        void BuildMapGraph(
            IReadOnlyList<RuntimeMapNode> nodes,
            IReadOnlyList<MapNodeLayout> layouts,
            IReadOnlyList<int> availableNodeIndices,
            int currentFloor,
            int currentNodeIndex,
            IReadOnlyList<int> mapRouteNodeIndices,
            Action<int> onNodeClicked);
        void SetScrollPosition(int floor);
    }
}
