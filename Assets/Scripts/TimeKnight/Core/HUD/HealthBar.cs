using UnityEngine;
using TimeKnight.Core.Player;
using UnityEngine.UI;

namespace TimeKnight.Core.HUD
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Slider HealthFillSlider;
        private PlayerManager _playerManager;

        private void OnValidate()
        {
            Debug.Assert(HealthFillSlider != null, $"HealthFillSlider reference in {gameObject.name} not assigned.");
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
            SetMaxHealth(_playerManager.MaxHealth);
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

        public void SetHealth(int health)
        {
            HealthFillSlider.value = health;
        }

        public void SetMaxHealth(int maxHealth)
        {
            HealthFillSlider.maxValue = maxHealth;
        }
    }
}