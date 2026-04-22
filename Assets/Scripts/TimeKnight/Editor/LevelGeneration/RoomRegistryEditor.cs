#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TimeKnight.Core.LevelGeneration;
using UnityEditor;

namespace TimeKnight.Editor.LevelGeneration
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(RoomRegistry))]
	public class RoomRegistryEditor : UnityEditor.Editor
	{
		private IEnumerable<RoomRegistry> RegistryTargets => targets.Select(o => o as RoomRegistry)!;
		
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			
			EditorGUI.BeginChangeCheck();
			DrawDefaultInspector();
			if (!EditorGUI.EndChangeCheck()) return;
			
			serializedObject.ApplyModifiedProperties();
			foreach (var registry in RegistryTargets)
			{
				registry.Initialize();
			}
		}
	}
}
#endif