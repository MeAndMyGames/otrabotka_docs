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
            // 1) Сдвигаем «мир» (освещение, часы и т.п.)
            _timeShifter.ShiftTime(deltaTime);

            // 2) Сдвигаем все чанки навстречу камере
            float shift = scrollSpeed * deltaTime;
            foreach (var inst in _active)
                inst.transform.Translate(-shift, 0, 0, Space.World);

            if (_active.Count == 0) return;

            // 3) Спавн нового чанка
            var last = _active.Last.Value;
            float spawnThresh = referencePoint.position.x + spawnDistance;
            if (last.transform.position.x < spawnThresh)
                Advance(1);

            // 4) Деспавн старого чанка
            var first = _active.First.Value;
            float despawnThresh = referencePoint.position.x - despawnDistance;
            if (first.transform.position.x < despawnThresh)
                Advance(-1);
        }

        public override void Shutdown()
        {
            // Убираем всё по завершении
            foreach (var inst in _active)
                Destroy(inst.gameObject);
            _active.Clear();
        }

        // Двигаем окно вперед или назад на 1 индекс
        private void Advance(int dir)
        {
            int old = _currentIndex;
            _currentIndex = Mathf.Clamp(_currentIndex + dir, 0, templateBuffer.Template.Count - 1);

            if (dir > 0)
            {
                // спавним впереди
                int spawnIdx = _currentIndex + bufferAhead;
                if (spawnIdx < templateBuffer.Template.Count)
                    SpawnAt(spawnIdx);

                // деспавним позади
                int despawnIdx = old - bufferBehind;
                if (despawnIdx >= 0)
                    DespawnAt(despawnIdx);
            }
            else
            {
                // спавним позади, если возвращаемся назад
                int spawnIdx = _currentIndex - bufferBehind;
                if (spawnIdx >= 0)
                    SpawnAt(spawnIdx);

                // деспавним впереди
                int despawnIdx = old + bufferAhead;
                if (despawnIdx < templateBuffer.Template.Count)
                    DespawnAt(despawnIdx);
            }
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

            // 2) Инстанцируем пустой контейнер чанка
            var go = new GameObject($"Chunk[{index}]");
            go.transform.SetParent(chunksParent, false);
            go.transform.position = referencePoint.position + spawnOffset;
            go.transform.rotation = Quaternion.Euler(spawnRotationEuler);

            // 3) Добавляем компонент и инициализируем
            var inst = go.AddComponent<ChunkInstance>();
            inst.Init(cfg, index, cfg.secondaryPrefab);

            // 4) Регистрируем
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
                if (node.Value.Config == templateBuffer.Template[idx])
                {
                    Destroy(node.Value.gameObject);
                    _active.Remove(node);
                    break;
                }
                node = next;
            }
        }
    }
}
