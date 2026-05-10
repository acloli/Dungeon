using UnityEngine;

namespace Dungeon.Runtime.InGame.Domain
{
    [CreateAssetMenu(fileName = "CardDefinition", menuName = "Dungeon/InGame/Card Definition")]
    public class CardDefinition : ScriptableObject
    {
        [SerializeField] private string _cardId = "card_strike";
        [SerializeField] private string _displayName = "Strike";
        [SerializeField] private int _cost = 1;
        [SerializeField] private int _damage = 6;

        public string CardId => _cardId;
        public string DisplayName => _displayName;
        public int Cost => _cost;
        public int Damage => _damage;
    }
}
