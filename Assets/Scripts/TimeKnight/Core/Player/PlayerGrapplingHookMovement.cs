using System;
using System.Collections;
using TimeKnight.Core.GrapplingHook;
using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.Player
{
    public class PlayerGrapplingHookMovement : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private Rigidbody2D playerBody = null!;
        [SerializeField] private PlayerHorizontalMovement playerHorizontalMovement = null!;

        [Header("Grappling Hook")]
        [SerializeField] private GrapplingHook.GrapplingHook grapplingHook = null!;

        [Header("Animator")]
        [SerializeField] private PlayerAnimator playerAnimator = null!;

        public HookState HookState => grapplingHook.CurrentState;
        public event Action OnGrappleEnterIdle = delegate { };
        public event Action OnGrappleExitIdle = delegate { };
        public event Action OnGrappleUpdateStuck = delegate { };
        public event Action<Vector3> OnGrappleEnterStuck = delegate { };

        private bool _isBeingPulled;
        private float _prevGravity;

        private void OnValidate()
        {
            Validation.NotNull(this, playerBody, nameof(playerBody));
            Validation.NotNull(this, grapplingHook, nameof(grapplingHook));
            Validation.NotNull(this, playerAnimator, nameof(playerAnimator));
            Validation.NotNull(this, playerHorizontalMovement, nameof(playerHorizontalMovement));
        }

        private void OnEnable()
        {
            grapplingHook.OnEnterIdle += OnEnterIdle;
            grapplingHook.OnExitIdle += OnExitIdle;
            grapplingHook.OnEnterStuck += OnEnterStuck;
            grapplingHook.OnUpdateStuck += OnUpdateStuck;
        }

        private void OnDisable()
        {
            grapplingHook.OnEnterIdle -= OnEnterIdle;
            grapplingHook.OnExitIdle -= OnExitIdle;
            grapplingHook.OnEnterStuck -= OnEnterStuck;
            grapplingHook.OnUpdateStuck -= OnUpdateStuck;
        }

        public void StartGrappling()
        {
            grapplingHook.TransitionTo(HookState.Extending);
        }

        public void StopGrappling()
        {
            if (!grapplingHook.CurrentState.IsStuck()) return;
            _isBeingPulled = false;

            playerBody.gravityScale = _prevGravity;
            playerBody.linearVelocity = Vector2.zero;

            grapplingHook.TransitionTo(HookState.Retracting);
        }

        public void InterruptGrapple()
        {
            if (grapplingHook.CurrentState.IsExtending())
            {
                grapplingHook.TransitionTo(HookState.Retracting);
            }
            else if (grapplingHook.CurrentState.IsStuck())
            {
                StopGrappling();
            }
        }

        private void OnEnterIdle()
        {
            OnGrappleEnterIdle.Invoke();
        }

        private void OnExitIdle()
        {
            OnGrappleExitIdle.Invoke();
        }

        private void OnEnterStuck(Vector3 collisionPoint)
        {
            StartCoroutine(PullPlayer(collisionPoint, grapplingHook.PullSpeed));
            OnGrappleEnterStuck.Invoke(collisionPoint);
        }

        private void OnUpdateStuck()
        {
            OnGrappleUpdateStuck.Invoke();
        }

        private IEnumerator PullPlayer(Vector3 targetPosition, float pullSpeed)
        {
            _prevGravity = playerBody.gravityScale;
            playerBody.gravityScale = 0;
            playerAnimator.SetTrigger(playerAnimator.GrappleTriggerHash);
            _isBeingPulled = true;
            while (_isBeingPulled)
            {
                // -0.3 offset to get to players feet
                bool isTargetBelowPlayer = targetPosition.y < transform.position.y - 0.35;
                if (Vector2.Distance(targetPosition, transform.position) < 2.5 && !isTargetBelowPlayer)
                {
                    playerAnimator.SetTrigger(playerAnimator.CloseToGrappleWallTriggerHash);
                    bool isWallToLeftOfPlayer = targetPosition.x < transform.position.x;
                    playerHorizontalMovement.UpdateSpriteDirection(isWallToLeftOfPlayer ? -1 : 1);
                }
                else if (isTargetBelowPlayer && Math.Abs(transform.position.x - targetPosition.x) < 0.3 && Vector2.Distance(targetPosition, transform.position) < 2)
                {
                    playerAnimator.SetTrigger(playerAnimator.CloseToGrappleFloorTriggerHash);
                }

                var pullVelocity = ((Vector2)(targetPosition - transform.position)).normalized * pullSpeed;
                playerBody.linearVelocity = pullVelocity;
                yield return null;
            }
        }
    }
}