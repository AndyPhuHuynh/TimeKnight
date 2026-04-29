using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeKnight.Core.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
    public class InputReader : ScriptableObject
    {
        private PlayerInputActions? _actions; 
        public PlayerInputActions Actions => _actions ?? CreateActions();
        
        private static readonly Action<InputActionMap> EnableMap = m => m.Enable();
        private static readonly Action<InputActionMap> DisableMap = m => m.Disable();
        
        private static readonly Action<InputAction> EnableAction = a => a.Enable();
        private static readonly Action<InputAction> DisableAction = a => a.Disable();

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
            actions.Dialogue.Disable();
            return actions;
        }

        public void SetMapStatus(InputStatus status, ActionMaps map)
        {
            var op = status == InputStatus.Disabled ? DisableMap : EnableMap;
            if ((map & ActionMaps.Gameplay) != 0) op(Actions.Gameplay);
            if ((map & ActionMaps.Dialogue) != 0) op(Actions.Dialogue);
        }

        public void SetActionStatus(InputStatus status, GameplayActions action)
        {
            var op = status == InputStatus.Disabled ? DisableAction : EnableAction;
            if ((action & GameplayActions.MoveHorizontal) != 0) op(Actions.Gameplay.MoveHorizontal);
            if ((action & GameplayActions.MoveJump) != 0) op(Actions.Gameplay.MoveJump);
            if ((action & GameplayActions.Attack) != 0) op(Actions.Gameplay.Attack);
            if ((action & GameplayActions.GrappleFire) != 0) op(Actions.Gameplay.GrappleFire);
            if ((action & GameplayActions.GrappleStop) != 0) op(Actions.Gameplay.GrappleStop);
            if ((action & GameplayActions.InteractionInteract) != 0) op(Actions.Gameplay.InteractionInteract);
            if ((action & GameplayActions.InteractionNavigate) != 0) op(Actions.Gameplay.InteractionNavigate);
        }

        private void SetActionStatus(InputStatus status, DialogueActions action)
        {
            var op = status == InputStatus.Disabled ? DisableAction : EnableAction;
            if ((action & DialogueActions.Advance) != 0) op(Actions.Dialogue.Advance);
        }

        public InputState SaveState()
        {
            var map = ActionMaps.None;
            var gameplayActions = GameplayActions.None;
            var dialogueActions = DialogueActions.None;
            
            if (Actions.Gameplay.enabled) map |= ActionMaps.Gameplay;
            if (Actions.Dialogue.enabled) map |= ActionMaps.Dialogue;
            
            if (Actions.Gameplay.MoveHorizontal.enabled) gameplayActions |= GameplayActions.MoveHorizontal;
            if (Actions.Gameplay.MoveJump.enabled) gameplayActions |= GameplayActions.MoveJump;
            if (Actions.Gameplay.Attack.enabled) gameplayActions |= GameplayActions.Attack;
            if (Actions.Gameplay.GrappleFire.enabled) gameplayActions |= GameplayActions.GrappleFire;
            if (Actions.Gameplay.GrappleStop.enabled) gameplayActions |= GameplayActions.GrappleStop;
            if (Actions.Gameplay.InteractionInteract.enabled) gameplayActions |= GameplayActions.InteractionInteract;
            if (Actions.Gameplay.InteractionNavigate.enabled) gameplayActions |= GameplayActions.InteractionNavigate;
            
            if (Actions.Dialogue.Advance.enabled) dialogueActions |= DialogueActions.Advance;
            
            return new InputState
            {
                ActionMaps =  map,
                GameplayActions =  gameplayActions,
                DialogueActions =  dialogueActions,
            };
        }

        public void RestoreState(InputState input)
        {
            SetMapStatus(InputStatus.Enabled, input.ActionMaps);
            SetMapStatus(InputStatus.Disabled, ~input.ActionMaps);
            
            SetActionStatus(InputStatus.Enabled, input.GameplayActions);
            SetActionStatus(InputStatus.Disabled, ~input.GameplayActions);
            
            SetActionStatus(InputStatus.Enabled, input.DialogueActions);
            SetActionStatus(InputStatus.Disabled, ~input.DialogueActions);
        }
    }
}
