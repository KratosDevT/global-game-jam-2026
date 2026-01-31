using UnityEngine;

namespace Script.Level
{
    [CreateAssetMenu(fileName = "TilePreset", menuName = "Tiles/TilePreset")]
    public class MazePreset : ScriptableObject
    {
        [SerializeField]
        GameObject end, straight, corner, tJunction, xJunction;
    }
}