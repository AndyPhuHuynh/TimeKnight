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

    [Header("Jump")]
    [SerializeField] private float baseJumpForce = 10;
    [SerializeField] private float holdJumpForce = 2;
    [SerializeField] private int holdJumpUpdates = 10;
    private Coroutine _jumpCoroutine;

    // Grounding Variables
    [SerializeField] private Vector2 groundCheckDimensions = new(0.7f, 0.2f);
    private LayerMask _groundLayer;
    private bool _isGrounded;

    private void Awake()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
        _jumpAction = InputSystem.actions.FindAction("Jump");
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _groundLayer = LayerMask.GetMask("Ground");
    }

    // Update gets player input.
    private void Update()
    {
        CheckForGround();

        _jumpPressed = _jumpAction.IsPressed();

        if (_jumpAction.WasPressedThisFrame() && _isGrounded && _jumpCoroutine == null)
        {
            _jumpCoroutine = StartCoroutine(ApplyJump());
        } 

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

    private IEnumerator ApplyJump()
    {

        _rb.linearVelocityY += baseJumpForce;

        //_rb.AddForce(Vector2.up * _baseJumpForce, ForceMode2D.Impulse);
        yield return null;

        for (int i = 0; i < holdJumpUpdates; i++)
        {
            if (!_jumpPressed) break;

            // After a couple frames of jumping, if we hit the ground then end jump early.
            if (i > 2 && _isGrounded)
            {
                _rb.linearVelocityY = 0;
                break;
            }
            
            _rb.linearVelocityY += holdJumpForce;

            yield return new WaitForFixedUpdate();  // Keeps synced with physics calculations.
        }

        _jumpCoroutine = null;
    }

    private void ApplyMovement()
    {
        // Calculate move speed by taking the min of the maxMoveSpeed and adding acceleration.
        // Multiplied by absolute value of horizontal input to zero out current speed when released.
        _currentMoveSpeed = Math.Min(_currentMoveSpeed + acceleration, maxMoveSpeed) * Math.Abs(_currentMovementInput.x);

        // Multiply by horizontal movement again to capture direction of input.
        _rb.linearVelocity = new Vector2(_currentMovementInput.x * _currentMoveSpeed, _rb.linearVelocityY);
    }

    private void CheckForGround()
    {
        _isGrounded = Physics2D.BoxCast(transform.position, groundCheckDimensions, 0f, -transform.up, 0.1f, _groundLayer);
    }

    // Used to visualize the CheckForGround box.
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position, groundCheckDimensions);
    }
}
