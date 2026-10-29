using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TFramework.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Dungeon.Runtime.OutGame.Main.UI
{
    internal interface IMainMenuScreenBinding : IDisposable
    {
        void Apply(Component view);
    }

    /// <summary>
    /// 画面表示中だけ入力と表示の接続を保持
    /// </summary>
    internal sealed class MainMenuScreenBinding<T> : IMainMenuScreenBinding where T : Component
    {
        private readonly Func<T, IDisposable> _bind;
        private IDisposable _activeBinding;

        public MainMenuScreenBinding(Func<T, IDisposable> bind) => _bind = bind;

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

    /// <summary>
    /// 画面を閉じたときに登録した入力だけを解除
    /// </summary>
    internal sealed class MainMenuBindings : IDisposable
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

    /// <summary>
    /// ダイアログの表示期間と入力接続の寿命を合わせる
    /// </summary>
    public abstract class MainMenuDialogBase : UIDialogBase
    {
        private IMainMenuScreenBinding _binding;

        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            _binding?.Dispose();
            _binding = param as IMainMenuScreenBinding;
            _binding?.Apply(this);
            return UniTask.CompletedTask;
        }

        protected override void OnClosed() => ReleaseBinding();
        protected override void OnTerminate() => ReleaseBinding();

        private void ReleaseBinding()
        {
            _binding?.Dispose();
            _binding = null;
        }
    }

    /// <summary>
    /// キャッシュされたページを再表示するときも入力を接続
    /// </summary>
    public abstract class MainMenuPageBase : UIPageBase
    {
        private IMainMenuScreenBinding _binding;

        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            if (param is IMainMenuScreenBinding binding) _binding = binding;
            _binding?.Apply(this);
            return UniTask.CompletedTask;
        }

        protected override void OnClosed() => _binding?.Dispose();
        protected override void OnTerminate()
        {
            _binding?.Dispose();
            _binding = null;
        }
    }
}
