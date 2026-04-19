using System;
using System.Collections;
using TimeKnight.Core.Input;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeKnight.Core.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public class PlayerHorizontalMovementController : MonoBehaviour
    {
        private Rigidbody2D _rb = null!;
        
        [Header("Input")] 
        [SerializeField] private InputReader input = null!;
    
        [Header("Movement")]
        [SerializeField] private float maxMoveSpeed = 5;
        [SerializeField] private float acceleration = 1;
        
        private float _currentMoveSpeed;
        private Coroutine? _onMoveHeldCoroutine;
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnValidate()
        {
            Validation.NotNull(this, input, nameof(input));
        }

        private void OnEnable()
        {
            input.Actions.Player.HorizontalMove.started   += OnMoveStarted;
            input.Actions.Player.HorizontalMove.performed += OnMovePerformed;
            input.Actions.Player.HorizontalMove.canceled  += OnMoveCanceled;
        }

        private void OnDisable()
        {
            input.Actions.Player.HorizontalMove.started   -= OnMoveStarted;
            input.Actions.Player.HorizontalMove.performed -= OnMovePerformed;
            input.Actions.Player.HorizontalMove.canceled  -= OnMoveCanceled;
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
        
        private void UpdateSpriteDirection()
        {
            bool isFacingLeft = input.Actions.Player.HorizontalMove.ReadValue<float>() < 0;
            transform.localScale = isFacingLeft ?  new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
        }
        
        private void ApplyMovement()
        {
            // Calculate move speed by taking the min of the maxMoveSpeed and adding acceleration.
            _currentMoveSpeed = Math.Min(_currentMoveSpeed + acceleration, maxMoveSpeed);

            // Multiply by horizontal movement again to capture direction of input.
            var moveInput = input.Actions.Player.HorizontalMove.ReadValue<float>();
            _rb.linearVelocity = new Vector2(moveInput * _currentMoveSpeed, _rb.linearVelocityY);
        }

        private IEnumerator OnMovementHeld()
        {
            while (input.Actions.Player.HorizontalMove.IsPressed())
            {
                ApplyMovement();
                yield return null;
            }
        }
    }
}