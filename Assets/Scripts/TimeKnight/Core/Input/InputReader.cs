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
            Actions = new PlayerInputActions();
            Actions.Player.Enable();
            Actions.Interaction.Enable();
        }

        private void OnDisable()
        {
            Actions.Player.Disable();
            Actions.Interaction.Disable();
        }
        
        public void EnableOnly(InputActionMap mapToEnable)
        {
            foreach (var map in Actions.asset.actionMaps)
            {
                if (map == mapToEnable)
                {
                    Debug.Log($"{map.name} is enabled");
                    map.Enable();
                }
                else
                {
                    Debug.Log($"{map.name} is disabled");
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
