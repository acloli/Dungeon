using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;
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
        [SerializeField] private Sprite _goldRewardIcon;

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
                if (entry == null)
                {
                    continue;
                }

                BattleOptionButtonView button = Instantiate(_rewardButtonTemplate, _rewardRoot);
                button.gameObject.SetActive(true);

                string label = BuildRewardLabel(entry);

                button.Configure(
                    label,
                    delegate
                    {
                        onClicked?.Invoke(entry);
                    });
                button.SetIcon(BuildRewardIcon(entry));
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
        /// 報酬ボタンの表示文言を組み立てる
        /// </summary>
        private static string BuildRewardLabel(RuntimeRewardEntry entry)
        {
            if (entry.RewardType == RewardType.Card)
            {
                if (entry.Card != null)
                {
                    return string.Format(BattleSceneConstants.RewardLabelFormat, entry.Card.DisplayName, entry.Card.Cost, entry.Card.PreviewDamage);
                }

                return RewardType.Card.ToString();
            }

            if (entry.RewardType == RewardType.Gold)
            {
                return $"{entry.RewardValue} Gold";
            }

            return entry.RewardType.ToString();
        }

        /// <summary>
        /// 報酬タイプごとのアイコンを返す
        /// </summary>
        private Sprite BuildRewardIcon(RuntimeRewardEntry entry)
        {
            if (entry.RewardType == RewardType.Gold)
            {
                return _goldRewardIcon;
            }

            return null;
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
