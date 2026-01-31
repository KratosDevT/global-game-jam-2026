using Script.Enums;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [System.Serializable]
    public struct TileConnections
    {
        public bool north;
        public bool east;
        public bool south;
        public bool west;
    }
    
    public TileConnections connections;
    public Vector2Int gridPosition;
    public GameObject visualPrefab;
    
    public int rotation;
    
    public bool CanConnectTo(Tile other, EDirection dir)
    {
        return GetConnection(dir) && other.GetConnection(GetOpposite(dir));
    }
    
    public bool GetConnection(EDirection dir)
    {
        return dir switch
        {
            EDirection.North => connections.north,
            EDirection.East => connections.east,
            EDirection.South => connections.south,
            EDirection.West => connections.west,
            _ => false
        };
    }
    
    private EDirection GetOpposite(EDirection dir)
    {
        return dir switch
        {
            EDirection.North => EDirection.South,
            EDirection.South => EDirection.North,
            EDirection.East  => EDirection.West,
            EDirection.West  => EDirection.East,
            _ => dir
        };
    }
}
