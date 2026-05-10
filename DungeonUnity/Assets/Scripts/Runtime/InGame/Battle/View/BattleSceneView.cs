using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    public sealed class BattleSceneView : MonoBehaviour
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

        public void WireStaticButtons(Action onEnemyTargetClicked, Action onEndTurnClicked, Action onRestClicked, Action onUpgradeClicked, Action onShopClicked, Action onRestShopContinueClicked, Action onResultBackClicked)
        {
            UnwireStaticButtons();

            if (_enemyTargetButton != null)
            {
                _enemyTargetButton.onClick.AddListener(() => onEnemyTargetClicked());
            }

            if (_endTurnButton != null)
            {
                _endTurnButton.onClick.AddListener(() => onEndTurnClicked());
            }

            if (_restButton != null)
            {
                _restButton.onClick.AddListener(() => onRestClicked());
            }

            if (_upgradeButton != null)
            {
                _upgradeButton.onClick.AddListener(() => onUpgradeClicked());
            }

            if (_shopButton != null)
            {
                _shopButton.onClick.AddListener(() => onShopClicked());
            }

            if (_restShopContinueButton != null)
            {
                _restShopContinueButton.onClick.AddListener(() => onRestShopContinueClicked());
            }

            if (_resultBackButton != null)
            {
                _resultBackButton.onClick.AddListener(() => onResultBackClicked());
            }
        }

        public void UnwireStaticButtons()
        {
            if (_enemyTargetButton != null)
            {
                _enemyTargetButton.onClick.RemoveAllListeners();
            }

            if (_endTurnButton != null)
            {
                _endTurnButton.onClick.RemoveAllListeners();
            }

            if (_restButton != null)
            {
                _restButton.onClick.RemoveAllListeners();
            }

            if (_upgradeButton != null)
            {
                _upgradeButton.onClick.RemoveAllListeners();
            }

            if (_shopButton != null)
            {
                _shopButton.onClick.RemoveAllListeners();
            }

            if (_restShopContinueButton != null)
            {
                _restShopContinueButton.onClick.RemoveAllListeners();
            }

            if (_resultBackButton != null)
            {
                _resultBackButton.onClick.RemoveAllListeners();
            }
        }

        public void SetPanels(bool map, bool battle, bool reward, bool restShop, bool result)
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

        public void SetMapStateText(string message)
        {
            if (_mapStateText != null)
            {
                _mapStateText.text = message;
            }
        }

        public void SetBattleStateText(string playerText, string enemyText, string hintText)
        {
            if (_playerStatText != null)
            {
                _playerStatText.text = playerText;
            }

            if (_enemyStatText != null)
            {
                _enemyStatText.text = enemyText;
            }

            if (_battleHintText != null)
            {
                _battleHintText.text = hintText;
            }
        }

        public void SetBattleHintText(string message)
        {
            if (_battleHintText != null)
            {
                _battleHintText.text = message;
            }
        }

        public void SetRestShopText(string message)
        {
            if (_restShopText != null)
            {
                _restShopText.text = message;
            }
        }

        public void SetRestShopContinueInteractable(bool interactable)
        {
            if (_restShopContinueButton != null)
            {
                _restShopContinueButton.interactable = interactable;
            }
        }

        public void SetResultText(string message)
        {
            if (_resultText != null)
            {
                _resultText.text = message;
            }
        }

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

        public void SetMapButtonInteractable(int allowedIndex)
        {
            for (int i = 0; i < _mapButtons.Count; i++)
            {
                _mapButtons[i].SetInteractable(i == allowedIndex);
            }
        }

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

        public void ClearDynamicButtons()
        {
            ClearButtons(_mapButtons);
            ClearButtons(_handButtons);
            ClearButtons(_rewardButtons);
        }

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
    }
}
