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
        [SerializeField] private PlayerManager _playerManager;   // Used for getting damage values

        [Header("Attack Properties")]
        [SerializeField] private Transform AttackTransform;
        [SerializeField] private float AttackRadius = 0.5f;
        [SerializeField] LayerMask AttackableLayer;

        [Header("Animation/State management")]
        [SerializeField] Animator Animator;
        // In order to behave normally, AttackCooldown should be greater than or equal to the duration of the attack animation (to avoid animation cancelling).
        [SerializeField] float AttackCooldown = 1;
        public bool SwordSwinging = false;  // This is public as it is controlled by the animator to set sword swinging.
        private float _attackTimer;
        private int _attackTriggerHash;

        // Collision management
        private RaycastHit2D[] _swordCollisions;
        private HashSet<IDamageable> _previouslyDamagedThisAttack = new HashSet<IDamageable>();

        private void Start()
        {
            _attackTriggerHash = Animator.StringToHash("Attack");
            _attackTimer = AttackCooldown;  // Set current timer to be at cooldown so player can attack right away.
        }

        private void Update()
        {
            _attackTimer += Time.deltaTime;
        }

        public void BeginSwing()
        {
            if (_attackTimer > AttackCooldown)
            {
                Animator.SetTrigger(_attackTriggerHash);
                _attackTimer = 0;
                StartCoroutine(DamageWhileAttackActive());
            }
        }

        public IEnumerator DamageWhileAttackActive()
        {
            SwordSwinging = true;

            // SwordSwinging is disabled by the animation when sword is finished swinging.
            while (SwordSwinging)
            {
                float damage = _playerManager.GetCurrentDamageOutput();
                _swordCollisions = Physics2D.CircleCastAll(AttackTransform.position, AttackRadius, Vector2.right, 0f, AttackableLayer);
                for (int i = 0; i < _swordCollisions.Length; i++)
                {
                    IDamageable iDamageable = _swordCollisions[i].collider.gameObject.GetComponent<IDamageable>();

                    // Ignore enemies that have already been hit with this swing.
                    if (iDamageable != null && !_previouslyDamagedThisAttack.Contains(iDamageable))
                    {
                        iDamageable.Damage(damage);
                        _previouslyDamagedThisAttack.Add(iDamageable);
                    }
                }

                yield return null;
            }

            // Clear out previously damaged objects so they can be hit on the next sword swing.
            _previouslyDamagedThisAttack.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(AttackTransform.position, AttackRadius);
        }
    }
}

