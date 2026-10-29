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
        public string Name;
        [TextArea] public string Description;
        [TextArea] public string UpgradedDescription;
        public int Cost;
        public int UpgradedCost;
        public string Category;
        public CardRarity Rarity;
        public Sprite Artwork;
        public int StartingQuantity;
    }

    [Serializable]
    public sealed class MainMenuRelicData
    {
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
        public string Name;
        [TextArea] public string Description;
        [TextArea] public string CombatDescription;
        public Sprite Artwork;
        public bool IsUnlocked;
        public int Health = 80;
        public int Gold = 99;
    }

    /// <summary>
    /// メニューに必要な表示用スナップショット
    /// </summary>
    [Serializable]
    public sealed class MainMenuData
    {
        public bool HasResumableAdventure = true;
        public bool BgmEnabled = true;
        public bool SoundEffectEnabled;
        public string Version = "1.0";
        public string GameId = "123456789";
        public MainMenuCardData[] Cards = Array.Empty<MainMenuCardData>();
        public int CollectionCardCount = 11;
        public MainMenuRelicData[] Relics = Array.Empty<MainMenuRelicData>();
        public MainMenuCharacterData[] Characters = Array.Empty<MainMenuCharacterData>();
        public string ResultHeadline = "勝利……？";
        public int ReachedFloor = 8;
        public int EnemyCount = 7;
        public int EliteCount = 1;
        public int BossCount = 1;
    }
}
