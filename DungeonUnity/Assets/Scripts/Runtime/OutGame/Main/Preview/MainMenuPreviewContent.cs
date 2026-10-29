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
}
