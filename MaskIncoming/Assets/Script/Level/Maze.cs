using System.Collections.Generic;
using UnityEngine;

namespace Script.Level
{
    public class Maze : IMaze
    {
        private int _width = 10;
        private int _height = 10;
        private float _cellSize = 1.0f;
        
        private Tile[,] _grid;
        
        public Maze(int width, int height, float cellSize)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;

            _grid = new Tile[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _grid[x, y] = new Tile(x, y);
        }

        public bool IsValid(int x, int y)
            => x >= 0 && x < _width && y >= 0 && y < _height;

        public Tile GetTile(int x, int y)
            => IsValid(x, y) ? _grid[x, y] : null;

        public Tile GetTileFromWorld(Vector3 worldPos)
        {
            Vector2Int coords = WorldToTile(worldPos);
            return GetTile(coords.x, coords.y);
        }

        public Tile GetNeighbor(Tile tile, int direction)
        {
            if (!tile.HasPath(direction)) return null;

            int nx = tile.X;
            int ny = tile.Y;

            switch (direction)
            {
                case 0: ny++; break; // N
                case 1: nx++; break; // E
                case 2: ny--; break; // S
                case 3: nx--; break; // W
            }

            return GetTile(nx, ny);
        }

        public IEnumerable<Tile> GetNeighborsMinusPrevious(Tile currentTile, Tile previousTile)
        {
            for (int d = 0; d < 4; d++)
            {
                Tile n = GetNeighbor(currentTile, d);
                if (n != null && n != previousTile) yield return n;
            }
        }

        public IEnumerable<Tile> GetNeighbors(Tile tile)
        {
            for (int d = 0; d < 4; d++)
            {
                Tile n = GetNeighbor(tile, d);
                if (n != null) yield return n;
            }
        }

        public Vector3 TileToWorld(Tile tile)
            => new Vector3(tile.X * _cellSize, tile.Y * _cellSize, 0);

        public Vector2Int WorldToTile(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt(worldPos.x / _cellSize);
            int y = Mathf.FloorToInt(worldPos.y / _cellSize);
            return new Vector2Int(x, y);
        }
    }
}