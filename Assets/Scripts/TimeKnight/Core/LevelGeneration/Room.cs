using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TimeKnight.Core.LevelGeneration
{
    public enum ConnectionType
    {
        Left,
        Right,
        Up,
        Down
    }
    
    public class Connection
    {
        public ConnectionType Type;
        public Vector3Int Position;
    }
    
    public class Room : MonoBehaviour
    {
        [SerializeField] private Tileset tileset;
        [SerializeField] private Tilemap connectionMap;

        private readonly List<Connection> connectionList = new();
        public IReadOnlyList<Connection> ConnectionList => connectionList;

        private void OnValidate()
        {
            Debug.Assert(tileset       != null, $"Missing {nameof(tileset)}",       this);
            Debug.Assert(connectionMap != null, $"Missing {nameof(connectionMap)}", this);
        }

        public void BakeConnections()
        {
            connectionList.Clear();
            foreach (var pos in connectionMap.cellBounds.allPositionsWithin)
            {
                var tile = connectionMap.GetTile(pos);
                if (!IsConnectionTile(tile)) continue;
                connectionList.Add(new Connection
                {
                    Type = GetConnectionType(tile),
                    Position = pos
                });
            }
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
            throw new ArgumentException("Invalid connection type");
        }
    }
}