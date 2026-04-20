using System;
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
    
    // TODO: Change sword and interaction maps to be under gameplay map
    // TODO: Remove the old system of enable only and restoring all
    
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
            
            _actions = CreateActions();
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

        private static PlayerInputActions CreateActions()
        {
            var actions = new PlayerInputActions();
            actions.Gameplay.Enable();
            actions.Gameplay.GrappleStop.Disable();
            actions.Interaction.Enable();
            actions.Dialogue.Disable();
            actions.Sword.Enable();
            return actions;
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
        
        private static readonly Action<InputActionMap> EnableMap = m => m.Enable();
        private static readonly Action<InputActionMap> DisableMap = m => m.Disable();
        
        private static readonly Action<InputAction> EnableAction = a => a.Enable();
        private static readonly Action<InputAction> DisableAction = a => a.Disable();

        public void SetMapStatus(InputStatus status, InputMapState map)
        {
            var op = status == InputStatus.Disabled ? DisableMap : EnableMap;
            if ((map & InputMapState.Gameplay) != 0) op(Actions.Gameplay);
            if ((map & InputMapState.Dialogue) != 0) op(Actions.Dialogue);
        }

        public void SetActionStatus(InputStatus status, GameplayActions action)
        {
            var op = status == InputStatus.Disabled ? DisableAction : EnableAction;
            if ((action & GameplayActions.MoveHorizontal) != 0) op(Actions.Gameplay.MoveHorizontal);
            if ((action & GameplayActions.MoveJump)       != 0) op(Actions.Gameplay.MoveJump);
            if ((action & GameplayActions.GrappleFire)    != 0) op(Actions.Gameplay.GrappleFire);
            if ((action & GameplayActions.GrappleStop)    != 0) op(Actions.Gameplay.GrappleStop);
        }
    }
}
