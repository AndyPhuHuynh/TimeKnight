using System;
using System.Collections;
using TimeKnight.Core.Input;
using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.Player
{
    public class PlayerCombatManager : MonoBehaviour
    {
        public int maxHealth = 10;
        public int Health { get; private set; }

        // Actions for UI elements to sync with player.
        public event Action<int> MaxHealthChanged = delegate { };
        public event Action<int> HealthChanged = delegate { };

        // Actions for PlayerController to handle input
        public event Action OnPlayerStunBegin = delegate { };
        public event Action OnPlayerStunEnd = delegate { };

        [Header("Knockback Application")]
        [SerializeField] private Rigidbody2D rb = null!;

        [Header("Player Stats")]
        [SerializeField] private float baseDamage = 2;
        [SerializeField] private float critChance = 0.1f;
        [SerializeField] private float critDamageMultiplier = 2.0f;
        [SerializeField] private float damageResistance = 1.0f;
        [SerializeField] private float damageRecoveryTime = 1f;
        [SerializeField] private float knockbackStunTime = 0.5f;
        private bool _isInvincible;

        private void OnValidate()
        {
            Validation.NotNull(this, rb, "Rigidbody2D");
        }

        private void Awake()
        {
            Health = maxHealth;
            MaxHealthChanged.Invoke(maxHealth);
            HealthChanged.Invoke(Health);
        }

        public float GetCurrentDamageOutput()
        {
            var damage = baseDamage;
            var roll = UnityEngine.Random.value;

            if (roll < critChance)
            {
                damage *= critDamageMultiplier;
            }

            return damage;
        }

        public void Damage(float damage, Vector2? knockback = null)
        {
            if (_isInvincible) return;

            StartCoroutine(DamageInvulnerability());
            Health -= (int)Math.Round(damage * damageResistance);
            HealthChanged.Invoke(Health);

            if (knockback != null)
            {
                StartCoroutine(KnockbackStun());
                Combat.ApplyKnockback(rb, (Vector2)knockback);
            }
        }

        private IEnumerator DamageInvulnerability()
        {
            _isInvincible = true;
            float timePassed = 0;
            while (timePassed < damageRecoveryTime)
            {
                timePassed += Time.deltaTime;
                yield return null;
            }
            _isInvincible = false;
        }

        private IEnumerator KnockbackStun()
        {
            OnPlayerStunBegin.Invoke();
            float timePassed = 0;
            while (timePassed < knockbackStunTime)
            {
                timePassed += Time.deltaTime;
                yield return null;
            }

            rb.linearVelocity = new Vector2(0f, rb.linearVelocityY);
            OnPlayerStunEnd.Invoke();
        }
    }
}