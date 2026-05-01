using System;
using System.Collections.Generic;
using TimeKnight.Attributes;
using TimeKnight.Extensions;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TimeKnight.Core.LevelGeneration
{
    public class RoomDefinition : MonoBehaviour
    {
        [Header("Tiles")]
        [SerializeField] private Tileset tileset = null!;
        [SerializeField] private Tilemap terrainMap = null!;
        [SerializeField] private Tilemap backgroundMap = null!;
        
        [Header("Connections")]
        [SerializeField] private Tilemap connectionMap = null!;
        [SerializeField] private GameObject connectionFillContainer = null!;
        
        [Header("Type")]
        [field: SerializeField] public RoomType RoomType { get; private set; } = RoomType.None;
        
        [Header("Connections")]
        [SerializeField, ReadOnly] private List<ConnectionDefinition> connectionList = new();
        public IReadOnlyList<ConnectionDefinition> ConnectionList => connectionList;
        
        private void OnValidate()
        {
            Validation.NotNull(this, tileset, nameof(tileset));
            Validation.NotNull(this, terrainMap, nameof(terrainMap));
            Validation.NotNull(this, backgroundMap, nameof(backgroundMap));
            Validation.NotNull(this, connectionMap, nameof(connectionMap));
            Validation.NotNull(this, connectionFillContainer, nameof(connectionFillContainer));

            if (Validation.IsExactPrefabAtPath(gameObject, Paths.RoomBase)) return;
            if (RoomType == RoomType.None) Debug.LogWarning($"RoomType on {gameObject.name} is None", this);
        }
        
        public void BakeConnections()
        {
            connectionList.Clear();
            foreach (var pos in connectionMap.cellBounds.allPositionsWithin)
            {
                var connectionTile = connectionMap.GetTile(pos);
                if (!IsConnectionTile(connectionTile)) continue;

                Tilemap? fillTiles = null;
                foreach (var fillTilemap in connectionFillContainer.transform.GetComponentsInChildren<Tilemap>())
                {
                    if (fillTilemap.GetTile(pos) is null) continue;
                    fillTiles = fillTilemap; 
                    break;
                }
                
                connectionList.Add(new ConnectionDefinition
                {
                    type = GetConnectionType(connectionTile),
                    centerOffset = pos,
                    fillTilemap = fillTiles
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

        public Vector3 GetCenter() => terrainMap.localBounds.center;
        public IEnumerable<TileEntry> GetTerrainLocalTiles() => terrainMap.GetLocalTiles();
        public IEnumerable<TileEntry> GetBackgroundLocalTiles() => backgroundMap.GetLocalTiles();
    }
}