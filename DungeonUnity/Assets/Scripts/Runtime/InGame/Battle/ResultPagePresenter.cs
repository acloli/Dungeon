using System;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.View;

namespace Dungeon.Runtime.InGame.Battle
{
    /// <summary>
    /// 結果画面仲介クラス
    /// </summary>
    public sealed class ResultPagePresenter
    {
        private IResultPageView _view;

        /// <summary>
        /// View 接続初期化
        /// </summary>
        public void Initialize(IResultPageView view, Action onBackClicked)
        {
            _view = view;
            _view.WireButtons(onBackClicked);
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
            _view.SetResultText(snapshot.ResultMessage);
        }

        /// <summary>
        /// View切り離し処理
        /// </summary>
        public void Dispose()
        {
            if (_view == null)
            {
                return;
            }
            _view.UnwireButtons();
            _view = null;
        }
    }
}
