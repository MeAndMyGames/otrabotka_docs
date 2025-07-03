namespace Otrabotka.Systems
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using Otrabotka.Core;
    using Otrabotka.Interfaces;
    using Otrabotka.Configs;
    /// <summary>
    /// Управляет оставшимся временем миссии: тикает вместе с автопрогрессом и уменьшает на ShiftTime.
    /// </summary>
    [DisallowMultipleComponent]
    public class MissionTimer : MonoBehaviour, IMissionTimer
    {
        [Tooltip("Конфиг тайм-аута миссии")]
        [SerializeField] private MissionSettings missionSettings;
        [Tooltip("UI Text для отображения оставшегося времени до таймаута")]
        [SerializeField] private Text countdownText;
        public event Action OnTimeout;
        private float _remainingHours;
        public float RemainingHours => _remainingHours;
        private bool _notified = false;
        private void Awake()
        {
            ServiceLocator.Register<IMissionTimer>(this);
            _remainingHours = missionSettings.missionTimeoutHours;
        }
        private void Update()
        {
            if (_notified) return;
            float delta = Time.deltaTime;
            var env = ServiceLocator.Get<ITimeShifter>() as EnvironmentManager;
            if (env != null && env.autoTime)
            {
                float dtHour = delta * env.timeScale / 3600f;
                DecreaseTime(dtHour);
            }
            UpdateCountdown();
        }
        public void RegisterShift(float hours)
        {
            DecreaseTime(hours);
        }
        private void DecreaseTime(float hours)
        {
            _remainingHours -= hours;
            if (_remainingHours <= 0f) TriggerTimeout();
        }
        private void TriggerTimeout()
        {
            if (_notified) return;
            _notified = true;
            _remainingHours = 0f;
            Debug.LogWarning("[MissionTimer] Тайм-аут миссии достигнут, день окончен, игрок проиграл.");
            OnTimeout?.Invoke();
            ServiceLocator.Get<ICinematicPlayer>()?.PlayFailureCinematic(OnCinematicComplete);
        }
        private void UpdateCountdown()
        {
            if (countdownText == null) return;
            TimeSpan span = TimeSpan.FromHours(_remainingHours);
            countdownText.text = $"Time Left: {span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";
        }
        private void OnCinematicComplete()
        {
            PlayerPrefs.DeleteKey("MissionStartBinary");
            PlayerPrefs.DeleteKey("AccumulatedShiftHours");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        /// <summary>
        /// Сброс таймера к начальным значениям.
        /// </summary>
        public void ResetTimer()
        {
            _notified = false;
            _remainingHours = missionSettings.missionTimeoutHours;
            UpdateCountdown();
        }
    }
}