using UnityEngine.UI;
using UnityEngine;
using TimeKnight.Core.TimePower;
using TimeKnight.Utils;

namespace TimeKnight.Core.HUD
{
    public class SlowTimeIndicator : MonoBehaviour
    {
        [SerializeField] private Image cooldownImage = null!;
        private TimeManager _slowTimeManager = null!;

        private void OnValidate()
        {
            Validation.NotNull(this, cooldownImage, nameof(cooldownImage));
        }

        private void Awake()
        {
            _slowTimeManager = GameObject.FindWithTag("Player").GetComponentInChildren<TimeManager>();
            UpdateCooldownOverlay(0);
        }

        private void OnEnable()
        {
            _slowTimeManager.OnCooldownChange += UpdateCooldownOverlay;
        }

        private void OnDisable()
        {
            _slowTimeManager.OnCooldownChange -= UpdateCooldownOverlay;
        }

        private void UpdateCooldownOverlay(float value)
        {
            cooldownImage.fillAmount = value;
        }
    }
}