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
    private bool _jumpPressed = false;

    // Movement variables
    [SerializeField] private float _maxMoveSpeed = 5;
    [SerializeField] private float _acceleration = 1;
    private float _currentMoveSpeed = 0;

    // Jump Variables
    [SerializeField] private float _baseJumpForce = 10;
    [SerializeField] private float _holdJumpForce = 2;
    [SerializeField] private int _holdJumpUpdates = 15;
    private Coroutine _jumpCoroutine;

    // Grounding Variables
    [SerializeField] private Vector2 _groundCheckDimenisons = new Vector2(0.7f, 0.2f);
    [SerializeField] private LayerMask _groundLayer;
    private bool _isGrounded = false;

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

        _sr.flipX = _currentMovementInput.x < 0 ? true : false;
    }

    private IEnumerator ApplyJump()
    {

        _rb.linearVelocityY += _baseJumpForce;

        //_rb.AddForce(Vector2.up * _baseJumpForce, ForceMode2D.Impulse);
        yield return null;

        for (int i = 0; i < _holdJumpUpdates; i++)
        {
            if (!_jumpPressed) break;

            // After a couple frames of jumping, if we hit the ground then end jump early.
            if (i > 2 && _isGrounded)
            {
                _rb.linearVelocityY = 0;
                break;
            }
            
            _rb.linearVelocityY += _holdJumpForce;

            yield return new WaitForFixedUpdate();  // Keeps synced with physics calculations.
        }

        _jumpCoroutine = null;
    }

    private void ApplyMovement()
    {
        // Calculate move speed by taking the min of the maxMoveSpeed and adding acceleration.
        // Multiplied by absolute value of horizontal input to zero out current speed when released.
        _currentMoveSpeed = Math.Min(_currentMoveSpeed + _acceleration, _maxMoveSpeed) * Math.Abs(_currentMovementInput.x);

        // Multiply by horizontal movement again to capture direction of input.
        _rb.linearVelocity = new Vector2(_currentMovementInput.x * _currentMoveSpeed, _rb.linearVelocityY);
    }

    private void CheckForGround()
    {
        _isGrounded = Physics2D.BoxCast(transform.position, _groundCheckDimenisons, 0f, -transform.up, 0.1f, _groundLayer);
    }

    // Used to visualize the CheckForGround box.
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position, (Vector3)_groundCheckDimenisons);
    }
}
