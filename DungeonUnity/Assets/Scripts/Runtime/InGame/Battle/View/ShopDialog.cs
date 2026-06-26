using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    public sealed class ShopDialog : UIDialogBase<ShopDialogResult>
    {
        [SerializeField] private Button _leaveButton;
        [SerializeField] private TFTextUGUI _goldText;
        [SerializeField] private Button _cardRemovalButton;
        [SerializeField] private TFTextUGUI _cardRemovalPriceText;
        [SerializeField] private Transform _shopItemsContainer;
        [SerializeField] private BattleShopItemView _shopItemTemplate;

        private readonly List<BattleShopItemView> _itemViews = new List<BattleShopItemView>();
        private BattleShopDialogParam _param;

        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            _param = param as BattleShopDialogParam;
            return UniTask.CompletedTask;
        }

        protected override void OnOpened()
        {
            WireButtons();
            UpdateView();
        }

        protected override void OnClosed()
        {
            UnwireButtons();
            ClearDynamicItems();
        }

        private void UpdateView()
        {
            if (_param?.Snapshot == null)
            {
                SetGoldText(string.Empty);
                SetCardRemovalState(BattleSceneConstants.EmptyValueLabel, false);
                ClearDynamicItems();
                return;
            }

            BattleShopSnapshot snapshot = _param.Snapshot;
            SetGoldText(snapshot.Gold.ToString());
            BuildCardRemovalView(snapshot);
            BuildShopItems(snapshot.ShopItems);
        }

        private void WireButtons()
        {
            UnwireButtons();

            if (_leaveButton != null)
            {
                _leaveButton.onClick.AddListener(OnLeaveClicked);
            }

            if (_cardRemovalButton != null)
            {
                _cardRemovalButton.onClick.AddListener(OnCardRemovalClicked);
            }
        }

        private void UnwireButtons()
        {
            if (_leaveButton != null)
            {
                _leaveButton.onClick.RemoveAllListeners();
            }

            if (_cardRemovalButton != null)
            {
                _cardRemovalButton.onClick.RemoveAllListeners();
            }
        }

        private void BuildCardRemovalView(BattleShopSnapshot snapshot)
        {
            if (snapshot.IsCardRemovalSoldOut)
            {
                SetCardRemovalState(BattleSceneConstants.SoldOutLabel, false);
                return;
            }

            bool canPurchase = snapshot.Gold >= snapshot.CardRemovalPrice;
            SetCardRemovalState(snapshot.CardRemovalPrice.ToString(), canPurchase);
        }

        private void BuildShopItems(IReadOnlyList<BattleShopItemViewModel> items)
        {
            ClearDynamicItems();

            if (_shopItemsContainer == null || _shopItemTemplate == null || items == null)
            {
                return;
            }

            _shopItemTemplate.gameObject.SetActive(false);

            for (int i = 0; i < items.Count; i++)
            {
                BattleShopItemViewModel item = items[i];
                if (item == null)
                {
                    continue;
                }

                BattleShopItemView itemView = Instantiate(_shopItemTemplate, _shopItemsContainer);
                itemView.gameObject.SetActive(true);
                itemView.Bind(
                    item,
                    slotIndex => CloseWithResult(new ShopDialogResult
                    {
                        Action = ShopDialogActionType.PurchaseItem,
                        SlotIndex = slotIndex
                    }));
                _itemViews.Add(itemView);
            }
        }

        private void ClearDynamicItems()
        {
            for (int i = 0; i < _itemViews.Count; i++)
            {
                BattleShopItemView itemView = _itemViews[i];
                if (itemView == null)
                {
                    continue;
                }

                itemView.Clear();
                Destroy(itemView.gameObject);
            }

            _itemViews.Clear();
        }

        private void SetGoldText(string label)
        {
            if (_goldText != null)
            {
                _goldText.text = label;
            }
        }

        private void SetCardRemovalState(string label, bool interactable)
        {
            if (_cardRemovalPriceText != null)
            {
                _cardRemovalPriceText.text = label;
            }

            if (_cardRemovalButton != null)
            {
                _cardRemovalButton.interactable = interactable;
            }
        }

        private void OnLeaveClicked()
        {
            CloseWithResult(new ShopDialogResult { Action = ShopDialogActionType.Leave });
        }

        private void OnCardRemovalClicked()
        {
            CloseWithResult(new ShopDialogResult { Action = ShopDialogActionType.PurchaseCardRemoval });
        }
    }
}
