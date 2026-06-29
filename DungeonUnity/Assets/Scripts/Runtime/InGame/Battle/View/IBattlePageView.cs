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
        void WirePileButtons(Action onDrawPileClicked, Action onDiscardPileClicked, Action onExhaustPileClicked);
        void UnwireButtons();
        void UnwirePileButtons();
        void SetBattleHud(BattleHudViewModel hud);
        void SetBattleStateText(string playerText, string enemyText, string hintText);
        void SetPileCounters(int drawPileCount, int discardPileCount, int exhaustPileCount, int handCount, int maxHandCount);
        void BuildEnemyButtons(IReadOnlyList<BattleEnemyViewModel> enemies, int selectedEnemyIndex, Action<int> onClicked);
        void BuildHandCards(IReadOnlyList<BattleHandCardViewModel> handCards, Action<int> onClicked);
        void ClearDynamicButtons();
    }
}
