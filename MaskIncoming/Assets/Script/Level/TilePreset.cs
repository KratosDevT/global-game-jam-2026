using UnityEngine;

namespace Script.Level
{
    [CreateAssetMenu(fileName = "TilePreset", menuName = "Tiles/TilePreset")]
    public class TilePreset : ScriptableObject
    {
        public GameObject prefab;
        public Tile.TileConnections connections;
        public Sprite previewIcon;
    }
}