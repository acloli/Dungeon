using System;
using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using UnityEngine;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// 報酬画面Viewクラス
    /// </summary>
    public sealed class RewardPageView : MonoBehaviour, IRewardPageView
    {
        [SerializeField] private Transform _rewardRoot;
        [SerializeField] private BattleOptionButtonView _rewardButtonTemplate;

        private readonly List<BattleOptionButtonView> _buttons = new List<BattleOptionButtonView>();

        /// <summary>
        /// 実行時参照補完
        /// </summary>
        private void Awake()
        {
            ResolveBindings();
        }

        /// <summary>
        /// エディタ参照補完
        /// </summary>
        private void OnValidate()
        {
            ResolveBindings();
        }

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

        /// <summary>
        /// 参照補完
        /// </summary>
        private void ResolveBindings()
        {
            _rewardRoot ??= BattleSceneViewBindingUtility.FindTransform("RewardRoot");
            _rewardButtonTemplate ??= BattleSceneViewBindingUtility.FindComponent<BattleOptionButtonView>("RewardButtonTemplate");
        }
    }
}
