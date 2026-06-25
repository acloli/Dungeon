using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// 報酬画面用カード選択ダイアログ
    /// </summary>
    public sealed class CardPickDialog : UIDialogBase<RuntimeRewardEntry>
    {
        [SerializeField] private Transform _cardContainer;
        [SerializeField] private BattleCardIconView _cardTemplate;
        [SerializeField] private Button _backButton;

        private readonly List<BattleCardIconView> _cardViews = new List<BattleCardIconView>();
        private BattleCardPickDialogParam _param;

        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            _param = param as BattleCardPickDialogParam;
            return UniTask.CompletedTask;
        }

        protected override void OnOpened()
        {
            WireBackButton();
            BuildCardButtons();
        }

        protected override void OnClosed()
        {
            UnwireBackButton();
            ClearDynamicCards();
        }

        private void WireBackButton()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackClicked);
        }

        private void UnwireBackButton()
        {
            if (_backButton != null)
                _backButton.onClick.RemoveAllListeners();
        }

        private void OnBackClicked()
        {
            CloseWithResult(null);
        }

        private void BuildCardButtons()
        {
            ClearDynamicCards();

            IReadOnlyList<RuntimeRewardEntry> entries = _param?.Snapshot?.RewardChoices;
            if (_cardContainer == null || _cardTemplate == null || entries == null)
            {
                return;
            }

            _cardTemplate.gameObject.SetActive(false);

            for (int i = 0; i < entries.Count; i++)
            {
                RuntimeRewardEntry entry = entries[i];
                if (entry?.Card == null)
                {
                    continue;
                }

                BattleCardIconView cardView = Instantiate(_cardTemplate, _cardContainer);
                cardView.gameObject.SetActive(true);
                RuntimeRewardEntry captured = entry;
                cardView.Bind(entry.Card, true, false, _ => CloseWithResult(captured));
                _cardViews.Add(cardView);
            }
        }

        private void ClearDynamicCards()
        {
            for (int i = 0; i < _cardViews.Count; i++)
            {
                BattleCardIconView cardView = _cardViews[i];
                if (cardView == null) continue;
                cardView.Clear();
                Destroy(cardView.gameObject);
            }
            _cardViews.Clear();
        }
    }
}
