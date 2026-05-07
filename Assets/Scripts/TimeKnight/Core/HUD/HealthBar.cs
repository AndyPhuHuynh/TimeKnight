using UnityEngine;
using TimeKnight.Core.Player;
using UnityEngine.UI;

namespace TimeKnight.Core.HUD
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Slider healthFillSlider = null!;
        private PlayerManager? _playerManager;
        
        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Initialize(PlayerManager player)
        {
            _playerManager = player;
            Subscribe();
        }

        private void Subscribe()
        {
            if (_playerManager == null) return;
            Unsubscribe();
            
            _playerManager.MaxHealthChanged += SetMaxHealth;
            _playerManager.HealthChanged += SetHealth;

            SetMaxHealth(_playerManager.maxHealth);
            SetHealth(_playerManager.Health);
        }

        private void Unsubscribe()
        {
            if (_playerManager == null) return;
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