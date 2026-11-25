using System;
using System.Linq;
using UnityEngine;

namespace Dungeon.Runtime.OutGame.Main
{
    /// <summary>
    /// 表示用の画像ID対応と用語説明を管理
    /// </summary>
    [CreateAssetMenu(menuName = "Dungeon/UI/Menu Presentation Catalog")]
    public sealed class MenuPresentationCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class ArtworkEntry
        {
            public string ImageId;
            public Sprite Sprite;
        }

        public ArtworkEntry[] Artwork = Array.Empty<ArtworkEntry>();
        public Sprite MissingArtwork;
        public Sprite MissingPortrait;
        public MainMenuKeywordData[] KeywordDefinitions = Array.Empty<MainMenuKeywordData>();

        public Sprite FindArtwork(string id, bool portrait = false) =>
            Artwork.FirstOrDefault(entry => entry.ImageId == id)?.Sprite ??
            (portrait ? MissingPortrait : MissingArtwork);

        public MainMenuKeywordData[] FindKeywords(string description) => KeywordDefinitions
            .Where(entry => !string.IsNullOrEmpty(entry.Title) && (description ?? string.Empty).Contains(entry.Title))
            .ToArray();
    }
}
