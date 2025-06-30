using System.Collections.Generic;
using Otrabotka.Scenario.Providers;
using System.IO;
using UnityEngine;
using Otrabotka.Scenario;


namespace Otrabotka.Core
{
    /// <summary>
    /// Настройки сценария: используйте для конфигурации событий, длительности дня и др.
    /// </summary>
    public class ScenarioSettings : MonoBehaviour
    {
        [Tooltip("Конфигурация статической секвенции эвентов")]
        public StaticSequenceConfig StaticSequenceConfig;

        /// <summary>
        /// Провайдер статической последовательности, создаётся на основе StaticSequenceConfig.
        /// </summary>
        public IStaticSequenceProvider StaticSequenceProvider =>
            new StaticSequenceProvider(StaticSequenceConfig);

        /// <summary>
        /// Заглушечный провайдер ключевых эвентов.
        /// </summary>
        public IKeyEventsProvider KeyEventsProvider =>
            new KeyEventsProvider();

        /// <summary>
        /// Заглушечный провайдер рандомных эвентов.
        /// </summary>
        public IRandomEventProvider RandomEventProvider =>
            new RandomEventProvider();
    }

    /// <summary>
    /// Строит события и диалоги для каждого дня.
    /// </summary>
    public class DayConstructor
    {
        public List<Event> Events { get; private set; }

        private readonly IStaticSequenceProvider _static;
        private readonly IKeyEventsProvider _keys;
        private readonly IRandomEventProvider _random;

        public DayConstructor(ScenarioSettings settings)
        {
            _static = settings.StaticSequenceProvider;
            _keys = settings.KeyEventsProvider;
            _random = settings.RandomEventProvider;

            Events = new List<Event>();
        }

        public void BuildDay(int day)
        {
            // статика
            foreach (var tpl in _static.GetStaticSequence(day))
                Events.Add(new Event
                {
                    Id = tpl.Id,
                    Duration = tpl.Duration,
                    ResultData = tpl.ResultData
                });

            // ключевые
            foreach (var tpl in _keys.GetKeyEvents(day))
                Events.Add(new Event
                {
                    Id = tpl.Id,
                    Duration = tpl.Duration,
                    ResultData = tpl.ResultData
                });

            // рандомные
            EventTemplate rnd;
            while ((rnd = _random.GetRandomTemplate()) != null)
                Events.Add(new Event
                {
                    Duration = rnd.Duration,
                    ResultData = rnd.ResultData
                });
        }

        public List<Dialogue> DialoguesForDay(int day)
        {
            return new List<Dialogue>
            {
                new Dialogue { /* текст диалога */ }
            };
        }

        public void SaveProgress() { }
    }

    /// <summary>
    /// Трекер прогресса текущего события внутри дня.
    /// </summary>
    public class ScenarioProgressTracker
    {
        public Event CurrentEvent { get; private set; }
        public bool IsCurrentEventComplete => CurrentEvent != null && CurrentEvent.IsComplete;

        public void StartTracking(List<Event> events)
        {
            if (events.Count > 0)
                CurrentEvent = events[0];
        }

        public void AdvanceToNextEvent()
        {
            CurrentEvent = null;
        }

        public void StopTracking()
        {
            CurrentEvent = null;
        }
    }

    /// <summary>
    /// Диалоговый движок для показа и обработки диалогов.
    /// </summary>
    public class DialogueEngine
    {
        public void StartDialogue(List<Dialogue> dialogues)
        {
            if (dialogues.Count > 0)
                Debug.Log($"[Dialogue] {dialogues[0].ToString()}");
        }

        public void ProcessEventResult(object resultData)
        {
            // обработка
        }
    }

    /// <summary>
    /// Модель игрового события.
    /// </summary>
    public class Event
    {
        public int Id;
        public float Duration;
        public object ResultData;
        public bool IsComplete;
        /// <summary>Есть ли у события «после»-состояние (нужна ли подмена чанка)?</summary>
        public bool HasSecondaryState;
        /// <summary>На сколько часов сдвигать время при провале</summary>
        public float TimeShiftOnFail;
        /// <summary>ID чанка до события</summary>
        public int PrimaryChunkPrefabId;
        /// <summary>ID чанка после провала</summary>
        public int SecondaryChunkPrefabId;
    }

    /// <summary>
    /// Модель диалога.
    /// </summary>
    public class Dialogue
    {
        public string Text;
        public override string ToString() => Text;
    }

    /// <summary>
    /// Данные, сохранённые для одного дня: время и инвентарь.
    /// </summary>
    [System.Serializable]
    public class DayData
    {
        public int DayNumber;
        public float TimeOfDay;
        public InventoryData Inventory;
    }

    /// <summary>
    /// Заглушка для данных инвентаря.
    /// </summary>
    [System.Serializable]
    public class InventoryData { }

    /// <summary>
    /// Система сохранений и загрузок прогресса сценария.
    /// </summary>
    public static class ScenarioSaveManager
    {
        private const string SaveFileName = "scenario_save.json";
        private static List<DayData> _daysData;

        public static void LoadProgress()
        {
            var path = Path.Combine(Application.persistentDataPath, SaveFileName);
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var wrapper = JsonUtility.FromJson<DayDataListWrapper>(json);
                _daysData = wrapper?.Days ?? new List<DayData>();
            }
            else
            {
                _daysData = new List<DayData>();
                for (int i = 1; i <= 10; i++)
                    _daysData.Add(new DayData { DayNumber = i, TimeOfDay = 0f, Inventory = new InventoryData() });
                SaveProgress();
            }
        }

        public static void SaveProgress()
        {
            var path = Path.Combine(Application.persistentDataPath, SaveFileName);
            var wrapper = new DayDataListWrapper { Days = _daysData };
            var json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(path, json);
        }

        public static DayData GetDayData(int day)
        {
            return _daysData.Find(d => d.DayNumber == day);
        }

        public static void SetDayData(DayData data)
        {
            var index = _daysData.FindIndex(d => d.DayNumber == data.DayNumber);
            if (index >= 0)
                _daysData[index] = data;
            else
                _daysData.Add(data);
            SaveProgress();
        }

        [System.Serializable]
        private class DayDataListWrapper { public List<DayData> Days; }
    }
}
