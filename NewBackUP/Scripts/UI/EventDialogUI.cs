using UnityEngine;
using UnityEngine.UI;
using System;
using Otrabotka.Core;
using EventData = Otrabotka.Core.Event;

namespace Otrabotka.UI
{
    public class EventDialogUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private Text eventTitleText;
        [SerializeField] private Text eventDescriptionText;
        [SerializeField] private Button successButton;
        [SerializeField] private Button failButton;
        [SerializeField] private Button neutralButton;

        public event Action<bool> OnChoiceMade;

        private void Awake()
        {
            HideDialog();
            SetupButtons();
        }

        private void SetupButtons()
        {
            successButton?.onClick.AddListener(() => OnChoiceMade?.Invoke(true));
            failButton?.onClick.AddListener(() => OnChoiceMade?.Invoke(false));
            neutralButton?.onClick.AddListener(() => OnChoiceMade?.Invoke(false));
        }

        public void ShowEventDialog(EventData eventData)
        {
            if (eventData == null) return;

            eventTitleText.text = $"Событие #{eventData.Id}";
            eventDescriptionText.text = eventData.ResultData?.ToString() ?? "Описание события...";
            
            dialogPanel.SetActive(true);
            
            // Настраиваем кнопки в зависимости от типа события
            SetupButtonsForEvent(eventData);
        }

        private void SetupButtonsForEvent(EventData eventData)
        {
            // Логика настройки кнопок в зависимости от события
            successButton.gameObject.SetActive(true);
            failButton.gameObject.SetActive(true);
            neutralButton.gameObject.SetActive(false);
        }

        public void HideDialog()
        {
            dialogPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            successButton?.onClick.RemoveAllListeners();
            failButton?.onClick.RemoveAllListeners();
            neutralButton?.onClick.RemoveAllListeners();
        }
    }
} 