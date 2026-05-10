using UnityEngine;
using TimeKnight.Core.Player;
using System;
using System.Collections;
using TimeKnight.Core.Audio;
using TimeKnight.Extensions;
using TimeKnight.Utils;

namespace TimeKnight.Core.Enemy.Skeleton
{
    public class Skeleton : MonoBehaviour, IDamageable
    {
        private Rigidbody2D _rb = null!;
        private SpriteRenderer _sr = null!;

        [Header("Enemy Properties")]
        [SerializeField] private int maxHealth = 5;
        [SerializeField] private int playerCollisionDamage = 3;
        private int _currentHealth;

        [Header("Attack Damage Properties")]
        [SerializeField] private float attackDamage = 2f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private float horizontalKnockbackForce = 6f;
        [SerializeField] private float verticalKnockbackForce = 4f;
        private float _attackTimer;
        private bool AttackTimerReady => _attackTimer >= attackCooldown;

        [Header("Attack Hitbox Properties")]
        [SerializeField] private float attackPlayerRange = 2f;
        [SerializeField] private float attackHitboxRadius = 0.5f;
        [SerializeField] private Transform attackTransform = null!;

        [Header("Hit While Attacking Properties")]
        [SerializeField] private float flashDurationWhenHit = 0.8f;
        [SerializeField] private Color flashColor;
        private Color _baseSpriteColor;

        [Header("Audio")]
        [SerializeField] private AudioSource swordAudioSource = null!;
        [SerializeField] private AudioSource hurtAudioSource = null!;
        [SerializeField] private AudioClip swordAttackSound = null!;
        [SerializeField] private AudioClip hurtSound = null!;

        // Animation State Management
        private PatrolEnemyMovement _patrolScript = null!;
        private Animator _skeletonAnimator = null!;
        private readonly int _lostSightTriggerHash = Animator.StringToHash("LostSight");
        private readonly int _walkingTriggerHash = Animator.StringToHash("Walking");
        private readonly int _tooCloseTriggerHash = Animator.StringToHash("TooClose");
        private readonly int _damagedTriggerHash = Animator.StringToHash("Damaged");
        private readonly int _attackTriggerHash = Animator.StringToHash("Attack");

        // Combat State management
        private EnemyCombatState _combatState = EnemyCombatState.None;
        private bool _isHitboxActive;
        private Coroutine? _receiveKnockbackCoroutine;
        
        // Audio
        private readonly AudioClipParams _swordSoundParams = new()
        {
            PitchVariance = 0.25f
        };

        private readonly AudioClipParams _hurtSoundParams = new()
        {
            PitchVariance = 0.25f
        };

        private void OnValidate()
        {
            Validation.NotNull(this, swordAudioSource, nameof(swordAudioSource));
            Validation.NotNull(this, hurtAudioSource, nameof(hurtAudioSource));
            Validation.NotNull(this, swordAttackSound, nameof(swordAttackSound));
            Validation.NotNull(this, hurtSound, nameof(hurtSound));
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            _baseSpriteColor = _sr.color;
            _skeletonAnimator = GetComponent<Animator>();
            _patrolScript = GetComponent<PatrolEnemyMovement>();
            _currentHealth = maxHealth;
            _attackTimer = attackCooldown;
        }

        private void Update()
        {
            if (!AttackTimerReady && !_combatState.IsAttacking())
            {
                _attackTimer += Time.deltaTime;
            }
            else if (CanAttackPlayer())
            {
                _combatState = EnemyCombatState.Attacking;
                StartCoroutine(AttackPlayer());
            }
        }

        private void OnEnable()
        {
            _patrolScript.OnEnemyStateEnter += UpdateAnimation;
        }

        private void OnDisable()
        {
            _patrolScript.OnEnemyStateEnter -= UpdateAnimation;
        }

        #region Animation State Management
        private void UpdateAnimation(EnemyPatrolState newState)
        {
            switch (newState)
            {
                case EnemyPatrolState.LostSight:
                    _skeletonAnimator.SetTrigger(_lostSightTriggerHash);
                    break;
                case EnemyPatrolState.TooClose:
                    _skeletonAnimator.SetTrigger(_tooCloseTriggerHash);
                    break;
                case EnemyPatrolState.Chase:
                case EnemyPatrolState.Patrol:
                    _skeletonAnimator.SetTrigger(_walkingTriggerHash);
                    break;
            }
        }

        // Called From animation clips to exit combat animations.
        public void ResetCombatState()
        {
            _combatState = EnemyCombatState.None;
        }
        #endregion

        #region Receiving Damage From PLayer
        public void Damage(float damage, Vector2? knockback)
        {
            hurtAudioSource.PlayWithParams(hurtSound, _hurtSoundParams);
            
            _currentHealth -= (int)Math.Round(damage);
            if (_currentHealth <= 0)
            {
                Die();
                return;
            }

            if (_combatState.IsAttacking())
            {
                StartCoroutine(FlashWhenHit());
            }
            else
            {
                _combatState = EnemyCombatState.BeingDamaged;
                // Exit Damage coroutine prematurely if hit in quick succession. This prevents resuming AI prematurely between hits.
                if (_receiveKnockbackCoroutine != null) StopCoroutine(_receiveKnockbackCoroutine);
                _receiveKnockbackCoroutine = StartCoroutine(ReceiveKnockbackWhenHit(knockback));
            }
        }

        // This only runs when the skeleton is damaged while it is attacking the player.
        private IEnumerator FlashWhenHit()
        {
            float flashTimer = 0f;

            _sr.color = flashColor;
            while (flashTimer < flashDurationWhenHit)
            {
                flashTimer += Time.deltaTime;
                yield return null;
            }
            _sr.color = _baseSpriteColor;
        }

        private IEnumerator ReceiveKnockbackWhenHit(Vector2? knockback)
        {
            _patrolScript.PauseAI();
            _skeletonAnimator.SetTrigger(_damagedTriggerHash);

            // Knockback applied after movement AI paused.
            if (knockback != null)
            {
                Combat.ApplyKnockback(_rb, (Vector2)knockback);
            }

            // Wait for animation to finish.
            while (_combatState.IsBeingDamaged())
            {
                yield return null;
            }

            _patrolScript.ResumeAI();
            _receiveKnockbackCoroutine = null;
        }

        private void Die()
        {
            Destroy(gameObject);
        }
        #endregion

        #region Dealing Damage To Player
        private bool CanAttackPlayer()
        {
            bool isPlayerInRange = Vector3.Distance(PlayerController.PlayerPosition, transform.position) <= attackPlayerRange;
            return isPlayerInRange && AttackTimerReady && !_combatState.IsBeingDamaged();
        }

        private IEnumerator AttackPlayer()
        {
            swordAudioSource.PlayWithParams(swordAttackSound, _swordSoundParams);
            _patrolScript.PauseAI();
            _skeletonAnimator.SetTrigger(_attackTriggerHash);
            _attackTimer = 0;
            _isHitboxActive = false;
            var wasPlayerHit = false;

            while (_combatState.IsAttacking())
            {
                // Don't check physics casts when enemy already hit player or hitbox is inactive.
                if (wasPlayerHit || !_isHitboxActive)
                {
                    yield return null;
                    continue;
                }

                var colliders = Physics2D.OverlapCircleAll(attackTransform.position, attackHitboxRadius);
                foreach (var hit in colliders)
                {
                    if (hit.gameObject.TryGetComponent(out PlayerManager player))
                    {
                        wasPlayerHit = true;
                        Vector2 knockback = Combat.CalculateKnockback(transform.position, PlayerController.PlayerPosition, horizontalKnockbackForce, verticalKnockbackForce);
                        player.Damage(attackDamage, knockback);
                    }
                }
                yield return null;
            }

            _patrolScript.ResumeAI();
        }

        // These are called from the animator to sync hitbox to sprite animation.
        public void EnableHitbox()
        {
            _isHitboxActive = true;
        }

        public void DisableHitbox()
        {
            _isHitboxActive = false;
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.gameObject.TryGetComponent(out PlayerManager player))
            {
                Vector2 knockback = Combat.CalculateKnockback(transform.position, PlayerController.PlayerPosition, horizontalKnockbackForce, verticalKnockbackForce);

                player.Damage(playerCollisionDamage, knockback);
            }
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackTransform.position, attackHitboxRadius);
        }
    }
}
