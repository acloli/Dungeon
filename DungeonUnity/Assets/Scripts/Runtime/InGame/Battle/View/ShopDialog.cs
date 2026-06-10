using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using TMPro;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    public sealed class ShopDialog : UIDialogBase<ShopDialogResult>
    {
        [SerializeField] private Button _leaveButton;
        [SerializeField] private TMP_Text _goldText;
        [SerializeField] private Button _cardRemovalButton;
        [SerializeField] private TMP_Text _cardRemovalPriceText;

        // アイテム表示用のプレハブ等があればここに追加

        private BattleShopDialogParam _param;

        private void Awake()
        {
            _leaveButton.onClick.AddListener(() =>
            {
                CloseWithResult(new ShopDialogResult { Action = ShopDialogActionType.Leave });
            });

            _cardRemovalButton.onClick.AddListener(() =>
            {
                CloseWithResult(new ShopDialogResult { Action = ShopDialogActionType.PurchaseCardRemoval });
            });
        }

        protected override Cysharp.Threading.Tasks.UniTask OnPreOpenAsync(object param, System.Threading.CancellationToken ct)
        {
            _param = (BattleShopDialogParam)param;
            UpdateView();
            return Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }

        private void UpdateView()
        {
            if (_param == null || _param.Snapshot == null) return;
            var snapshot = _param.Snapshot;
            _goldText.text = snapshot.Gold.ToString();

            // カード削除
            if (snapshot.IsCardRemovalSoldOut)
            {
                _cardRemovalPriceText.text = "Sold Out";
                _cardRemovalButton.interactable = false;
            }
            else
            {
                _cardRemovalPriceText.text = snapshot.CardRemovalPrice.ToString();
                _cardRemovalButton.interactable = snapshot.Gold >= snapshot.CardRemovalPrice;
            }

            // TODO: アイテムの描画
        }
    }
}
