using UnityEngine;
using TimeKnight.Core.Player;
using TimeKnight.Utils;
using UnityEngine.UI;

namespace TimeKnight.Core.HUD
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Slider healthFillSlider = null!;
        private PlayerManager playerManager = null!;

        private void Awake()
        {
            // This must be run in awake in order for the syncing to occur in enable correctly; because enable happens after awake.
            playerManager = GameObject.FindWithTag("Player").GetComponentInChildren<PlayerManager>();
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