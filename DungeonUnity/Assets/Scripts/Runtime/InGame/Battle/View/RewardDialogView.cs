using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using TFramework.UI;
using UnityEngine;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// 報酬ダイアログViewクラス
    /// </summary>
    public sealed class RewardDialogView : UIDialogBase<CardDefinition>, IRewardDialogView
    {
        [SerializeField] private Transform _rewardRoot;
        [SerializeField] private BattleOptionButtonView _rewardButtonTemplate;

        private readonly List<BattleOptionButtonView> _buttons = new List<BattleOptionButtonView>();
        private BattleRewardDialogParam _param;

        /// <summary>
        /// 報酬ボタン構築
        /// </summary>
        public void BuildRewardButtons(IReadOnlyList<CardDefinition> cards, Action<CardDefinition> onClicked)
        {
            ClearDynamicButtons();

            if (_rewardRoot == null || _rewardButtonTemplate == null || cards == null)
            {
                return;
            }

            _rewardButtonTemplate.gameObject.SetActive(false);

            for (int i = 0; i < cards.Count; i++)
            {
                CardDefinition card = cards[i];
                BattleOptionButtonView button = Instantiate(_rewardButtonTemplate, _rewardRoot);
                button.gameObject.SetActive(true);
                button.Configure(
                    string.Format(BattleSceneConstants.RewardLabelFormat, card.DisplayName, card.Cost, card.Damage),
                    delegate
                    {
                        onClicked?.Invoke(card);
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

            BuildRewardButtons(_param.Snapshot.RewardChoices, card => CloseWithResult(card));
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
