using System;
using Dungeon.Runtime.OutGame.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Main.View
{
    public class LobbyFooterToggleGroup : MonoBehaviour
    {
        [Header("トグル")] [SerializeField] private Toggle _home;
        [SerializeField] private Toggle _shop;
        [SerializeField] private Toggle _history;

        [Header("Badges")] [SerializeField] private Badge _homeBadge;
        [SerializeField] private Badge _shopBadge;
        [SerializeField] private Badge _historyBadge;

        public Badge HomeBadge => _homeBadge;
        public Badge ShopBadge => _shopBadge;
        public Badge HistoryBadge => _historyBadge;

        public event Action<bool> onHomeToggled;
        public event Action<bool> onShopToggled;
        public event Action<bool> onHistoryToggled;

        private void Awake()
        {
            _home.onValueChanged.AddListener(OnHomeToggled);
            _shop.onValueChanged.AddListener(OnShopToggled);
            _history.onValueChanged.AddListener(OnHistoryToggled);
        }

        private void OnDestroy()
        {
            _home.onValueChanged.RemoveListener(OnHomeToggled);
            _shop.onValueChanged.RemoveListener(OnShopToggled);
            _history.onValueChanged.RemoveListener(OnHistoryToggled);
        }

        private void OnHomeToggled(bool isOn)
        {
            onHomeToggled?.Invoke(isOn);
        }

        private void OnShopToggled(bool isOn)
        {
            onShopToggled?.Invoke(isOn);
        }

        private void OnHistoryToggled(bool isOn)
        {
            onHistoryToggled?.Invoke(isOn);
        }
    }
}
