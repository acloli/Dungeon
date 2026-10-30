using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.MasterData.Generated;
using UnityEngine;

namespace Dungeon.Runtime.OutGame.Main
{
    /// <summary>
    /// 実データ取得を差し替えるためのメニュー表示境界
    /// </summary>
    public interface IMainMenuDataSource
    {
        UniTask<MainMenuData> LoadAsync(CancellationToken ct);
    }

    /// <summary>
    /// 保存や外部機能を伴う操作の接続境界
    /// </summary>
    public interface IMainMenuActionHandler
    {
        UniTask<bool> ExecuteAsync(MainMenuAction action, CancellationToken ct);
    }

    public enum MainMenuAction
    {
        StartAdventure, ResumeAdventure, Language, Account, ClearCache,
        Support, Credits, Logout, Terms, Privacy, Shop
    }

    [Serializable]
    public sealed class MainMenuCardData
    {
        public string Id;
        public int MasterId;
        public bool IsDiscovered = true;
        public bool HasUpgrade;
        public string UpgradedName;
        public MainMenuKeywordData[] Keywords = Array.Empty<MainMenuKeywordData>();
        public MainMenuKeywordData[] UpgradedKeywords = Array.Empty<MainMenuKeywordData>();
        public string Name;
        [TextArea] public string Description;
        [TextArea] public string UpgradedDescription;
        public int Cost;
        public int UpgradedCost;
        public string Category;
        public CardRarity Rarity;
        public Sprite Artwork;
    }

    [Serializable]
    public sealed class MainMenuKeywordData
    {
        public string Title;
        public string Description;
    }

    [Serializable]
    public sealed class MainMenuDeckEntry
    {
        public MainMenuCardData Card;
        public int Quantity;
    }

    [Serializable]
    public sealed class MainMenuRelicData
    {
        public int Id;
        public string Name;
        [TextArea] public string Description;
        public Sprite Artwork;
        [TextArea] public string EffectText;
        public Color FrameColor = Color.white;
        public bool IsDiscovered = true;
    }

    [Serializable]
    public sealed class MainMenuCharacterData
    {
        public int Id;
        public int RunProfileId;
        public MainMenuDeckEntry[] Deck = Array.Empty<MainMenuDeckEntry>();
        [TextArea] public string Biography;
        public string Name;
        [TextArea] public string Description;
        [TextArea] public string CombatDescription;
        public Sprite Artwork;
        public bool IsUnlocked;
        public int Health;
        public int Gold;
    }

    /// <summary>
    /// メニューに必要な表示用スナップショット
    /// </summary>
    [Serializable]
    public sealed class MainMenuData
    {
        public bool HasResumableAdventure;
        public bool BgmEnabled = true;
        public bool SoundEffectEnabled;
        public string Version = string.Empty;
        public string GameId = string.Empty;
        public MainMenuCardData[] Cards = Array.Empty<MainMenuCardData>();
        public MainMenuRelicData[] Relics = Array.Empty<MainMenuRelicData>();
        public MainMenuCharacterData[] Characters = Array.Empty<MainMenuCharacterData>();
        public string ResultHeadline = "冒険の記録はありません";
        public int ReachedFloor;
        public int EnemyCount;
        public int EliteCount;
        public int BossCount;
    }
}
