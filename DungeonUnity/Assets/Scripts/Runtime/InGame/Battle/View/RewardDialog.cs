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
    /// 報酬ダイアログクラス
    /// </summary>
    public sealed class RewardDialog : UIDialogBase<RewardDialogResult>
    {
        [SerializeField] private Transform _rewardRoot;
        [SerializeField] private BattleOptionButtonView _rewardButtonTemplate;
        [SerializeField] private Button _continueButton;

        private readonly List<BattleOptionButtonView> _rows = new List<BattleOptionButtonView>();
        private BattleRewardDialogParam _param;

        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            _param = param as BattleRewardDialogParam;
            return UniTask.CompletedTask;
        }

        protected override void OnOpened()
        {
            WireButtons();
            BuildRows();
        }

        protected override void OnClosed()
        {
            UnwireButtons();
            ClearRows();
        }

        private void BuildRows()
        {
            ClearRows();

            BattleSceneSnapshot snapshot = _param?.Snapshot;
            if (snapshot == null || _rewardRoot == null || _rewardButtonTemplate == null) return;

            _rewardButtonTemplate.gameObject.SetActive(false);

            // Gold行 — クリックで ClaimGold を返し、Presenter 側で状態更新＋再表示
            if (!snapshot.GoldClaimed)
                AddRow(string.Format(BattleSceneConstants.RewardGoldFormat, snapshot.BattleGoldReward),
                    () => CloseWithResult(new RewardDialogResult { Action = RewardDialogActionType.ClaimGold }),
                    true);

            // Card行 — クリックで PickCard、選択済みなら非表示
            if (!snapshot.CardRewardPicked)
                AddRow(BattleSceneConstants.PickCardLabel,
                    () => CloseWithResult(new RewardDialogResult { Action = RewardDialogActionType.PickCard }),
                    true);
            else
                AddRow(BattleSceneConstants.CardPickedLabel, null, false);

            // Potion行
            if (snapshot.PotionDropped && !snapshot.PotionClaimed)
                AddRow(snapshot.PendingPotionReward != null ? snapshot.PendingPotionReward.DisplayName : BattleSceneConstants.PotionDroppedLabel,
                    () => CloseWithResult(new RewardDialogResult { Action = RewardDialogActionType.ClaimPotion }),
                    true);

            // Relic行
            if (snapshot.RelicDropped && !snapshot.RelicClaimed)
                AddRow(snapshot.PendingRelicReward != null ? snapshot.PendingRelicReward.DisplayName : BattleSceneConstants.RelicDroppedLabel,
                    () => CloseWithResult(new RewardDialogResult { Action = RewardDialogActionType.ClaimRelic }),
                    true);
        }

        private void AddRow(string label, System.Action onClicked, bool interactable = false)
        {
            BattleOptionButtonView row = Instantiate(_rewardButtonTemplate, _rewardRoot);
            row.gameObject.SetActive(true);
            if (interactable && onClicked != null)
            {
                row.Configure(label, () => onClicked.Invoke());
            }
            else
            {
                row.Configure(label, null);
            }
            _rows.Add(row);
        }

        private void ClearRows()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                BattleOptionButtonView row = _rows[i];
                if (row == null) continue;
                row.Clear();
                Destroy(row.gameObject);
            }
            _rows.Clear();
        }

        private void WireButtons()
        {
            UnwireButtons();
            if (_continueButton != null)
                _continueButton.onClick.AddListener(OnContinueClicked);
        }

        private void UnwireButtons()
        {
            if (_continueButton != null)
                _continueButton.onClick.RemoveAllListeners();
        }

        private void OnContinueClicked()
        {
            CloseWithResult(new RewardDialogResult { Action = RewardDialogActionType.Continue });
        }
    }
}
