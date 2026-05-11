#if UNITY_EDITOR
using TimeKnight.Core.Input;
using UnityEditor;
using UnityEngine;

namespace TimeKnight.Editor.Input
{
    [CustomEditor(typeof(InputReader))]
    public class InputReaderEditor : UnityEditor.Editor
    {
        private bool _mapsFoldout = true;
        private bool _gameplayFoldout = true;
        private bool _dialogueFoldout = true;
        private bool _globalFoldout = true;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var reader = (InputReader)target;
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to inspect input state.", MessageType.Info);
                return;
            }

            var state = reader.SaveState();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Active Input State", EditorStyles.boldLabel);

            _mapsFoldout = EditorGUILayout.Foldout(_mapsFoldout, "Action Maps", true);
            if (_mapsFoldout)
            {
                EditorGUI.indentLevel++;
                DrawFlag("Gameplay", (state.ActionMaps & ActionMaps.Gameplay) != 0);
                DrawFlag("Dialogue", (state.ActionMaps & ActionMaps.Dialogue) != 0);
                DrawFlag("Global",   (state.ActionMaps & ActionMaps.Global) != 0);
                EditorGUI.indentLevel--;
            }

            _gameplayFoldout = EditorGUILayout.Foldout(_gameplayFoldout, "Gameplay Actions", true);
            if (_gameplayFoldout)
            {
                EditorGUI.indentLevel++;
                DrawFlag("Move Horizontal",      (state.GameplayActions & GameplayActions.MoveHorizontal) != 0);
                DrawFlag("Move Jump",            (state.GameplayActions & GameplayActions.MoveJump) != 0);
                DrawFlag("Attack",               (state.GameplayActions & GameplayActions.Attack) != 0);
                DrawFlag("Grapple Fire",         (state.GameplayActions & GameplayActions.GrappleFire) != 0);
                DrawFlag("Grapple Stop",         (state.GameplayActions & GameplayActions.GrappleStop) != 0);
                DrawFlag("Interaction Interact", (state.GameplayActions & GameplayActions.InteractionInteract) != 0);
                DrawFlag("Interaction Navigate", (state.GameplayActions & GameplayActions.InteractionNavigate) != 0);
                DrawFlag("Slow Time",            (state.GameplayActions & GameplayActions.SlowTime) != 0);
                EditorGUI.indentLevel--;
            }

            _dialogueFoldout = EditorGUILayout.Foldout(_dialogueFoldout, "Dialogue Actions", true);
            if (_dialogueFoldout)
            {
                EditorGUI.indentLevel++;
                DrawFlag("Advance", (state.DialogueActions & DialogueActions.Advance) != 0);
                EditorGUI.indentLevel--;
            }
            
            _globalFoldout = EditorGUILayout.Foldout(_globalFoldout, "Global Actions", true);
            if (_globalFoldout)
            {
                EditorGUI.indentLevel++;
                DrawFlag("Open Pause Menu", (state.GlobalActions & GlobalActions.OpenPause) != 0);
                DrawFlag("Close Pause Menu", (state.GlobalActions & GlobalActions.ClosePause) != 0);
                EditorGUI.indentLevel--;
            }

            if (Application.isPlaying) Repaint();
        }

        private static void DrawFlag(string label, bool active)
        {
            var color = GUI.color;
            GUI.color = active ? Color.softGreen : Color.softRed;
            EditorGUILayout.LabelField(label, active ? "● Enabled" : "○ Disabled");
            GUI.color = color;
        }
    }
}
#endif