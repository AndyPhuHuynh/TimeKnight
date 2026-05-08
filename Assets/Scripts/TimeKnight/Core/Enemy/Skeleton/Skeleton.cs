using UnityEngine;
using TimeKnight.Core.Player;
using System;
using System.Collections;
using TimeKnight.Utils;
using TimeKnight.Core.TimePower;

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
        private float _baseGravity;
        private float _maxKnockbackDuration = 0.3f;

        [Header("Attack Damage Properties")]
        [SerializeField] private float attackDamage = 2f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private float horizontalKnockbackForce = 6f;
        [SerializeField] private float verticalKnockbackForce = 4f;
        private float _attackTimer;
        private bool _attackTimerReady => _attackTimer >= attackCooldown;

        [Header("Attack Hitbox Properties")]
        [SerializeField] private float attackPlayerRange = 2f;
        [SerializeField] private float attackHitboxRadius = 0.5f;
        [SerializeField] private Transform attackTransform = null!;

        [Header("Hit While Attacking Properties")]
        [SerializeField] private float flashDurationWhenHit = 0.8f;
        [SerializeField] private Color flashColor;
        private Color _baseSpriteColor;

        // Animation State Management
        private PatrolEnemyMovement _patrolScript = null!;
        private Animator _skeletonAnimator = null!;
        private readonly int _lostSightTriggerHash = Animator.StringToHash("LostSight");
        private readonly int _walkingTriggerHash = Animator.StringToHash("Walking");
        private readonly int _idleTriggerHash = Animator.StringToHash("Idle");
        private readonly int _damagedTriggerHash = Animator.StringToHash("Damaged");
        private readonly int _attackTriggerHash = Animator.StringToHash("Attack");

        // Combat State management
        private EnemyCombatState _combatState = EnemyCombatState.None;
        private bool _isHitboxActive = false;
        private CoWrapper _activeCombatCoWrapper = null!;   // One CoWrapper is used for all combat actions as they are mutually exclusive.

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            _skeletonAnimator = GetComponent<Animator>();
            _patrolScript = GetComponent<PatrolEnemyMovement>();
            _currentHealth = maxHealth;
            _attackTimer = attackCooldown;
            _activeCombatCoWrapper = new CoWrapper(this);
            _baseGravity = _rb.gravityScale;
            _baseSpriteColor = _sr.color;
        }

        private void Update()
        {
            HandleTimeDilation();
            
            if (!_attackTimerReady)
            {
                _attackTimer += TimeManager.CustomDelta;
            }
            if (CanAttackPlayer())
            {
                _combatState = EnemyCombatState.Attacking;
                StartCombatCoroutine(AttackPlayer());
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
                case EnemyPatrolState.PatrolStuck:
                case EnemyPatrolState.ChaseStuck:
                case EnemyPatrolState.TooClose:
                    _skeletonAnimator.SetTrigger(_idleTriggerHash);
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
                StartCombatCoroutine(ReceiveKnockbackWhenHit(knockback));
            }
        }

        // This only runs when the skeleton is damaged while it is attacking the player.
        private IEnumerator FlashWhenHit()
        {
            float flashTimer = 0f;

            _sr.color = flashColor;
            while (flashTimer < flashDurationWhenHit)
            {
                flashTimer += TimeManager.CustomDelta;
                yield return null;
            }
            _sr.color = _baseSpriteColor;
        }

        private IEnumerator ReceiveKnockbackWhenHit(Vector2? knockback)
        {
            _patrolScript.PauseAI();
            _skeletonAnimator.SetTrigger(_damagedTriggerHash);
            float _currentKnockbackDuration = 0;
            // Knockback applied after movement AI paused.
            if (knockback != null)
            {
                Combat.ApplyKnockback(_rb, (Vector2)knockback);
            }

            // Wait for animation to finish.
            while (_combatState.IsBeingDamaged())
            {
                // This uses regular Time.deltaTime so enemy doesn't slide for super long when time is slowed down.
                _currentKnockbackDuration += Time.deltaTime;
                if (_currentKnockbackDuration >= _maxKnockbackDuration)
                {
                    _rb.linearVelocity = new Vector2(0, _rb.linearVelocityY);
                }
                
                yield return null;
            }

            _patrolScript.ResumeAI();
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
            return isPlayerInRange && _attackTimerReady && _combatState.IsNone();
        }

        private IEnumerator AttackPlayer()
        {
            _patrolScript.PauseAI();
            _skeletonAnimator.SetTrigger(_attackTriggerHash);
            _attackTimer = 0;
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
                    if (hit.gameObject.TryGetComponent(out PlayerCombatManager player))
                    {
                        _wasPlayerHit = true;
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

        // Player Contact Collision Damage
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.gameObject.TryGetComponent(out PlayerCombatManager player))
            {
                Vector2 knockback = Combat.CalculateKnockback(transform.position, PlayerController.PlayerPosition, horizontalKnockbackForce, verticalKnockbackForce);
                player.Damage(playerCollisionDamage, knockback);
            }
        }

        #endregion
        
        #region Time Dilation Management
        private void HandleTimeDilation()
        {
            _rb.gravityScale = _baseGravity * TimeManager.CurrentTimeModifier;
            _skeletonAnimator.speed = TimeManager.CurrentTimeModifier;
        }
        #endregion

        private void StartCombatCoroutine(IEnumerator combatCoroutine)
        {
            _activeCombatCoWrapper.Start(combatCoroutine);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackTransform.position, attackHitboxRadius);
        }
    }
}
