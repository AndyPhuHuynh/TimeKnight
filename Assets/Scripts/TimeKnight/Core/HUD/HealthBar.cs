using UnityEngine;
using TimeKnight.Core.Player;
using TimeKnight.Utils;
using UnityEngine.UI;

namespace TimeKnight.Core.HUD
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Slider healthFillSlider = null!;
        private PlayerManager _playerManager = null!;

        private void OnValidate()
        {
            Validation.NotNull(this, healthFillSlider, nameof(healthFillSlider));
        }

        private void OnEnable()
        {
            _playerManager = GameObject.FindWithTag("PlayerManager").GetComponent<PlayerManager>();
            
            if (_playerManager == null)
            {
                Debug.LogWarning("HealthBar could not find PlayerManager in the scene.");
                return;
            }

            _playerManager.MaxHealthChanged += SetMaxHealth;
            _playerManager.HealthChanged += SetHealth;

            // Sync immediately in case the player already initialized.
            SetMaxHealth(_playerManager.maxHealth);
            SetHealth(_playerManager.Health);
        }

        private void OnDisable()
        {
            if (_playerManager == null)
            {
                return;
            }

            _playerManager.MaxHealthChanged -= SetMaxHealth;
            _playerManager.HealthChanged -= SetHealth;
        }

        private void SetHealth(int health)
        {
            healthFillSlider.value = health;
        }

        private void SetMaxHealth(int maxHealth)
        {
            healthFillSlider.maxValue = maxHealth;
        }
    }
}