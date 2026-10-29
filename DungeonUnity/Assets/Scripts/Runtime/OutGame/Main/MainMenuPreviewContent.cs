using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Dungeon.Runtime.OutGame.Main
{
    /// <summary>
    /// メニュー確認用
    /// </summary>
    [CreateAssetMenu(menuName = "Dungeon/UI/Main Menu Preview Content")]
    public sealed class MainMenuPreviewContent : ScriptableObject
    {
        [SerializeField] private MainMenuData _data = new MainMenuData();
        public MainMenuData Data => _data;
    }

    /// <summary>
    /// ゲーム進行を変更せず確認用データだけを提供
    /// </summary>
    public sealed class MainMenuPreviewService : IMainMenuDataSource, IMainMenuActionHandler
    {
        private readonly MainMenuPreviewContent _content;

        public MainMenuPreviewService(MainMenuPreviewContent content) => _content = content;

        public UniTask<MainMenuData> LoadAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (_content == null) throw new InvalidOperationException("MainMenuPreviewContent is missing");
            return UniTask.FromResult(_content.Data);
        }

        public UniTask<bool> ExecuteAsync(MainMenuAction action, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return UniTask.FromResult(false);
        }
    }
}
