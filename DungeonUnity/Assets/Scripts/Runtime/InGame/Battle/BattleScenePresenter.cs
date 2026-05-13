using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Battle.View;
using Dungeon.Runtime.InGame.Domain;

namespace Dungeon.Runtime.InGame.Battle
{
    /// <summary>
    /// BattleSceneの表示仲介クラス
    /// </summary>
    public sealed class BattleScenePresenter
    {
        private readonly IBattleSceneFlowService _flowService;
        private IBattleSceneView _view;

        public BattleScenePresenter(IBattleSceneFlowService flowService)
        {
            _flowService = flowService;
        }

        /// <summary>
        /// View接続初期化
        /// </summary>
        public void Initialize(IBattleSceneView view, RunStartConfig runStartConfig)
        {
            _view = view;
            _flowService.Initialize(runStartConfig);
            Render();
        }

        /// <summary>
        /// View切り離し処理
        /// </summary>
        public void Dispose()
        {
            _view = null;
        }

        /// <summary>
        /// マップ選択通知
        /// </summary>
        public void OnMapNodeClicked(int index)
        {
            _flowService.SelectMapNode(index);
            Render();
        }

        /// <summary>
        /// 手札選択通知
        /// </summary>
        public void OnHandCardClicked(int index)
        {
            _flowService.SelectHandCard(index);
            Render();
        }

        /// <summary>
        /// 敵対象クリック通知
        /// </summary>
        public void OnEnemyTargetClicked()
        {
            _flowService.TryPlaySelectedCard();
            Render();
        }

        /// <summary>
        /// ターン終了通知
        /// </summary>
        public void OnEndTurnClicked()
        {
            _flowService.EndTurn();
            Render();
        }

        /// <summary>
        /// 報酬選択通知
        /// </summary>
        public void OnRewardSelected(CardDefinition card)
        {
            _flowService.SelectReward(card);
            Render();
        }

        /// <summary>
        /// 休憩選択通知
        /// </summary>
        public void OnRestClicked()
        {
            _flowService.ApplyRest();
            Render();
        }

        /// <summary>
        /// 強化選択通知
        /// </summary>
        public void OnUpgradeClicked()
        {
            _flowService.ApplyUpgrade();
            Render();
        }

        /// <summary>
        /// 購入選択通知
        /// </summary>
        public void OnShopClicked()
        {
            _flowService.ApplyShopPurchase();
            Render();
        }

        /// <summary>
        /// 補給継続通知
        /// </summary>
        public void OnRestShopContinueClicked()
        {
            _flowService.ContinueFromRestShop();
            Render();
        }

        /// <summary>
        /// スナップショット反映処理
        /// </summary>
        private void Render()
        {
            if (_view == null)
            {
                return;
            }

            BattleSceneSnapshot snapshot = _flowService.CreateSnapshot();
            _view.ClearDynamicButtons();

            switch (snapshot.CurrentPage)
            {
                case BattleScenePage.Map:
                    _view.SetPanels(true, false, false, false, false);
                    _view.BuildMapButtons(snapshot.Nodes, OnMapNodeClicked);
                    _view.SetMapButtonInteractable(snapshot.CurrentNodeIndex + 1);
                    _view.SetMapStateText(snapshot.MapMessage);
                    break;
                case BattleScenePage.Battle:
                    _view.SetPanels(false, true, false, false, false);
                    _view.BuildHandButtons(snapshot.Hand, OnHandCardClicked);
                    _view.SetBattleStateText(
                        string.Format(
                            BattleSceneConstants.PlayerStateFormat,
                            snapshot.PlayerHp,
                            snapshot.PlayerMaxHp,
                            snapshot.PlayerEnergy,
                            snapshot.Gold),
                        string.Format(
                            BattleSceneConstants.EnemyStateFormat,
                            snapshot.CurrentEnemy != null ? snapshot.CurrentEnemy.DisplayName : "Enemy",
                            snapshot.EnemyHp),
                        snapshot.BattleHintMessage);
                    break;
                case BattleScenePage.Reward:
                    _view.SetPanels(false, false, true, false, false);
                    _view.BuildRewardButtons(snapshot.RewardChoices, OnRewardSelected);
                    break;
                case BattleScenePage.RestShop:
                    _view.SetPanels(false, false, false, true, false);
                    _view.SetRestShopText(snapshot.RestShopMessage);
                    _view.SetRestShopContinueInteractable(snapshot.IsRestShopContinueEnabled);
                    break;
                case BattleScenePage.Result:
                    _view.SetPanels(false, false, false, false, true);
                    _view.SetResultText(snapshot.ResultMessage);
                    break;
            }
        }
    }
}
