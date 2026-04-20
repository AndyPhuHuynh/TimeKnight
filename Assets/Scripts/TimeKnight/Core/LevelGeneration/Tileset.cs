using TimeKnight.Utils;
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
            Validation.NotNull(this, ConnectionLeft,  nameof(ConnectionLeft));  
            Validation.NotNull(this, ConnectionRight, nameof(ConnectionRight)); 
            Validation.NotNull(this, ConnectionUp,    nameof(ConnectionUp));    
            Validation.NotNull(this, ConnectionDown,  nameof(ConnectionDown));  
        }
    }
}