using UnityEngine;

namespace Dungeon.Runtime.InGame.Domain
{
    [CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Dungeon/InGame/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string _enemyId = "enemy_slime";
        [SerializeField] private string _displayName = "Slime";
        [SerializeField] private int _maxHp = 20;
        [SerializeField] private int _intentDamage = 4;

        public string EnemyId => _enemyId;
        public string DisplayName => _displayName;
        public int MaxHp => _maxHp;
        public int IntentDamage => _intentDamage;
    }
}
