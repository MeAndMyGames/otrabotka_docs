using System.Collections.Generic;
using UnityEngine;                  // ← для TooltipAttribute
using Otrabotka.Core;              // ← для Core.Event
using Otrabotka.Scenario.Providers; // ← для интерфейсов

namespace Otrabotka.Scenario
{
    [System.Serializable]
    public class EventTemplate
    {
        [Tooltip("Уникальный идентификатор шаблона эвента")]
        public int Id;
        [Tooltip("Длительность эвента в секундах")]
        public float Duration;
        [Tooltip("Дополнительная строка с данными результата")]
        public string ResultData;
        [Tooltip("Есть ли у этого события «после»-состояние?")]
        public bool HasSecondaryState;
        [Tooltip("На сколько часов сдвигать время при провале")]
        public float TimeShiftOnFail;
        [Tooltip("ID префаба чанка до события")]
        public int PrimaryChunkPrefabId;
        [Tooltip("ID префаба чанка после провала события")]
        public int SecondaryChunkPrefabId;
    }

    public class EventSequenceBuilder
    {
        private readonly IStaticSequenceProvider _staticProvider;
        private readonly IKeyEventsProvider _keyProvider;
        private readonly IRandomEventProvider _randomProvider;

        public EventSequenceBuilder(
            IStaticSequenceProvider staticProvider,
            IKeyEventsProvider keyProvider,
            IRandomEventProvider randomProvider)
        {
            _staticProvider = staticProvider;
            _keyProvider = keyProvider;
            _randomProvider = randomProvider;
        }

        public List<Core.Event> Compose(int day)
        {
            var result = new List<Core.Event>();

            foreach (var tpl in _staticProvider.GetStaticSequence(day))
                result.Add(CreateEventInstance(tpl));
            foreach (var tpl in _keyProvider.GetKeyEvents(day))
                result.Add(CreateEventInstance(tpl));

            EventTemplate randomTpl;
            while ((randomTpl = _randomProvider.GetRandomTemplate()) != null)
                result.Add(CreateEventInstance(randomTpl));

            return result;
        }

        private Core.Event CreateEventInstance(EventTemplate tpl)
        {
            return new Core.Event
            {
                Id = tpl.Id,
                Duration = tpl.Duration,
                ResultData = tpl.ResultData,
                IsComplete = false,
                HasSecondaryState = tpl.HasSecondaryState,
                TimeShiftOnFail = tpl.TimeShiftOnFail,
                PrimaryChunkPrefabId = tpl.PrimaryChunkPrefabId,
                SecondaryChunkPrefabId = tpl.SecondaryChunkPrefabId
            };
        }
    }
}
