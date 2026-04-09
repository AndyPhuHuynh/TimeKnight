using System;
using System.Collections;
using UnityEngine;

namespace TimeKnight.Core.Player
{
    public class PlayerManager : MonoBehaviour
    {
        // TODO: Finalize how we handle health. right now I'm thinking all health is integer, and we round when calculating damage.
        public int MaxHealth { get; private set; } = 10;
        public int Health { get; private set; } = 10;
        public event Action<int> MaxHealthChanged;
        public event Action<int> HealthChanged;
        private float _critChance = 0.0f;
        private float _critDamage = 2.0f;
        private float _damageResistance = 1.0f;
        private bool _isInvincible = false;
        private float _damageRecoveryTime = 1f;

        private void Awake()
        {
            MaxHealthChanged?.Invoke(MaxHealth);
            HealthChanged?.Invoke(Health);
        }

        public void TakeDamage(float damage)
        {
            if (_isInvincible) return;
            
            StartCoroutine(DamageInvulnerability());
            Health -= (int)Math.Round(damage * _damageResistance);
            HealthChanged?.Invoke(Health);
        }

        private IEnumerator DamageInvulnerability()
        {
            _isInvincible = true;
            float timePassed = 0;
            while (timePassed < _damageRecoveryTime)
            {
                timePassed += Time.deltaTime;
                yield return null;
            }
            _isInvincible = false;
            yield return null;
        }
    }
}