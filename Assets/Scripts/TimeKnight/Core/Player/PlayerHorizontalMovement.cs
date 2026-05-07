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

        public void StartMove(Func<float> readInput)
        {
            if (_isMoving) return;
            _isMoving = true;
            StartCoroutine(MoveCoroutine(readInput));
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
        
        private IEnumerator MoveCoroutine(Func<float> readInput)
        {
            while (_isMoving)
            {
                _currentMoveSpeed = Math.Min(_currentMoveSpeed + acceleration, maxMoveSpeed);
                _rb.linearVelocityX = readInput() * _currentMoveSpeed;
                yield return null;
            }
        }
        
    }
}