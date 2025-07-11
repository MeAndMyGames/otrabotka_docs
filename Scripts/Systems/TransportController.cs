using UnityEngine;
using Otrabotka.Configs;
using Otrabotka.Interfaces;
using Otrabotka.Level;
using System.Collections.Generic;
using Otrabotka.Interactions;
using Otrabotka.Systems;

namespace Otrabotka.Systems
{
    public class TransportController : MonoBehaviour, ITransportUser
    {
        public static TransportController Instance { get; private set; }

        public bool IsInVehicle { get; private set; }
        private GameObject currentVehicle;
        // Текущие конфигурации взаимодействия: entry for re-entry and exit for spawning exit events
        private InteractionDefinition _entryDefinition;
        private InteractionDefinition _exitDefinition;
        private Transform spriteHolder;

        // Информация об оставленных транспортных средствах по индексу чанка
        public class ExitInfo { public InteractionDefinition exitDefinition; public InteractionDefinition entryDefinition; public Vector3 localPosition; }
        private readonly Dictionary<int, ExitInfo> exitMap = new Dictionary<int, ExitInfo>();

        // Vehicle mode settings override tracker
        private bool _settingsApplied = false;
        private float _originalScrollSpeedPositive;
        private float _originalScrollSpeedNegative;
        private bool _originalScrollInertiaEnabled;
        private float _originalScrollInertiaTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void EnterVehicle(GameObject vehiclePrefab, InteractionDefinition definition)
        {
            if (IsInVehicle) return;
            // Override chunk and camera settings for vehicle mode
            if (!_settingsApplied)
            {
                var spawner = FindObjectOfType<ChunkSpawner>();
                if (spawner != null)
                {
                    _originalScrollSpeedPositive = spawner.ScrollSpeedPositive;
                    _originalScrollSpeedNegative = spawner.ScrollSpeedNegative;
                    _originalScrollInertiaEnabled = spawner.ScrollInertiaEnabled;
                    _originalScrollInertiaTime = spawner.ScrollInertiaTime;
                    spawner.SetScrollSpeeds(definition.chunkScrollSpeedPositive, definition.chunkScrollSpeedNegative);
                    spawner.SetInertiaSettings(definition.chunkScrollInertiaEnabled, definition.chunkScrollInertiaTime);
                }
                var cameraController = FindObjectOfType<CameraController>();
                if (cameraController != null)
                {
                    cameraController.SaveSettings();
                    cameraController.ApplySettings(definition.cameraSettings);
                }
                _settingsApplied = true;
            }
            // Save entry definition for future re-entry
            _entryDefinition = definition;
            var playerRoot = GameObject.FindGameObjectWithTag("Player");
            if (playerRoot == null) return;
            // Удаляем старый ExitInfo для этого чанка, чтобы не было дубликатов при повторном спавне
            var chunksContainer = GameObject.Find("ChunksContainer");
            if (chunksContainer != null)
            {
                Transform parentChunk = null;
                float bestDist = float.MaxValue;
                foreach (Transform chunk in chunksContainer.transform)
                {
                    float dist = Mathf.Abs(chunk.position.x - playerRoot.transform.position.x);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        parentChunk = chunk;
                    }
                }
                if (parentChunk != null)
                {
                    int chunkIndex = ParseChunkIndex(parentChunk.name);
                    exitMap.Remove(chunkIndex);
                }
            }
            var holder = playerRoot.transform.Find("SpriteHolder");
            if (holder != null)
            {
                foreach (var r in holder.GetComponentsInChildren<Renderer>(true))
                    r.enabled = false;
            }
            // Instantiate and ensure vehicle is fully active (root and all children)
            currentVehicle = Instantiate(vehiclePrefab, playerRoot.transform.position, playerRoot.transform.rotation);
            // Activate root
            if (!currentVehicle.activeSelf)
                currentVehicle.SetActive(true);
            // Activate all child game objects, in case any are disabled by default
            var allTransforms = currentVehicle.GetComponentsInChildren<Transform>(true);
            foreach (var tr in allTransforms)
                tr.gameObject.SetActive(true);
            currentVehicle.tag = playerRoot.tag;
            currentVehicle.layer = playerRoot.layer;
            spriteHolder = holder;
            // Запоминаем определение для последующего выхода: сначала ищем VehicleExitComponent на префабе
            var exitComp = currentVehicle.GetComponent<Otrabotka.Interactions.VehicleExitComponent>();
            if (exitComp != null && exitComp.ExitDefinition != null)
                _exitDefinition = exitComp.ExitDefinition;
            else
                _exitDefinition = definition;

            IsInVehicle = true;
            Debug.LogWarning("[TransportController] EnterVehicle");
        }

        public void ExitVehicle()
        {
            if (!IsInVehicle) return;
            // Определяем позицию выхода
            Vector3 spawnPos = currentVehicle != null ? currentVehicle.transform.position : Vector3.zero;
            // Ищем ближайший чанк в контейнере чанков
            GameObject chunksContainer = GameObject.Find("ChunksContainer");
            Transform parentChunk = null;
            float bestDist = float.MaxValue;
            if (chunksContainer != null)
            {
                foreach (Transform chunk in chunksContainer.transform)
                {
                    float dist = Mathf.Abs(chunk.position.x - spawnPos.x);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        parentChunk = chunk;
                    }
                }
            }
            if (parentChunk != null && _exitDefinition != null)
            {
                // Парсим индекс чанка из имени, формата Chunk[8]
                int chunkIndex = ParseChunkIndex(parentChunk.name);
                // Локальная позиция внутри чанка
                Vector3 localPos = parentChunk.InverseTransformPoint(spawnPos);
                // Сохраняем информацию: exit and entry definitions
                exitMap[chunkIndex] = new ExitInfo { exitDefinition = _exitDefinition, entryDefinition = _entryDefinition, localPosition = localPos };
                // Инстанцируем префаб события выхода (используем exitDefinition)
                var exitGO = Instantiate(_exitDefinition.exitEventPrefab, parentChunk);

                exitGO.transform.localPosition = localPos;
                // Гарантируем активацию
                exitGO.SetActive(true);
                foreach (var tr in exitGO.GetComponentsInChildren<Transform>(true))
                    tr.gameObject.SetActive(true);
                // Override interactable definition to entryDefinition for re-entry
                var interactable = exitGO.GetComponentInChildren<InteractableComponent>(true);
                if (interactable != null)
                    interactable.SetDefinition(_entryDefinition);
            }

            // Destroy vehicle instance
            if (currentVehicle != null)
                Destroy(currentVehicle);
            if (spriteHolder != null)
            {
                foreach (var r in spriteHolder.GetComponentsInChildren<Renderer>(true))
                    r.enabled = true;
            }
            // Сброс состояния
            IsInVehicle = false;
            _exitDefinition = null;
            // Restore original chunk and camera settings
            if (_settingsApplied)
            {
                var spawner = FindObjectOfType<ChunkSpawner>();
                if (spawner != null)
                {
                    spawner.SetScrollSpeeds(_originalScrollSpeedPositive, _originalScrollSpeedNegative);
                    spawner.SetInertiaSettings(_originalScrollInertiaEnabled, _originalScrollInertiaTime);
                }
                var cameraController = FindObjectOfType<CameraController>();
                if (cameraController != null)
                    cameraController.RestoreSettings();
                _settingsApplied = false;
            }
            Debug.LogWarning("[TransportController] ExitVehicle");
        }

        private void Update()
        {
            if (IsInVehicle && Input.GetKeyDown(KeyCode.E))
                ExitVehicle();
        }

        /// <summary>
        /// Проверяет, есть ли сохранённый выход для указанного чанка.
        /// </summary>
        public bool HasExitForChunk(int chunkIndex) => exitMap.ContainsKey(chunkIndex);

        /// <summary>
        /// Возвращает информацию о выходе из транспорта для чанка.
        /// </summary>
        public ExitInfo GetExitInfo(int chunkIndex) => exitMap[chunkIndex];

        // Парсит индекс чанка из имени контейнера формата "Chunk[8]"
        private int ParseChunkIndex(string chunkName)
        {
            int start = chunkName.IndexOf('[');
            int end = chunkName.IndexOf(']');
            if (start >= 0 && end > start)
            {
                string num = chunkName.Substring(start + 1, end - start - 1);
                if (int.TryParse(num, out int idx))
                    return idx;
            }
            return -1;
        }
    }
} 