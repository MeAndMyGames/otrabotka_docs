using System.Collections.Generic;
using Otrabotka.Scenario.Providers;
using System.IO;
using UnityEngine;
using Otrabotka.Scenario;
using Otrabotka.Level.Configs;
using Otrabotka.Scenario.Configs;


namespace Otrabotka.Core
{
    /// <summary>
    /// ��������� ��������: ����������� ��� ������������ �������, ������������ ��� � ��.
    /// </summary>
    public class ScenarioSettings : MonoBehaviour
    {
        [Tooltip("������������ ����������� ��������� �������")]
        public StaticSequenceConfig StaticSequenceConfig;

        [Header("Hospital Biome")]
        [Tooltip("Префабы чанков основного биома больницы (Диспетчерская, Приемная, Гараж)")]
        public List<ChunkConfig> HospitalChunks;
        [Tooltip("Количество чанков больницы в начале дня")]
        public int HospitalChunkCount = 3;

        [Header("Simple Random Chunks")]
        [Tooltip("Список простых рандомных чанков")]
        public List<ChunkConfig> SimpleChunkConfigs;
        [Tooltip("Вероятность простых событий на чанке (0-1)")]
        [Range(0f, 1f)]
        public float SimpleEventProbability = 0.1f;

        [Header("Complex Events")]
        [Tooltip("Конфигурации сложных событий")]
        public List<ComplexEventConfig> ComplexEventConfigs;

        [Header("Final Biomes")]
        [Tooltip("Конфигурации финальных биомов")]
        public List<FinalBiomeConfig> FinalBiomes;

        [Header("End Day")]
        [Tooltip("Имя сцены или метод для конца дня")]
        public string EndDaySceneName = "EndOfDayScene";

        [Header("Level Generation")]
        [Tooltip("Общее число чанков в уровне")]
        public int TotalChunks = 50;

        [Header("Day Dialogues")]
        [Tooltip("Список ассетов диалогов для каждого дня")]        
        public List<DialogueAsset> DayDialogues;

        /// <summary>
        ///   ,    StaticSequenceConfig.
        /// </summary>
        public IStaticSequenceProvider StaticSequenceProvider =>
            new StaticSequenceProvider(StaticSequenceConfig);

        /// <summary>
        ///    .
        /// </summary>
        public IKeyEventsProvider KeyEventsProvider =>
            new KeyEventsProvider();

        /// <summary>
        ///    .
        /// </summary>
        public IRandomEventProvider RandomEventProvider =>
            new RandomEventProvider();
    }

    /// <summary>
    ///      .
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
            // 
            foreach (var tpl in _static.GetStaticSequence(day))
                Events.Add(new Event
                {
                    Id = tpl.Id,
                    Duration = tpl.Duration,
                    ResultData = tpl.ResultData
                });

            // 
            foreach (var tpl in _keys.GetKeyEvents(day))
                Events.Add(new Event
                {
                    Id = tpl.Id,
                    Duration = tpl.Duration,
                    ResultData = tpl.ResultData
                });

            // 
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
                new Dialogue { /*   */ }
            };
        }

        public void SaveProgress() { }
    }

    /// <summary>
    ///      .
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
    ///      .
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
            // 
        }
    }

    /// <summary>
    ///   .
    /// </summary>
    public class Event
    {
        public int Id;
        public float Duration;
        public object ResultData;
        public bool IsComplete;
        /// <summary>   - (   )?</summary>
        public bool HasSecondaryState;
        /// <summary>      </summary>
        public float TimeShiftOnFail;
        /// <summary>ID ����� �� �������</summary>
        public int PrimaryChunkPrefabId;
        /// <summary>ID ����� ����� �������</summary>
        public int SecondaryChunkPrefabId;
    }

    /// <summary>
    /// ������ �������.
    /// </summary>
    public class Dialogue
    {
        public string Text;
        public override string ToString() => Text;
    }

    /// <summary>
    /// ������, ����������� ��� ������ ���: ����� � ���������.
    /// </summary>
    [System.Serializable]
    public class DayData
    {
        public int DayNumber;
        public float TimeOfDay;
        public InventoryData Inventory;
        /// <summary>
        /// Список идентификаторов выполненных взаимодействий за этот день.
        /// </summary>
        public List<string> CompletedInteractions = new List<string>();
    }

    /// <summary>
    ///    .
    /// </summary>
    [System.Serializable]
    public class InventoryData { }

    /// <summary>
    /// ������� ���������� � �������� ��������� ��������.
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
