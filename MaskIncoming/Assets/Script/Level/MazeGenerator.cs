using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Script.Level
{
    public class MazeGenerator : MonoBehaviour
    {
        [Header("Maze Settings")]
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [SerializeField] private float cellSize = 1.0f;
        [SerializeField][Range(0f, 1f)] private float illusoryChance = 0.2f;
        [SerializeField][Range(0f, 1f)] private float loopChance = 0.5f;
        
        [Header("Prefabs References")]
        [SerializeField] private IllusoryTile singleTile; // vicolo cieco
        [SerializeField] private IllusoryTile angleTile;  // corner
        [SerializeField] private IllusoryTile dualTile;   // corridor
        [SerializeField] private IllusoryTile tripleTile; // t-corridor
        [SerializeField] private IllusoryTile quadTile;   // cross

        [Header("Fake Pool")]
        [SerializeField] private List<Texture> availableFakeTexture;
        
        [SerializeField] private GameObject enemyPrefab;
        
        private Maze _maze;

        private bool debug = false;
        
        private void Start()
        {
            GenerateMaze();
        }
        
        private void Cleanup()
        {
            foreach (Transform child in transform) Destroy(child.gameObject);
        }
        
        private void GenerateMaze()
        {
            // Clean up (rimuove istanze precedenti)
            Cleanup();

            _maze = new Maze(width, height, cellSize);

            // Algorithm to populate maze
            RunRecursiveBacktracker();

            BraidMaze();

            // Diagnostica: verifica che tutta la griglia sia connessa
            ValidateConnectivity();

            DrawMap();
            
            debug = true;

            Tile origin = _maze.GetTile(0, 0);
            Vector3 pos = _maze.TileToWorld(origin);
            GameObject temp = Instantiate(enemyPrefab, pos, Quaternion.identity);
            Enemy enemy = temp.GetComponent<Enemy>();
            enemy.InitializeMazeData(_maze);
        }

        private void RunRecursiveBacktracker()
        {
            Stack<Tile> stack = new Stack<Tile>();
            Tile start = _maze.GetTile(0, 0);
            start.Visited = true;
            stack.Push(start);

            // Variante con Peek(): più leggibile e classica
            while (stack.Count > 0)
            {
                Tile current = stack.Peek();
                List<Tile> neighbors = GetUnvisitedNeighbors(current);

                if (neighbors.Count > 0)
                {
                    Tile next = neighbors[Random.Range(0, neighbors.Count)];
                    RemoveWall(current, next);
                    next.Visited = true;
                    stack.Push(next);
                }
                else
                {
                    stack.Pop();
                }
            }
        }

        /// <summary>
        /// Controllo veloce di connettività: flood-fill usando le 'paths' (non usa .visited)
        /// Logga celle non raggiungibili da (0,0).
        /// </summary>
        private void ValidateConnectivity()
        {
            bool[,] seen = new bool[width, height];
            Queue<Tile> q = new Queue<Tile>();
            q.Enqueue(_maze.GetTile(0, 0));
            seen[0, 0] = true;
            int reachable = 0;

            while (q.Count > 0)
            {
                Tile t = q.Dequeue();
                reachable++;

                // Nord
                if (t.Paths[0] && IsValid(t.X, t.Y + 1) && !seen[t.X, t.Y + 1])
                { seen[t.X, t.Y + 1] = true; q.Enqueue(_maze.GetTile(t.X, t.Y + 1)); }
                // Est
                if (t.Paths[1] && IsValid(t.X + 1, t.Y) && !seen[t.X + 1, t.Y])
                { seen[t.X + 1, t.Y] = true; q.Enqueue(_maze.GetTile(t.X + 1, t.Y)); }
                // Sud
                if (t.Paths[2] && IsValid(t.X, t.Y - 1) && !seen[t.X, t.Y - 1])
                { seen[t.X, t.Y - 1] = true; q.Enqueue(_maze.GetTile(t.X, t.Y - 1)); }
                // Ovest
                if (t.Paths[3] && IsValid(t.X - 1, t.Y) && !seen[t.X - 1, t.Y])
                { seen[t.X - 1, t.Y] = true; q.Enqueue(_maze.GetTile(t.X - 1, t.Y)); }
            }

            int total = width * height;
            if (reachable != total)
            {
                Debug.LogWarning($"Maze non connesso: raggiunte {reachable}/{total} celle. Ecco le non raggiunte:");
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        if (!seen[x, y]) Debug.LogWarning($" - Tile non raggiunta: {x},{y}");
            }
            else
            {
                Debug.Log($"Maze connesso: tutte le {total} celle raggiungibili.");
            }
        }
        private void RemoveWall(Tile a, Tile b)
        {
            if (a.X == b.X) // Verticale
            {
                if (a.Y > b.Y) { a.Paths[2] = true; b.Paths[0] = true; } // a Sud di b
                else           { a.Paths[0] = true; b.Paths[2] = true; } // a Nord di b
            }
            else // Orizzontale
            {
                if (a.X > b.X) { a.Paths[3] = true; b.Paths[1] = true; } // a Ovest di b
                else           { a.Paths[1] = true; b.Paths[3] = true; } // a Est di b
            }
        }

        private List<Tile> GetUnvisitedNeighbors(Tile cell)
        {
            List<Tile> neighbors = new List<Tile>();
            if (IsValid(cell.X, cell.Y + 1) && !_maze.GetTile(cell.X, cell.Y + 1).Visited) neighbors.Add(_maze.GetTile(cell.X, cell.Y + 1));
            if (IsValid(cell.X + 1, cell.Y) && !_maze.GetTile(cell.X + 1, cell.Y).Visited) neighbors.Add(_maze.GetTile(cell.X + 1, cell.Y));
            if (IsValid(cell.X, cell.Y - 1) && !_maze.GetTile(cell.X, cell.Y - 1).Visited) neighbors.Add(_maze.GetTile(cell.X, cell.Y - 1));
            if (IsValid(cell.X - 1, cell.Y) && !_maze.GetTile(cell.X - 1, cell.Y).Visited) neighbors.Add(_maze.GetTile(cell.X - 1, cell.Y));
            return neighbors;
        }

        void BraidMaze()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tile cell = _maze.GetTile(x, y);
                    int connectionCount = GetConnectionCount(cell);

                    // Se è un vicolo cieco (1 sola connessione) e il caso vuole...
                    if (connectionCount == 1 && Random.value < loopChance)
                    {
                        // Troviamo un vicino verso cui c'è un MURO (quindi non connesso)
                        List<Tile> closedNeighbors = GetClosedNeighbors(cell);
                    
                        if (closedNeighbors.Count > 0)
                        {
                            // Abbattiamo il muro verso un vicino a caso -> Creiamo un Loop!
                            Tile target = closedNeighbors[Random.Range(0, closedNeighbors.Count)];
                            RemoveWall(cell, target);
                        }
                    }
                }
            }
        }
        
        // Serve per il Braiding (trova i muri che possiamo abbattere)
        List<Tile> GetClosedNeighbors(Tile cell)
        {
            List<Tile> closed = new List<Tile>();
            // Controlliamo i vicini validi che NON sono connessi a 'cell'
            // Nord
            if (IsValid(cell.X, cell.Y + 1) && !cell.Paths[0]) closed.Add(_maze.GetTile(cell.X, cell.Y + 1));
            // Est
            if (IsValid(cell.X + 1, cell.Y) && !cell.Paths[1]) closed.Add(_maze.GetTile(cell.X + 1, cell.Y));
            // Sud
            if (IsValid(cell.X, cell.Y - 1) && !cell.Paths[2]) closed.Add(_maze.GetTile(cell.X, cell.Y - 1));
            // Ovest
            if (IsValid(cell.X - 1, cell.Y) && !cell.Paths[3]) closed.Add(_maze.GetTile(cell.X - 1, cell.Y));
        
            return closed;
        }

        int GetConnectionCount(Tile cell)
        {
            int count = 0;
            foreach (bool p in cell.Paths) if (p) count++;
            return count;
        }

        bool IsValid(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }
        
        private void DrawMap()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tile cell = _maze.GetTile(x, y);
                    Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0);

                    // Calcolo Bitmask: N=1, E=2, S=4, O=8
                    int mask = 0;
                    if (cell.Paths[0]) mask += 1;
                    if (cell.Paths[1]) mask += 2;
                    if (cell.Paths[2]) mask += 4;
                    if (cell.Paths[3]) mask += 8;

                    IllusoryTile prefab = null;
                    float rot = 0;

                    // MAPPING E ROTAZIONE
                    // Assumo orientamenti base: Single(N), Angle(N+E), Dual(N+S), Triple(N+E+S)
                    // Se i tuoi sprite base sono diversi, aggiusta 'rot' qui.
                    switch (mask)
                    {
                        case 0: break; 
                        // Single
                        case 1: prefab = singleTile; rot = 0; break;
                        case 2: prefab = singleTile; rot = -90; break;
                        case 4: prefab = singleTile; rot = 180; break;
                        case 8: prefab = singleTile; rot = 90; break;
                        // Angle
                        case 3: prefab = angleTile; rot = 0; break;
                        case 6: prefab = angleTile; rot = -90; break;
                        case 12: prefab = angleTile; rot = 180; break;
                        case 9: prefab = angleTile; rot = 90; break;
                        // Dual
                        case 5: prefab = dualTile; rot = 0; break;
                        case 10: prefab = dualTile; rot = 90; break;
                        // Triple
                        case 7: prefab = tripleTile; rot = 0; break; // Punta Est
                        case 14: prefab = tripleTile; rot = -90; break; // Punta Sud
                        case 13: prefab = tripleTile; rot = 180; break; // Punta Ovest
                        case 11: prefab = tripleTile; rot = 90; break; // Punta Nord
                        // Quad
                        case 15: prefab = quadTile; rot = 0; break;
                    }

                    if (prefab != null)
                    {
                        IllusoryTile instance = Instantiate(prefab, pos, Quaternion.Euler(0, 0, rot), transform);
                        instance.name = $"Tile_{x}_{y}";
                        
                        // LOGICA ILLUSORIA RANDOM
                        bool isIllusory = Random.value < illusoryChance;
                        Texture randomFakeTexture = null;
                        float randomFakeRot = 0;

                        if (isIllusory && availableFakeTexture.Count > 0)
                        {
                            randomFakeTexture = availableFakeTexture[Random.Range(0, availableFakeTexture.Count)];
                            randomFakeRot = Random.Range(0, 4) * 90f;
                        
                            // NOTA: Poiché 'fakeRenderer' è figlio dell'oggetto ruotato,
                            // questa rotazione si somma a quella del padre.
                            // Per farla sembrare totalmente casuale rispetto al mondo,
                            // dovremmo sottrarre la rotazione del padre, ma spesso sommarla aumenta solo il caos (che va bene).
                            // Se vuoi controllo assoluto: randomFakeRot -= rot;
                        }
                        
                        instance.Setup(isIllusory, randomFakeTexture, randomFakeRot);
                        instance.name = $"Tile_{x}_{y}";
                        instance.SetDebug(mask);
                    }
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!debug) return;
            Gizmos.color = Color.green;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tile t = _maze.GetTile(x, y);
                    Vector3 pos = _maze.TileToWorld(t);
                    Gizmos.DrawWireCube(pos, new Vector3(cellSize, cellSize, 0.1f));
                }
            }
        }
    }
}