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
        [SerializeField] private Button _confirmButton;
        [SerializeField] private TFTextUGUI _messageText;
        [SerializeField] private Transform _cardContainer;
        [SerializeField] private Transform _previewContainer;
        [SerializeField] private BattleCardIconView _cardTemplate;

        private readonly List<BattleCardIconView> _cardViews = new List<BattleCardIconView>();
        private readonly List<BattleCardIconView> _previewCardViews = new List<BattleCardIconView>();
        private BattleCardSelectDialogParam _param;
        private IReadOnlyList<RuntimeCard> _deckCards;
        private IReadOnlyDictionary<int, int> _cardPrices;
        private IReadOnlyDictionary<int, RuntimeCard> _upgradedCards;
        private RuntimeCard _selectedCard;
        private int _selectedCardIndex;
        private int _gold;
        private bool _isPreviewOpen;

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
            _upgradedCards = _param?.UpgradedCards ?? new Dictionary<int, RuntimeCard>();
            _selectedCard = null;
            _selectedCardIndex = -1;
            _gold = _param?.Snapshot?.Gold ?? 0;
            _isPreviewOpen = false;
            SetMessage(_param?.Message);
            SetPreviewVisible(false);
            SetConfirmVisible(_param?.Mode == CardSelectMode.Upgrade);
            SetConfirmInteractable(false);
            BuildCardViews();
        }

        protected override void OnClosed()
        {
            UnwireButtons();
            ClearDynamicCards();
            ClearPreviewCards();
        }

        private void WireButtons()
        {
            UnwireButtons();
            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(OnCancelClicked);
            }
            if (_confirmButton != null)
            {
                _confirmButton.onClick.AddListener(OnConfirmClicked);
            }
        }

        private void UnwireButtons()
        {
            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveAllListeners();
            }
            if (_confirmButton != null)
            {
                _confirmButton.onClick.RemoveAllListeners();
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
                int cardIndex = i;
                int cardPrice = ResolveCardPrice(card);
                cardView.Bind(
                    card,
                    true,
                    !_isPreviewOpen,
                    IsSelectedCard(cardIndex),
                    _param.ShowPrice && cardPrice > 0,
                    cardPrice,
                    _ => OnCardClicked(card, cardIndex));
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
                DestroyCardView(_cardViews[i]);
            }

            _cardViews.Clear();
        }

        private void ClearPreviewCards()
        {
            for (int i = 0; i < _previewCardViews.Count; i++)
            {
                DestroyCardView(_previewCardViews[i]);
            }

            _previewCardViews.Clear();
        }

        private void OnCancelClicked()
        {
            if (_isPreviewOpen)
            {
                ClosePreview();
                return;
            }

            CloseWithResult(new CardSelectDialogResult { IsCanceled = true });
        }

        private void OnCardClicked(RuntimeCard card, int cardIndex)
        {
            if (_param?.Mode == CardSelectMode.Upgrade && _param.OnCardConfirmed != null)
            {
                SelectUpgradePreview(card, cardIndex);
                return;
            }

            CloseWithResult(new CardSelectDialogResult { IsCanceled = false, SelectedCard = card });
        }

        private void OnConfirmClicked()
        {
            if (_selectedCard == null || _param?.OnCardConfirmed == null)
            {
                return;
            }

            if (!CanConfirmCard(_selectedCard))
            {
                SetConfirmInteractable(false);
                SetMessage(BattleSceneConstants.NotEnoughGold);
                return;
            }

            BattleCardSelectDialogRefreshData refreshData = _param.OnCardConfirmed.Invoke(_selectedCard);
            _selectedCard = null;
            _selectedCardIndex = -1;
            if (refreshData != null)
            {
                _deckCards = refreshData.DeckCards;
                _cardPrices = refreshData.CardPrices;
                _upgradedCards = refreshData.UpgradedCards;
                _gold = refreshData.Gold;
                SetMessage(refreshData.Message);
            }

            ClearPreviewCards();
            _isPreviewOpen = false;
            SetPreviewVisible(false);
            SetConfirmInteractable(false);
            BuildCardViews();
        }

        private void SelectUpgradePreview(RuntimeCard card, int cardIndex)
        {
            _selectedCard = card;
            _selectedCardIndex = cardIndex;
            _isPreviewOpen = true;
            BuildCardViews();
            BuildPreviewCards(card);

            bool canConfirm = CanConfirmCard(card);
            SetConfirmInteractable(canConfirm);
            if (!canConfirm)
            {
                SetMessage(BattleSceneConstants.NotEnoughGold);
                return;
            }

            SetMessage(_param?.Message);
        }

        private void ClosePreview()
        {
            _selectedCard = null;
            _selectedCardIndex = -1;
            _isPreviewOpen = false;
            ClearPreviewCards();
            SetPreviewVisible(false);
            SetConfirmInteractable(false);
            SetMessage(_param?.Message);
            BuildCardViews();
        }

        private void BuildPreviewCards(RuntimeCard sourceCard)
        {
            ClearPreviewCards();
            if (_previewContainer == null || sourceCard == null || !_upgradedCards.TryGetValue(sourceCard.Id, out RuntimeCard upgradedCard))
            {
                SetPreviewVisible(false);
                return;
            }

            SetPreviewVisible(true);
            CreatePreviewCard(sourceCard, new Vector2(-140f, 0f));
            CreatePreviewCard(upgradedCard, new Vector2(140f, 0f));
        }

        private void CreatePreviewCard(RuntimeCard card, Vector2 anchoredPosition)
        {
            BattleCardIconView cardView = Instantiate(_cardTemplate, _previewContainer);
            cardView.gameObject.SetActive(true);
            if (cardView.transform is RectTransform rectTransform)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = anchoredPosition;
                rectTransform.sizeDelta = new Vector2(220f, 320f);
            }

            cardView.Bind(card, true, false, null);
            _previewCardViews.Add(cardView);
        }

        private bool IsSelectedCard(int cardIndex)
        {
            return _selectedCardIndex >= 0 && _selectedCardIndex == cardIndex;
        }

        private bool CanConfirmCard(RuntimeCard card)
        {
            return card != null
                   && _upgradedCards.ContainsKey(card.Id)
                   && _cardPrices.TryGetValue(card.Id, out int price)
                   && _gold >= price;
        }

        private void SetPreviewVisible(bool isVisible)
        {
            if (_previewContainer != null)
            {
                _previewContainer.gameObject.SetActive(isVisible);
            }
        }

        private void SetConfirmVisible(bool isVisible)
        {
            if (_confirmButton != null)
            {
                _confirmButton.gameObject.SetActive(isVisible);
            }
        }

        private void SetConfirmInteractable(bool isInteractable)
        {
            if (_confirmButton != null)
            {
                _confirmButton.interactable = isInteractable;
            }
        }

        private void SetMessage(string message)
        {
            if (_messageText != null)
            {
                _messageText.text = message ?? string.Empty;
            }
        }

        private static void DestroyCardView(BattleCardIconView cardView)
        {
            if (cardView == null)
            {
                return;
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
    }
}
