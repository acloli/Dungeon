using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Dungeon.Runtime.Common.UI.Binding
{
    /// <summary>
    /// 画面を閉じたときに登録した入力だけを解除
    /// </summary>
    public sealed class ScreenInputBindings : IDisposable
    {
        private readonly List<Action> _cleanup = new List<Action>();

        public void OnDispose(Action cleanup) => _cleanup.Add(cleanup);

        public void Add(Button button, Action action)
        {
            UnityAction listener = () => action();
            button.onClick.AddListener(listener);
            _cleanup.Add(() => { if (button != null) button.onClick.RemoveListener(listener); });
        }

        public void Add(Toggle toggle, Action<bool> action)
        {
            UnityAction<bool> listener = value => action(value);
            toggle.onValueChanged.AddListener(listener);
            _cleanup.Add(() => { if (toggle != null) toggle.onValueChanged.RemoveListener(listener); });
        }

        public void Dispose()
        {
            foreach (Action cleanup in _cleanup) cleanup();
            _cleanup.Clear();
        }
    }
}
