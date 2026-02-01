using UnityEngine;

namespace Script.Level
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Level/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Prefabs to spawn")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject exitPrefab;
        [SerializeField] private GameObject keyPrefab;
        [SerializeField] private GameObject orbPrefab;
        [SerializeField] private GameObject altarPrefab;

        [Header("Quantity data")] 
        [SerializeField] private int numberOfExitsToSpawn;
        [SerializeField] private int numberOfKeysToSpawn;
        [SerializeField] private int numberOfEnemiesToSpawn;
        [SerializeField] private int numberOfOrbsToSpawn;
        [SerializeField] private int numberOfAltarsToSpawn;
        
        public GameObject AltarPrefab => altarPrefab;
        public GameObject ExitPrefab => exitPrefab;
        public GameObject KeyPrefab => keyPrefab;
        public GameObject OrbPrefab => orbPrefab;
        public GameObject PlayerPrefab => playerPrefab;
        public GameObject EnemyPrefab => enemyPrefab;
        
        public int  NumberOfExitsToSpawn => numberOfExitsToSpawn;
        public int NumberOfKeysToSpawn => numberOfKeysToSpawn;
        public int NumberOfEnemiesToSpawn => numberOfEnemiesToSpawn;
        public int NumberOfOrbsToSpawn => numberOfOrbsToSpawn;
        public int NumberOfAltarsToSpawn => numberOfAltarsToSpawn;
    }
}