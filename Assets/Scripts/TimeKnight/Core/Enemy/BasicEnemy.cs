using System;
using TimeKnight.Core.Player;
using UnityEngine;

namespace TimeKnight.Core.Enemy
{
    public class BasicEnemy : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 5;
        [SerializeField] private int playerCollisionDamage = 3;
        private int _currentHealth;
        
        private void Awake()
        {
            _currentHealth = maxHealth;
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.gameObject.TryGetComponent(out PlayerCombatManager player))
            {
                player.Damage(playerCollisionDamage);
            }
        }

        public void Damage(float damage, Vector2? knockback = null)
        {
            _currentHealth -= (int)Math.Round(damage);
            if (_currentHealth <= 0) Die();
        }

        private void Die()
        {
            Destroy(gameObject);
        }
    }
}