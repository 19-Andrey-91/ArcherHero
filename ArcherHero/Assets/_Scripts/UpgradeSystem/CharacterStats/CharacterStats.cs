using System.Collections.Generic;
using UnityEngine;

namespace PlayerStats
{

    [CreateAssetMenu(fileName = "CharacterStats", menuName = "ScriptableObjects/CharacterStatsUpgrades")]
    public class CharacterStats : ScriptableObject
    {
        [SerializeField] private StatInfo _maxHP;
        [SerializeField] private StatInfo _damage;
        [SerializeField] private StatInfo _attackSpeed;
        [SerializeField] private StatInfo _movementSpeed;

        public StatInfo MaxHP => _maxHP;
        public StatInfo Damage => _damage;
        public StatInfo AttackSpeed => _attackSpeed;
        public StatInfo MovementSpeed => _movementSpeed;

        public IEnumerable<StatInfo> Stats => Stats ?? new List<StatInfo>()
        {
            _maxHP,
            _damage,
            _attackSpeed,
            _movementSpeed,
        };
    }
}