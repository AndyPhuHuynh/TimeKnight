using System;
using System.Collections;
using System.Collections.Generic;
using TimeKnight.Core.Enemy;
using TimeKnight.Core.Player;
using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.Sword
{
    public class Sword : MonoBehaviour
    {
        [Header("Player Combat Reference")]
        [SerializeField] private PlayerCombatManager playerManager = null!; // Used for getting damage values

        [Header("Attack Properties")]
        [SerializeField] private Transform attackTransform = null!;
        [SerializeField] private float attackRadius = 0.5f;
        [SerializeField] private LayerMask attackableLayer;
        [SerializeField] private float attackCooldown = 1;
        [SerializeField] private float horizontalKnockbackForce = 6f;
        [SerializeField] private float verticalKnockbackForce = 4f;

        private bool _isSwordSwinging;
        private float _attackTimer;
        private Coroutine? _swordHitboxCoroutine = null;
        public event Action OnSwordSwingEnd = delegate { };

        // Collision management
        private readonly HashSet<IDamageable> _previouslyDamagedThisAttack = new();

        private void OnValidate()
        {
            Validation.NotNull(this, attackTransform, nameof(attackTransform));
        }
        
        private void Awake()
        {
            _attackTimer = attackCooldown; // Set current timer to be at cooldown so player can attack right away.
        }

        private void Update()
        {
            _attackTimer += Time.deltaTime;
        }

        private void OnEnable()
        {
            // These are on the player, not the sword object so we check in OnEnable, not in OnValidate
            Validation.NotNull(this, playerManager, nameof(playerManager));
        }

        public bool CanAttack()
        {
            return _attackTimer >= attackCooldown;
        }
        
        public void BeginSwing()
        {
            // End Old sword hit if one was happening.
            if (_swordHitboxCoroutine != null)
            {
                StopCoroutine(_swordHitboxCoroutine);
                _previouslyDamagedThisAttack.Clear();
            }

            _attackTimer = 0;
            _isSwordSwinging = true;
            _swordHitboxCoroutine = StartCoroutine(DamageWhileAttackActive());
        }

        public void EndSwing()
        {
            _isSwordSwinging = false;
            OnSwordSwingEnd.Invoke();
        }

        private IEnumerator DamageWhileAttackActive()
        {
            // SwordSwinging is disabled by the animation when sword is finished swinging.
            while (_isSwordSwinging)
            {
                var damage = playerManager.GetCurrentDamageOutput();
                var colliders = Physics2D.OverlapCircleAll(attackTransform.position, attackRadius, attackableLayer);
                foreach (var hit in colliders)
                {
                    var iDamageable = hit.gameObject.GetComponent<IDamageable>();

                    // Ignore enemies that have already been hit with this swing.
                    if (iDamageable == null || _previouslyDamagedThisAttack.Contains(iDamageable)) continue;

                    // Knockback calculated here because we need the position of the enemy for calculations.                    
                    Vector2 knockback = Combat.CalculateKnockback(PlayerController.PlayerPosition, hit.gameObject.transform.position, horizontalKnockbackForce, verticalKnockbackForce);
                    iDamageable.Damage(damage, knockback);
                    
                    _previouslyDamagedThisAttack.Add(iDamageable);
                }

                yield return null;
            }

            // Clear out previously damaged objects so they can be hit on the next sword swing.
            _previouslyDamagedThisAttack.Clear();
            _swordHitboxCoroutine = null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackTransform.position, attackRadius);
        }
    }
}

