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
}
