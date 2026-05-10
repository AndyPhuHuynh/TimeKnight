using System;
using System.Collections;
using System.Collections.Generic;
using TimeKnight.Core.Audio;
using TimeKnight.Extensions;
using TimeKnight.Utils;
using UnityEngine;
using Yarn.Saliency;

namespace TimeKnight.Core.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(AudioSource))]
    public class PlayerHorizontalMovement : MonoBehaviour
    {
        private Rigidbody2D _rb = null!;
        private AudioSource _audioSource = null!;
        
        [Header("Movement")]
        [SerializeField] private float maxMoveSpeed = 5;
        [SerializeField] private float acceleration = 1;
        
        [Header("Audio")]
        [SerializeField] private List<AudioClip> moveSounds = new();
        [SerializeField] private GroundCheck groundCheck = null!;
        
        private float _currentMoveSpeed;
        private bool _isMoving;

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
            _audioSource = GetComponent<AudioSource>();
            
            Validation.NotFound(this, _rb, nameof(_rb));
            Validation.NotNull(this, _audioSource, nameof(_audioSource));
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
            _audioSource.Stop();
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
                    _audioSource.Stop();
                }
                else if (!_audioSource.isPlaying)
                {
                    _audioSource.clip = moveSounds.RandomElement();
                    _audioSource.SetParams(_soundParams);
                    _audioSource.Play();
                }
                _currentMoveSpeed = Math.Min(_currentMoveSpeed + acceleration, maxMoveSpeed);
                _rb.linearVelocityX = readInput() * _currentMoveSpeed;
                yield return null;
            }
        }
    }
}