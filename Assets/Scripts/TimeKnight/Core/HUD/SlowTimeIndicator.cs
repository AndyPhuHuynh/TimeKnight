using UnityEngine.UI;
using UnityEngine;
using TimeKnight.Core.TimePower;
using TimeKnight.Utils;

namespace TimeKnight.Core.HUD
{
    public class SlowTimeIndicator : MonoBehaviour
    {
        [SerializeField] private Image cooldownImage = null!;

        private void OnValidate()
        {
            Validation.NotNull(this, cooldownImage, nameof(cooldownImage));
        }

        private void Awake()
        {
            UpdateCooldownOverlay(0);
        }

        private void OnEnable()
        {
            TimeManager.OnCooldownChange += UpdateCooldownOverlay;
        }

        private void OnDisable()
        {
            TimeManager.OnCooldownChange -= UpdateCooldownOverlay;
        }

        private void UpdateCooldownOverlay(float value)
        {
            cooldownImage.fillAmount = value;
        }
    }
}