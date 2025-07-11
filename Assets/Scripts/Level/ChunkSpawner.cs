using UnityEngine;
using System.Collections.Generic;
using Otrabotka.Core;             // ManagerBase, ServiceLocator
using Otrabotka.Configs;          // DayCycleSettings
using Otrabotka.Interfaces;       // ITimeShifter
using Otrabotka.Level.Configs;    // ChunkConfig
using Otrabotka.Systems;          // ChunkManager, ChunkInstance
using Otrabotka.Interactions;

namespace Otrabotka.Level
{
    [DisallowMultipleComponent]
    public class ChunkSpawner : ManagerBase
    {
        [Header("Данные чанков")]
        [SerializeField] private DayCycleSettings daySettings;
        [SerializeField] private ScenarioSettings scenarioSettings;
        [SerializeField] private ChunkTemplateBuffer templateBuffer;

        [Header("Контейнер для инстансов")]
        [SerializeField] private Transform chunksParent;

        [Header("Псевдо-движение мира")]
        [SerializeField] private Transform referencePoint;
        [Header("Настройки скролла чанков (вправо)")]
        [SerializeField] private float scrollSpeed = 5f;
        [Tooltip("Скорость движения чанков при движении влево")]
        [SerializeField] private float scrollSpeedNegative = 5f;
        [Header("Инерция движения чанков")]
        [Tooltip("Включить плавное изменение скорости движения чанков при смене направления или остановке")]
        [SerializeField] private bool scrollInertiaEnabled = false;
        [Tooltip("Время (сек) сглаживания скорости движения чанков")]
        [SerializeField] private float scrollInertiaTime = 0.5f;
        [SerializeField] private bool enableMovement = true;
        [Header("Настройка движения")]
        [SerializeField] private bool autoScroll = true;

        [Header("Настройки спавна")]
        [SerializeField] private Vector3 spawnOffset = Vector3.zero;
        [SerializeField] private Vector3 spawnRotationEuler = Vector3.zero;

        [Header("Расстояния до порогов")]
        [Tooltip("Когда последний чанк отъехал от референс-точки дальше этого — спавним новый")]
        [SerializeField] private float spawnDistance;

        [Tooltip("Когда первый чанк уехал за кораблем дальше этого — деспавним")]
        [SerializeField] private float despawnDistance;

        [Header("Размер буфера чанков")]
        [SerializeField] private int bufferAhead = 5;
        [SerializeField] private int bufferBehind = 2;

        private ITimeShifter _timeShifter;
        private ChunkManager _chunkManager;
        private LinkedList<ChunkInstance> _active = new LinkedList<ChunkInstance>();
        private int _currentIndex;
        private float _currentScrollVelocity = 0f;

        /// <summary>
        /// Текущая скорость прокрутки чанков вправо.
        /// </summary>
        public float ScrollSpeedPositive => scrollSpeed;
        /// <summary>
        /// Текущая скорость прокрутки чанков влево.
        /// </summary>
        public float ScrollSpeedNegative => scrollSpeedNegative;
        public bool ScrollInertiaEnabled => scrollInertiaEnabled;
        public float ScrollInertiaTime => scrollInertiaTime;

        public override void Initialize()
        {
            Debug.Log($"[Init] active list is {(_active == null ? "NULL" : "OK")} (count={_active.Count})");
            // 1) Занимаем зависимости
            _timeShifter = ServiceLocator.Get<ITimeShifter>();
            _chunkManager = ServiceLocator.Get<ChunkManager>();
            Debug.Log($"[Init] _chunkManager is {(_chunkManager == null ? "NULL" : "OK")}");

            // 2) Генерируем линейный шаблон
            // Проверяем, что scenarioSettings назначены
            if (scenarioSettings == null)
            {
                scenarioSettings = FindObjectOfType<ScenarioSettings>();
                if (scenarioSettings == null)
                {
                    Debug.LogError("ChunkSpawner: ScenarioSettings не назначены и не найдены на сцене.");
                    return;
                }
            }
            templateBuffer.GenerateTemplate(scenarioSettings, 0f);

            // 3) Спавним окно [0…bufferAhead]
            _currentIndex = 0;
            int max = Mathf.Min(bufferAhead, templateBuffer.Template.Count - 1);
            for (int i = 0; i <= max; i++)
                SpawnAt(i);
        }

        public override void Tick(float deltaTime)
        {
            // Блокируем движение во время диалога
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
                return;

            float direction = 0f;
            if (enableMovement)
            {
                float input = Input.GetAxis("Horizontal");
                direction = Mathf.Abs(input) > 0f ? input : (autoScroll ? 1f : 0f);
            }

            // Рассчитываем желаемую скорость (юниты/сек)
            float desiredVelocity = 0f;
            if (direction > 0f)
                desiredVelocity = scrollSpeed * direction;
            else if (direction < 0f)
                desiredVelocity = scrollSpeedNegative * direction;

            // Применяем инерцию, если включена
            if (scrollInertiaEnabled && scrollInertiaTime > 0f)
                _currentScrollVelocity = Mathf.Lerp(_currentScrollVelocity, desiredVelocity, deltaTime / scrollInertiaTime);
            else
                _currentScrollVelocity = desiredVelocity;

            // Смещение чанков
            float shift = _currentScrollVelocity * deltaTime;

            Debug.Log($"[ChunkSpawner] Tick: direction={direction}, shift={shift:F2}, currentIndex={_currentIndex}, activeCount={_active.Count}");
            var activeChunks = new List<ChunkInstance>(_active);
            foreach (var inst in activeChunks)
            {
                if (inst != null && inst.transform != null)
                {
                    inst.transform.Translate(-shift, 0, 0, Space.World);
                }
            }

            if (_active.Count == 0) return;

            if (direction > 0f)
            {
                Debug.Log("[ChunkSpawner] Tick: movement forward");
                var last = _active.Last.Value;
                Transform lastExit = FindDeepChild(last.transform, last.Config.exitPointName);
                float spawnThresh = referencePoint.position.x + spawnDistance;
                Debug.Log($"[ChunkSpawner] Forward check: lastExitX={(lastExit!=null?lastExit.position.x.ToString("F2"):"NULL")}, spawnThresh={spawnThresh:F2}");
                if ((lastExit != null && lastExit.position.x < spawnThresh) ||
                    (lastExit == null && last.transform.position.x < spawnThresh))
                {
                    Debug.Log("[ChunkSpawner] Forward threshold passed, calling Advance(1)");
                    Advance(1);
                }
            }
            else if (direction < 0f)
            {
                Debug.Log("[ChunkSpawner] Tick: movement backward");
                var first = _active.First.Value;
                Transform firstEntry = FindDeepChild(first.transform, first.Config.entryPointName);
                float backSpawnThresh = referencePoint.position.x - spawnDistance;
                Debug.Log($"[ChunkSpawner] Backward check: firstEntryX={(firstEntry!=null?firstEntry.position.x.ToString("F2"):"NULL")}, backSpawnThresh={backSpawnThresh:F2}");
                if ((firstEntry != null && firstEntry.position.x > backSpawnThresh) ||
                    (firstEntry == null && first.transform.position.x > backSpawnThresh))
                {
                    Debug.Log("[ChunkSpawner] Backward threshold passed, calling Advance(-1)");
                    Advance(-1);
                }
            }
        }

        public override void Shutdown()
        {
            // Убираем всё по завершении
            var chunksToDestroy = new List<ChunkInstance>(_active);
            foreach (var inst in chunksToDestroy)
            {
                if (inst != null && inst.gameObject != null)
                {
                    Destroy(inst.gameObject);
                }
            }
            _active.Clear();
        }

        // Двигаем окно вперед или назад на 1 индекс
        private void Advance(int dir)
        {
            Debug.Log($"[ChunkSpawner] Advance: dir={dir}, currentIndex(before)={_currentIndex}, bufferAhead={bufferAhead}, bufferBehind={bufferBehind}, templateCount={templateBuffer.Template.Count}");
            if (dir > 0)
            {
                int spawnIdx = _currentIndex + bufferAhead + 1;
                Debug.Log($"[ChunkSpawner] Advance forward: calculated spawnIdx={spawnIdx}");
                if (spawnIdx < templateBuffer.Template.Count)
                {
                    SpawnAt(spawnIdx);
                    DespawnOldest();
                    _currentIndex++;
                    Debug.Log($"[ChunkSpawner] Advance forward complete: new currentIndex={_currentIndex}");
                }
            }
            else if (dir < 0)
            {
                // симметричный спавн назад: берём предыдущий индекс
                int spawnIdx = _currentIndex - 1;
                Debug.Log($"[ChunkSpawner] Advance backward symmetric: calculated spawnIdx={spawnIdx}");
                if (spawnIdx >= 0)
                {
                    SpawnBefore(spawnIdx);
                    DespawnNewest();
                    _currentIndex--;
                    Debug.Log($"[ChunkSpawner] Advance backward complete: new currentIndex={_currentIndex}");
                }
            }
        }

        // Дестрой самого первого элемента
        private void DespawnOldest()
        {
            if (_active.Count == 0) return;
            var node = _active.First;
            if (node.Value != null)
                Destroy(node.Value.gameObject);
            _active.RemoveFirst();
        }

        // Дестрой самого последнего элемента
        private void DespawnNewest()
        {
            if (_active.Count == 0) return;
            var node = _active.Last;
            if (node.Value != null)
                Destroy(node.Value.gameObject);
            _active.RemoveLast();
        }

        // Рекурсивный поиск Transform по имени
        private Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
                var result = FindDeepChild(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        // Инстанцирует чанк по индексу шаблона
        private void SpawnAt(int index)
        {
            // 0) Получаем конфиг
            var cfg = templateBuffer.Template[index];

            // Защита от null
            if (cfg == null || cfg.primaryPrefab == null)
                return;

            // 1) Логи состояния — используем index и cfg
            Debug.Log($"[SpawnAt] index={index}, cfg={(cfg == null ? "NULL" : cfg.name)}, primaryPrefab={(cfg.primaryPrefab == null ? "NULL" : "OK")}");
            Debug.Log($" referencePoint={(referencePoint == null ? "NULL" : referencePoint.name)}, chunksParent={(chunksParent == null ? "NULL" : chunksParent.name)}");

            // 2) Создаём контейнер чанка
            var container = new GameObject($"Chunk[{index}]");
            container.transform.SetParent(chunksParent, false);
            if (_active.Count == 0)
            {
                // первый чанк — от позиции контейнера
                container.transform.position = chunksParent.position + cfg.spawnOffset;
            }
            else
            {
                // остальные чанки — от referencePoint
                container.transform.position = referencePoint.position + cfg.spawnOffset;
            }
            // контейнер остаётся без дополнительного поворота
            container.transform.rotation = chunksParent.rotation;

            // 3) Инстанцируем prefab как дочерний объект контейнера с локальной позицией и нужным поворотом из конфига
            var prefabGo = Instantiate(cfg.primaryPrefab, Vector3.zero, Quaternion.Euler(cfg.spawnRotationEuler), container.transform);
            prefabGo.name = cfg.primaryPrefab.name;

            // 4) Добавляем компонент и инициализируем
            var inst = container.AddComponent<ChunkInstance>();
            inst.Init(cfg, index, cfg.secondaryPrefab);

            // --- Стыковка по entry/exit точкам ---
            if (_active.Count > 0)
            {
                var prevChunk = _active.Last.Value;
                if (prevChunk != null)
                {
                    var prevExit = FindDeepChild(prevChunk.transform, "exitPoint");
                    var newEntry = FindDeepChild(prefabGo.transform, "entryPoint");
                    if (prevExit != null && newEntry != null)
                    {
                        Vector3 offset = prevExit.position - newEntry.position;
                        container.transform.position += offset;
                    }
                    else
                    {
                        Debug.LogWarning($"[ChunkSpawner] Не найдены entryPoint или exitPoint для стыковки чанков! index={index}");
                    }
                }
            }
            // --- конец стыковки ---

            // 5) Регистрируем
            _chunkManager.RegisterChunkInstance(inst);
            _active.AddLast(inst);
            // Восстанавливаем оставленные машины, если они были покинуты в этом чанке
            var transport = Otrabotka.Systems.TransportController.Instance;
            if (transport != null && transport.HasExitForChunk(index))
            {
                var exitInfo = transport.GetExitInfo(index);
                // spawn exit event prefab (used for both entry and exit visuals)
                var exitPrefab = exitInfo.exitDefinition.exitEventPrefab;
                if (exitPrefab != null)
                {
                    var exitGO = Instantiate(exitPrefab, Vector3.zero, Quaternion.identity, container.transform);
                    exitGO.name = exitPrefab.name;
                    exitGO.transform.localPosition = exitInfo.localPosition;
                    exitGO.SetActive(true);
                    foreach (var tr in exitGO.GetComponentsInChildren<Transform>(true))
                        tr.gameObject.SetActive(true);
                    // override the interactable definition to entryDefinition for re-entry
                    var interactable = exitGO.GetComponentInChildren<InteractableComponent>(true);
                    if (interactable != null)
                        interactable.SetDefinition(exitInfo.entryDefinition);
                }
            }
        }

        // Симметричный спавн чанка позади существующих
        private void SpawnBefore(int index)
        {
            var cfg = templateBuffer.Template[index];
            if (cfg == null || cfg.primaryPrefab == null)
                return;
            Debug.Log($"[ChunkSpawner] SpawnBefore: index={index}, cfg={(cfg == null ? "NULL" : cfg.name)}");
            // 1) Создаём контейнер чанка
            var container = new GameObject($"Chunk[{index}]");
            container.transform.SetParent(chunksParent, false);
            container.transform.position = referencePoint.position + cfg.spawnOffset;
            container.transform.rotation = chunksParent.rotation;
            // 2) Инстанцируем визуал
            var prefabGo = Instantiate(cfg.primaryPrefab, Vector3.zero, Quaternion.Euler(cfg.spawnRotationEuler), container.transform);
            prefabGo.name = cfg.primaryPrefab.name;
            // 3) Добавляем компонент и инициализируем
            var inst = container.AddComponent<ChunkInstance>();
            inst.Init(cfg, index, cfg.secondaryPrefab);
            // 4) Стыковка назад: соединяем exit новой с entry первого
            if (_active.Count > 0)
            {
                var firstChunk = _active.First.Value;
                var firstEntry = FindDeepChild(firstChunk.transform, firstChunk.Config.entryPointName);
                var newExit = FindDeepChild(prefabGo.transform, cfg.exitPointName);
                if (firstEntry != null && newExit != null)
                {
                    Vector3 offset = firstEntry.position - newExit.position;
                    container.transform.position += offset;
                }
                else
                {
                    Debug.LogWarning($"[ChunkSpawner] SpawnBefore: точки entry/exit не найдены для стыковки! index={index}");
                }
            }
            // 5) Регистрируем и добавляем в начало списка
            _chunkManager.RegisterChunkInstance(inst);
            _active.AddFirst(inst);
            // Восстанавливаем оставленные машины при обратном спавне
            var transport = Otrabotka.Systems.TransportController.Instance;
            if (transport != null && transport.HasExitForChunk(index))
            {
                var exitInfo = transport.GetExitInfo(index);
                // spawn exit event prefab for re-entry
                var exitPrefab = exitInfo.exitDefinition.exitEventPrefab;
                if (exitPrefab != null)
                {
                    var exitGO = Instantiate(exitPrefab, Vector3.zero, Quaternion.identity, container.transform);
                    exitGO.name = exitPrefab.name;
                    exitGO.transform.localPosition = exitInfo.localPosition;
                    exitGO.SetActive(true);
                    foreach (var tr in exitGO.GetComponentsInChildren<Transform>(true))
                        tr.gameObject.SetActive(true);
                    // override definition to entryDefinition for interaction to re-enter
                    var interactable = exitGO.GetComponentInChildren<InteractableComponent>(true);
                    if (interactable != null)
                        interactable.SetDefinition(exitInfo.entryDefinition);
                }
            }
        }

        // Удаляет из сцены и из списка чанк, чей Config == template[idx]
        private void DespawnAt(int idx)
        {
            var node = _active.First;
            while (node != null)
            {
                var next = node.Next;
                if (node.Value != null && node.Value.Config == templateBuffer.Template[idx])
                {
                    if (node.Value.gameObject != null)
                    {
                        Destroy(node.Value.gameObject);
                    }
                    _active.Remove(node);
                    break;
                }
                node = next;
            }
        }

        /// <summary>
        /// Установить новые скорости скролла чанков.
        /// </summary>
        public void SetScrollSpeeds(float speedPositive, float speedNegative)
        {
            scrollSpeed = speedPositive;
            scrollSpeedNegative = speedNegative;
        }

        /// <summary>
        /// Установить параметры инерции прокрутки чанков.
        /// </summary>
        public void SetInertiaSettings(bool enabled, float time)
        {
            scrollInertiaEnabled = enabled;
            scrollInertiaTime = time;
        }
    }
}
