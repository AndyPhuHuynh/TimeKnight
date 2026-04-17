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
        private PlayerInputActions? _actions; 
        public PlayerInputActions Actions => _actions ?? CreateActions();

        private void OnEnable()
        {
            OnDisable();
            
            if (_actions != null)
            {
                _actions.Disable();
                _actions.Dispose();
            }
            
            CreateActions();
        }

        private void OnDisable()
        {
            if (_actions == null) return;
            _actions.Disable();
    
        #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // In edit mode, we can't call Dispose() as it uses Destroy() internally.
                // Just null the reference and let GC handle cleanup.
                _actions = null;
                return;
            }
        #endif
    
            // Dispose can only be called in play mode
            _actions.Dispose();
            _actions = null;
        }

        private PlayerInputActions CreateActions()
        {
            _actions = new PlayerInputActions();
            _actions.Player.Enable();
            _actions.Interaction.Enable();
            _actions.Dialogue.Disable();
            _actions.GrapplingHook.Enable();
            _actions.GrapplingHook.StopGrapple.Disable();
            _actions.Sword.Enable();
            return _actions;
        }
        
        public void EnableOnly(InputActionMap mapToEnable)
        {
            if (_actions == null) return;
            foreach (var map in _actions.asset.actionMaps)
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
