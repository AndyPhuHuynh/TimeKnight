using System;
using System.Collections;
using TimeKnight.Core.Input;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeKnight.Core.Player
{
    public class PlayerJumpController : MonoBehaviour
    {
        private Rigidbody2D _rb = null!;

        [Header("Input")] 
        [SerializeField] private InputReader input = null!;
    
        [Header("Jump")]
        [SerializeField] private float baseJumpForce = 10;
        [SerializeField] private float holdJumpForce = 2;
        [SerializeField] private int holdJumpUpdates = 10;
        private Coroutine? _jumpCoroutine;

        // Grounding Variables
        [SerializeField] private GroundCheck groundCheck = null!;

        private void OnValidate()
        {
            Validation.NotNull(this, input, nameof(input));
        }
        
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
            if (groundCheck.IsGrounded && _jumpCoroutine == null)
            {
                ApplyJump(() => input.Actions.Player.Jump.IsPressed());
            }
        }
        
        public void ApplyJump(Func<bool> shouldContinueJump)
        {
            _jumpCoroutine = StartCoroutine(ApplyJumpCoroutine(shouldContinueJump));
        }

        private IEnumerator ApplyJumpCoroutine(Func<bool> shouldContinueJump)
        {
            _rb.linearVelocityY += baseJumpForce;
            yield return null;

            for (int i = 0; i < holdJumpUpdates; i++)
            {
                if (!shouldContinueJump()) break;

                _rb.linearVelocityY += holdJumpForce;
                yield return new WaitForFixedUpdate(); // Keeps synced with physics calculations.
            }

            _jumpCoroutine = null;
        }
    }
}