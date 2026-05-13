using System;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.View;
using Dungeon.Runtime.InGame.Domain;

namespace Dungeon.Runtime.InGame.Battle
{
    /// <summary>
    /// 報酬画面仲介クラス
    /// </summary>
    public sealed class RewardPagePresenter
    {
        private IRewardPageView _view;
        private Action<CardDefinition> _onRewardSelected;

        /// <summary>
        /// View接続初期化
        /// </summary>
        public void Initialize(IRewardPageView view, Action<CardDefinition> onRewardSelected)
        {
            _view = view;
            _onRewardSelected = onRewardSelected;
        }

        /// <summary>
        /// 画面描画処理
        /// </summary>
        public void Render(BattleSceneSnapshot snapshot)
        {
            if (_view == null)
            {
                return;
            }
            _view.BuildRewardButtons(snapshot.RewardChoices, _onRewardSelected);
        }

        /// <summary>
        /// 動的要素消去
        /// </summary>
        public void Clear()
        {
            if (_view == null)
            {
                return;
            }
            _view.ClearDynamicButtons();
        }
    }
}
