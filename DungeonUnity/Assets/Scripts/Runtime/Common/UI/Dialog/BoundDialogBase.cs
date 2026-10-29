using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.Common.UI.Binding;
using TFramework.UI;

namespace Dungeon.Runtime.Common.UI.Dialog
{
    /// <summary>
    /// ダイアログの表示期間と入力接続の寿命を合わせる
    /// </summary>
    public abstract class BoundDialogBase : UIDialogBase
    {
        private IScreenBinding _binding;

        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            _binding?.Dispose();
            _binding = param as IScreenBinding;
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
}
