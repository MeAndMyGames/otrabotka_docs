namespace Otrabotka.Systems
{
    using UnityEngine;
    using UnityEngine.UI;
    using Otrabotka.Core;
    using Otrabotka.Interfaces;

    /// <summary>
    /// ���������� UI ��� ������������ ������� � �������.
    /// </summary>
    [DisallowMultipleComponent]
    public class DebugOverlay : MonoBehaviour
    {
        [SerializeField] private Text currentHourText;
        [SerializeField] private Text countdownText;

        private EnvironmentManager _envManager;
        private MissionTimer _timer;

        private void Start()
        {
            _envManager = ServiceLocator.Get<ITimeShifter>() as EnvironmentManager;
            _timer = ServiceLocator.Get<IMissionTimer>() as MissionTimer;
            if (_timer != null)
                _timer.OnTimeout += () => Debug.Log("[DebugOverlay] ������� ����-��� ������.");
        }

        private void Update()
        {
            if (_envManager != null && currentHourText != null)
                currentHourText.text = $"Game Hour: {_envManager.CurrentHour:F2}";
            if (countdownText != null && _timer != null)
                countdownText.text = $"Time Left: {_timer.RemainingHours:F2}h";
        }

        private void OnGUI()
        {
            if (_envManager == null || _timer == null) return;

            GUILayout.BeginArea(new Rect(10, 150, 180, 140), "Debug Controls", GUI.skin.window);

            // �������������� ����� � �������
            _envManager.autoTime = GUILayout.Toggle(_envManager.autoTime, "Auto Time");
            GUILayout.Label($"Scale: {_envManager.timeScale:F1}x");
            _envManager.timeScale = GUILayout.HorizontalSlider(_envManager.timeScale, 0.1f, 1000f);

            // ������ ������ �������
            if (GUILayout.Button("+1h")) _envManager.ShiftTime(1f);
            if (GUILayout.Button("+6h")) _envManager.ShiftTime(6f);

            // ����� ������� ������
            if (GUILayout.Button("Reset Timer"))
            {
                _timer.ResetTimer();
            }

            GUILayout.EndArea();
        }
    }
}