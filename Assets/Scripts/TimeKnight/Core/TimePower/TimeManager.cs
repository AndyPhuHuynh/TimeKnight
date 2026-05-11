using System;
using System.Collections;
using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.TimePower
{
    public class TimeManager : MonoBehaviour
    {
        // Static properties enemies can reference to slow themselves down.
        public static float CurrentTimeModifier { get; private set; }  = 1f;
        public static float CustomDelta => Time.deltaTime * CurrentTimeModifier;
        // Reference for UI element to update as cooldown changes.
        public static event Action<float> OnCooldownChange = delegate { };
        // Reference for enemies who need to do a specific action on time slow start or end.
        public static event Action OnTimeSlowActivate = delegate { };
        public static event Action OnTimeSlowDeactivate = delegate { };

        [Header("Slow Time Properties")]
        [SerializeField] private float timeModifier = 0.5f;
        [SerializeField] private float slowTimeDuration = 5f;
        [SerializeField] private float slowTimeCooldown = 10f;
        private float _slowTimeCooldownTimer = 0f;
        private bool _isSlowTimeReady => _slowTimeCooldownTimer <= 0;
        private CoWrapper _slowTimeCoWrapper = null!;

        private void Awake()
        {
            _slowTimeCoWrapper = new CoWrapper(this);
        }

        private void Update()
        {
            if (!_isSlowTimeReady && !_slowTimeCoWrapper.IsRunning)
            {
                UpdateCooldown(_slowTimeCooldownTimer -= Time.deltaTime);
            }
        }

        public void ActivateSlowTime()
        {
            if (_isSlowTimeReady)
            {
                _slowTimeCoWrapper.Start(SlowTime());
            }
        }

        private IEnumerator SlowTime()
        {
            UpdateCooldown(slowTimeCooldown);
            float timer = 0f;
            CurrentTimeModifier = timeModifier;
            OnTimeSlowActivate.Invoke();
            while (timer < slowTimeDuration)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            CurrentTimeModifier = 1;
            OnTimeSlowDeactivate.Invoke();
        }


        private void UpdateCooldown(float newValue)
        {
            _slowTimeCooldownTimer = newValue;
            // Action returns a value [0-1] with how much of cooldown is remaining so UI can update properly.
            OnCooldownChange.Invoke(_slowTimeCooldownTimer / slowTimeCooldown);
        }
    }
}