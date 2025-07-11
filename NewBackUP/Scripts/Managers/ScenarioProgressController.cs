using UnityEngine;                           
using System;
using System.Collections.Generic;
using Otrabotka.Managers;                    
using Otrabotka.UI;
using EventData = Otrabotka.Core.Event;

namespace Otrabotka.Managers
{
    public class ScenarioProgressController
    {
        private List<EventData> _events;
        private int _currentIndex;

        public event Action<float> OnTimeShift;
        public event Action<int> OnReplaceNextChunk;

        private EventDialogUI _dialogUI;

        public bool HasMoreEvents => _events != null && _currentIndex < _events.Count;

        public EventData CurrentEvent => HasMoreEvents ? _events[_currentIndex] : null;

        public void Initialize(EventDialogUI dialogUI)
        {
            _dialogUI = dialogUI;
            if (_dialogUI != null)
            {
                _dialogUI.OnChoiceMade += HandlePlayerChoice;
            }
        }

        public void StartTracking(List<EventData> events)
        {
            _events = new List<EventData>(events);
            _currentIndex = 0;
            TriggerCurrent();
        }

        public void Tick(float deltaTime)
        {
            // Тут можно обновлять таймер текущего события, если он сам не управляется извне
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
            var evt = CurrentEvent;
            if (evt != null)
            {
                _dialogUI?.ShowEventDialog(evt);
                Debug.Log($"[ScenarioProgress] Запуск события id={evt.Id}");
            }
            else
            {
                Debug.Log("[ScenarioProgress] Все события дня обработаны");
            }
        }

        private void HandlePlayerChoice(bool success)
        {
            var currentEvent = CurrentEvent;
            if (currentEvent != null)
            {
                float timeShift = success ? 0f : currentEvent.TimeShiftOnFail;
                CompleteCurrentEvent(success, timeShift);
                _dialogUI?.HideDialog();
            }
        }

        public void Shutdown()
        {
            if (_dialogUI != null)
            {
                _dialogUI.OnChoiceMade -= HandlePlayerChoice;
            }
        }
    }
}

