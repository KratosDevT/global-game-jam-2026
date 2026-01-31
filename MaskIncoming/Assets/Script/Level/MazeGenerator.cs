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
        
        [Header("Prefabs References")]
        [SerializeField] private IllusoryTile singleTile;
        [SerializeField] private IllusoryTile angleTile;
        [SerializeField] private IllusoryTile dualTile;
        [SerializeField] private IllusoryTile tripleTile;
        [SerializeField] private IllusoryTile quadTile;

        private Tile[,] grid;

        private void Start()
        {
            GenerateMaze();
        }

        private void GenerateMaze()
        {
            // Clean up
            
            // Create Grid
            grid = new Tile[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = new Tile(x, y);
            
            // Algorithm to populate maze
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
            
            DrawMap();
        }

        private void Cleanup()
        {
            foreach (Transform child in transform) Destroy(child.gameObject);
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
            List<Tile> neighbors = new List<Tile>();
            if (cell.y + 1 < height && !grid[cell.x, cell.y + 1].visited) neighbors.Add(grid[cell.x, cell.y + 1]); // N
            if (cell.x + 1 < width && !grid[cell.x + 1, cell.y].visited) neighbors.Add(grid[cell.x + 1, cell.y]); // E
            if (cell.y - 1 >= 0 && !grid[cell.x, cell.y - 1].visited) neighbors.Add(grid[cell.x, cell.y - 1]); // S
            if (cell.x - 1 >= 0 && !grid[cell.x - 1, cell.y].visited) neighbors.Add(grid[cell.x - 1, cell.y]); // O
            return neighbors;
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
                        
                        // Decide se è illusorio
                        bool isIllusory = Random.value < illusoryChance;
                        instance.Setup(isIllusory);
                    }
                }
            }
        }
    }
}