using System;
using System.Collections;
using UnityEngine;

namespace TimeKnight.Core.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerHorizontalMovement : MonoBehaviour
    {
        private Rigidbody2D _rb = null!;
        
        [Header("Movement")]
        [SerializeField] private float maxMoveSpeed = 5;
        [SerializeField] private float acceleration = 1;
        
        private float _currentMoveSpeed;
        private bool _isMoving;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void StartMove(float input)
        {
            if (_isMoving) return;
            _isMoving = true;
            StartCoroutine(MoveCoroutine(input));
        }

        public void StopMove()
        {
            _isMoving = false;
            _currentMoveSpeed = 0;
            _rb.linearVelocityX = 0;
        }

        public void UpdateSpriteDirection(float input)
        {
            var isFacingLeft = input < 0;
            transform.localScale = isFacingLeft ?  new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
        }
        
        private IEnumerator MoveCoroutine(float input)
        {
            while (_isMoving)
            {
                _currentMoveSpeed = Math.Min(_currentMoveSpeed + acceleration, maxMoveSpeed);
                _rb.linearVelocityX = input * _currentMoveSpeed;
                yield return null;
            }
        }
        
    }
}