using System;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.View;

namespace Dungeon.Runtime.InGame.Battle
{
    /// <summary>
    /// 戦闘画面仲介クラス
    /// </summary>
    public sealed class BattlePagePresenter
    {
        private IBattlePageView _view;
        private Action<int> _onHandCardClicked;
        private Action _onEnemyTargetClicked;
        private Action _onEndTurnClicked;

        /// <summary>
        /// View 接続初期化
        /// </summary>
        public void Initialize(IBattlePageView view, Action<int> onHandCardClicked, Action onEnemyTargetClicked, Action onEndTurnClicked)
        {
            _view = view;
            _onHandCardClicked = onHandCardClicked;
            _onEnemyTargetClicked = onEnemyTargetClicked;
            _onEndTurnClicked = onEndTurnClicked;
            _view.WireButtons(_onEnemyTargetClicked, _onEndTurnClicked);
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

            _view.BuildHandButtons(snapshot.Hand, _onHandCardClicked);
            _view.SetBattleStateText(
                string.Format(
                    BattleSceneConstants.PlayerStateFormat,
                    snapshot.PlayerHp,
                    snapshot.PlayerMaxHp,
                    snapshot.PlayerBlock,
                    snapshot.PlayerEnergy,
                    snapshot.Gold),
                string.Format(
                    BattleSceneConstants.EnemyStateFormat,
                    snapshot.CurrentEnemy != null ? snapshot.CurrentEnemy.DisplayName : BattleSceneConstants.UnknownEnemyName,
                    snapshot.EnemyHp,
                    snapshot.EnemyBlock),
                snapshot.BattleHintMessage);
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
