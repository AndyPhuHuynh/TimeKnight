using System.Collections;
using TimeKnight.Core.GrapplingHook;
using TimeKnight.Core.Input;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeKnight.Core.Player
{
    public class PlayerGrapplingHookController : MonoBehaviour
    {
        [Header("Input")] 
        [SerializeField] private InputReader input;

        [Header("Player")] 
        [SerializeField] private Rigidbody2D playerBody;
        [SerializeField] private PlayerJumpController playerJump;
        
        [Header("Grappling Hook")]
        [SerializeField] private GrapplingHook.GrapplingHook grapplingHook;

        // Pull player state
        private bool _isBeingPulled;
        private float _prevGravity;

        private void OnValidate()
        {
            Debug.Assert(input         != null, $"Missing {nameof(input)}",         this);
            Debug.Assert(playerBody    != null, $"Missing {nameof(playerBody)}",    this);
            Debug.Assert(playerJump    != null, $"Missing {nameof(playerJump)}",    this);
            Debug.Assert(grapplingHook != null, $"Missing {nameof(grapplingHook)}", this);
        }
        
        private void OnEnable()
        {
            input.Actions.GrapplingHook.PrimaryFire.performed += OnPrimaryFirePerformed;
            input.Actions.GrapplingHook.StopGrapple.performed += OnStopGrapple;
            grapplingHook.OnStuckEnter += OnStuckEnter;
        }

        private void OnDisable()
        {
            input.Actions.GrapplingHook.PrimaryFire.performed -= OnPrimaryFirePerformed;
            input.Actions.GrapplingHook.StopGrapple.performed -= OnStopGrapple;
            grapplingHook.OnStuckEnter -= OnStuckEnter;
        }

        private void OnPrimaryFirePerformed(InputAction.CallbackContext _)
        {
            grapplingHook.TransitionTo(HookState.Extending);
        }

        private void OnStopGrapple(InputAction.CallbackContext _)
        {
            _isBeingPulled = false;
            
            playerBody.gravityScale = _prevGravity;
            playerBody.linearVelocity = Vector2.zero;
            
            input.Actions.Player.HorizontalMove.Enable();
            input.Actions.GrapplingHook.StopGrapple.Disable();
            
            playerJump.ApplyJump(() => input.Actions.GrapplingHook.StopGrapple.IsPressedRegardlessOfEnableStatus());
            grapplingHook.TransitionTo(HookState.Retracting);
        }
        
        private void OnStuckEnter(Vector3 collisionPoint)
        {
            StartCoroutine(PullPlayer(collisionPoint, grapplingHook.PullSpeed));
            input.Actions.Player.HorizontalMove.Disable();
            input.Actions.GrapplingHook.StopGrapple.Enable();
        }

        private IEnumerator PullPlayer(Vector3 targetPosition, float pullSpeed)
        {
            _prevGravity = playerBody.gravityScale;
            playerBody.gravityScale = 0;
        
            _isBeingPulled = true;
            while (_isBeingPulled)
            {
                Vector2 pullVelocity = ((Vector2)(targetPosition - transform.position)).normalized * pullSpeed;
                playerBody.linearVelocity = pullVelocity;
                yield return null;
            }
        }
    }
}