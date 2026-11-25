using System;
using UnityEngine;

namespace Dungeon.Runtime.Common.UI.Binding
{
    public interface IScreenBinding : IDisposable
    {
        void Apply(Component view);
    }

    /// <summary>
    /// 画面表示中だけ入力と表示の接続を保持
    /// </summary>
    public sealed class ScreenBinding<T> : IScreenBinding where T : Component
    {
        private readonly Func<T, IDisposable> _bind;
        private IDisposable _activeBinding;

        public ScreenBinding(Func<T, IDisposable> bind) => _bind = bind;

        public void Apply(Component view)
        {
            Dispose();
            _activeBinding = _bind((T)view);
        }

        public void Dispose()
        {
            _activeBinding?.Dispose();
            _activeBinding = null;
        }
    }
}
