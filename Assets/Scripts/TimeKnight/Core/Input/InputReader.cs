using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeKnight.Core.Input
{
    public struct PreviousMapState
    {
        public readonly bool WasEnabled;
        public readonly InputActionMap Map;

        public PreviousMapState(bool wasEnabled, InputActionMap map)
        {
            WasEnabled = wasEnabled;
            Map = map;
        }
    }
    
    [CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
    public class InputReader : ScriptableObject
    {
        public PlayerInputActions Actions { get; private set; }

        private void OnEnable()
        {
            OnDisable();
            
            if (Actions != null)
            {
                Actions.Disable();
                Actions.Dispose();
            }
    
            Actions = new PlayerInputActions();
            Actions.Player.Enable();
            Actions.Interaction.Enable();
            Actions.Dialogue.Disable();
            Actions.GrapplingHook.Enable();
            Actions.GrapplingHook.StopGrapple.Disable();
        }

        private void OnDisable()
        {
            if (Actions == null) return;
            Actions.Disable();
    
        #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // In edit mode, we can't call Dispose() as it uses Destroy() internally.
                // Just null the reference and let GC handle cleanup.
                Actions = null;
                return;
            }
        #endif
    
            // Dispose can only be called in play mode
            Actions.Dispose();
            Actions = null;
        }
        
        public void EnableOnly(InputActionMap mapToEnable)
        {
            foreach (var map in Actions.asset.actionMaps)
            {
                if (map == mapToEnable)
                {
                    map.Enable();
                }
                else
                {
                    map.Disable();
                }
            }
        }

        public List<PreviousMapState> GetMapStates()
        {
           return Actions.asset.actionMaps.Select(map => new PreviousMapState(map.enabled, map)).ToList();
        }

        public static void RestoreMapStates(List<PreviousMapState> states)
        {
            foreach (var state in states)
            {
                if (state.WasEnabled)
                {
                    state.Map.Enable();
                }
                else
                {
                    state.Map.Disable();
                }
            }
            
        }
    }
}
