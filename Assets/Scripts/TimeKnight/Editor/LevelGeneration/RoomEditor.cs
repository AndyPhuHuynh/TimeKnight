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
    public class RoomEditor : UnityEditor.Editor
    {
        private IEnumerable<RoomDefinition> RoomTargets => targets.Select(o => o as RoomDefinition)!;
        
        public void OnEnable()
        {
            if (target == null) return;
            
            Tilemap.tilemapTileChanged += OnConnectionTilemapChanged;
            Undo.undoRedoPerformed += OnUndoRedo;
            
            foreach (var room in RoomTargets)
            {
                room?.BakeConnections();
            }
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
                foreach (var room in RoomTargets)
                {
                    room?.BakeConnections();
                }
            }
            
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Connections bake automatically on tile or inspector change.", MessageType.Info);
            
            if (!GUILayout.Button("Force Re-Bake Connections")) return;
            foreach (var room in RoomTargets)
            {
                room?.BakeConnections();
            }
        }
        
        private void OnUndoRedo()
        {
            if (targets == null || targets.Length == 0) return;
            foreach (var room in RoomTargets)
            {
                room?.BakeConnections();
            }
        }

        private void OnConnectionTilemapChanged(Tilemap tilemap, Tilemap.SyncTile[] _)
        {
            if (targets == null || targets.Length == 0) return;
            foreach (var room in RoomTargets)
            {
                if (room != null && tilemap == room.ConnectionMap)
                {
                    room.BakeConnections();
                }
            }
        }
    }
}
#endif