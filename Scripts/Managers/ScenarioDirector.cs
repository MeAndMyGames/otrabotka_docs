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
        [SerializeField] private ChunkManager _chunkManager;
        [SerializeField] private EnvironmentManager _envManager;

        private ScenarioSettings _settings;
        private DayConstructor _dayConstructor;
        private ScenarioProgressController _progress;
        private DialogueEngine _dialogue;

        private int _currentDay;

        public override void Initialize()
        {
            _settings = GetComponent<ScenarioSettings>();
            _dayConstructor = new DayConstructor(_settings);
            _progress = new ScenarioProgressController();
            _dialogue = new DialogueEngine();

            // подписываемся на события прогресса
            _progress.OnTimeShift += HandleTimeShift;
            _progress.OnReplaceNextChunk += _chunkManager.ReplaceChunkForEvent;

            // старт первого дня
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
            _dialogue.StartDialogue(_dayConstructor.DialoguesForDay(_currentDay));
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
