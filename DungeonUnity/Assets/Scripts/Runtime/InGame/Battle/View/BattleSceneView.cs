using Dungeon.Runtime.InGame.Battle.Model;
using UnityEngine;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// BattleSceneルートViewクラス
    /// </summary>
    public sealed class BattleSceneView : MonoBehaviour, IBattleSceneView
    {
        [Header("Panels")]
        [SerializeField] private GameObject _mapPanel;
        [SerializeField] private GameObject _battlePanel;
        [SerializeField] private GameObject _rewardPanel;
        [SerializeField] private GameObject _restShopPanel;
        [SerializeField] private GameObject _resultPanel;

        [Header("Page Views")]
        [SerializeField] private MapPageView _mapPageView;
        [SerializeField] private BattlePageView _battlePageView;
        [SerializeField] private RewardPageView _rewardPageView;
        [SerializeField] private RestShopPageView _restShopPageView;
        [SerializeField] private ResultPageView _resultPageView;

        /// <summary>
        /// マップ画面View取得
        /// </summary>
        public IMapPageView MapPageView => _mapPageView != null ? _mapPageView : _mapPageView = GetComponent<MapPageView>();

        /// <summary>
        /// 戦闘画面View取得
        /// </summary>
        public IBattlePageView BattlePageView => _battlePageView != null ? _battlePageView : _battlePageView = GetComponent<BattlePageView>();

        /// <summary>
        /// 報酬画面View取得
        /// </summary>
        public IRewardPageView RewardPageView => _rewardPageView != null ? _rewardPageView : _rewardPageView = GetComponent<RewardPageView>();

        /// <summary>
        /// 補給画面View取得
        /// </summary>
        public IRestShopPageView RestShopPageView => _restShopPageView != null ? _restShopPageView : _restShopPageView = GetComponent<RestShopPageView>();

        /// <summary>
        /// 結果画面View取得
        /// </summary>
        public IResultPageView ResultPageView => _resultPageView != null ? _resultPageView : _resultPageView = GetComponent<ResultPageView>();

        /// <summary>
        /// 自己参照補完
        /// </summary>
        private void Awake()
        {
            CachePageViews();
        }

        /// <summary>
        /// エディタ参照補完
        /// </summary>
        private void OnValidate()
        {
            CachePageViews();
        }

        /// <summary>
        /// 表示ページ切り替え
        /// </summary>
        public void ShowPage(BattleScenePage page)
        {
            SetPanels(
                page == BattleScenePage.Map,
                page == BattleScenePage.Battle,
                page == BattleScenePage.Reward,
                page == BattleScenePage.RestShop,
                page == BattleScenePage.Result);
        }

        /// <summary>
        /// 画面表示切り替え
        /// </summary>
        private void SetPanels(bool map, bool battle, bool reward, bool restShop, bool result)
        {
            if (_mapPanel != null)
            {
                _mapPanel.SetActive(map);
            }
            if (_battlePanel != null)
            {
                _battlePanel.SetActive(battle);
            }
            if (_rewardPanel != null)
            {
                _rewardPanel.SetActive(reward);
            }
            if (_restShopPanel != null)
            {
                _restShopPanel.SetActive(restShop);
            }
            if (_resultPanel != null)
            {
                _resultPanel.SetActive(result);
            }
        }

        /// <summary>
        /// page view参照補完
        /// </summary>
        private void CachePageViews()
        {
            _mapPageView ??= GetComponent<MapPageView>();
            _battlePageView ??= GetComponent<BattlePageView>();
            _rewardPageView ??= GetComponent<RewardPageView>();
            _restShopPageView ??= GetComponent<RestShopPageView>();
            _resultPageView ??= GetComponent<ResultPageView>();
        }
    }
}
