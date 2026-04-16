using UnityEngine;
using UnityEngine.Tilemaps;

namespace TimeKnight.Core.LevelGeneration
{
    [CreateAssetMenu(fileName = "TileSet", menuName = "Scriptable Objects/TileSet")]
    public class Tileset : ScriptableObject
    {
        [Header("Connections")]
        [field: SerializeField] public Tile ConnectionLeft  { get; private set; }
        [field: SerializeField] public Tile ConnectionRight { get; private set; }
        [field: SerializeField] public Tile ConnectionUp    { get; private set; }
        [field: SerializeField] public Tile ConnectionDown  { get; private set; }
    }
}