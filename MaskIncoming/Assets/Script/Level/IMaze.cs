using System.Collections.Generic;
using UnityEngine;

namespace Script.Level
{
    public interface IMaze
    {
        Tile GetTile(int x, int y);
        Tile GetTileFromWorld(Vector3 worldPos);
        IEnumerable<Tile> GetNeighborsMinusPrevious(Tile currentTile, Tile previousTile);
        IEnumerable<Tile> GetNeighbors(Tile tile);
        Tile GetNeighbor(Tile tile, int directionIndex);
        Vector3 TileToWorld(Tile tile);
        Vector2Int WorldToTile(Vector3 worldPos);
        List<Tile> Pathfinding(Tile start, Tile end);
    }
}