using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dungeon.Runtime.InGame.Save.Model;
using Game.MasterData.Generated;
using TFramework.Localization;
using TFramework.MasterData;
using UnityEngine;

namespace Dungeon.Runtime.OutGame.Main
{
    /// <summary>
    /// MasterDataと保存済みの解放情報をメニュー表示用に組立
    /// </summary>
    public sealed class MenuMasterDataFacade
    {
        private readonly IMasterDataService _master;
        private readonly ILocalizationService _localization;
        private readonly MenuPresentationCatalog _presentation;

        public MenuMasterDataFacade(IMasterDataService master, MenuPresentationCatalog presentation,
            ILocalizationService localization = null)
        {
            _master = master;
            _presentation = presentation;
            _localization = localization;
        }

        public MainMenuData Build(CollectionProgressData progress)
        {
            var masters = _master.GetAll<CardMaster>().OrderBy(card => card.Id).ToArray();
            var cards = masters.ToDictionary(card => card.Id, card => Card(card));
            var upgradedIds =
                new HashSet<int>(masters.Where(card => card.UpgradeCardId > 0).Select(card => card.UpgradeCardId));
            var profiles = _master.GetAll<RunProfileMaster>().ToDictionary(profile => profile.Id);
            var characters = _master.GetAll<CharacterMaster>().OrderBy(character => character.Id).ToArray();
            int defaultId = characters.FirstOrDefault(character => profiles.ContainsKey(character.RunProfileId))?.Id ??
                            0;
            var knownCards = new HashSet<int>(progress.CardIds ?? new List<int>());
            var unlocked = new HashSet<int>(progress.CharacterIds ?? new List<int>()) { defaultId };
            var decks = _master.GetAll<DeckGroupMaster>();
            var characterViews = characters.Select(character =>
            {
                profiles.TryGetValue(character.RunProfileId, out RunProfileMaster profile);
                bool available = profile != null && unlocked.Contains(character.Id);
                var deck = profile == null
                    ? Array.Empty<MainMenuDeckEntry>()
                    : decks
                        .Where(entry =>
                            entry.DeckGroupId == profile.StarterDeckGroupId && entry.Count > 0 &&
                            cards.ContainsKey(entry.CardId))
                        .OrderBy(entry => entry.Order).ThenBy(entry => entry.Id)
                        .Select(entry => new MainMenuDeckEntry { Card = cards[entry.CardId], Quantity = entry.Count })
                        .ToArray();
                if (available)
                    foreach (var entry in deck)
                        knownCards.Add(entry.Card.MasterId);
                return new MainMenuCharacterData
                {
                    Id = character.Id, RunProfileId = profile?.Id ?? 0,
                    Name = Text(character.LocalizationKey, character.Name),
                    Description = Text(character.DescriptionKey, string.Empty),
                    CombatDescription = Text(character.BiographyKey, string.Empty),
                    Artwork = _presentation.FindArtwork(character.PortraitImageId, true),
                    IsUnlocked = available, Health = profile?.PlayerMaxHp ?? 0, Gold = profile?.StartingGold ?? 0,
                    Deck = deck
                };
            }).ToArray();
            foreach (var master in masters)
            {
                MainMenuCardData card = cards[master.Id];
                card.IsDiscovered = knownCards.Contains(master.Id) ||
                                    (master.UpgradeCardId > 0 && knownCards.Contains(master.UpgradeCardId));
                if (cards.TryGetValue(master.UpgradeCardId, out MainMenuCardData upgrade))
                {
                    card.HasUpgrade = true;
                    card.UpgradedName = upgrade.Name;
                    card.UpgradedDescription = upgrade.Description;
                    card.UpgradedCost = upgrade.Cost;
                    card.UpgradedKeywords = upgrade.Keywords;
                }
                else
                {
                    card.HasUpgrade = false;
                    card.UpgradedName = card.Name;
                    card.UpgradedDescription = card.Description;
                    card.UpgradedCost = card.Cost;
                }
            }

            var collection = masters.Where(card => !upgradedIds.Contains(card.Id)).Select(card => cards[card.Id])
                .ToArray();
            return new MainMenuData
            {
                Cards = collection, Characters = characterViews,
                Relics = _master.GetAll<RelicMaster>().OrderBy(relic => relic.Id).Select(relic => new MainMenuRelicData
                {
                    Id = relic.Id, Name = Text(relic.LocalizationKey, relic.Name), Description = string.Empty,
                    EffectText = Text(relic.DescriptionKey, string.Empty),
                    Artwork = _presentation.FindArtwork(relic.ImageId),
                    IsDiscovered = progress.RelicIds?.Contains(relic.Id) == true, FrameColor = RarityColor(relic.Rarity)
                }).ToArray(),
                Version = Application.version
            };
        }

        private MainMenuCardData Card(CardMaster card)
        {
            string description = Text(card.DescriptionKey, string.Empty);
            return new MainMenuCardData
            {
                Id = card.Id.ToString(CultureInfo.InvariantCulture), MasterId = card.Id,
                Name = Text(card.LocalizationKey, card.Name), Description = description, Cost = card.Cost,
                Category = card.CardType == CardType.Attack ? "アタック" :
                    card.CardType == CardType.Skill ? "スキル" :
                    card.CardType == CardType.Power ? "パワー" : string.Empty,
                Rarity = card.Rarity, Artwork = _presentation.FindArtwork(card.ImageId),
                Keywords = _presentation.FindKeywords(description)
            };
        }

        private string Text(string key, string fallback) =>
            _localization != null && !string.IsNullOrEmpty(key) && _localization.HasKey(key)
                ? _localization.Get(key)
                : fallback ?? string.Empty;

        private static Color RarityColor(CardRarity rarity) => rarity == CardRarity.Rare
            ?
            new Color(0.95f, 0.8f, 0.3f)
            : rarity == CardRarity.Uncommon
                ? new Color(0.3f, 0.63f, 0.91f)
                : new Color(0.36f, 0.33f, 0.5f);
    }
}
