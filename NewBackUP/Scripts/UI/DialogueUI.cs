using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Otrabotka.Scenario.Configs;

namespace Otrabotka.UI
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private Text speakerText;
        [SerializeField] private Text contentText;
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private Button optionButtonPrefab;
        [SerializeField] private GameObject dialoguePanel;

        private Action<DialogueOption> onOptionSelected;

        private void Awake()
        {
            // Авто-назначение полей, если не назначены в инспекторе
            // (удалён автоматический поиск dialoguePanel)
            if (speakerText == null)
            {
                var st = transform.Find("SpeakerText")?.GetComponent<Text>();
                if (st == null && transform.parent != null)
                    st = transform.parent.Find("DialoguePanel/SpeakerText")?.GetComponent<Text>();
                speakerText = st;
            }
            if (speakerText == null)
                Debug.LogError("[DialogueUI] speakerText is not assigned and not found in hierarchy!");

            // ContentText
            if (contentText == null)
            {
                var ct = transform.Find("ContentText")?.GetComponent<Text>();
                if (ct == null && transform.parent != null)
                    ct = transform.parent.Find("DialoguePanel/ContentText")?.GetComponent<Text>();
                contentText = ct;
            }
            if (contentText == null)
                Debug.LogError("[DialogueUI] contentText is not assigned and not found in hierarchy!");

            // OptionsContainer
            if (optionsContainer == null)
            {
                var oc = transform.Find("OptionsContainer");
                if (oc == null && transform.parent != null)
                    oc = transform.parent.Find("DialoguePanel/OptionsContainer");
                optionsContainer = oc;
            }
            if (optionsContainer == null)
                Debug.LogError("[DialogueUI] optionsContainer is not assigned and not found in hierarchy!");

            if (optionButtonPrefab == null)
                Debug.LogError("[DialogueUI] optionButtonPrefab is not assigned!");
            // Панель не скрываем здесь — управляем через Show/Hide вызовы
        }

        public void Show(string speaker, string text, List<DialogueOption> options, Action<DialogueOption> callback)
        {
            Debug.Log($"[DialogueUI] Show() called: speaker={speaker}, text={text.Substring(0, Math.Min(text.Length,20))}...");
            // Показываем DialoguePanel
            gameObject.SetActive(true);
            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);
            speakerText.text = speaker;
            contentText.text = text;
            onOptionSelected = callback;

            // Удаляем старые кнопки
            foreach (Transform t in optionsContainer)
                Destroy(t.gameObject);

            if (options != null && options.Count > 0)
            {
                foreach (var opt in options)
                {
                    var btn = Instantiate(optionButtonPrefab, optionsContainer);
                    var btnText = btn.GetComponentInChildren<Text>();
                    if (btnText != null) btnText.text = opt.Text;
                    btn.onClick.AddListener(() => onOptionSelected(opt));
                }
            }
            else
            {
                // Кнопка завершения диалога
                var btn = Instantiate(optionButtonPrefab, optionsContainer);
                var btnText = btn.GetComponentInChildren<Text>();
                if (btnText != null) btnText.text = "Закончить диалог";
                btn.onClick.AddListener(() => onOptionSelected(new DialogueOption { NextNodeId = -1 }));
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
        }
    }
} 