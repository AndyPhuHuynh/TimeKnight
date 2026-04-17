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
        private const float BaseDamage = 2;
        private const float CritChance = 0.0f;
        private const float CritDamageMultiplier = 2.0f;
        private const float DamageResistance = 1.0f;
        private const float DamageRecoveryTime = 1f;
        private bool _isInvincible;

        private void Awake()
        {
            Health = maxHealth;
            MaxHealthChanged.Invoke(maxHealth);
            HealthChanged.Invoke(Health);
        }

        public static float GetCurrentDamageOutput()
        {
            var damage = BaseDamage;
            var roll = UnityEngine.Random.value;

            if (roll < CritChance)
            {
                damage *= CritDamageMultiplier;
            }

            return damage;
        }

        public void Damage(float damage)
        {
            if (_isInvincible) return;
            
            StartCoroutine(DamageInvulnerability());
            Health -= (int)Math.Round(damage * DamageResistance);
            HealthChanged.Invoke(Health);
        }

        private IEnumerator DamageInvulnerability()
        {
            _isInvincible = true;
            float timePassed = 0;
            while (timePassed < DamageRecoveryTime)
            {
                timePassed += Time.deltaTime;
                yield return null;
            }
            _isInvincible = false;
        }
    }
}