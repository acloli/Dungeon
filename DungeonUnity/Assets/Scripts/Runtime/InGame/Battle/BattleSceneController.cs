using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.Services;
using Dungeon.Runtime.InGame.Battle.View;
using Dungeon.Runtime.InGame.Domain;
using TFramework.Scene;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using VContainer;

namespace Dungeon.Runtime.InGame.Battle
{
    public sealed class BattleSceneController : SceneControllerBase
    {
        [Header("Config")]
        [SerializeField] private RunStartConfig _runStartConfig;
        [SerializeField] private string _mainSceneName = BattleSceneConstants.MainSceneName;

        [Header("View")]
        [SerializeField] private BattleSceneView _view;

        private readonly BattleSceneState _state = new BattleSceneState();
        private BattleSceneRules _rules;
        private int _selectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
        private string _battleHintMessage = BattleSceneConstants.SelectCardAndTarget;

        [Inject]
        private void Construct(BattleSceneRules rules)
        {
            _rules = rules;
        }

        protected override UniTask OnInitializeInternalAsync(ISceneBridgeData bridgeData, CancellationToken ct)
        {
            if (_rules == null)
            {
                _rules = new BattleSceneRules();
            }

            ValidateConfiguration();

            if (_view == null)
            {
                Debug.LogError("BattleSceneView is missing.");
                return UniTask.CompletedTask;
            }

            _view.WireStaticButtons(OnEnemyTargetClicked, OnEndTurnClicked, OnRestClicked, OnUpgradeClicked, OnShopClicked, OnRestShopContinueClicked, OnResultBackClicked);

            InitializeRun();
            OpenMap();
            return UniTask.CompletedTask;
        }

        protected override void OnTerminateInternal()
        {
            if (_view == null)
            {
                return;
            }

            _view.UnwireStaticButtons();
            _view.ClearDynamicButtons();
        }

        private void ValidateConfiguration()
        {
            if (_runStartConfig == null)
            {
                Debug.LogError(BattleSceneConstants.MissingRunConfig);
            }
        }

        private void InitializeRun()
        {
            _rules.InitializeRun(_state, _runStartConfig);
            _selectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
        }

        private void OpenMap()
        {
            if (_view == null)
            {
                return;
            }

            _state.BattleFinished = false;
            _selectedCardIndex = BattleSceneConstants.UnselectedCardIndex;

            _view.SetPanels(true, false, false, false, false);
            _view.BuildMapButtons(_state.Nodes, OnMapNodeClicked);
            _view.SetMapButtonInteractable(_state.CurrentNodeIndex + 1);
            _view.SetMapStateText(string.Format(
                BattleSceneConstants.MapStateFormat,
                _state.PlayerHp,
                _state.PlayerMaxHp,
                _state.Gold,
                _state.CurrentNodeIndex + 2,
                _state.Nodes.Count));
        }

        private void OnMapNodeClicked(int index)
        {
            int expectedIndex = _state.CurrentNodeIndex + 1;
            if (index != expectedIndex)
            {
                _view.SetMapStateText(BattleSceneConstants.NextNodeOnly);
                return;
            }

            if (_state.Nodes == null || index < 0 || index >= _state.Nodes.Count)
            {
                return;
            }

            _state.CurrentNodeIndex = index;

            InGameNodeType nodeType = _state.Nodes[index].NodeType;
            if (nodeType == InGameNodeType.RestShop)
            {
                OpenRestShop();
                return;
            }

            OpenBattle(nodeType);
        }

        private void OpenBattle(InGameNodeType nodeType)
        {
            if (_view == null)
            {
                return;
            }

            _view.SetPanels(false, true, false, false, false);
            _state.BattleFinished = false;
            _selectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
            _state.PlayerEnergy = BattleSceneConstants.DefaultPlayerEnergy;
            _state.CurrentEnemy = _rules.SelectEnemy(_runStartConfig, nodeType);
            _state.EnemyHp = _state.CurrentEnemy != null ? _state.CurrentEnemy.MaxHp : BattleSceneConstants.DefaultEnemyHp;
            DrawHand();
            _view.BuildHandButtons(_state.Hand, OnHandCardClicked);
            _battleHintMessage = BattleSceneConstants.SelectCardAndTarget;
            _view.SetBattleHintText(_battleHintMessage);
            RefreshBattleState();
        }

        private void DrawHand()
        {
            _state.Hand.Clear();

            if (_state.Deck.Count == 0)
            {
                return;
            }

            int drawCount = Mathf.Min(BattleSceneConstants.DefaultHandSize, _state.Deck.Count);
            for (int i = 0; i < drawCount; i++)
            {
                int index = Random.Range(0, _state.Deck.Count);
                CardDefinition card = _state.Deck[index];
                if (card != null)
                {
                    _state.Hand.Add(card);
                }
            }
        }

        private void OnHandCardClicked(int index)
        {
            if (_state.BattleFinished)
            {
                return;
            }

            if (index < 0 || index >= _state.Hand.Count)
            {
                return;
            }

            _selectedCardIndex = index;
            CardDefinition card = _state.Hand[index];
            if (card != null)
            {
                _battleHintMessage = string.Format(BattleSceneConstants.CardSelectedFormat, card.DisplayName);
                _view.SetBattleHintText(_battleHintMessage);
            }
        }

        private void OnEnemyTargetClicked()
        {
            if (_state.BattleFinished)
            {
                return;
            }

            if (_selectedCardIndex < 0 || _selectedCardIndex >= _state.Hand.Count)
            {
                _battleHintMessage = BattleSceneConstants.SelectCardFirst;
                _view.SetBattleHintText(_battleHintMessage);
                return;
            }

            CardDefinition card = _state.Hand[_selectedCardIndex];
            if (!_rules.CanPlayCard(_state, card))
            {
                _battleHintMessage = BattleSceneConstants.NotEnoughEnergy;
                _view.SetBattleHintText(_battleHintMessage);
                _selectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
                return;
            }

            _rules.PlayCard(_state, card);
            _battleHintMessage = string.Format(BattleSceneConstants.DealDamageFormat, card.Damage);
            _view.SetBattleHintText(_battleHintMessage);
            _selectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
            RefreshBattleState();

            if (_state.EnemyHp <= 0)
            {
                OnBattleVictory();
            }
        }

        private void OnEndTurnClicked()
        {
            if (_state.BattleFinished)
            {
                return;
            }

            int intentDamage = _rules.ResolveEnemyTurn(_state);
            _battleHintMessage = string.Format(BattleSceneConstants.EnemyTurnFormat, intentDamage);
            _view.SetBattleHintText(_battleHintMessage);
            _state.PlayerEnergy = BattleSceneConstants.DefaultPlayerEnergy;
            DrawHand();
            _view.BuildHandButtons(_state.Hand, OnHandCardClicked);
            RefreshBattleState();

            if (_state.PlayerHp <= 0)
            {
                OpenResult(false);
            }
        }

        private void OnBattleVictory()
        {
            _state.BattleFinished = true;
            _state.Gold += _rules.GetBattleGoldReward(GetCurrentNodeType());

            if (GetCurrentNodeType() == InGameNodeType.Boss)
            {
                OpenResult(true);
                return;
            }

            OpenReward();
        }

        private void OpenReward()
        {
            if (_view == null)
            {
                return;
            }

            _view.SetPanels(false, false, true, false, false);
            IReadOnlyList<CardDefinition> rewardCards = _rules.SelectRewardChoices(_state, _runStartConfig);
            _view.BuildRewardButtons(rewardCards, OnRewardSelected);
        }

        private void OnRewardSelected(CardDefinition card)
        {
            if (card != null)
            {
                _state.Deck.Add(card);
            }

            OpenMap();
        }

        private void OpenRestShop()
        {
            if (_view == null)
            {
                return;
            }

            _view.SetPanels(false, false, false, true, false);
            _view.SetRestShopContinueInteractable(false);
            _view.SetRestShopText(string.Format(
                BattleSceneConstants.RestShopStateFormat,
                _state.PlayerHp,
                _state.PlayerMaxHp,
                _state.Gold));
        }

        private void OnRestClicked()
        {
            _rules.ApplyRest(_state);
            _view.SetRestShopText(string.Format(
                BattleSceneConstants.RestDoneFormat,
                _state.PlayerHp,
                _state.PlayerMaxHp));
            _view.SetRestShopContinueInteractable(true);
        }

        private void OnUpgradeClicked()
        {
            _view.SetRestShopText(BattleSceneConstants.UpgradeDone);
            _view.SetRestShopContinueInteractable(true);
        }

        private void OnShopClicked()
        {
            bool success = _rules.ApplyShopPurchase(_state);
            if (success)
            {
                _view.SetRestShopText(string.Format(BattleSceneConstants.PurchaseSuccessFormat, _state.Gold));
            }
            else
            {
                _view.SetRestShopText(BattleSceneConstants.NotEnoughGold);
            }

            _view.SetRestShopContinueInteractable(true);
        }

        private void OnRestShopContinueClicked()
        {
            OpenMap();
        }

        private void OpenResult(bool victory)
        {
            if (_view == null)
            {
                return;
            }

            _view.SetPanels(false, false, false, false, true);
            if (victory)
            {
                _view.SetResultText(string.Format(
                    BattleSceneConstants.ResultVictoryFormat,
                    _state.PlayerHp,
                    _state.PlayerMaxHp,
                    _state.Gold));
                return;
            }

            _view.SetResultText(BattleSceneConstants.RunFailedMessage);
        }

        private void OnResultBackClicked()
        {
            UnitySceneManager.LoadScene(_mainSceneName);
        }

        private void RefreshBattleState()
        {
            if (_view == null)
            {
                return;
            }

            string enemyName = "Enemy";
            if (_state.CurrentEnemy != null)
            {
                enemyName = _state.CurrentEnemy.DisplayName;
            }

            _view.SetBattleStateText(
                string.Format(
                    BattleSceneConstants.PlayerStateFormat,
                    _state.PlayerHp,
                    _state.PlayerMaxHp,
                    _state.PlayerEnergy,
                    _state.Gold),
                string.Format(
                    BattleSceneConstants.EnemyStateFormat,
                    enemyName,
                    _state.EnemyHp),
                _battleHintMessage);
        }

        private InGameNodeType GetCurrentNodeType()
        {
            if (_state.Nodes == null)
            {
                return InGameNodeType.Battle;
            }

            if (_state.CurrentNodeIndex < 0 || _state.CurrentNodeIndex >= _state.Nodes.Count)
            {
                return InGameNodeType.Battle;
            }

            return _state.Nodes[_state.CurrentNodeIndex].NodeType;
        }
    }
}
