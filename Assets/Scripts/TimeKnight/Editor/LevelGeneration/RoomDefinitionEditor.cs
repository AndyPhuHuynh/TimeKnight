#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TimeKnight.Core.LevelGeneration;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TimeKnight.Editor.LevelGeneration
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RoomDefinition))]
    public class RoomDefinitionEditor : UnityEditor.Editor
    {
        private IEnumerable<RoomDefinition> RoomTargets => targets.Select(o => o as RoomDefinition)!;
        
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
                BakeAllConnections();
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
            RoomRegistry.ReinitializeAllRegistries();
        }
    }
}
#endif