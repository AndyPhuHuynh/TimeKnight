using System;
using System.Collections;
using UnityEngine;

namespace TimeKnight.Core.Player
{
    public class PlayerManager : MonoBehaviour
    {
        // TODO: Finalize how we handle health. right now I'm thinking all health is integer, and we round when calculating damage.
        public int maxHealth = 10;
        public int Health { get; private set; }
        public event Action<int> MaxHealthChanged = delegate {}; 
        public event Action<int> HealthChanged = delegate {};
        [SerializeField] private float baseDamage = 2;
        [SerializeField] private float critChance = 0.1f;
        [SerializeField] private float critDamageMultiplier = 2.0f;
        [SerializeField] private float damageResistance = 1.0f;
        [SerializeField] private float damageRecoveryTime = 1f;
        private bool _isInvincible;

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

        public void Damage(float damage)
        {
            if (_isInvincible) return;
            
            StartCoroutine(DamageInvulnerability());
            Health -= (int)Math.Round(damage * damageResistance);
            HealthChanged.Invoke(Health);
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
    }
}