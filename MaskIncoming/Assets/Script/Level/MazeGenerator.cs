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
        [SerializeField] private List<Sprite> availableFakeSprites;
        
        private Tile[,] grid;

        private void Start()
        {
            GenerateMaze();
        }

        private void Cleanup()
        {
            foreach (Transform child in transform) Destroy(child.gameObject);
        }

        /*
        private void RunRecursiveBacktracker()
        {
            Stack<Tile> stack = new Stack<Tile>();
            Tile current = grid[0, 0];
            current.visited = true;
            stack.Push(current);

            while (stack.Count > 0)
            {
                current = stack.Pop();
                List<Tile> neighbors = GetUnvisitedNeighbors(current);

                if (neighbors.Count > 0)
                {
                    stack.Push(current);
                    Tile next = neighbors[Random.Range(0, neighbors.Count)];
                    RemoveWall(current, next);
                    next.visited = true;
                    stack.Push(next);
                }
            }
        }*/
        
        private void GenerateMaze()
        {
            // Clean up (rimuove istanze precedenti)
            Cleanup();

            // Create Grid
            grid = new Tile[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = new Tile(x, y);

            // Algorithm to populate maze
            RunRecursiveBacktracker();

            BraidMaze();

            // Diagnostica: verifica che tutta la griglia sia connessa
            ValidateConnectivity();

            DrawMap();
        }

        private void RunRecursiveBacktracker()
        {
            Stack<Tile> stack = new Stack<Tile>();
            Tile start = grid[0, 0];
            start.visited = true;
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
                    next.visited = true;
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
            q.Enqueue(grid[0, 0]);
            seen[0, 0] = true;
            int reachable = 0;

            while (q.Count > 0)
            {
                Tile t = q.Dequeue();
                reachable++;

                // Nord
                if (t.paths[0] && IsValid(t.x, t.y + 1) && !seen[t.x, t.y + 1])
                { seen[t.x, t.y + 1] = true; q.Enqueue(grid[t.x, t.y + 1]); }
                // Est
                if (t.paths[1] && IsValid(t.x + 1, t.y) && !seen[t.x + 1, t.y])
                { seen[t.x + 1, t.y] = true; q.Enqueue(grid[t.x + 1, t.y]); }
                // Sud
                if (t.paths[2] && IsValid(t.x, t.y - 1) && !seen[t.x, t.y - 1])
                { seen[t.x, t.y - 1] = true; q.Enqueue(grid[t.x, t.y - 1]); }
                // Ovest
                if (t.paths[3] && IsValid(t.x - 1, t.y) && !seen[t.x - 1, t.y])
                { seen[t.x - 1, t.y] = true; q.Enqueue(grid[t.x - 1, t.y]); }
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
            if (a.x == b.x) // Verticale
            {
                if (a.y > b.y) { a.paths[2] = true; b.paths[0] = true; } // a Sud di b
                else           { a.paths[0] = true; b.paths[2] = true; } // a Nord di b
            }
            else // Orizzontale
            {
                if (a.x > b.x) { a.paths[3] = true; b.paths[1] = true; } // a Ovest di b
                else           { a.paths[1] = true; b.paths[3] = true; } // a Est di b
            }
        }

        private List<Tile> GetUnvisitedNeighbors(Tile cell)
        {
            /*
            List<Tile> neighbors = new List<Tile>();
            if (cell.y + 1 < height && !grid[cell.x, cell.y + 1].visited) neighbors.Add(grid[cell.x, cell.y + 1]); // N
            if (cell.x + 1 < width && !grid[cell.x + 1, cell.y].visited) neighbors.Add(grid[cell.x + 1, cell.y]); // E
            if (cell.y - 1 >= 0 && !grid[cell.x, cell.y - 1].visited) neighbors.Add(grid[cell.x, cell.y - 1]); // S
            if (cell.x - 1 >= 0 && !grid[cell.x - 1, cell.y].visited) neighbors.Add(grid[cell.x - 1, cell.y]); // O
            return neighbors;
            */
            
            List<Tile> neighbors = new List<Tile>();
            if (IsValid(cell.x, cell.y + 1) && !grid[cell.x, cell.y + 1].visited) neighbors.Add(grid[cell.x, cell.y + 1]);
            if (IsValid(cell.x + 1, cell.y) && !grid[cell.x + 1, cell.y].visited) neighbors.Add(grid[cell.x + 1, cell.y]);
            if (IsValid(cell.x, cell.y - 1) && !grid[cell.x, cell.y - 1].visited) neighbors.Add(grid[cell.x, cell.y - 1]);
            if (IsValid(cell.x - 1, cell.y) && !grid[cell.x - 1, cell.y].visited) neighbors.Add(grid[cell.x - 1, cell.y]);
            return neighbors;
        }

        void BraidMaze()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tile cell = grid[x, y];
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
            if (IsValid(cell.x, cell.y + 1) && !cell.paths[0]) closed.Add(grid[cell.x, cell.y + 1]);
            // Est
            if (IsValid(cell.x + 1, cell.y) && !cell.paths[1]) closed.Add(grid[cell.x + 1, cell.y]);
            // Sud
            if (IsValid(cell.x, cell.y - 1) && !cell.paths[2]) closed.Add(grid[cell.x, cell.y - 1]);
            // Ovest
            if (IsValid(cell.x - 1, cell.y) && !cell.paths[3]) closed.Add(grid[cell.x - 1, cell.y]);
        
            return closed;
        }

        int GetConnectionCount(Tile cell)
        {
            int count = 0;
            foreach (bool p in cell.paths) if (p) count++;
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
                    Tile cell = grid[x, y];
                    Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0);

                    // Calcolo Bitmask: N=1, E=2, S=4, O=8
                    int mask = 0;
                    if (cell.paths[0]) mask += 1;
                    if (cell.paths[1]) mask += 2;
                    if (cell.paths[2]) mask += 4;
                    if (cell.paths[3]) mask += 8;

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
                        Sprite randomFakeSprite = null;
                        float randomFakeRot = 0;

                        if (isIllusory && availableFakeSprites.Count > 0)
                        {
                            // 1. Pesca uno sprite a caso dalla lista
                            randomFakeSprite = availableFakeSprites[Random.Range(0, availableFakeSprites.Count)];
                        
                            // 2. Ruotalo a caso (0, 90, 180, 270)
                            // Moltiplichiamo per 90 per avere angoli retti precisi
                            randomFakeRot = Random.Range(0, 4) * 90f;
                        
                            // NOTA: Poiché 'fakeRenderer' è figlio dell'oggetto ruotato,
                            // questa rotazione si somma a quella del padre.
                            // Per farla sembrare totalmente casuale rispetto al mondo,
                            // dovremmo sottrarre la rotazione del padre, ma spesso sommarla aumenta solo il caos (che va bene).
                            // Se vuoi controllo assoluto: randomFakeRot -= rot;
                        }

                        instance.Setup(isIllusory, randomFakeSprite, randomFakeRot);
                        instance.name = $"Tile_{x}_{y}";
                        instance.SetDebug(mask);
                    }
                }
            }
        }
    }
}