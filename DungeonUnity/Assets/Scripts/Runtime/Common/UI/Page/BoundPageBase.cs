using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.Common.UI.Binding;
using TFramework.UI;

namespace Dungeon.Runtime.Common.UI.Page
{
    /// <summary>
    /// キャッシュされたページを再表示するときも入力を接続
    /// </summary>
    public abstract class BoundPageBase : UIPageBase
    {
        private IScreenBinding _binding;

        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            if (param is IScreenBinding binding) _binding = binding;
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
