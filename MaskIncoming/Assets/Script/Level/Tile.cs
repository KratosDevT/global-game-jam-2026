using System.Collections;
using Script.Enums;
using UnityEngine;

public class Tile
{
    public int x, y;
    public bool visited = false;
    // an array for directions: 0=Nord, 1=Est, 2=Sud, 3=Ovest
    public bool[] paths = new bool[4]; 

    public Tile(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    
    // ---------------------------------------------------------------------------------------------------------------
    // Helpers for enemies
    public bool HasPath(int directionIndex)
    {
        if (directionIndex < 0 || directionIndex > 3) return false;
        return paths[directionIndex];
    }
    
    public Vector3 GetWorldPosition(float cellSize)
    {
        return new Vector3(x * cellSize, y * cellSize, 0);
    }
}
