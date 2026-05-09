using System;
using System.Collections;
using System.Collections.Generic;
using TimeKnight.Core.Audio;
using TimeKnight.Utils;
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
        
        [Header("Audio")]
        [SerializeField] private List<AudioClip> moveSounds = new();
        [SerializeField] private GroundCheck groundCheck = null!;
        
        private float _currentMoveSpeed;
        private bool _isMoving;
        private CoWrapper _soundPlaying = null!;

        private readonly AudioClipParams _soundParams = new()
        {
            PitchVariance = 0.25f,
            Volume = 0.1f
        };

        private void OnValidate()
        {
            Validation.NotEmpty(this, moveSounds, nameof(moveSounds));
            Validation.NotNull(this, groundCheck, nameof(groundCheck));
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _soundPlaying = new CoWrapper(this);
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
            _soundPlaying.Stop();
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
                if (!groundCheck.IsGrounded)
                {
                    _soundPlaying.Stop();
                }
                else if (!_soundPlaying.IsRunning)
                {
                    _soundPlaying.Start(AudioManager.Instance.PlaySoundEffect(moveSounds, _rb.transform.position, _soundParams));
                }
                _currentMoveSpeed = Math.Min(_currentMoveSpeed + acceleration, maxMoveSpeed);
                _rb.linearVelocityX = readInput() * _currentMoveSpeed;
                yield return null;
            }
        }
    }
}