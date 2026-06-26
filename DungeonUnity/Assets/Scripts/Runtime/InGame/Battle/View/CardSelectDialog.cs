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
        [SerializeField] private TFTextUGUI _messageText;
        [SerializeField] private Transform _cardContainer;
        [SerializeField] private BattleCardIconView _cardTemplate;

        private readonly List<BattleCardIconView> _cardViews = new List<BattleCardIconView>();
        private BattleCardSelectDialogParam _param;
        private IReadOnlyList<RuntimeCard> _deckCards;
        private IReadOnlyDictionary<int, int> _cardPrices;

        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            _param = param as BattleCardSelectDialogParam;
            return UniTask.CompletedTask;
        }

        protected override void OnOpened()
        {
            WireButtons();
            _deckCards = _param?.DeckCards ?? System.Array.Empty<RuntimeCard>();
            _cardPrices = _param?.CardPrices ?? new Dictionary<int, int>();
            SetMessage(_param?.Message);
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

            for (int i = 0; i < _deckCards.Count; i++)
            {
                RuntimeCard card = _deckCards[i];
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
            if (card == null || !_cardPrices.TryGetValue(card.Id, out int price))
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
                if (Application.isPlaying)
                {
                    Destroy(cardView.gameObject);
                }
                else
                {
                    DestroyImmediate(cardView.gameObject);
                }
            }

            _cardViews.Clear();
        }

        private void OnCancelClicked()
        {
            CloseWithResult(new CardSelectDialogResult { IsCanceled = true });
        }

        private void OnCardClicked(RuntimeCard card)
        {
            if (_param?.OnCardSelected != null)
            {
                BattleCardSelectDialogRefreshData refreshData = _param.OnCardSelected.Invoke(card);
                if (refreshData != null)
                {
                    _deckCards = refreshData.DeckCards;
                    _cardPrices = refreshData.CardPrices;
                    SetMessage(refreshData.Message);
                    BuildCardViews();
                }

                return;
            }

            CloseWithResult(new CardSelectDialogResult { IsCanceled = false, SelectedCard = card });
        }

        private void SetMessage(string message)
        {
            if (_messageText != null)
            {
                _messageText.text = message ?? string.Empty;
            }
        }
    }
}
