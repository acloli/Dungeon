using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// 薬水使用確認ダイアログクラス
    /// </summary>
    public sealed class PotionUseConfirmDialog : UIDialogBase<PotionUseConfirmDialogResult>
    {
        [SerializeField] private BattleMultiIconView _potionIconView;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        private BattlePotionUseConfirmDialogParam _param;

        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            _param = param as BattlePotionUseConfirmDialogParam;
            return UniTask.CompletedTask;
        }

        protected override void OnOpened()
        {
            WireButtons();
            BindPotion();
        }

        protected override void OnClosed()
        {
            UnwireButtons();
            if (_potionIconView != null)
            {
                _potionIconView.Clear();
            }
        }

        private void BindPotion()
        {
            BattleMultiIconViewModel potion = _param?.Snapshot?.OwnedPotions != null &&
                                              _param.Snapshot.SelectedOwnedPotionIndex >= 0 &&
                                              _param.Snapshot.SelectedOwnedPotionIndex < _param.Snapshot.OwnedPotions.Count
                ? _param.Snapshot.OwnedPotions[_param.Snapshot.SelectedOwnedPotionIndex]
                : null;
            _potionIconView?.Bind(potion, null);
        }

        private void WireButtons()
        {
            UnwireButtons();
            if (_confirmButton != null)
            {
                _confirmButton.onClick.AddListener(OnConfirmClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(OnCancelClicked);
            }
        }

        private void UnwireButtons()
        {
            if (_confirmButton != null)
            {
                _confirmButton.onClick.RemoveAllListeners();
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveAllListeners();
            }
        }

        private void OnConfirmClicked()
        {
            CloseWithResult(new PotionUseConfirmDialogResult { IsConfirmed = true });
        }

        private void OnCancelClicked()
        {
            CloseWithResult(new PotionUseConfirmDialogResult { IsConfirmed = false });
        }
    }
}
