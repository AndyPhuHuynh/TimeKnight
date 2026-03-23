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
        [SerializeField] private GrapplingHookTip grapplingHookTip;

        // Pull player state
        private bool _isBeingPulled;
        private float _prevGravity;

        private void OnValidate()
        {
            Debug.Assert(input            != null, $"Missing {nameof(input)}",            this);
            Debug.Assert(playerBody       != null, $"Missing {nameof(playerBody)}",       this);
            Debug.Assert(playerJump       != null, $"Missing {nameof(playerJump)}",       this);
            Debug.Assert(grapplingHook    != null, $"Missing {nameof(grapplingHook)}",    this);
            Debug.Assert(grapplingHookTip != null, $"Missing {nameof(grapplingHookTip)}", this);
        }

        private void OnEnable()
        {
            input.Actions.GrapplingHook.PrimaryFire.performed += OnPrimaryFirePerformed;
            input.Actions.GrapplingHook.StopGrapple.performed += OnStopGrapplePerformed;
            grapplingHook.OnEnterIdle += OnEnterIdle;
            grapplingHook.OnExitIdle += OnExitIdle;
            grapplingHook.OnEnterStuck += OnEnterStuck;
        }

        private void OnDisable()
        {
            input.Actions.GrapplingHook.PrimaryFire.performed -= OnPrimaryFirePerformed;
            input.Actions.GrapplingHook.StopGrapple.performed -= OnStopGrapplePerformed;
            grapplingHook.OnEnterIdle -= OnEnterIdle;
            grapplingHook.OnExitIdle -= OnExitIdle;
            grapplingHook.OnEnterStuck -= OnEnterStuck;
        }

        private void OnPrimaryFirePerformed(InputAction.CallbackContext _)
        {
            if (grapplingHookTip.IsTipTouchingGround) return;
            grapplingHook.TransitionTo(HookState.Extending);
        }

        private void OnStopGrapplePerformed(InputAction.CallbackContext _)
        {
            if (!grapplingHook.CurrentState.IsStuck()) return;
            _isBeingPulled = false;

            playerBody.gravityScale = _prevGravity;
            playerBody.linearVelocity = Vector2.zero;

            input.Actions.Player.Enable();
            input.Actions.GrapplingHook.StopGrapple.Disable();
            
            playerJump.ApplyJump(() => input.Actions.GrapplingHook.StopGrapple.IsPressedRegardlessOfEnableStatus());
            grapplingHook.TransitionTo(HookState.Retracting);
        }

        private void OnEnterIdle()
        {
            input.Actions.GrapplingHook.PrimaryFire.Enable();
        }

        private void OnExitIdle()
        {
            input.Actions.GrapplingHook.PrimaryFire.Disable();
        }
        
        private void OnEnterStuck(Vector3 collisionPoint)
        {
            StartCoroutine(PullPlayer(collisionPoint, grapplingHook.PullSpeed));
            input.Actions.Player.Disable();
            input.Actions.GrapplingHook.StopGrapple.Enable();
        }

        private IEnumerator PullPlayer(Vector3 targetPosition, float pullSpeed)
        {
            _prevGravity = playerBody.gravityScale;
            playerBody.gravityScale = 0;

            _isBeingPulled = true;
            while (_isBeingPulled)
            {
                var pullVelocity = ((Vector2)(targetPosition - transform.position)).normalized * pullSpeed;
                playerBody.linearVelocity = pullVelocity;
                yield return null;
            }
        }
    }
}