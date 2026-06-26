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
    /// 薬水交換ダイアログクラス
    /// </summary>
    public sealed class PotionReplaceDialog : UIDialogBase<PotionReplaceDialogResult>
    {
        [SerializeField] private Transform _ownedPotionRoot;
        [SerializeField] private BattleMultiIconView _ownedPotionTemplate;
        [SerializeField] private BattleMultiIconView _offeredPotionView;
        [SerializeField] private Button _cancelButton;

        private readonly List<BattleMultiIconView> _ownedPotionViews = new List<BattleMultiIconView>();
        private BattlePotionReplaceDialogParam _param;

        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            _param = param as BattlePotionReplaceDialogParam;
            return UniTask.CompletedTask;
        }

        protected override void OnOpened()
        {
            WireButtons();
            BuildOwnedPotions();
            BindOfferedPotion();
        }

        protected override void OnClosed()
        {
            UnwireButtons();
            ClearOwnedPotions();
            if (_offeredPotionView != null)
            {
                _offeredPotionView.Clear();
            }
        }

        private void BuildOwnedPotions()
        {
            ClearOwnedPotions();

            IReadOnlyList<BattleMultiIconViewModel> potions = _param?.Snapshot?.OwnedPotions;
            if (_ownedPotionRoot == null || _ownedPotionTemplate == null || potions == null)
            {
                return;
            }

            _ownedPotionTemplate.gameObject.SetActive(false);
            for (int i = 0; i < potions.Count; i++)
            {
                BattleMultiIconViewModel potion = potions[i];
                if (potion == null)
                {
                    continue;
                }

                int potionIndex = i;
                BattleMultiIconView potionView = Instantiate(_ownedPotionTemplate, _ownedPotionRoot);
                potionView.gameObject.SetActive(true);
                potionView.Bind(potion, () => OnPotionClicked(potionIndex));
                _ownedPotionViews.Add(potionView);
            }
        }

        private void BindOfferedPotion()
        {
            BattleMultiIconViewModel offeredPotion = _param?.Snapshot?.PendingPotionOffer?.Potion == null
                ? null
                : BattleMultiIconViewModel.CreatePotion(_param.Snapshot.PendingPotionOffer.Potion);
            _offeredPotionView?.Bind(offeredPotion, null);
        }

        private void ClearOwnedPotions()
        {
            for (int i = 0; i < _ownedPotionViews.Count; i++)
            {
                BattleMultiIconView potionView = _ownedPotionViews[i];
                if (potionView == null)
                {
                    continue;
                }

                potionView.Clear();
                Destroy(potionView.gameObject);
            }

            _ownedPotionViews.Clear();
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

        private void OnPotionClicked(int potionIndex)
        {
            CloseWithResult(new PotionReplaceDialogResult
            {
                IsCanceled = false,
                SelectedPotionIndex = potionIndex
            });
        }

        private void OnCancelClicked()
        {
            CloseWithResult(new PotionReplaceDialogResult
            {
                IsCanceled = true,
                SelectedPotionIndex = BattleSceneConstants.UnselectedCardIndex
            });
        }
    }
}
