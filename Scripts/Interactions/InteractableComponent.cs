using System;
using UnityEngine;
using Otrabotka.Interfaces;
using Otrabotka.Configs;
using Otrabotka.Systems;
using Otrabotka.Core;
using Otrabotka.Managers;
using System.Linq;

namespace Otrabotka.Interactions
{
    [RequireComponent(typeof(Collider))]
    public class InteractableComponent : MonoBehaviour, IInteractable
    {
        [SerializeField] private InteractionDefinition definition;
        private bool _hasInteracted = false;
        private GameObject _currentVehicle;

        public event Action<InteractionResult> OnInteractionComplete;

        private void Awake()
        {
            // Инициализируем состояние из сохранения
            if (ScenarioDirector.Instance != null && !string.IsNullOrEmpty(definition.id))
            {
                var dayData = ScenarioSaveManager.GetDayData(ScenarioDirector.Instance.CurrentDay);
                if (dayData != null && dayData.CompletedInteractions != null && dayData.CompletedInteractions.Contains(definition.id))
                {
                    _hasInteracted = true;
                    if (definition.disableAfterUse)
                        gameObject.SetActive(false);
                }
            }
            // Подписываемся на завершение для сохранения и вложенных
            OnInteractionComplete += result => HandleCompletion();
        }

        public void ExecuteInteraction()
        {
            if (!definition.isRepeatable && _hasInteracted) return;

            switch (definition.interactionType)
            {
                case InteractionType.Dialogue:
                    ExecuteDialogue();
                    break;
                case InteractionType.ItemTransfer:
                    ExecuteItemTransfer();
                    break;
                case InteractionType.InventorySelection:
                    ExecuteInventorySelection();
                    break;
                case InteractionType.CharacterSwitch:
                    ExecuteCharacterSwitch();
                    break;
                case InteractionType.Animation:
                    ExecuteAnimation();
                    break;
                case InteractionType.Cutscene:
                    ExecuteCutscene();
                    break;
                case InteractionType.FinalEvent:
                    ExecuteFinalEvent();
                    break;
                case InteractionType.VehicleExit:
                    ExecuteVehicleExit();
                    break;
                default:
                    OnInteractionComplete?.Invoke(new InteractionResult { Type = definition.interactionType, Data = null });
                    break;
            }

            _hasInteracted = true;
            if (definition.disableAfterUse)
                gameObject.SetActive(false);
        }

        /// <summary>
        /// Устанавливает новое определение взаимодействия (для вложенных).
        /// </summary>
        public void SetDefinition(InteractionDefinition def)
        {
            definition = def;
            _hasInteracted = false;
        }

        /// <summary>
        /// Обработка завершения: сохранение и вызов вложенных взаимодействий.
        /// </summary>
        private void HandleCompletion()
        {
            PersistInteraction();
            ExecuteNestedInteractions();
        }

        private void PersistInteraction()
        {
            // Сохраняем ID выполненного взаимодействия
            var director = Managers.ScenarioDirector.Instance;
            if (director == null || string.IsNullOrEmpty(definition.id)) return;
            int day = director.CurrentDay;
            var dayData = Core.ScenarioSaveManager.GetDayData(day);
            if (dayData.CompletedInteractions == null)
                dayData.CompletedInteractions = new System.Collections.Generic.List<string>();
            if (!dayData.CompletedInteractions.Contains(definition.id))
            {
                dayData.CompletedInteractions.Add(definition.id);
                Core.ScenarioSaveManager.SetDayData(dayData);
            }
        }

        private void ExecuteNestedInteractions()
        {
            if (definition.nestedInteractions == null) return;
            foreach (var nested in definition.nestedInteractions)
            {
                if (nested == null) continue;
                var comp = gameObject.AddComponent<InteractableComponent>();
                comp.SetDefinition(nested);
                comp.ExecuteInteraction();
            }
        }

        private void ExecuteDialogue()
        {
            if (definition.dialogueAsset == null)
            {
                OnInteractionComplete?.Invoke(new InteractionResult { Type = InteractionType.Dialogue });
                return;
            }
            DialogueManager.Instance.OnDialogueComplete += HandleDialogueComplete;
            DialogueManager.Instance.StartDialogue(definition.dialogueAsset);
        }

        private void HandleDialogueComplete()
        {
            DialogueManager.Instance.OnDialogueComplete -= HandleDialogueComplete;
            OnInteractionComplete?.Invoke(new InteractionResult { Type = InteractionType.Dialogue });
        }

        private void ExecuteItemTransfer()
        {
            // TODO: реализация добавления предмета в инвентарь
            OnInteractionComplete?.Invoke(new InteractionResult { Type = InteractionType.ItemTransfer, Data = new { id = definition.itemIdToGive, amount = definition.itemAmount } });
        }

        private void ExecuteInventorySelection()
        {
            // TODO: реализация открытия инвентаря и выбора предмета
            OnInteractionComplete?.Invoke(new InteractionResult { Type = InteractionType.InventorySelection, Data = null });
        }

        private void ExecuteCharacterSwitch()
        {
            // Delegate transport logic to TransportController
            TransportController.Instance.EnterVehicle(definition.newCharacterPrefab, definition);
            OnInteractionComplete?.Invoke(new InteractionResult { Type = InteractionType.CharacterSwitch, Data = definition.newCharacterPrefab });
        }

        private void ExecuteAnimation()
        {
            // TODO: реализация проигрывания анимации
            OnInteractionComplete?.Invoke(new InteractionResult { Type = InteractionType.Animation, Data = definition.animationClips });
        }

        private void ExecuteCutscene()
        {
            // TODO: реализация запуска кат-сцены
            OnInteractionComplete?.Invoke(new InteractionResult { Type = InteractionType.Cutscene, Data = definition.cutsceneName });
        }

        private void ExecuteFinalEvent()
        {
            // TODO: реализация финального события
            OnInteractionComplete?.Invoke(new InteractionResult { Type = InteractionType.FinalEvent, Data = null });
        }

        private void ExecuteVehicleExit()
        {
            // Delegate exit logic to TransportController
            TransportController.Instance.ExitVehicle();
            OnInteractionComplete?.Invoke(new InteractionResult { Type = InteractionType.VehicleExit, Data = null });
            if (definition.disableAfterUse)
                gameObject.SetActive(false);
        }
    }
} 