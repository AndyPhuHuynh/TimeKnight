using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;

    // Input variables
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private Vector2 _currentMovementInput;
    private bool _jumpPressed;

    [Header("Movement")]
    [SerializeField] private float maxMoveSpeed = 5;
    [SerializeField] private float acceleration = 1;
    private float _currentMoveSpeed;
    private bool _isBeingPulled = false;

    [Header("Jump")]
    [SerializeField] private float baseJumpForce = 10;
    [SerializeField] private float holdJumpForce = 2;
    [SerializeField] private int holdJumpUpdates = 10;
    private Coroutine _jumpCoroutine;

    // Grounding Variables
    [SerializeField] private GroundCheck _groundCheck;

    private void Awake()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
        _jumpAction = InputSystem.actions.FindAction("Jump");
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
    }

    // Update gets player input.
    private void Update()
    {
        CheckForJumpInput();

        _currentMovementInput = _moveAction.ReadValue<Vector2>();
    }

    // Physics adjustments.
    private void FixedUpdate()
    {
        UpdateSpriteDirection();
        ApplyMovement();
    }

    private void UpdateSpriteDirection()
    {
        if (_currentMovementInput.x == 0) return;
        _sr.flipX = _currentMovementInput.x < 0;
    }

    public void CheckForJumpInput()
    {
        _jumpPressed = _jumpAction.IsPressed();

        if (_jumpAction.WasPressedThisFrame() && _groundCheck.isGrounded && _jumpCoroutine == null && !_isBeingPulled)
        {
            _jumpCoroutine = StartCoroutine(ApplyJump());
        }
    }

    private IEnumerator ApplyJump()
    {

        _rb.linearVelocityY += baseJumpForce;

        //_rb.AddForce(Vector2.up * _baseJumpForce, ForceMode2D.Impulse);
        yield return null;

        for (int i = 0; i < holdJumpUpdates; i++)
        {
            if (!_jumpPressed) break;

            _rb.linearVelocityY += holdJumpForce;

            yield return new WaitForFixedUpdate();  // Keeps synced with physics calculations.
        }

        _jumpCoroutine = null;
    }

    private void ApplyMovement()
    {
        if (!_moveAction.enabled) return;

        // Calculate move speed by taking the min of the maxMoveSpeed and adding acceleration.
        // Multiplied by absolute value of horizontal input to zero out current speed when released.
        _currentMoveSpeed = Math.Min(_currentMoveSpeed + acceleration, maxMoveSpeed) * Math.Abs(_currentMovementInput.x);

        // Multiply by horizontal movement again to capture direction of input.
        _rb.linearVelocity = new Vector2(_currentMovementInput.x * _currentMoveSpeed, _rb.linearVelocityY);
    }

    public IEnumerator PullPlayer(Vector3 targetPosition, float _pullSpeed)
    {
        _moveAction.Disable();
        float previousGravity = _rb.gravityScale;
        _rb.gravityScale = 0;
        _isBeingPulled = true;

        while (true)
        {
            if (_jumpAction.WasPressedThisFrame())
            {
                _rb.gravityScale = previousGravity;
                _rb.linearVelocity = new Vector2(0, 0);
                _moveAction.Enable();
                _jumpCoroutine = StartCoroutine(ApplyJump());
                _isBeingPulled = false;
                break; 
            }

            Vector2 pullVelocity = ((Vector2)(targetPosition - transform.position)).normalized * _pullSpeed;

            _rb.linearVelocity = pullVelocity;
            yield return null;
        }
    }
}