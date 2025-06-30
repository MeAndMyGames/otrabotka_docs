using UnityEngine;                           
using System;
using System.Collections.Generic;
using Otrabotka.Managers;                    
using EventData = Otrabotka.Core.Event;

namespace Otrabotka.Managers
{
    public class ScenarioProgressController
    {
        private List<EventData> _events;
        private int _currentIndex;

        public event Action<float> OnTimeShift;
        public event Action<int> OnReplaceNextChunk;

        public bool HasMoreEvents => _events != null && _currentIndex < _events.Count;

        public EventData CurrentEvent => HasMoreEvents ? _events[_currentIndex] : null;

        public void StartTracking(List<EventData> events)
        {
            _events = new List<EventData>(events);
            _currentIndex = 0;
            TriggerCurrent();
        }

        public void CompleteCurrentEvent(bool success, float timeShift = 0f)
        {
            var evt = CurrentEvent;
            if (evt == null) return;

            if (!success && timeShift > 0f)
            {
                OnTimeShift?.Invoke(timeShift);
                if (_currentIndex + 1 < _events.Count)
                    OnReplaceNextChunk?.Invoke(_events[_currentIndex + 1].Id);
            }

            _currentIndex++;
            TriggerCurrent();
        }

        private void TriggerCurrent()
        {
            if (CurrentEvent != null)
                Debug.Log($"[ScenarioProgress] Запуск события id={CurrentEvent.Id}");
            else
                Debug.Log("[ScenarioProgress] Все события дня обработаны");
        }
    }
}

