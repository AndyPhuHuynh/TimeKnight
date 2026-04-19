using UnityEngine;
using TimeKnight.Core.Player;
using TimeKnight.Utils;
using UnityEngine.UI;

namespace TimeKnight.Core.HUD
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Slider healthFillSlider = null!;
        [SerializeField] private PlayerManager playerManager = null!;

        private void OnValidate()
        {
            Validation.NotNull(this, healthFillSlider, nameof(healthFillSlider));
        }

        private void OnEnable()
        {
            Validation.NotNull(this, playerManager, nameof(playerManager));
            
            playerManager.MaxHealthChanged += SetMaxHealth;
            playerManager.HealthChanged += SetHealth;

            // Sync immediately in case the player already initialized.
            SetMaxHealth(playerManager.maxHealth);
            SetHealth(playerManager.Health);
        }

        private void OnDisable()
        {
            if (playerManager == null)
            {
                return;
            }

            playerManager.MaxHealthChanged -= SetMaxHealth;
            playerManager.HealthChanged -= SetHealth;
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