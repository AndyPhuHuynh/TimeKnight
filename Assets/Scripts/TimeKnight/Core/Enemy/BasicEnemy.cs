using System;
using TimeKnight.Core.Player;
using UnityEngine;

namespace TimeKnight.Core.Enemy
{
    public class BasicEnemy : MonoBehaviour, IDamageable
    {
        [SerializeField] private int MaxHealth = 5;
        [SerializeField] private int PlayerCollisionDamage = 3;
        private int _currentHealth;
        

        private void Start()
        {
            _currentHealth = MaxHealth;
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.tag != "PlayerManager") return;

            collision.gameObject.GetComponent<PlayerManager>().Damage(PlayerCollisionDamage);
        }

        public void Damage(float damage)
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