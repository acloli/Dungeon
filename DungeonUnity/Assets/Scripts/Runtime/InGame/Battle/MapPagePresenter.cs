using System;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.View;

namespace Dungeon.Runtime.InGame.Battle
{
    /// <summary>
    /// マップ画面仲介クラス
    /// </summary>
    public sealed class MapPagePresenter
    {
        private IMapPageView _view;
        private Action<int> _onMapNodeClicked;

        /// <summary>
        /// View接続初期化
        /// </summary>
        public void Initialize(IMapPageView view, Action<int> onMapNodeClicked)
        {
            _view = view;
            _onMapNodeClicked = onMapNodeClicked;
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

            _view.BuildMapButtons(snapshot.Nodes, _onMapNodeClicked);
            _view.SetMapButtonInteractable(snapshot.CurrentNodeIndex + 1);
            _view.SetMapStateText(snapshot.MapMessage);
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
