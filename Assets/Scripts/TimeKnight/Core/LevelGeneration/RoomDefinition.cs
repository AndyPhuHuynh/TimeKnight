using System;
using System.Collections.Generic;
using TimeKnight.Attributes;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TimeKnight.Core.LevelGeneration
{
    public class RoomDefinition : MonoBehaviour
    {
        [SerializeField] private Tileset tileset;
        [SerializeField] private Tilemap terrainMap;
        [field: SerializeField] public Tilemap ConnectionMap { get; private set; }

        [SerializeField, ReadOnly] private List<ConnectionDefinition> connectionList = new();
        public IReadOnlyList<ConnectionDefinition> ConnectionList => connectionList;
        
        private void OnValidate()
        {
            Debug.Assert(tileset       != null, $"Missing {nameof(tileset)}",       this);
            Debug.Assert(terrainMap    != null, $"Missing {nameof(terrainMap)}",    this);
            Debug.Assert(ConnectionMap != null, $"Missing {nameof(ConnectionMap)}", this);
        }

        public void BakeConnections()
        {
            connectionList.Clear();
            foreach (var pos in ConnectionMap.cellBounds.allPositionsWithin)
            {
                var tile = ConnectionMap.GetTile(pos);
                if (!IsConnectionTile(tile)) continue;
                connectionList.Add(new ConnectionDefinition
                {
                    type = GetConnectionType(tile),
                    centerOffset = pos
                });
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private bool IsConnectionTile(TileBase tile)
        {
            return tile == tileset.ConnectionLeft || tile == tileset.ConnectionRight 
                || tile == tileset.ConnectionUp || tile == tileset.ConnectionDown;
        }

        private ConnectionType GetConnectionType(TileBase tile)
        {
            if (tile == tileset.ConnectionDown)  return ConnectionType.Down;
            if (tile == tileset.ConnectionUp)    return ConnectionType.Up;
            if (tile == tileset.ConnectionLeft)  return ConnectionType.Left;
            if (tile == tileset.ConnectionRight) return ConnectionType.Right;
            throw new ArgumentException($"Invalid tile on connection tileset: {tile.name}");
        }

        public List<Vector3Int> GetTileLocalPositions()
        {
            var result = new List<Vector3Int>();
            foreach (var pos in terrainMap.cellBounds.allPositionsWithin)
            {
                var tile = terrainMap.GetTile(pos);
                if (tile is null) continue;
                result.Add(pos);
            }
            return result;
        }
    }
}