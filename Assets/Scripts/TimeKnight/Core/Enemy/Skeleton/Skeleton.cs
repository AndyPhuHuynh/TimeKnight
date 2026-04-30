using UnityEngine;
using TimeKnight.Core.Player;
using System;
using System.Collections;
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
        [Header("Attack Properties")]
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackDamage = 2f;
        [SerializeField] private float horizontalKnockbackForce = 6f;
        [SerializeField] private float verticalKnockbackForce = 4f;
        [SerializeField] private Transform attackTransform = null!;
        [SerializeField] private float attackHitboxRadius = 0.5f;
        private float _attackTimer;
        private int _currentHealth;

        // State Management
        private PatrolEnemyMovement _patrolScript = null!;

        [Header("Animation Management")]
        [SerializeField] private float flashWhenHitDuration = 0.8f;
        [SerializeField] private Color flashColor;
        private Animator _skeletonAnimator = null!;
        private readonly int _lostSightTriggerHash = Animator.StringToHash("LostSight");
        private readonly int _walkingTriggerHash = Animator.StringToHash("Walking");
        private readonly int _tooCloseTriggerHash = Animator.StringToHash("TooClose");
        private readonly int _damagedTriggerHash = Animator.StringToHash("Damaged");
        private readonly int _attackTriggerHash = Animator.StringToHash("Attack");
        private Color _baseSpriteColor;

        // Combat management

        private EnemyCombatState _combatState = EnemyCombatState.None;

        private bool _attackTimerReady => _attackTimer >= attackCooldown;
        private bool _isHitboxActive = false;
        private Coroutine? _isBeingDamagedCoroutine = null;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            _baseSpriteColor = _sr.color;
            _skeletonAnimator = GetComponent<Animator>();
            _patrolScript = GetComponent<PatrolEnemyMovement>();
            _currentHealth = maxHealth;
            _attackTimer = attackCooldown + 1;
        }

        private void Update()
        {
            // If statement prevents attack timer growing very large when going a long time without attacking.
            if (_attackTimer < attackCooldown + 2 && !_combatState.IsAttacking())
            {
                _attackTimer += Time.deltaTime;
            }
            if (CanAttackPlayer())
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

        public void ResetCombatState()
        {
            _combatState = EnemyCombatState.None;
        }

        #endregion

        #region Receiving Damage From PLayer

        public void Damage(float damage, Vector2? knockback)
        {
            // First determine if enemy should die.
            _currentHealth -= (int)Math.Round(damage);
            if (_currentHealth <= 0) Die();

            // If enemy is attacking, then don't play damaged animation.
            if (_combatState.IsAttacking())
            {
                StartCoroutine(FlashWhenHit());
            }
            else
            {
                _combatState = EnemyCombatState.BeingDamaged;
                // Exit Damage coroutine prematurely if hit in quick succession. This prevents the reset animation UpdateAnimation be called in between hits.
                if (_isBeingDamagedCoroutine != null) StopCoroutine(_isBeingDamagedCoroutine);
                _isBeingDamagedCoroutine = StartCoroutine(PlayDamagedAnimationWhenHit(knockback));
            }
        }

        // This only runs when the skeleton is damaged while it is attacking.
        private IEnumerator FlashWhenHit()
        {
            float flashTimer = 0f;

            _sr.color = flashColor;
            while (flashTimer < flashWhenHitDuration)
            {
                flashTimer += Time.deltaTime;
                yield return null;
            }
            _sr.color = _baseSpriteColor;
            yield return null;
        }

        // This coroutine will end when either the skeleton is damaged again, or when the damage animation finishes and DamageAnimationComplete is called.
        private IEnumerator PlayDamagedAnimationWhenHit(Vector2? knockback)
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

            // Reset animation to its previous state before resuming AI.
            UpdateAnimation(_patrolScript.CurrentState);
            _patrolScript.ResumeAI();

            _isBeingDamagedCoroutine = null;
        }

        private void Die()
        {
            Destroy(gameObject);
        }
        #endregion

        #region Dealing Damage To Player
        private bool CanAttackPlayer()
        {
            bool isPlayerInRange = Vector3.Distance(PlayerController.PlayerPosition, transform.position) <= attackRange;
            return isPlayerInRange && _attackTimerReady && _combatState.IsNone();
        }

        private IEnumerator AttackPlayer()
        {
            _attackTimer = 0;
            _patrolScript.PauseAI();
            _skeletonAnimator.SetTrigger(_attackTriggerHash);
            _isHitboxActive = false;
            bool _wasPlayerHit = false;
            while (_combatState.IsAttacking())
            {
                // Don't check physics casts when enemy already hit player or hitbox is inactive.
                if (_wasPlayerHit || !_isHitboxActive)
                {
                    yield return null;
                    continue;
                }

                var colliders = Physics2D.OverlapCircleAll(attackTransform.position, attackHitboxRadius);
                foreach (var hit in colliders)
                {
                    if (hit.gameObject.TryGetComponent(out PlayerManager player))
                    {
                        _wasPlayerHit = true;
                        Vector2 knockback = Combat.CalculateKnockback(transform.position, PlayerController.PlayerPosition, horizontalKnockbackForce, verticalKnockbackForce);
                        player.Damage(attackDamage, knockback);
                    }
                }
                yield return null;
            }

            UpdateAnimation(_patrolScript.CurrentState);
            _patrolScript.ResumeAI();
        }

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
