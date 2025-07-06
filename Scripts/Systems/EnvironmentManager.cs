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
        // исходная локальная ротация pivot
        private Quaternion _pivotOriginalRotation;
        [SerializeField] private Light SunLight;
        [SerializeField] private DayCycleSettings dayCycleSettings;
        [Header("Автоматический прогресс времени")]
        [SerializeField] public bool autoTime = false;
        [Tooltip("Множитель скорости времени (1 = реальное время)")]
        [SerializeField] public float timeScale = 1f;
        [Header("Sun Rotation Settings")]
        [Tooltip("Center point for sun rotation")]
        [SerializeField] private Transform sunPivot;
        [Tooltip("Distance of the light source from its rotation center")]
        [SerializeField] private float sunDistance = 10f;
        [Header("Sun Orbit Timing")]
        [Tooltip("Duration in hours for first half of sun orbit (e.g., 18)")]
        [SerializeField] private float firstHalfOrbitDuration = 18f;
        [Tooltip("Duration in hours for second half of sun orbit (e.g., 6)")]
        [SerializeField] private float secondHalfOrbitDuration = 6f;

        private float _currentHour;
        private float _lastRealtime;
        public float CurrentHour => _currentHour;

        private void Awake()
        {
            ServiceLocator.Register<ITimeShifter>(this);
            _currentHour = dayCycleSettings.startHour;
            _lastRealtime = Time.realtimeSinceStartup;
            UpdateLighting();
            // сохраняем исходную локальную ротацию pivot и делаем SunLight его дочерним
            if (sunPivot != null && SunLight != null)
            {
                _pivotOriginalRotation = sunPivot.localRotation;
                SunLight.transform.SetParent(sunPivot, false);
                // сразу устанавливаем локальную позицию
                SunLight.transform.localPosition = Vector3.forward * sunDistance;
                SunLight.transform.localRotation = Quaternion.identity;
            }
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
            // вычисляем асимметричный угол X: первая половина орбиты (над горизонтом) за firstHalfOrbitDuration, вторая за secondHalfOrbitDuration
            float startHour = dayCycleSettings.startHour;
            float timeSinceDayStart = (_currentHour - startHour + 24f) % 24f;
            float angleX;
            if (timeSinceDayStart <= firstHalfOrbitDuration)
                angleX = (timeSinceDayStart / firstHalfOrbitDuration) * 180f;
            else
                angleX = 180f + ((timeSinceDayStart - firstHalfOrbitDuration) / secondHalfOrbitDuration) * 180f;
            // динамический поворот pivot: сохраняем исходную ориентацию и добавляем вращение по X
            if (sunPivot != null)
            {
                sunPivot.localRotation = _pivotOriginalRotation * Quaternion.Euler(angleX, 0f, 0f);
                // SunLight как дочерний — позиция задаётся локально
                SunLight.transform.localPosition = Vector3.forward * sunDistance;
                SunLight.transform.localRotation = Quaternion.identity;
            }
            else
            {
                // без pivot — вращение вокруг мировых осей и позиция
                Quaternion rotation = Quaternion.Euler(angleX, 170f, 0f);
                Vector3 center = Vector3.zero;
                SunLight.transform.rotation = rotation;
                SunLight.transform.position = center + rotation * Vector3.forward * sunDistance;
            }
            // интенсивность по кривой времени суток
            float normalizedIntensity = _currentHour / 24f;
            SunLight.intensity = dayCycleSettings.intensityOverDay.Evaluate(normalizedIntensity);
        }
    }
}