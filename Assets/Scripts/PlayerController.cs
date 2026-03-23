using System.Collections;
using TimeKnight.Core.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;

    [Header("Input")] 
    [SerializeField] private InputReader input;
    
    [Header("Jump")]
    [SerializeField] private float baseJumpForce = 10;
    [SerializeField] private float holdJumpForce = 2;
    [SerializeField] private int holdJumpUpdates = 10;
    private Coroutine _jumpCoroutine;

    // Grounding Variables
    [SerializeField] private GroundCheck groundCheck;
    
    private bool _isBeingPulled;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        input.Actions.Player.Jump.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        input.Actions.Player.Jump.performed -= OnJumpPerformed;
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

    public IEnumerator PullPlayer(Vector3 targetPosition, float pullSpeed)
    {
        input.Actions.Player.HorizontalMove.Disable();
        float previousGravity = _rb.gravityScale;
        _rb.gravityScale = 0;
        _isBeingPulled = true;

        while (true)
        {
            if (input.Actions.Player.Jump.WasPressedThisFrame())
            {
                _rb.gravityScale = previousGravity;
                _rb.linearVelocity = new Vector2(0, 0);
                input.Actions.Player.HorizontalMove.Enable();
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