using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Script.Level
{
    public class SpawnManager : MonoBehaviour
    {
        [SerializeField] private LevelConfig levelConfig;
        private IMaze _maze;

        [SerializeField] private int numberOfEnemiesToSpawn;
        
        private List<Tile> _occupiedTiles = new();
        private List<Tile> _enemyTiles = new();
        private List<Tile> _keyTiles = new();
        private List<Tile> _orbTiles = new();

        private Tile _exitTile;
        private Tile _playerTile;
        private Tile _altarTile;
        
        public void InitializeMaze(IMaze inMaze)
        {
            _maze = inMaze;
        }
        
        #region Spawn flow

        public void SpawnAll()
        {
            SpawnAltar();     
            SpawnExit();      
            SpawnOrbs();      
            SpawnKeys();      
            SpawnPlayer();    
            SpawnEnemies();
        }

        public void SpawnOnlyEnemiesSigh()
        {
            _playerTile = _maze.GetTile(0,0);
            _occupiedTiles.Add(_playerTile);
            RandomFill();
            //SpawnEnemies();
            RandomEnemySpawn();
        }

        #endregion
        
        #region Spawn Methods

        private void Spawn(GameObject prefab, Tile tile)
        {
            Instantiate(prefab, _maze.TileToWorld(tile), Quaternion.identity);
            _occupiedTiles.Add(tile);
        }

        private void SpawnEnemy(GameObject prefab, Tile tile)
        {
            Vector3 pos = _maze.TileToWorld(tile);
            GameObject temp = Instantiate(prefab, pos, Quaternion.identity);
            Enemy enemy = temp.GetComponent<Enemy>();
            enemy.InitializeMazeData(_maze);
        }
        
        private void SpawnAltar()
        {
            _altarTile = PickCentralTile();
            _occupiedTiles.Add(_altarTile); 
            Spawn(levelConfig.AltarPrefab, _altarTile);
        }
        
        private void SpawnExit()
        {
            _exitTile = PickBestTile(t =>
                PathDistance(t, _altarTile)
            );

            _occupiedTiles.Add(_exitTile);
            Spawn(levelConfig.ExitPrefab, _exitTile);
        }

        private void RandomFill()
        {
            List<Tile> freeTiles = new(GetFreeTiles());

            int count = Mathf.Min(4, freeTiles.Count);

            for (int i = 0; i < count; i++)
            {
                int index = Random.Range(0, freeTiles.Count);
                Tile t = freeTiles[index];

                _occupiedTiles.Add(t);
                freeTiles.RemoveAt(index); // evita duplicati
            }
        }
        
        private void RandomEnemySpawn()
        {
            List<Tile> freeTiles = new(GetFreeTiles());

            int count = Mathf.Min(10, freeTiles.Count);

            for (int i = 0; i < count; i++)
            {
                int index = Random.Range(0, freeTiles.Count);
                Tile t = freeTiles[index];

                _occupiedTiles.Add(t);
                freeTiles.RemoveAt(index);
                SpawnEnemy(levelConfig.EnemyPrefab, t);
            }
        }
        
        private void SpawnOrbs()
        {
            for (int i = 0; i < levelConfig.NumberOfOrbsToSpawn; i++)
            {
                Tile t = PickBestTile(tile =>
                {
                    int d = PathDistance(tile, _altarTile);
                    if (d < 4) return -1000;
                    return d;
                });

                _orbTiles.Add(t);
                _occupiedTiles.Add(t); 
                Spawn(levelConfig.OrbPrefab, t);
            }
        }
        
        private void SpawnKeys()
        {
            for (int i = 0; i < levelConfig.NumberOfKeysToSpawn; i++)
            {
                Tile t = PickBestTile(tile =>
                {
                    int d = PathDistance(tile, _exitTile);
                    if (d < 5) return -1000;
                    return d;
                });

                _keyTiles.Add(t);
                _occupiedTiles.Add(t); 
                Spawn(levelConfig.KeyPrefab, t);
            }
        }

        private void SpawnPlayer()
        {
            _playerTile = PickBestTile(tile =>
                PathDistance(tile, _exitTile) +
                PathDistance(tile, _altarTile) +
                DistanceFromAll(tile, _keyTiles)
            );

            Spawn(levelConfig.PlayerPrefab, _playerTile);
        }
        
        private void SpawnEnemies()
        {
            for (int i = 0; i < levelConfig.NumberOfEnemiesToSpawn; i++)
            {
                Tile t = PickBestTile(tile =>
                {
                    int d = PathDistance(tile, _playerTile);
                    if (d < 5) return -1000;
                    return d;
                });

                _enemyTiles.Add(t);
                _occupiedTiles.Add(t); 
                SpawnEnemy(levelConfig.EnemyPrefab, t);
            }
        }
        
        #endregion
        
        #region Heuristics
        
        private Tile PickBestTile(Func<Tile, float> scoreFunc)
        {
            Tile best = null;
            float bestScore = float.MinValue;

            foreach (Tile t in GetFreeTiles())
            {
                float s = scoreFunc(t);
                if (s > bestScore)
                {
                    bestScore = s;
                    best = t;
                }
            }
            return best;
        }
        
        // minimize central distance from the others
        private Tile PickCentralTile()
        {
            Tile best = null;
            float bestScore = float.MaxValue;

            foreach (Tile t in GetFreeTiles())
            {
                float sum = 0;

                foreach (Tile other in GetFreeTiles())
                    sum += PathDistance(t, other);

                if (sum < bestScore)
                {
                    bestScore = sum;
                    best = t;
                }
            }
            return best;
        }
        
        #endregion
        
        
        #region Utility
        
        private IEnumerable<Tile> GetFreeTiles()
        {
            for (int x = 0; x < _maze.GetWidth(); x++)
            for (int y = 0; y < _maze.GetHeight(); y++)
            {
                Tile t = _maze.GetTile(x, y);
                if (t != null && !_occupiedTiles.Contains(t))
                    yield return t;
            }
        }
        
        private int PathDistance(Tile a, Tile b)
        {
            if (a == null || b == null) return int.MaxValue;
            return _maze.Pathfinding(a, b).Count;
        }

        private float DistanceFromAll(Tile tile, List<Tile> others)
        {
            float sum = 0;
            foreach (Tile t in others)
                sum += PathDistance(tile, t);
            return sum;
        }
        
        #endregion
    }
}