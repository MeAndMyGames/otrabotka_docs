using System;
using UnityEngine;

namespace Otrabotka.Interfaces
{
    /// <summary>
    /// Интерактивный объект, поддерживающий выполнение взаимодействия.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Событие окончания выполнения взаимодействия.
        /// </summary>
        event Action<InteractionResult> OnInteractionComplete;

        /// <summary>
        /// Запуск взаимодействия.
        /// </summary>
        void ExecuteInteraction();
    }

    /// <summary>
    /// Типы взаимодействий.
    /// </summary>
    public enum InteractionType
    {
        Dialogue,
        ItemTransfer,
        InventorySelection,
        CharacterSwitch,
        Animation,
        Cutscene,
        FinalEvent,
        VehicleExit
    }

    /// <summary>
    /// Результат выполнения взаимодействия.
    /// </summary>
    public class InteractionResult
    {
        public InteractionType Type { get; set; }
        public object Data { get; set; }
    }
} 