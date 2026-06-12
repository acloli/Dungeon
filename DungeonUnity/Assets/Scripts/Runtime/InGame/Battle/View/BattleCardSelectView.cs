using System;
using Dungeon.Runtime.InGame.Battle.Model;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    public sealed class BattleCardSelectView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TFTextUGUI _nameText;
        [SerializeField] private TFTextUGUI _costText;

        public void Bind(RuntimeCard card, Action<RuntimeCard> onClick)
        {
            if (_nameText != null)
            {
                _nameText.text = card.DisplayName;
            }

            if (_costText != null)
            {
                _costText.text = card.Cost.ToString();
            }

            if (_button == null)
            {
                return;
            }

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke(card));
        }

        public void Clear()
        {
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
            }
        }
    }
}
