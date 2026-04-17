using System.Collections;
using System.Collections.Generic;
using TimeKnight.Core.Enemy;
using TimeKnight.Core.Player;
using UnityEngine;


namespace TimeKnight.Core.Sword
{
    public class Sword : MonoBehaviour
    {
        [Header("Player Reference")]
        [SerializeField] private PlayerManager playerManager = null!;   // Used for getting damage values

        [Header("Attack Properties")]
        [SerializeField] private Transform attackTransform = null!;
        [SerializeField] private float attackRadius = 0.5f;
        [SerializeField] private LayerMask attackableLayer;

        [Header("Animation/State management")]
        [SerializeField] private Animator animator = null!;
        // In order to behave normally, AttackCooldown should be greater than or equal to the duration of the attack animation (to avoid animation cancelling).
        [SerializeField] private float attackCooldown = 1;
        public bool swordSwinging;  // This is public as it is controlled by the animator to set sword swinging.
        private float _attackTimer;
        private int _attackTriggerHash;

        // Collision management
        private RaycastHit2D[]? _swordCollisions;
        private readonly HashSet<IDamageable> _previouslyDamagedThisAttack = new();

        private void Start()
        {
            _attackTriggerHash = Animator.StringToHash("Attack");
            _attackTimer = attackCooldown;  // Set current timer to be at cooldown so player can attack right away.
        }

        private void Update()
        {
            _attackTimer += Time.deltaTime;
        }

        public void BeginSwing()
        {
            if (_attackTimer <= attackCooldown)  return;

            animator.SetTrigger(_attackTriggerHash);
            _attackTimer = 0;
            StartCoroutine(DamageWhileAttackActive());
        }

        private IEnumerator DamageWhileAttackActive()
        {
            swordSwinging = true;

            // SwordSwinging is disabled by the animation when sword is finished swinging.
            while (swordSwinging)
            {
                var damage = playerManager.GetCurrentDamageOutput();
                _swordCollisions = Physics2D.CircleCastAll(attackTransform.position, attackRadius, Vector2.right, 0f, attackableLayer);
                foreach (var t in _swordCollisions)
                {
                    IDamageable iDamageable = t.collider.gameObject.GetComponent<IDamageable>();

                    // Ignore enemies that have already been hit with this swing.
                    if (iDamageable == null || _previouslyDamagedThisAttack.Contains(iDamageable)) continue;
                    
                    iDamageable.Damage(damage);
                    _previouslyDamagedThisAttack.Add(iDamageable);
                }

                yield return null;
            }

            // Clear out previously damaged objects so they can be hit on the next sword swing.
            _previouslyDamagedThisAttack.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackTransform.position, attackRadius);
        }
    }
}

