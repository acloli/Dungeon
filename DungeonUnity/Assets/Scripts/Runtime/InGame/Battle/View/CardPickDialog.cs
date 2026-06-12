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
    /// 報酬画面用カード選択ダイアログ
    /// </summary>
    public sealed class CardPickDialog : UIDialogBase<RuntimeRewardEntry>
    {
        [SerializeField] private Transform _cardContainer;
        [SerializeField] private BattleOptionButtonView _cardButtonTemplate;
        [SerializeField] private Button _backButton;

        private readonly List<BattleOptionButtonView> _buttons = new List<BattleOptionButtonView>();
        private BattleCardPickDialogParam _param;

        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            _param = param as BattleCardPickDialogParam;
            return UniTask.CompletedTask;
        }

        protected override void OnOpened()
        {
            WireBackButton();
            BuildCardButtons();
        }

        protected override void OnClosed()
        {
            UnwireBackButton();
            ClearDynamicButtons();
        }

        private void WireBackButton()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackClicked);
        }

        private void UnwireBackButton()
        {
            if (_backButton != null)
                _backButton.onClick.RemoveAllListeners();
        }

        private void OnBackClicked()
        {
            CloseWithResult(null);
        }

        private void BuildCardButtons()
        {
            ClearDynamicButtons();

            IReadOnlyList<RuntimeRewardEntry> entries = _param?.Snapshot?.RewardChoices;
            if (_cardContainer == null || _cardButtonTemplate == null || entries == null)
            {
                return;
            }

            _cardButtonTemplate.gameObject.SetActive(false);

            for (int i = 0; i < entries.Count; i++)
            {
                RuntimeRewardEntry entry = entries[i];
                if (entry?.Card == null)
                {
                    continue;
                }

                BattleOptionButtonView button = Instantiate(_cardButtonTemplate, _cardContainer);
                button.gameObject.SetActive(true);

                string label = string.Format(
                    BattleSceneConstants.RewardLabelFormat,
                    entry.Card.DisplayName,
                    entry.Card.Cost,
                    entry.Card.PreviewDamage);

                RuntimeRewardEntry captured = entry;
                button.Configure(label, () => CloseWithResult(captured));
                _buttons.Add(button);
            }
        }

        private void ClearDynamicButtons()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                BattleOptionButtonView btn = _buttons[i];
                if (btn == null) continue;
                btn.Clear();
                Destroy(btn.gameObject);
            }
            _buttons.Clear();
        }
    }
}