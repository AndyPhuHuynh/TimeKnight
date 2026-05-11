#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TimeKnight.Core.LevelGeneration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TimeKnight.Editor.LevelGeneration
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RoomDefinition))]
    public class RoomDefinitionEditor : UnityEditor.Editor
    {
        private IEnumerable<RoomDefinition> RoomTargets => targets.Select(o => o as RoomDefinition)!;
        private System.Action? _pendingBaking;
        
        public void OnEnable()
        {
            Tilemap.tilemapTileChanged += OnConnectionTilemapChanged;
            Undo.undoRedoPerformed += OnUndoRedo;
            
            BakeAllConnections();
        }

        public void OnDisable()
        {
            Tilemap.tilemapTileChanged -= OnConnectionTilemapChanged;
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorApplication.delayCall += BakeAllConnections;
            }
            
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Connections bake automatically on tile or inspector change.", MessageType.Info);
            
            if (!GUILayout.Button("Force Re-Bake Connections")) return;
            BakeAllConnections();
        }
        
        private void OnUndoRedo()
        {
            BakeAllConnections();
        }

        private void OnConnectionTilemapChanged(Tilemap tilemap, Tilemap.SyncTile[] _)
        {
            BakeAllConnections();
        }

        private void BakeAllConnections()
        {
            if (targets == null || targets.Length == 0) return;
            foreach (var room in RoomTargets)
            {
                room?.BakeConnections();
            }
            
            // Save prefab stage if we're currently editing a prefab
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                // This saves the prefab stage contents back to the asset on disk
                PrefabUtility.SaveAsPrefabAsset(
                    prefabStage.prefabContentsRoot,
                    prefabStage.assetPath
                );
            }
            
            AssetDatabase.SaveAssets();
            RoomRegistry.ReinitializeAllRegistries();
        }
    }
}
#endif