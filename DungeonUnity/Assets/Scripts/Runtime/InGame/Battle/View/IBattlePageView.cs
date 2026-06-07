using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// 戦闘画面表示インターフェース
    /// </summary>
    public interface IBattlePageView
    {
        void WireButtons(Action onEndTurnClicked);
        void UnwireButtons();
        void SetBattleStateText(string playerText, string enemyText, string hintText);
        void BuildEnemyButtons(IReadOnlyList<BattleEnemyViewModel> enemies, int selectedEnemyIndex, Action<int> onClicked);
        void BuildHandButtons(IReadOnlyList<RuntimeCard> hand, Action<int> onClicked);
        void ClearDynamicButtons();
    }
}
