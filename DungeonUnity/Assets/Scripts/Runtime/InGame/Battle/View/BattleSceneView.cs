using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// BattleSceneのviewクラス
    /// </summary>
    public sealed class BattleSceneView : MonoBehaviour, IBattleSceneView
    {
        [Header("Panels")]
        [SerializeField] private GameObject _mapPanel;
        [SerializeField] private GameObject _battlePanel;
        [SerializeField] private GameObject _rewardPanel;
        [SerializeField] private GameObject _restShopPanel;
        [SerializeField] private GameObject _resultPanel;

        [Header("Map UI")]
        [SerializeField] private Transform _mapNodeRoot;
        [SerializeField] private BattleOptionButtonView _mapNodeButtonTemplate;
        [SerializeField] private TFTextUGUI _mapStateText;

        [Header("Battle UI")]
        [SerializeField] private TFTextUGUI _playerStatText;
        [SerializeField] private TFTextUGUI _enemyStatText;
        [SerializeField] private TFTextUGUI _battleHintText;
        [SerializeField] private Transform _handCardRoot;
        [SerializeField] private BattleOptionButtonView _handCardButtonTemplate;
        [SerializeField] private Button _enemyTargetButton;
        [SerializeField] private Button _endTurnButton;

        [Header("Reward UI")]
        [SerializeField] private Transform _rewardRoot;
        [SerializeField] private BattleOptionButtonView _rewardButtonTemplate;

        [Header("RestShop UI")]
        [SerializeField] private TFTextUGUI _restShopText;
        [SerializeField] private Button _restButton;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _restShopContinueButton;

        [Header("Result UI")]
        [SerializeField] private TFTextUGUI _resultText;
        [SerializeField] private Button _resultBackButton;

        private readonly List<BattleOptionButtonView> _mapButtons = new List<BattleOptionButtonView>();
        private readonly List<BattleOptionButtonView> _handButtons = new List<BattleOptionButtonView>();
        private readonly List<BattleOptionButtonView> _rewardButtons = new List<BattleOptionButtonView>();

        private IMapPageView _mapPageView;
        private IBattlePageView _battlePageView;
        private IRewardPageView _rewardPageView;
        private IRestShopPageView _restShopPageView;
        private IResultPageView _resultPageView;

        /// <summary>
        /// マップ画面View取得
        /// </summary>
        public IMapPageView MapPageView
        {
            get { return _mapPageView ??= new MapPageViewAdapter(this); }
        }

        /// <summary>
        /// 戦闘画面View取得
        /// </summary>
        public IBattlePageView BattlePageView
        {
            get { return _battlePageView ??= new BattlePageViewAdapter(this); }
        }

        /// <summary>
        /// 報酬画面View取得
        /// </summary>
        public IRewardPageView RewardPageView
        {
            get { return _rewardPageView ??= new RewardPageViewAdapter(this); }
        }

        /// <summary>
        /// 補給画面View取得
        /// </summary>
        public IRestShopPageView RestShopPageView
        {
            get { return _restShopPageView ??= new RestShopPageViewAdapter(this); }
        }

        /// <summary>
        /// 結果画面View取得
        /// </summary>
        public IResultPageView ResultPageView
        {
            get { return _resultPageView ??= new ResultPageViewAdapter(this); }
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
        /// マップボタン構築
        /// </summary>
        public void BuildMapButtons(IReadOnlyList<MapTemplate.Node> nodes, Action<int> onClicked)
        {
            ClearButtons(_mapButtons);
            if (_mapNodeRoot == null || _mapNodeButtonTemplate == null || nodes == null)
            {
                return;
            }

            _mapNodeButtonTemplate.gameObject.SetActive(false);

            for (int i = 0; i < nodes.Count; i++)
            {
                int nodeIndex = i;
                MapTemplate.Node node = nodes[nodeIndex];
                BattleOptionButtonView button = Instantiate(_mapNodeButtonTemplate, _mapNodeRoot);
                button.gameObject.SetActive(true);
                button.Configure(string.Format(BattleSceneConstants.MapNodeLabelFormat, nodeIndex + 1, node.Label), delegate
                {
                    if (onClicked != null)
                    {
                        onClicked(nodeIndex);
                    }
                });
                _mapButtons.Add(button);
            }
        }

        /// <summary>
        /// マップボタンのアクティブ切り替え
        /// </summary>
        public void SetMapButtonInteractable(int allowedIndex)
        {
            for (int i = 0; i < _mapButtons.Count; i++)
            {
                _mapButtons[i].SetInteractable(i == allowedIndex);
            }
        }

        /// <summary>
        /// 手札ボタン構築
        /// </summary>
        public void BuildHandButtons(IReadOnlyList<CardDefinition> hand, Action<int> onClicked)
        {
            ClearButtons(_handButtons);

            if (_handCardRoot == null || _handCardButtonTemplate == null || hand == null)
            {
                return;
            }

            _handCardButtonTemplate.gameObject.SetActive(false);

            for (int i = 0; i < hand.Count; i++)
            {
                int handIndex = i;
                CardDefinition card = hand[handIndex];
                BattleOptionButtonView button = Instantiate(_handCardButtonTemplate, _handCardRoot);
                button.gameObject.SetActive(true);
                button.Configure(string.Format(BattleSceneConstants.CardLabelFormat, card.DisplayName, card.Cost, card.Damage), delegate
                {
                    if (onClicked != null)
                    {
                        onClicked(handIndex);
                    }
                });
                _handButtons.Add(button);
            }
        }

        /// <summary>
        /// 報酬ボタン構築
        /// </summary>
        public void BuildRewardButtons(IReadOnlyList<CardDefinition> cards, Action<CardDefinition> onClicked)
        {
            ClearButtons(_rewardButtons);

            if (_rewardRoot == null || _rewardButtonTemplate == null || cards == null)
            {
                return;
            }

            _rewardButtonTemplate.gameObject.SetActive(false);

            for (int i = 0; i < cards.Count; i++)
            {
                CardDefinition card = cards[i];
                BattleOptionButtonView button = Instantiate(_rewardButtonTemplate, _rewardRoot);
                button.gameObject.SetActive(true);
                button.Configure(string.Format(BattleSceneConstants.RewardLabelFormat, card.DisplayName, card.Cost, card.Damage), delegate
                {
                    if (onClicked != null)
                    {
                        onClicked(card);
                    }
                });
                _rewardButtons.Add(button);
            }
        }

        /// <summary>
        /// ボタン一覧消去
        /// </summary>
        private static void ClearButtons(List<BattleOptionButtonView> buttons)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                BattleOptionButtonView button = buttons[i];
                if (button == null)
                {
                    continue;
                }

                button.Clear();
                Destroy(button.gameObject);
            }

            buttons.Clear();
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
        /// マップ画面表示実装クラス
        /// </summary>
        private sealed class MapPageViewAdapter : IMapPageView
        {
            private readonly BattleSceneView _owner;

            public MapPageViewAdapter(BattleSceneView owner)
            {
                _owner = owner;
            }

            public void SetMapStateText(string message)
            {
                if (_owner._mapStateText != null)
                {
                    _owner._mapStateText.text = message;
                }
            }

            public void BuildMapButtons(IReadOnlyList<MapTemplate.Node> nodes, Action<int> onClicked)
            {
                _owner.BuildMapButtons(nodes, onClicked);
            }

            public void SetMapButtonInteractable(int allowedIndex)
            {
                for (int i = 0; i < _owner._mapButtons.Count; i++)
                {
                    _owner._mapButtons[i].SetInteractable(i == allowedIndex);
                }
            }

            public void ClearDynamicButtons()
            {
                ClearButtons(_owner._mapButtons);
            }
        }

        /// <summary>
        /// 戦闘画面表示実装クラス
        /// </summary>
        private sealed class BattlePageViewAdapter : IBattlePageView
        {
            private readonly BattleSceneView _owner;

            public BattlePageViewAdapter(BattleSceneView owner)
            {
                _owner = owner;
            }

            public void WireButtons(Action onEnemyTargetClicked, Action onEndTurnClicked)
            {
                UnwireButtons();
                if (_owner._enemyTargetButton != null)
                {
                    _owner._enemyTargetButton.onClick.AddListener(() => onEnemyTargetClicked());
                }
                if (_owner._endTurnButton != null)
                {
                    _owner._endTurnButton.onClick.AddListener(() => onEndTurnClicked());
                }
            }

            public void UnwireButtons()
            {
                if (_owner._enemyTargetButton != null)
                {
                    _owner._enemyTargetButton.onClick.RemoveAllListeners();
                }
                if (_owner._endTurnButton != null)
                {
                    _owner._endTurnButton.onClick.RemoveAllListeners();
                }
            }

            public void SetBattleStateText(string playerText, string enemyText, string hintText)
            {
                if (_owner._playerStatText != null)
                {
                    _owner._playerStatText.text = playerText;
                }
                if (_owner._enemyStatText != null)
                {
                    _owner._enemyStatText.text = enemyText;
                }
                if (_owner._battleHintText != null)
                {
                    _owner._battleHintText.text = hintText;
                }
            }

            public void BuildHandButtons(IReadOnlyList<CardDefinition> hand, Action<int> onClicked)
            {
                _owner.BuildHandButtons(hand, onClicked);
            }

            public void ClearDynamicButtons()
            {
                ClearButtons(_owner._handButtons);
            }
        }

        /// <summary>
        /// 報酬画面表示実装クラス
        /// </summary>
        private sealed class RewardPageViewAdapter : IRewardPageView
        {
            private readonly BattleSceneView _owner;

            public RewardPageViewAdapter(BattleSceneView owner)
            {
                _owner = owner;
            }

            public void BuildRewardButtons(IReadOnlyList<CardDefinition> cards, Action<CardDefinition> onClicked)
            {
                _owner.BuildRewardButtons(cards, onClicked);
            }

            public void ClearDynamicButtons()
            {
                ClearButtons(_owner._rewardButtons);
            }
        }

        /// <summary>
        /// 補給画面表示実装クラス
        /// </summary>
        private sealed class RestShopPageViewAdapter : IRestShopPageView
        {
            private readonly BattleSceneView _owner;

            public RestShopPageViewAdapter(BattleSceneView owner)
            {
                _owner = owner;
            }

            public void WireButtons(Action onRestClicked, Action onUpgradeClicked, Action onShopClicked, Action onContinueClicked)
            {
                UnwireButtons();
                if (_owner._restButton != null)
                {
                    _owner._restButton.onClick.AddListener(() => onRestClicked());
                }
                if (_owner._upgradeButton != null)
                {
                    _owner._upgradeButton.onClick.AddListener(() => onUpgradeClicked());
                }
                if (_owner._shopButton != null)
                {
                    _owner._shopButton.onClick.AddListener(() => onShopClicked());
                }
                if (_owner._restShopContinueButton != null)
                {
                    _owner._restShopContinueButton.onClick.AddListener(() => onContinueClicked());
                }
            }

            public void UnwireButtons()
            {
                if (_owner._restButton != null)
                {
                    _owner._restButton.onClick.RemoveAllListeners();
                }
                if (_owner._upgradeButton != null)
                {
                    _owner._upgradeButton.onClick.RemoveAllListeners();
                }
                if (_owner._shopButton != null)
                {
                    _owner._shopButton.onClick.RemoveAllListeners();
                }
                if (_owner._restShopContinueButton != null)
                {
                    _owner._restShopContinueButton.onClick.RemoveAllListeners();
                }
            }

            public void SetRestShopText(string message)
            {
                if (_owner._restShopText != null)
                {
                    _owner._restShopText.text = message;
                }
            }

            public void SetRestShopContinueInteractable(bool interactable)
            {
                if (_owner._restShopContinueButton != null)
                {
                    _owner._restShopContinueButton.interactable = interactable;
                }
            }
        }

        /// <summary>
        /// 結果画面表示実装クラス
        /// </summary>
        private sealed class ResultPageViewAdapter : IResultPageView
        {
            private readonly BattleSceneView _owner;

            public ResultPageViewAdapter(BattleSceneView owner)
            {
                _owner = owner;
            }

            public void WireButtons(Action onBackClicked)
            {
                UnwireButtons();
                if (_owner._resultBackButton != null)
                {
                    _owner._resultBackButton.onClick.AddListener(() => onBackClicked());
                }
            }

            public void UnwireButtons()
            {
                if (_owner._resultBackButton != null)
                {
                    _owner._resultBackButton.onClick.RemoveAllListeners();
                }
            }

            public void SetResultText(string message)
            {
                if (_owner._resultText != null)
                {
                    _owner._resultText.text = message;
                }
            }
        }
    }
}
