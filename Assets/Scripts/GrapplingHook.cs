using System.Collections;
using TimeKnight.Core.Input;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class GrapplingHook : MonoBehaviour
{
    private SpriteRenderer _sr;

    // Input for firing
    private InputAction _fireHook;
    
    [Header("Input")] 
    [SerializeField] private InputReader input;

    [Header("Hook Tip")] 
    [SerializeField] private Transform tipTransform;

    [Header("Grapple Properties")]
    [SerializeField] private float baseLength = 3.48f;
    [SerializeField] private float maxLength = 10f;
    [SerializeField] private float fireSpeed = 5f;
    [SerializeField] private float retractSpeed = 10f;
    [SerializeField] private float pullSpeed = 10;

    // TODO: Initialize this with constructor
    [SerializeField] private PlayerController playerController;

    // State management
    private Coroutine _fireCoroutine; // Reference to coroutine needed so it can be ended early when hook collides.

    private enum HookState
    {
        Idle,
        Extending,
        Retracting,
        Stuck
    }

    private HookState _currentState = HookState.Idle;
    private Vector3 _collisionPoint;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _fireHook = input.Actions.GrapplingHook.PrimaryFire;
    }

    private void Update()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        if (IsStuck())
        {
            RotateGrapplingHook(_collisionPoint);

            float distance = Vector2.Distance(transform.position, _collisionPoint);
            UpdateLength(distance);
        }
        else if (IsIdle() && IsMouseInbounds(mousePosition))
        {
            // Rotate grappling hook to face mouse so it can be fired later.
            // Convert to a world position - Z axis set to 0 because depth doesn't matter for this object.
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
            RotateGrapplingHook(mouseWorldPos);

            if (_fireHook.WasPressedThisFrame())
            {
                _fireCoroutine = StartCoroutine(FireGrapplingHook());
            }
        }
    }

    private IEnumerator FireGrapplingHook()
    {
        _currentState = HookState.Extending;
        while (_sr.size.x < maxLength)
        {
            float newLength = _sr.size.x + (fireSpeed * Time.deltaTime);
            UpdateLength(newLength);
            yield return null;
        }

        yield return RetractGrapplingHook();

        _fireCoroutine = null;
    }

    private IEnumerator RetractGrapplingHook()
    {
        _currentState = HookState.Retracting;
        while (_sr.size.x > baseLength)
        {
            float newLength = _sr.size.x - (retractSpeed * Time.deltaTime);
            UpdateLength(newLength);
            yield return null;
        }
        UpdateLength(baseLength); // There might be some variance when subtracting length on last frame due to Time.deltaTime, this resets it to base length. 
        _currentState = HookState.Idle;
    }

    private IEnumerator BeginHookPull(Vector3 otherPosition)
    {
        yield return StartCoroutine(playerController.PullPlayer(otherPosition, pullSpeed));

        StartCoroutine(RetractGrapplingHook());
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

    public void HookTrigger()
    {
        // Only pay attention to collisions if fire coroutine is activated.
        if (!IsExtending()) return;
        StopCoroutine(_fireCoroutine);
        _fireCoroutine = null;
        _currentState = HookState.Stuck;
        _collisionPoint = tipTransform.position;
        StartCoroutine(BeginHookPull(_collisionPoint));
    }

    private bool IsIdle() => _currentState == HookState.Idle;
    private bool IsExtending() => _currentState == HookState.Extending;
    private bool IsStuck() => _currentState == HookState.Stuck;
}
