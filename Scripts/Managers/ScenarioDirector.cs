using UnityEngine;
using Otrabotka.Core;
using Otrabotka.Managers;
using Otrabotka.Systems;
using EventData = Otrabotka.Core.Event;

namespace Otrabotka.Managers
{
    [RequireComponent(typeof(ScenarioSettings))]
    public class ScenarioDirector : ManagerBase
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static ScenarioDirector Instance { get; private set; }
        [SerializeField] private ChunkManager _chunkManager;
        [SerializeField] private EnvironmentManager _envManager;

        private ScenarioSettings _settings;
        private DayConstructor _dayConstructor;
        private ScenarioProgressController _progress;
        private int _currentDay;
        /// <summary>
        /// Текущий номер дня.
        /// </summary>
        public int CurrentDay => _currentDay;

        public override void Initialize()
        {
            Instance = this;
            _settings = GetComponent<ScenarioSettings>();
            _dayConstructor = new DayConstructor(_settings);
            _progress = new ScenarioProgressController();

            // подписываемся на события прогресса
            _progress.OnTimeShift += HandleTimeShift;
            _progress.OnReplaceNextChunk += _chunkManager.ReplaceChunkForEvent;

            // День будет запущен в Start(), чтобы все Awake выполнились до этого
        }

        private void Start()
        {
            StartDay(1);
        }

        public override void Tick(float deltaTime)
        {
            // Проверяем, завершилось ли текущее событие
            // Предположим, что событие само вызывает CompleteCurrentEvent
            // Иначе — проверяем состояние через _progress.CurrentEvent.IsComplete
        }

        public override void Shutdown()
        {
            // отписываемся
            _progress.OnTimeShift -= HandleTimeShift;
            _progress.OnReplaceNextChunk -= _chunkManager.ReplaceChunkForEvent;
        }

        private void StartDay(int day)
        {
            _currentDay = day;
            Debug.Log($"[ScenarioDirector] День {_currentDay}");
            _dayConstructor.BuildDay(_currentDay);
            _progress.StartTracking(_dayConstructor.Events);
            // Запускаем диалог первого дня через DialogueManager
            if (_settings.DayDialogues != null && _settings.DayDialogues.Count >= _currentDay)
            {
                var dialogueAsset = _settings.DayDialogues[_currentDay - 1];
                Debug.Log($"[ScenarioDirector] Starting day dialogue: {dialogueAsset.name}");
                DialogueManager.Instance.StartDialogue(dialogueAsset);
            }
        }

        private void HandleTimeShift(float hours)
        {
            // здесь можно обновить EnvManager, изменить освещение и т.п.
            _envManager.ShiftTime(hours);
        }

        // Вызывается из ProgressController при CompleteCurrentEvent
        public void OnEventComplete(bool success, float timeShift = 0f)
        {
            _progress.CompleteCurrentEvent(success, timeShift);
        }
    }
}
