using System;
using System.Collections.Generic;
using TimeKnight.Attributes;
using TimeKnight.Extensions;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace TimeKnight.Core.LevelGeneration
{
    public class RoomDefinition : MonoBehaviour
    {
        [Header("Tiles")]
        [SerializeField] private Tileset tileset = null!;
        [SerializeField] private Tilemap terrainMap = null!;
        [SerializeField] private Tilemap backgroundMap = null!;
        [SerializeField] private Tilemap connectionMap = null!;
        
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

            if (IsBasePrefab()) return;
            if (RoomType == RoomType.None) Debug.LogWarning($"RoomType on {gameObject.name} is None", this);
        }
        
        public void BakeConnections()
        {
            connectionList.Clear();
            foreach (var pos in connectionMap.cellBounds.allPositionsWithin)
            {
                var tile = connectionMap.GetTile(pos);
                if (!IsConnectionTile(tile)) continue;
                connectionList.Add(new ConnectionDefinition
                {
                    type = GetConnectionType(tile),
                    centerOffset = pos
                });
            }
#if UNITY_EDITOR
            RoomRegistry.ReinitializeAllRegistries();
            EditorUtility.SetDirty(this);
#endif
        }

        private bool IsBasePrefab()
        {
#if UNITY_EDITOR
            // Check if we're currently editing this object in Prefab Mode
            try
            {
                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null && prefabStage.IsPartOfPrefabContents(gameObject))
                {
                    var stagePath = prefabStage.assetPath;
                    if (string.IsNullOrEmpty(stagePath)) return true;
                    return stagePath.Contains(Paths.RoomBase);
                }
            }
            catch (InvalidOperationException)
            {
                // Not safe to query prefab stage yet (called during Awake/OnEnable).
                // Fall through to asset path check below.
            }
            // Check if this is a prefab asset in the projects tab
            var partOfPrefab = PrefabUtility.IsPartOfPrefabAsset(gameObject);
            if (!partOfPrefab) return false;
            
            // Check the path
            var path = AssetDatabase.GetAssetPath(gameObject);
            
            // If we click off the item in the project tab, this returns an empty string
            // Return true here just to be safe
            if (string.IsNullOrEmpty(path)) return true;

            return path.Contains(Paths.RoomBase);
#else
            return false;
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

        public Vector3 GetCenter()
        {
            return terrainMap.localBounds.center;
        }

        public Dictionary<Vector3Int, TileBase> GetTerrainLocalPositions() => terrainMap.GetLocalPositions();
        public Dictionary<Vector3Int, TileBase> GetBackgroundLocalPositions() => backgroundMap.GetLocalPositions();
    }
}