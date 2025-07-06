using UnityEngine;
using System.Collections.Generic;
using Otrabotka.Core;             // ManagerBase, ServiceLocator
using Otrabotka.Configs;          // DayCycleSettings
using Otrabotka.Interfaces;       // ITimeShifter
using Otrabotka.Level.Configs;    // ChunkConfig
using Otrabotka.Systems;          // ChunkManager, ChunkInstance

namespace Otrabotka.Level
{
    [DisallowMultipleComponent]
    public class ChunkSpawner : ManagerBase
    {
        [Header("Данные чанков")]
        [SerializeField] private DayCycleSettings daySettings;
        [SerializeField] private ChunkTemplateBuffer templateBuffer;

        [Header("Контейнер для инстансов")]
        [SerializeField] private Transform chunksParent;

        [Header("Псевдо-движение мира")]
        [SerializeField] private Transform referencePoint;
        [SerializeField] private float scrollSpeed = 5f;

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

        public override void Initialize()
        {
            Debug.Log($"[Init] active list is {(_active == null ? "NULL" : "OK")} (count={_active.Count})");
            // 1) Занимаем зависимости
            _timeShifter = ServiceLocator.Get<ITimeShifter>();
            _chunkManager = ServiceLocator.Get<ChunkManager>();
            Debug.Log($"[Init] _chunkManager is {(_chunkManager == null ? "NULL" : "OK")}");

            // 2) Генерируем линейный шаблон
            templateBuffer.GenerateTemplate(daySettings);

            // 3) Спавним окно [0…bufferAhead]
            _currentIndex = 0;
            int max = Mathf.Min(bufferAhead, templateBuffer.Template.Count - 1);
            for (int i = 0; i <= max; i++)
                SpawnAt(i);
        }

        public override void Tick(float deltaTime)
        {
            // 1) Сдвигаем все чанки навстречу камере
            float shift = scrollSpeed * deltaTime;
            var activeChunks = new List<ChunkInstance>(_active);
            foreach (var inst in activeChunks)
            {
                if (inst != null && inst.transform != null)
                {
                    inst.transform.Translate(-shift, 0, 0, Space.World);
                }
            }

            if (_active.Count == 0) return;

            // 2) Спавн нового чанка по ExitPoint
            var last = _active.Last.Value;
            Transform lastExit = FindDeepChild(last.transform, last.Config.exitPointName);
            float spawnThresh = referencePoint.position.x + spawnDistance;
            if ((lastExit != null && lastExit.position.x < spawnThresh) ||
                (lastExit == null && last.transform.position.x < spawnThresh))
            {
                Advance(1);
            }

            // 3) Деспавн старого чанка по EntryPoint
            var first = _active.First.Value;
            Transform firstEntry = FindDeepChild(first.transform, first.Config.entryPointName);
            float despawnThresh = referencePoint.position.x - despawnDistance;
            if ((firstEntry != null && firstEntry.position.x < despawnThresh) ||
                (firstEntry == null && first.transform.position.x < despawnThresh))
            {
                Advance(-1);
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
            if (dir > 0)
            {
                // спавним впереди
                int spawnIdx = _currentIndex + bufferAhead + 1;
                if (spawnIdx < templateBuffer.Template.Count)
                {
                    SpawnAt(spawnIdx);
                    DespawnOldest();
                    _currentIndex++;
                }
            }
            else if (dir < 0)
            {
                // спавним позади
                int spawnIdx = _currentIndex - bufferBehind - 1;
                if (spawnIdx >= 0)
                {
                    SpawnAt(spawnIdx);
                    DespawnNewest();
                    _currentIndex--;
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
    }
}
