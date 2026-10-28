using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Main.UI
{
    /// <summary>
    /// トグル入力に対応する二つの表示状態を切り替える
    /// </summary>
    public class ToggleStateView : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private GameObject _onState;
        [SerializeField] private GameObject _offState;

        public Toggle Toggle => _toggle;

        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(RefreshState);
            RefreshState(_toggle.isOn);
        }

        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(RefreshState);
        }

        private void RefreshState(bool isOn)
        {
            _onState.SetActive(isOn);
            _offState.SetActive(!isOn);
        }
    }
}
