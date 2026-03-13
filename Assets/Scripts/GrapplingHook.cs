using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHook : MonoBehaviour
{
    private InputAction _fireHook;

    [SerializeField] private float baseLength = 3.48f;
    [SerializeField] private float maxLength = 10f;
    [SerializeField] private float fireSpeed = 5f;
    [SerializeField] private float retractSpeed = 10f;
    [SerializeField] private Transform tipTransform;
    //[SerializeField] private Rigidbody2D playerRigidBody;
    [SerializeField] private PlayerController playerController;

    private SpriteRenderer _sr;
    private float _pullSpeed = 10;

    private Coroutine _fireCoroutine = null;    // Reference to coroutine needed so it can be ended early when hook collides.
    private enum HookState
    {
        Idle,
        Extending,
        Retracting,
        Stuck
    }

    private HookState _currentState = HookState.Idle;

    private Vector3 _collisionPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _fireHook = InputSystem.actions.FindAction("Primary Fire");
        _sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        if (IsStuck())
        {
            rotateGrapplingHook(_collisionPoint);

            // Times 2 here is to account for the 0.5 scale currently applied to the chain. Going to change sprite soon to avoid scaling issues like this.
            float distance = Vector2.Distance(transform.position, _collisionPoint) * 2;
            updateLength(distance);
        }

        if (IsIdle() && isMouseInbounds(mousePosition))
        {
            // Rotate grappling hook to face mouse so it can be fired later.
            // Convert to a world position - Z axis set to 0 because depth doesn't matter for this object.
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
            rotateGrapplingHook(mouseWorldPos);


            if (_fireHook.WasPressedThisFrame())
            {
                _fireCoroutine = StartCoroutine(fireGrapplingHook());
            }
        }
    }

    private IEnumerator fireGrapplingHook()
    {
        _currentState = HookState.Extending;
        while (_sr.size.x < maxLength)
        {
            float newLength = _sr.size.x + (fireSpeed * Time.deltaTime);
            updateLength(newLength);
            yield return null;
        }

        yield return retractGrapplingHook();

        _fireCoroutine = null;
    }

    private IEnumerator retractGrapplingHook()
    {
        _currentState = HookState.Retracting;
        while (_sr.size.x > baseLength)
        {
            float newLength = _sr.size.x - (retractSpeed * Time.deltaTime);
            updateLength(newLength);
            yield return null;
        }
        updateLength(baseLength); // There might be some variance when subtracting length on last frame due to Time.deltaTime, this resets it to base length. 
        _currentState = HookState.Idle;
    }

    private IEnumerator BeginHookPull(Vector3 otherPosition) 
    {
        yield return StartCoroutine(playerController.PullPlayer(otherPosition, _pullSpeed));

        StartCoroutine(retractGrapplingHook());
    }

    private void updateLength(float newLength)
    {
        _sr.size = new Vector2(newLength, _sr.size.y);
        tipTransform.localPosition = new Vector3(newLength, 0f, 0f);
    }

    private bool isMouseInbounds(Vector2 mousePosition)
    {
        if (mousePosition == null) return false;

        if (mousePosition.x > 0 && mousePosition.x < Screen.width && mousePosition.y > 0 && mousePosition.y < Screen.height) return true;
        return false;
    }

    private void rotateGrapplingHook(Vector3 otherPosition)
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
    private bool IsRetracting() => _currentState == HookState.Retracting;
}
