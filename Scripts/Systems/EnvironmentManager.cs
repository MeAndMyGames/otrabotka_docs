namespace Otrabotka.Systems
{
    using UnityEngine;
    using Otrabotka.Core;
    using Otrabotka.Interfaces;
    using Otrabotka.Configs;

    /// <summary>
    /// Управление циклом дня: смещение и автопрогресс времени.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnvironmentManager : MonoBehaviour, ITimeShifter
    {
        [SerializeField] private Light SunLight;
        [SerializeField] private DayCycleSettings dayCycleSettings;
        [Header("Автоматический прогресс времени")]
        [SerializeField] public bool autoTime = false;
        [Tooltip("Множитель скорости времени (1 = реальное время)")]
        [SerializeField] public float timeScale = 1f;

        private float _currentHour;
        private float _lastRealtime;
        public float CurrentHour => _currentHour;

        private void Awake()
        {
            ServiceLocator.Register<ITimeShifter>(this);
            _currentHour = dayCycleSettings.startHour;
            _lastRealtime = Time.realtimeSinceStartup;
            UpdateLighting();
        }

        private void Update()
        {
            if (autoTime)
            {
                float now = Time.realtimeSinceStartup;
                float dtSec = (now - _lastRealtime) * timeScale;
                _lastRealtime = now;
                float dtHour = dtSec / 3600f;
                _currentHour = (_currentHour + dtHour) % 24f;
                UpdateLighting();
            }
        }

        /// <summary>
        /// Ручной сдвиг игрового часа (для тестов и событий).
        /// </summary>
        public void ShiftTime(float hours)
        {
            _currentHour = (_currentHour + hours) % 24f;
            UpdateLighting();
            ServiceLocator.Get<IMissionTimer>()?.RegisterShift(hours);
            Debug.Log($"[EnvironmentManager] ShiftTime: +{hours}h → {_currentHour:F2}h");
        }

        private void UpdateLighting()
        {
            if (SunLight == null) return;
            float normalized = _currentHour / 24f;
            float angleX = normalized * 360f - 90f;
            SunLight.transform.rotation = Quaternion.Euler(angleX, 170f, 0);
            SunLight.intensity = dayCycleSettings.intensityOverDay.Evaluate(normalized);
        }
    }
}