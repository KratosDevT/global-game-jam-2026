using System.Collections;
using System.Collections.Generic;
using Script.Enums;
using UnityEngine;

public class Tile
{
    public int X, Y;
    public bool Visited;
    public bool[] Paths = new bool[4];

    private Dictionary<string, int> _visitCountByEnemy 
        = new Dictionary<string, int>();

    public Tile(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public void RegisterVisit(Enemy enemy)
    {
        string id = enemy.gameObject.name;

        if (!_visitCountByEnemy.TryAdd(id, 1))
            _visitCountByEnemy[id]++;
    }

    public int GetVisitCount(Enemy enemy)
    {
        return _visitCountByEnemy.GetValueOrDefault(enemy.gameObject.name, 0);
    }

    public bool HasPath(int dirIndex)
        => Paths[dirIndex];
}
