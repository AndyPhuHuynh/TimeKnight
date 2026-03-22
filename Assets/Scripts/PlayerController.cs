using System;
using System.Collections;
using TimeKnight.Core.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;

    [Header("Input")] 
    [SerializeField] private InputReader input;
    
    [Header("Movement")]
    [SerializeField] private float maxMoveSpeed = 5;
    [SerializeField] private float acceleration = 1;
    private float _currentMoveSpeed;
    private bool _isBeingPulled;
    private Coroutine _onMoveHeldCoroutine;

    [Header("Jump")]
    [SerializeField] private float baseJumpForce = 10;
    [SerializeField] private float holdJumpForce = 2;
    [SerializeField] private int holdJumpUpdates = 10;
    private Coroutine _jumpCoroutine;

    // Grounding Variables
    [SerializeField] private GroundCheck groundCheck;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        input.Actions.Player.Move.started   += OnMoveStarted;
        input.Actions.Player.Move.performed += OnMovePerformed;
        input.Actions.Player.Move.canceled  += OnMoveCanceled;
        input.Actions.Player.Jump.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        input.Actions.Player.Move.started   -= OnMoveStarted;
        input.Actions.Player.Move.performed -= OnMovePerformed;
        input.Actions.Player.Move.canceled  -= OnMoveCanceled;
        input.Actions.Player.Jump.performed -= OnJumpPerformed;
    }

    private void OnMoveStarted(InputAction.CallbackContext _)
    {
        _onMoveHeldCoroutine = StartCoroutine(OnMovementHeld());
    }

    private void OnMovePerformed(InputAction.CallbackContext _)
    {
        UpdateSpriteDirection();
    }

    private void OnMoveCanceled(InputAction.CallbackContext _)
    {
        StopCoroutine(_onMoveHeldCoroutine);
        _onMoveHeldCoroutine = null;
        _currentMoveSpeed = 0;
        _rb.linearVelocity = Vector2.zero;
    }
    
    private void OnJumpPerformed(InputAction.CallbackContext _)
    {
        if (input.Actions.Player.Jump.WasPressedThisFrame() && 
            groundCheck.IsGrounded && 
            _jumpCoroutine == null && 
            !_isBeingPulled)
        {
            _jumpCoroutine = StartCoroutine(ApplyJump());
        }
    }
    
    private void UpdateSpriteDirection()
    {
        _sr.flipX = input.Actions.Player.Move.ReadValue<float>() < 0;
    }

    private IEnumerator ApplyJump()
    {
        _rb.linearVelocityY += baseJumpForce;
        yield return null;

        for (int i = 0; i < holdJumpUpdates; i++)
        {
            if (!input.Actions.Player.Jump.IsPressed()) break;
            _rb.linearVelocityY += holdJumpForce;
            yield return new WaitForFixedUpdate();  // Keeps synced with physics calculations.
        }

        _jumpCoroutine = null;
    }

    private void ApplyMovement()
    {
        // Calculate move speed by taking the min of the maxMoveSpeed and adding acceleration.
        _currentMoveSpeed = Math.Min(_currentMoveSpeed + acceleration, maxMoveSpeed);

        // Multiply by horizontal movement again to capture direction of input.
        var moveInput = input.Actions.Player.Move.ReadValue<float>();
        _rb.linearVelocity = new Vector2(moveInput * _currentMoveSpeed, _rb.linearVelocityY);
    }

    private IEnumerator OnMovementHeld()
    {
        while (input.Actions.Player.Move.IsPressed())
        {
            ApplyMovement();
            yield return null;
        }
    }

    public IEnumerator PullPlayer(Vector3 targetPosition, float pullSpeed)
    {
        input.Actions.Player.Move.Disable();
        float previousGravity = _rb.gravityScale;
        _rb.gravityScale = 0;
        _isBeingPulled = true;

        while (true)
        {
            if (input.Actions.Player.Jump.WasPressedThisFrame())
            {
                _rb.gravityScale = previousGravity;
                _rb.linearVelocity = new Vector2(0, 0);
                input.Actions.Player.Move.Enable();
                _jumpCoroutine = StartCoroutine(ApplyJump());
                _isBeingPulled = false;
                break; 
            }

            Vector2 pullVelocity = ((Vector2)(targetPosition - transform.position)).normalized * pullSpeed;

            _rb.linearVelocity = pullVelocity;
            yield return null;
        }
    }
}