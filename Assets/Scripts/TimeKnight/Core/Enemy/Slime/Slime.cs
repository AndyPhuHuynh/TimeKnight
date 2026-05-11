using System;
using System.Collections;
using TimeKnight.Core.Player;
using TimeKnight.Core.TimePower;
using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.Enemy.Slime
{
    public class Slime : MonoBehaviour, IDamageable
    {
        private Rigidbody2D _rb = null!;
        private SpriteRenderer _sr = null!;
        private Animator _slimeAnimator = null!;
        private JumpEnemyMovement _slimeMovement = null!;

        private readonly int _fallTriggerHash = Animator.StringToHash("Fall");
        private readonly int _jumpTriggerHash = Animator.StringToHash("JumpWindup");
        private float _baseGravity;

        [Header("Slime Properties")]
        [SerializeField] float maxHealth = 5f;
        private float _currentHealth;

        [Header("Collision Properties")]
        [SerializeField] float playerCollisionDamage = 3f;
        [SerializeField] private float horizontalKnockbackForce = 6f;
        [SerializeField] private float verticalKnockbackForce = 6f;

        [Header("Hit While Attacking Properties")]
        [SerializeField] private float knockbackDurationWhenHit = 0.3f;
        [SerializeField] private Color flashColor;
        private Color _baseSpriteColor;
        private CoWrapper _knockbackCoWrapper = null!;

        private void Awake()
        {
            _slimeAnimator = GetComponent<Animator>();
            _slimeMovement = GetComponent<JumpEnemyMovement>();
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            _baseSpriteColor = _sr.color;
            _baseGravity = _rb.gravityScale;
            _currentHealth = maxHealth;
            _knockbackCoWrapper = new CoWrapper(this);
        }

        private void OnEnable()
        {
            _slimeMovement.OnEnemyStateEnter += UpdateAnimationOnStateEnter;
            _slimeMovement.OnEnemyStateExit += UpdateAnimationOnStateExit;
            TimeManager.OnTimeSlowActivate += SlowVelocity;
        }

        private void OnDisable()
        {
            _slimeMovement.OnEnemyStateEnter -= UpdateAnimationOnStateEnter;
            _slimeMovement.OnEnemyStateExit -= UpdateAnimationOnStateExit;
            TimeManager.OnTimeSlowActivate -= SlowVelocity;
        }

        private void Update()
        {
            HandleTimeDilation();
        }

        private void UpdateAnimationOnStateEnter(JumpEnemyState newState)
        {
            switch (newState)
            {
                case JumpEnemyState.ChaseJumpWindup:
                case JumpEnemyState.PatrolJumpWindup:
                    _slimeAnimator.SetTrigger(_jumpTriggerHash);
                    break;
            }
        }

        private void UpdateAnimationOnStateExit(JumpEnemyState oldState)
        {
            switch (oldState)
            {
                case JumpEnemyState.Falling:
                    _slimeAnimator.SetTrigger(_fallTriggerHash);
                    break;
            }
        }

        private void HandleTimeDilation()
        {
            _rb.gravityScale = _baseGravity * TimeManager.CurrentTimeModifier;
            _slimeAnimator.speed = TimeManager.CurrentTimeModifier;
        }

        private void SlowVelocity()
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocityX * TimeManager.CurrentTimeModifier, _rb.linearVelocityY * TimeManager.CurrentTimeModifier);
        }

        public void Damage(float damage, Vector2? knockback)
        {
            _currentHealth -= (int)Math.Round(damage);
            if (_currentHealth <= 0)
            {
                Die();
                return;
            }

            _knockbackCoWrapper.Start(ReceiveKnockbackWhenHit(knockback));
        }

        private IEnumerator ReceiveKnockbackWhenHit(Vector2? knockback)
        {
            float knockbackTimer = 0f;

            _sr.color = flashColor;
            if (knockback != null) 
            {
                Combat.ApplyKnockback(_rb, (Vector2)knockback);
            }
            while (knockbackTimer < knockbackDurationWhenHit)
            {
                knockbackTimer += TimeManager.CustomDelta;
                yield return null;
            }
            _sr.color = _baseSpriteColor;
        }

        private void Die()
        {
            Destroy(gameObject);
        }


        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.gameObject.TryGetComponent(out PlayerCombatManager player))
            {
                Vector2 knockback = Combat.CalculateKnockback(transform.position, PlayerController.PlayerPosition, horizontalKnockbackForce, verticalKnockbackForce);
                player.Damage(playerCollisionDamage, knockback);
            }
        }
    }
}