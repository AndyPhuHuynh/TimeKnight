using TimeKnight.Core.Player;
using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.HUD
{
    public class PlayerStatsDisplay : MonoBehaviour
    {
        [SerializeField] private HealthBar healthBar = null!;

        private void OnValidate()
        {
            Validation.NotNull(this, healthBar, nameof(healthBar));
        }

        public void Initialize(PlayerCombatManager playerManager)
        {
            healthBar.Initialize(playerManager);
        }
    }
}
