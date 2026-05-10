using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Runtime.InGame.Domain
{
    [CreateAssetMenu(fileName = "RunStartConfig", menuName = "Dungeon/InGame/Run Start Config")]
    public class RunStartConfig : ScriptableObject
    {
        [SerializeField] private int _playerMaxHp = 40;
        [SerializeField] private int _startingGold = 100;
        [SerializeField] private MapTemplate _mapTemplate;
        [SerializeField] private List<CardDefinition> _starterDeck = new List<CardDefinition>();
        [SerializeField] private List<CardDefinition> _rewardPool = new List<CardDefinition>();
        [SerializeField] private EnemyDefinition _normalEnemy;
        [SerializeField] private EnemyDefinition _eliteEnemy;
        [SerializeField] private EnemyDefinition _bossEnemy;

        public int PlayerMaxHp => _playerMaxHp;
        public int StartingGold => _startingGold;
        public MapTemplate MapTemplate => _mapTemplate;
        public IReadOnlyList<CardDefinition> StarterDeck => _starterDeck;
        public IReadOnlyList<CardDefinition> RewardPool => _rewardPool;
        public EnemyDefinition NormalEnemy => _normalEnemy;
        public EnemyDefinition EliteEnemy => _eliteEnemy;
        public EnemyDefinition BossEnemy => _bossEnemy;
    }
}
