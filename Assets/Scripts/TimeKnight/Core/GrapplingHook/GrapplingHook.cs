using System;
using System.Collections;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeKnight.Core.GrapplingHook
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class GrapplingHook : MonoBehaviour
    {
        private SpriteRenderer _sr = null!;

        [Header("Grapple Properties")]
        [SerializeField] private float maxLength = 10f;
        [SerializeField] private float fireSpeed = 5f;
        [SerializeField] private float retractSpeed = 10f;
        [SerializeField] private float pullSpeed = 10;
        [field: SerializeField] public LayerMask GrappleSurfaceLayer { get; private set; }
        private float _baseLength = 0;
        private float _currentLength => _sr.size.x;

        [Header("Hook TIp")]
        [SerializeField] private GameObject hookTip = null!;
        private SpriteRenderer _hookTipSpriteRenderer = null!;
        private Transform _hookTipTransform = null!;

        // Callbacks
        public event Action OnEnterIdle =  delegate {};
        public event Action OnExitIdle = delegate {};
        public event Action<Vector3> OnEnterStuck = delegate {};

        // Hook Rotation Management
        private Vector3 _collisionPoint;
        private Vector3 _firingPoint;
        private float _directionModifier => IsPlayerFacingLeft() ? -1 : 1;

        // Properties
        public HookState CurrentState { get; private set; } = HookState.Idle;
        public float PullSpeed => pullSpeed;

        private void OnValidate()
        {
            Validation.NotNull(this, hookTip, nameof(hookTip));
        }

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _hookTipSpriteRenderer = hookTip.GetComponent<SpriteRenderer>();
            _hookTipTransform = hookTip.GetComponent<Transform>();
        }

        private void Start()
        {
            StartCoroutine(UpdateIdle());
        }

        public void TransitionTo(HookState newState)
        {
            if (CurrentState == newState) return;

            // Exit current state
            switch (CurrentState)
            {
                case HookState.Idle: ExitIdle(); break;
                case HookState.Extending:
                case HookState.Retracting:
                case HookState.Stuck:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Set new state
            CurrentState = newState;

            // Enter new state
            switch (newState)
            {
                case HookState.Stuck: EnterStuck(); break;
                case HookState.Idle: EnterIdle(); break;
                case HookState.Extending:
                case HookState.Retracting:
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            // Update new state
            switch (newState)
            {
                case HookState.Idle: StartCoroutine(UpdateIdle()); break;
                case HookState.Extending: StartCoroutine(UpdateExtending()); break;
                case HookState.Retracting: StartCoroutine(UpdateRetracting()); break;
                case HookState.Stuck: StartCoroutine(UpdateStuck()); break;
                default: throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        private void EnterIdle()
        {
            HideSprites();
            OnEnterIdle.Invoke();
        }

        private IEnumerator UpdateIdle()
        {
            while (CurrentState.IsIdle())
            {
                var mousePosition = Mouse.current.position.ReadValue();
                if (IsMouseInbounds(mousePosition))
                {
                    // Rotate grappling hook to face mouse so it can be fired later.
                    // Convert to a world position - Z axis set to 0 because depth doesn't matter for this object.
                    var mouseWorldPos = Camera.main!.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
                    RotateGrapplingHook(mouseWorldPos);
                }

                yield return null;
            }
        }

        private void ExitIdle()
        {
            ShowSprites();
            OnExitIdle.Invoke();
        }

        private void HideSprites()
        {
            _sr.enabled = false;
            _hookTipSpriteRenderer.enabled = false;
        }

        private void ShowSprites()
        {
            _sr.enabled = true;
            _hookTipSpriteRenderer.enabled = true;
        }

        private IEnumerator UpdateExtending()
        {   
            float _maxGrappleDistance = maxLength * _directionModifier;

            // Find a point in space for the grappling hook to aim towards/rotate around.
            _firingPoint = transform.position + transform.right * _maxGrappleDistance;

            RaycastHit2D wallHit = Physics2D.Raycast(transform.position, transform.right, _maxGrappleDistance, GrappleSurfaceLayer);
            if (wallHit.collider != null)
            {
                _collisionPoint = wallHit.point;
                _firingPoint = wallHit.point;       // Update firing point to keep rotation consistent with eventual collision point.
            }

            while (CurrentState.IsExtending())
            {
                // First check to see if we have reached collision point.
                if (wallHit.collider != null && _currentLength >= GetDistanceTo(_collisionPoint))
                {
                    TransitionTo(HookState.Stuck);
                    yield break;
                }

                if (_currentLength < GetDistanceTo(_firingPoint))
                {
                    RotateGrapplingHook(_firingPoint);  // Keeps grappling hook firing into the same direction regardless of player movement.
                    float newLength = _currentLength + (fireSpeed * Time.deltaTime);
                    UpdateLength(newLength);
                }
                else
                {
                    TransitionTo(HookState.Retracting);
                    yield break;
                }

                yield return null;
            }
        }

        private IEnumerator UpdateRetracting()
        {
            while (CurrentState.IsRetracting())
            {
                if (_currentLength > _baseLength)
                {
                    RotateGrapplingHook(_firingPoint);  // Keeps grappling hook retracting from the same direction it was fired from.
                    float newLength = _currentLength - (retractSpeed * Time.deltaTime);
                    UpdateLength(newLength);
                    yield return null;
                }
                else
                {
                    UpdateLength(_baseLength); // There might be some variance when subtracting length on last frame due to Time.deltaTime, this resets it to base length. 
                    TransitionTo(HookState.Idle);
                }
            }
            yield return null;
        }

        private void EnterStuck()
        {
            OnEnterStuck.Invoke(_collisionPoint);
        }

        private IEnumerator UpdateStuck()
        {
            while (CurrentState.IsStuck())
            {
                RotateGrapplingHook(_collisionPoint);
                float distance = GetDistanceTo(_collisionPoint);
                UpdateLength(distance);
                yield return null;
            }
        }

        private void UpdateLength(float newLength)
        {
            _sr.size = new Vector2(newLength, _sr.size.y);
            _hookTipTransform.localPosition = new Vector3(newLength, 0f, 0f);
        }

        private static bool IsMouseInbounds(Vector2 mousePosition)
        {
            return
                mousePosition.x > 0 &&
                mousePosition.x < Screen.width &&
                mousePosition.y > 0 &&
                mousePosition.y < Screen.height;
        }

        private float GetDistanceTo(Vector2 other)
        {
            return Vector2.Distance(transform.position, other);
        }

        private void RotateGrapplingHook(Vector3 otherPosition)
        {
            // Calculate x and y components of the vector, then get the angle using atan2.
            float angle = GetAngleTo(otherPosition);

            // Apply rotation using Quaternions.
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private bool IsPlayerFacingLeft()
        {
            return transform.parent != null && transform.parent.localScale.x < 0f;
        }

        private float GetAngleTo(Vector3 otherPosition)
        {
            Vector2 direction = (otherPosition - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // If player is facing left, the math gets flipped so add 180 to angle
            if (IsPlayerFacingLeft())
            {
                angle += 180f;
            }
            return angle;
        }
    }
}
