namespace Dungeon.Runtime.OutGame.Main
{
    /// <summary>
    /// MainSceneRunProfile表示モデル
    /// </summary>
    public sealed class MainRunProfileViewModel
    {
        public MainRunProfileViewModel(
            int id,
            string key,
            string displayName,
            string localizationKey,
            string characterArchetype,
            int playerMaxHp,
            int startingGold)
        {
            Id = id;
            Key = key;
            DisplayName = displayName;
            LocalizationKey = localizationKey;
            CharacterArchetype = characterArchetype;
            PlayerMaxHp = playerMaxHp;
            StartingGold = startingGold;
        }

        public int Id { get; }
        public string Key { get; }
        public string DisplayName { get; }
        public string LocalizationKey { get; }
        public string CharacterArchetype { get; }
        public int PlayerMaxHp { get; }
        public int StartingGold { get; }
    }
}
