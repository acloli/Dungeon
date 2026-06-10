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
    /// イベントダイアログ。事件テキストと選択肢ボタンを動的構築する
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
                SetTitle(string.Empty);
                SetBody(string.Empty);
                ClearChoiceButtons();
                return;
            }

            SetTitle(evt.EventName);
            SetBody(evt.LocalizationKey);
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
                    label.text = BuildChoiceLabel(choice);
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

        private void SetTitle(string text)
        {
            if (_titleText != null)
            {
                _titleText.text = text;
            }
        }

        private void SetBody(string text)
        {
            if (_bodyText != null)
            {
                _bodyText.text = text;
            }
        }

        private static string BuildChoiceLabel(RuntimeEventChoice choice)
        {
            string effectDesc = BuildEffectDescription(choice);
            if (string.IsNullOrEmpty(choice.LocalizationKey))
            {
                return effectDesc;
            }

            return string.IsNullOrEmpty(effectDesc)
                ? choice.LocalizationKey
                : $"{choice.LocalizationKey}  ({effectDesc})";
        }

        private static string BuildEffectDescription(RuntimeEventChoice choice)
        {
            return choice.EffectType switch
            {
                Game.MasterData.Generated.EffectType.LoseHp =>
                    $"-{choice.EffectValue} HP",
                Game.MasterData.Generated.EffectType.GainMaxHp =>
                    $"+{choice.EffectValue} MaxHP",
                Game.MasterData.Generated.EffectType.GainGold =>
                    $"+{choice.EffectValue} Gold",
                Game.MasterData.Generated.EffectType.DealDamage =>
                    $"-{choice.EffectValue} HP",
                _ => string.Empty
            };
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
