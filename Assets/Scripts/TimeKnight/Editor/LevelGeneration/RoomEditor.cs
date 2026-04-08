#if UNITY_EDITOR
using TimeKnight.Core.LevelGeneration;
using UnityEditor;
using UnityEngine;

namespace TimeKnight.Editor.LevelGeneration
{
    [CustomEditor(typeof(RoomDefinition))]
    public class RoomEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var room = (RoomDefinition)target;
            
            GUILayout.Space(10);
            if (GUILayout.Button("Apply Tilemap Changes"))
            {
                room.BakeConnections();
            }
        }
    }
}
#endif