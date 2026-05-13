using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Domain;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// 報酬画面表示インターフェース
    /// </summary>
    public interface IRewardPageView
    {
        void BuildRewardButtons(IReadOnlyList<CardDefinition> cards, Action<CardDefinition> onClicked);
        void ClearDynamicButtons();
    }
}
