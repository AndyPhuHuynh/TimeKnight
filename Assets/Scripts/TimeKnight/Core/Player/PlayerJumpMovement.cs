using System;
using System.Collections;
using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerJumpMovement : MonoBehaviour
    {
        private Rigidbody2D _rb = null!;
        
        [Header("Jump")]
        [SerializeField] private float baseJumpForce = 10;
        [SerializeField] private float holdJumpForce = 2;
        [SerializeField] private int holdJumpUpdates = 10;
        
        [Header("Ground Check")]
        [SerializeField] private GroundCheck groundCheck = null!;
        
        private bool _isJumping;
        
        private void OnValidate()
        {
            Validation.NotNull(this, groundCheck, nameof(groundCheck));
        }
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }
        
        public void StartJump(bool checkGround, Action? animatorCallback = null)
        {
            if (_isJumping) return;
            if (checkGround && !groundCheck.IsGrounded) return; 
            _isJumping = true;
            animatorCallback?.Invoke();
            StartCoroutine(JumpCoroutine());
        }

        public void StopJump()
        {
            _isJumping = false;
        }
        
        private IEnumerator JumpCoroutine()
        {
            _rb.linearVelocityY += baseJumpForce;
            yield return null;

            for (var i = 0; i < holdJumpUpdates; i++)
            {
                if (!_isJumping) break;

                _rb.linearVelocityY += holdJumpForce;
                yield return new WaitForFixedUpdate(); // Keeps synced with physics calculations.
            }
            
            _isJumping = false;
        }
    }
}