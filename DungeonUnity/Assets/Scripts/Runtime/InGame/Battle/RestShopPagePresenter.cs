using System;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.View;

namespace Dungeon.Runtime.InGame.Battle
{
    /// <summary>
    /// 補給画面仲介クラス
    /// </summary>
    public sealed class RestShopPagePresenter
    {
        private IRestShopPageView _view;

        /// <summary>
        /// View接続初期化
        /// </summary>
        public void Initialize(IRestShopPageView view, Action onRestClicked, Action onUpgradeClicked, Action onShopClicked, Action onContinueClicked)
        {
            _view = view;
            _view.WireButtons(onRestClicked, onUpgradeClicked, onShopClicked, onContinueClicked);
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
            _view.SetRestShopText(snapshot.RestShopMessage);
            _view.SetRestShopContinueInteractable(snapshot.IsRestShopContinueEnabled);
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
