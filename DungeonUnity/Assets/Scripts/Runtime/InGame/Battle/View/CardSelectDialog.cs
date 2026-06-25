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
        [SerializeField] private BattleCardIconView _cardTemplate;

        private readonly List<BattleCardIconView> _cardViews = new List<BattleCardIconView>();
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
                if (!CanDisplayCard(card))
                {
                    continue;
                }

                BattleCardIconView cardView = Instantiate(_cardTemplate, _cardContainer);
                cardView.gameObject.SetActive(true);
                int cardPrice = ResolveCardPrice(card);
                cardView.Bind(
                    card,
                    true,
                    false,
                    _param.ShowPrice && cardPrice > 0,
                    cardPrice,
                    OnCardClicked);
                _cardViews.Add(cardView);
            }
        }

        private int ResolveCardPrice(RuntimeCard card)
        {
            if (card == null || !_param.CardPrices.TryGetValue(card.Id, out int price))
            {
                return 0;
            }

            return price;
        }

        private bool CanDisplayCard(RuntimeCard card)
        {
            if (card == null)
            {
                return false;
            }

            if (_param.Mode != CardSelectMode.Upgrade)
            {
                return true;
            }

            return card.UpgradeCardId > 0;
        }

        private void ClearDynamicCards()
        {
            for (int i = 0; i < _cardViews.Count; i++)
            {
                BattleCardIconView cardView = _cardViews[i];
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
