using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    public sealed class CardSelectDialog : UIDialogBase<CardSelectDialogResult>
    {
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Transform _cardContainer;
        [SerializeField] private BattleCardSelectView _cardTemplate;

        private readonly List<BattleCardSelectView> _cardViews = new List<BattleCardSelectView>();
        private BattleCardSelectDialogParam _param;

        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            _param = param as BattleCardSelectDialogParam;
            return UniTask.CompletedTask;
        }

        protected override void OnOpened()
        {
            WireButtons();
            BuildCardViews();
        }

        protected override void OnClosed()
        {
            UnwireButtons();
            ClearDynamicCards();
        }

        private void WireButtons()
        {
            UnwireButtons();
            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(OnCancelClicked);
            }
        }

        private void UnwireButtons()
        {
            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveAllListeners();
            }
        }

        private void BuildCardViews()
        {
            ClearDynamicCards();

            if (_param?.Snapshot == null || _cardContainer == null || _cardTemplate == null)
            {
                return;
            }

            _cardTemplate.gameObject.SetActive(false);

            for (int i = 0; i < _param.DeckCards.Count; i++)
            {
                RuntimeCard card = _param.DeckCards[i];
                if (card == null)
                {
                    continue;
                }

                BattleCardSelectView cardView = Instantiate(_cardTemplate, _cardContainer);
                cardView.gameObject.SetActive(true);
                cardView.Bind(card, OnCardClicked);
                _cardViews.Add(cardView);
            }
        }

        private void ClearDynamicCards()
        {
            for (int i = 0; i < _cardViews.Count; i++)
            {
                BattleCardSelectView cardView = _cardViews[i];
                if (cardView == null)
                {
                    continue;
                }

                cardView.Clear();
                Destroy(cardView.gameObject);
            }

            _cardViews.Clear();
        }

        private void OnCancelClicked()
        {
            CloseWithResult(new CardSelectDialogResult { IsCanceled = true });
        }

        private void OnCardClicked(RuntimeCard card)
        {
            CloseWithResult(new CardSelectDialogResult { IsCanceled = false, SelectedCard = card });
        }
    }
}
