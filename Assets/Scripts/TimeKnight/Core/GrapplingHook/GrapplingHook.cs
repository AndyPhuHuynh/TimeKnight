using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeKnight.Core.GrapplingHook
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class GrapplingHook : MonoBehaviour
    {
        private SpriteRenderer _sr;

        [Header("Hook Tip")] 
        [SerializeField] private Transform tipTransform;

        [Header("Grapple Properties")]
        [SerializeField] private float baseLength = 3.48f;
        [SerializeField] private float maxLength = 10f;
        [SerializeField] private float fireSpeed = 5f;
        [SerializeField] private float retractSpeed = 10f;
        [SerializeField] private float pullSpeed = 10;
        
        // Callbacks
        public event Action OnEnterIdle;
        public event Action OnExitIdle;
        public event Action<Vector3> OnEnterStuck;
        
        // State management
        private Vector3 _collisionPoint;
        
        // Properties
        public HookState CurrentState { get; private set; } = HookState.Idle;
        public float PullSpeed => pullSpeed;
        
        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            StartCoroutine(UpdateIdle());
        }
        
        public void TransitionTo(HookState newState)
        {
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
            if (CurrentState == newState) return;
            CurrentState = newState;
            
            // Enter new state
            switch (newState)
            {
                case HookState.Stuck: EnterStuck(); break;
                case HookState.Idle:  EnterIdle();  break;
                case HookState.Extending:
                case HookState.Retracting:
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
            
            // Update new state
            switch (newState)
            {
                case HookState.Idle:       StartCoroutine(UpdateIdle());       break;
                case HookState.Extending:  StartCoroutine(UpdateExtending());  break;
                case HookState.Retracting: StartCoroutine(UpdateRetracting()); break;
                case HookState.Stuck:      StartCoroutine(UpdateStuck());      break;
                default: throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        private void EnterIdle()
        {
            OnEnterIdle?.Invoke();
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
                    var mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
                    RotateGrapplingHook(mouseWorldPos);
                }
                
                yield return null;
            }
        }

        private void ExitIdle()
        {
            OnExitIdle?.Invoke();
        }

        private IEnumerator UpdateExtending()
        {
            while (CurrentState.IsExtending())
            {
                if (_sr.size.x < maxLength)
                {
                    float newLength = _sr.size.x + (fireSpeed * Time.deltaTime);
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
                if (_sr.size.x > baseLength)
                {
                    float newLength = _sr.size.x - (retractSpeed * Time.deltaTime);
                    UpdateLength(newLength);
                    yield return null;
                }
                else
                {
                    UpdateLength(baseLength); // There might be some variance when subtracting length on last frame due to Time.deltaTime, this resets it to base length. 
                    TransitionTo(HookState.Idle);                    
                }
            }
            yield return null;
        }

        private void EnterStuck()
        {
            _collisionPoint = tipTransform.position;
            OnEnterStuck?.Invoke(_collisionPoint);
        }
        
        private IEnumerator UpdateStuck()
        {
            while (CurrentState.IsStuck())
            {
                RotateGrapplingHook(_collisionPoint);
                float distance = Vector2.Distance(transform.position, _collisionPoint);
                UpdateLength(distance);
                yield return null;
            }
        }
        
        private void UpdateLength(float newLength)
        {
            _sr.size = new Vector2(newLength, _sr.size.y);
            tipTransform.localPosition = new Vector3(newLength, 0f, 0f);
        }
        
        private static bool IsMouseInbounds(Vector2 mousePosition)
        {
            return 
                mousePosition.x > 0 && 
                mousePosition.x < Screen.width && 
                mousePosition.y > 0 && 
                mousePosition.y < Screen.height;
        }
        
        private void RotateGrapplingHook(Vector3 otherPosition)
        {
            // Calculate x and y components of the vector, then get the angle using atan2.
            float angle = GetAngleTo(otherPosition);

            // Apply rotation using Quaternions.
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        private float GetAngleTo(Vector3 otherPosition)
        {
            Vector2 direction = (otherPosition - transform.position).normalized;
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
    }
}