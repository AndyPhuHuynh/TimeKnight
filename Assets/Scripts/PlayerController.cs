using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private InputAction _moveAction;
    private InputAction _jumpAction;

    private Rigidbody2D _rb;

    private bool _startJump = false;

    private Vector2 _currentMovement;

    [SerializeField]
    private float _moveSpeed = 5;

    [SerializeField]
    private float _jumpForce = 20;

    [SerializeField]
    private Vector2 _groundCheckDimenisons = new Vector2(0.7f, 0.2f);
    [SerializeField]
    private LayerMask _groundLayer;

    private bool _isGrounded = false;

    private void Awake()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
        _jumpAction = InputSystem.actions.FindAction("Jump");
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update gets player input.
    private void Update()
    {
        CheckForGround();

        if (_jumpAction.WasPressedThisFrame() && _isGrounded)
        {
            _startJump = true;
        }

        _currentMovement = _moveAction.ReadValue<Vector2>();
    }

    // Physics adjustments.
    private void FixedUpdate()
    {
        if (_startJump)
        {
            _rb.linearVelocityY += _jumpForce;
            _startJump = false;
        }

        // Only adjusts horizontal movement for now
        _rb.linearVelocity = new Vector2(_currentMovement.x * _moveSpeed, _rb.linearVelocityY);
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
