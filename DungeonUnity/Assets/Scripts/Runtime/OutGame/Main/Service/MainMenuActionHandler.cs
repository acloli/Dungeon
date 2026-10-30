using System.Threading;
using Cysharp.Threading.Tasks;

namespace Dungeon.Runtime.OutGame.Main
{
    /// <summary>
    /// 未接続の外部操作を明示的に返す
    /// </summary>
    public sealed class MainMenuActionHandler : IMainMenuActionHandler
    {
        public UniTask<bool> ExecuteAsync(MainMenuAction action, CancellationToken ct)
        { ct.ThrowIfCancellationRequested(); return UniTask.FromResult(false); }
    }
}
