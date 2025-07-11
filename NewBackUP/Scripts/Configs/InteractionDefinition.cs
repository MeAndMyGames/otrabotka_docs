using System.Collections.Generic;
using UnityEngine;
using Otrabotka.Interfaces;
using Otrabotka.Scenario.Configs;

namespace Otrabotka.Configs
{
    [CreateAssetMenu(menuName = "Otrabotka/Configs/InteractionDefinition", fileName = "InteractionDefinition")]
    public class InteractionDefinition : ScriptableObject
    {
        [Tooltip("Уникальный идентификатор взаимодействия")]
        public string id;

        [Tooltip("Тип взаимодействия")]
        public InteractionType interactionType;

        [Tooltip("Повторяемое взаимодействие")]
        public bool isRepeatable = true;

        [Tooltip("Отключить после использования")]
        public bool disableAfterUse = false;

        [Header("Параметры диалога")]
        public DialogueAsset dialogueAsset;

        [Header("Параметры передачи предмета")]
        public string itemIdToGive;
        public int itemAmount = 1;

        [Header("Параметры выбора предмета")]
        public List<string> itemIdsToSelect;

        [Header("Параметры смены персонажа")]
        public GameObject newCharacterPrefab;

        [Header("Параметры анимации")]
        public List<AnimationClip> animationClips;
        public float animationDuration = 1f;

        [Header("Параметры кат-сцены")]
        public string cutsceneName;

        [Header("Параметры финального события")]
        public bool isFinalEvent = false;

        [Header("Вложенные взаимодействия")]
        public List<InteractionDefinition> nestedInteractions;

        [Header("Параметры движения чанков после смены персонажа")]
        [Tooltip("Скорость движения чанков при движении вправо после посадки")]    
        public float chunkScrollSpeedPositive = 0f;
        [Tooltip("Скорость движения чанков при движении влево после посадки")]    
        public float chunkScrollSpeedNegative = 0f;
        [Header("Параметры инерции прокрутки чанков после смены персонажа")]
        [Tooltip("Включить инерцию прокрутки чанков после посадки")]  
        public bool chunkScrollInertiaEnabled = false;
        [Tooltip("Время сглаживания скорости скролла (сек) после посадки")]
        public float chunkScrollInertiaTime = 0.5f;
        [Header("Параметры камеры после смены персонажа")]
        public CameraSettings cameraSettings;

        [Header("Параметры выхода из машины")]
        [Tooltip("Префаб события выхода из машины (VehicleExit)")]
        public GameObject exitEventPrefab;
    }
} 