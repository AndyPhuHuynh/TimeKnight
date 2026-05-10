using System;
using System.Collections;
using TimeKnight.Core.Audio;
using TimeKnight.Core.Input;
using TimeKnight.Extensions;
using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.Player
{
    public class PlayerManager : MonoBehaviour
    {
        public int maxHealth = 10;
        public int Health { get; private set; }
        public event Action<int> MaxHealthChanged = delegate { };
        public event Action<int> HealthChanged = delegate { };
        
        [Header("Knockback Application")]
        [SerializeField] private Rigidbody2D rb = null!;
        [SerializeField] private InputReader inputReader = null!;
        
        [Header("Player Stats")]
        [SerializeField] private float baseDamage = 2;
        [SerializeField] private float critChance = 0.1f;
        [SerializeField] private float critDamageMultiplier = 2.0f;
        [SerializeField] private float damageResistance = 1.0f;
        [SerializeField] private float damageRecoveryTime = 1f;
        [SerializeField] private float knockbackStunTime = 0.5f;
        private bool _isInvincible;
        
        [Header("Audio")]
        [SerializeField] private AudioSource hurtAudioSource = null!;
        [SerializeField] private AudioClip hurtAudioClip = null!;

        private readonly AudioClipParams _hurtAudioClipParams = new()
        {
            PitchVariance = 0.25f,
            Volume = 1.0f,
        };

        private void OnValidate()
        {
            Validation.NotNull(this, rb, "Rigidbody2D");
            Validation.NotNull(this, inputReader, "Input Reader");
            Validation.NotNull(this, hurtAudioSource, "AudioSource");
            Validation.NotNull(this, hurtAudioClip, "AudioClip");
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

        public void Damage(float damage)
        {
            if (_isInvincible) return;

            ApplyDamage(damage);
        }

        public void Damage(float damage, Vector2 knockback)
        {
            if (_isInvincible) return;

            ApplyDamage(damage);
            StartCoroutine(KnockbackStun());
            Combat.ApplyKnockback(rb, knockback);
        }

        private void ApplyDamage(float damage)
        {
            hurtAudioSource.clip = hurtAudioClip;
            hurtAudioSource.SetParams(_hurtAudioClipParams);
            hurtAudioSource.Play();
            
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

        private IEnumerator KnockbackStun()
        {
            float timePassed = 0;
            InputState initialState = inputReader.SaveState();
            inputReader.SetMapStatus(InputStatus.Disabled, ActionMaps.Every);
            while (timePassed < knockbackStunTime)
            {
                timePassed += Time.deltaTime;
                yield return null;
            }

            // Reset x velocity 
            rb.linearVelocity = new Vector2(0f, rb.linearVelocityY);
            inputReader.RestoreState(initialState);
        }
    }
}