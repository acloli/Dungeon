using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// 報酬画面表示インターフェース
    /// </summary>
    public interface IRewardDialogView
    {
        void BuildRewardButtons(IReadOnlyList<RuntimeRewardEntry> entries, Action<RuntimeRewardEntry> onClicked);
        void ClearDynamicButtons();
    }
}
