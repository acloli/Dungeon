using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using TFramework.UI;
using UnityEngine;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// 報酬ダイアログクラス
    /// </summary>
    public sealed class RewardDialog : UIDialogBase<RuntimeRewardEntry>, IRewardDialogView
    {
        [SerializeField] private Transform _rewardRoot;
        [SerializeField] private BattleOptionButtonView _rewardButtonTemplate;

        private readonly List<BattleOptionButtonView> _buttons = new List<BattleOptionButtonView>();
        private BattleRewardDialogParam _param;

        /// <summary>
        /// 報酬ボタン構築
        /// </summary>
        public void BuildRewardButtons(IReadOnlyList<RuntimeRewardEntry> entries, Action<RuntimeRewardEntry> onClicked)
        {
            ClearDynamicButtons();

            if (_rewardRoot == null || _rewardButtonTemplate == null || entries == null)
            {
                return;
            }

            _rewardButtonTemplate.gameObject.SetActive(false);

            for (int i = 0; i < entries.Count; i++)
            {
                RuntimeRewardEntry entry = entries[i];
                BattleOptionButtonView button = Instantiate(_rewardButtonTemplate, _rewardRoot);
                button.gameObject.SetActive(true);

                string label = string.Empty;
                if (entry.RewardType == Game.MasterData.Generated.RewardType.Card && entry.Card != null)
                {
                    label = string.Format(BattleSceneConstants.RewardLabelFormat, entry.Card.DisplayName, entry.Card.Cost, entry.Card.PreviewDamage);
                }
                else if (entry.RewardType == Game.MasterData.Generated.RewardType.Gold)
                {
                    label = $"{entry.RewardValue} Gold";
                }
                else
                {
                    label = $"{entry.RewardType}";
                }

                button.Configure(
                    label,
                    delegate
                    {
                        onClicked?.Invoke(entry);
                    });
                _buttons.Add(button);
            }
        }

        /// <summary>
        /// 動的報酬ボタン消去
        /// </summary>
        public void ClearDynamicButtons()
        {
            ClearButtons(_buttons);
        }

        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            _param = param as BattleRewardDialogParam;
            return UniTask.CompletedTask;
        }

        protected override void OnOpened()
        {
            if (_param == null)
            {
                ClearDynamicButtons();
                return;
            }

            BuildRewardButtons(_param.Snapshot.RewardChoices, entry => CloseWithResult(entry));
        }

        protected override void OnClosed()
        {
            ClearDynamicButtons();
        }

        /// <summary>
        /// 動的ボタン一覧消去
        /// </summary>
        private static void ClearButtons(List<BattleOptionButtonView> buttons)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                BattleOptionButtonView button = buttons[i];
                if (button == null)
                {
                    continue;
                }

                button.Clear();
                Destroy(button.gameObject);
            }

            buttons.Clear();
        }
    }
}
