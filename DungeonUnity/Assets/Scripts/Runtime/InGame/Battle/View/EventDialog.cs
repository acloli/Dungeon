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
    /// イベントダイアログ
    /// 事件テキストと選択肢ボタンを動的構築する
    /// </summary>
    public sealed class EventDialog : UIDialogBase<EventDialogResult>
    {
        [SerializeField] private TFTextUGUI _titleText;
        [SerializeField] private TFTextUGUI _bodyText;
        [SerializeField] private Transform _choicesContainer;
        [SerializeField] private Button _choiceButtonTemplate;

        private readonly List<Button> _choiceButtons = new List<Button>();
        private BattleEventDialogParam _param;

        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            _param = param as BattleEventDialogParam;
            return UniTask.CompletedTask;
        }

        protected override void OnOpened()
        {
            BuildView();
        }

        protected override void OnClosed()
        {
            ClearChoiceButtons();
        }

        private void BuildView()
        {
            RuntimeEvent evt = _param?.Snapshot?.CurrentEvent;
            if (evt == null)
            {
                SetLocalizedText(_titleText, string.Empty, string.Empty);
                SetLocalizedText(_bodyText, string.Empty, string.Empty);
                ClearChoiceButtons();
                return;
            }

            SetLocalizedText(_titleText, evt.TitleKey, evt.EventName);
            SetLocalizedText(_bodyText, evt.DescriptionKey, evt.DescriptionKey);
            BuildChoiceButtons(evt.Choices);
        }

        private void BuildChoiceButtons(IReadOnlyList<RuntimeEventChoice> choices)
        {
            ClearChoiceButtons();

            if (_choicesContainer == null || _choiceButtonTemplate == null || choices == null)
            {
                return;
            }

            _choiceButtonTemplate.gameObject.SetActive(false);

            for (int i = 0; i < choices.Count; i++)
            {
                RuntimeEventChoice choice = choices[i];
                if (choice == null)
                {
                    continue;
                }

                Button btn = Instantiate(_choiceButtonTemplate, _choicesContainer);
                btn.gameObject.SetActive(true);

                TFTextUGUI label = btn.GetComponentInChildren<TFTextUGUI>();
                if (label != null)
                {
                    label.text = choice.LocalizationKey;
                }

                int capturedChoiceId = choice.ChoiceId;
                btn.onClick.AddListener(() => OnChoiceClicked(capturedChoiceId));
                _choiceButtons.Add(btn);
            }
        }

        private void ClearChoiceButtons()
        {
            for (int i = 0; i < _choiceButtons.Count; i++)
            {
                Button btn = _choiceButtons[i];
                if (btn == null)
                {
                    continue;
                }

                btn.onClick.RemoveAllListeners();
                Destroy(btn.gameObject);
            }

            _choiceButtons.Clear();
        }

        private static void SetLocalizedText(TFTextUGUI text, string localizationKey, string fallbackText)
        {
            if (text == null)
            {
                return;
            }
            text.text = fallbackText;
            text.LocalizationKey = localizationKey;
        }

        private void OnChoiceClicked(int choiceId)
        {
            CloseWithResult(new EventDialogResult
            {
                Action = EventDialogActionType.SelectChoice,
                ChoiceId = choiceId
            });
        }
    }
}