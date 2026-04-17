using UnityEngine;
using UnityEngine.Tilemaps;

namespace TimeKnight.Core.LevelGeneration
{
    [CreateAssetMenu(fileName = "TileSet", menuName = "Scriptable Objects/TileSet")]
    public class Tileset : ScriptableObject
    {
        [Header("Connections")]
        [field: SerializeField] public Tile ConnectionLeft  { get; private set; } = null!;
        [field: SerializeField] public Tile ConnectionRight { get; private set; } = null!;
        [field: SerializeField] public Tile ConnectionUp    { get; private set; } = null!;
        [field: SerializeField] public Tile ConnectionDown  { get; private set; } = null!;

        private void OnValidate()
        {
            Debug.Assert(ConnectionLeft  != null, $"Missing {nameof(ConnectionLeft)}", this);
            Debug.Assert(ConnectionRight != null, $"Missing {nameof(ConnectionRight)}",this);
            Debug.Assert(ConnectionUp    != null, $"Missing {nameof(ConnectionUp)}",   this);
            Debug.Assert(ConnectionDown  != null, $"Missing {nameof(ConnectionDown)}", this);
        }
    }
}