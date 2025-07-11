using System;
using UnityEngine;
using UnityEngine.UI;
using Otrabotka.Core;
using Otrabotka.Interfaces;
using Otrabotka.Configs;

namespace Otrabotka.Systems
{
    [DisallowMultipleComponent]
    public class MissionTimer : MonoBehaviour, IMissionTimer
    {
        [Tooltip("Конфиг таймаута миссии")]
        [SerializeField] private MissionSettings missionSettings;
        [Tooltip("UI Text для отображения оставшегося времени")]
        [SerializeField] private Text countdownText;

        public event Action OnTimeout;
        private float _remainingHours;
        private bool _notified = false;

        public float RemainingHours => _remainingHours;

        private void Awake()
        {
            // Регистрируем себя как IMissionTimer
            ServiceLocator.Register<IMissionTimer>(this);
            _remainingHours = missionSettings.missionTimeoutHours;
            UpdateCountdown();
        }

        private void Update()
        {
            if (_notified) return;

            // Авто-прогресс времени через EnvironmentManager → ITimeShifter
            var env = ServiceLocator.Get<ITimeShifter>() as EnvironmentManager;
            if (env != null && env.autoTime)
            {
                float dtHour = Time.deltaTime * env.timeScale / 3600f;
                DecreaseTime(dtHour);
            }
            UpdateCountdown();
        }

        public void RegisterShift(float hours)
        {
            DecreaseTime(hours);
            UpdateCountdown();
        }

        private void DecreaseTime(float hours)
        {
            _remainingHours -= hours;
            if (_remainingHours <= 0f)
                TriggerTimeout();
        }

        private void TriggerTimeout()
        {
            if (_notified) return;
            _notified = true;
            _remainingHours = 0f;
            Debug.LogWarning("[MissionTimer] Тайм-аут миссии — провал дня.");
            OnTimeout?.Invoke();
            ServiceLocator.Get<ICinematicPlayer>()?
                           .PlayFailureCinematic(OnCinematicComplete);
        }

        private void UpdateCountdown()
        {
            if (countdownText == null) return;
            TimeSpan span = TimeSpan.FromHours(_remainingHours);
            countdownText.text = $"Time Left: {span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";
        }

        private void OnCinematicComplete()
        {
            // После кино возвращаем в главное меню
            PlayerPrefs.DeleteKey("MissionStartBinary");
            PlayerPrefs.DeleteKey("AccumulatedShiftHours");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        public void ResetTimer()
        {
            _notified = false;
            _remainingHours = missionSettings.missionTimeoutHours;
            UpdateCountdown();
        }
    }
}
