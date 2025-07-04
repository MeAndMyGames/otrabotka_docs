using UnityEngine;
using System.Collections.Generic;

// Core
using Otrabotka.Core;       // ManagerBase, ServiceLocator
using Otrabotka.Configs;    // DayCycleSettings
using Otrabotka.Interfaces; // ITimeShifter
using Otrabotka.Systems;    // ChunkManager, ChunkInstance
using Otrabotka.Level;      // ChunkTemplateBuffer

namespace Otrabotka.Level
{
    [DisallowMultipleComponent]
    public class ChunkSpawner : ManagerBase
    {
        [Header("Источник настроек и шаблон")]
        [SerializeField] private DayCycleSettings daySettings;
        [SerializeField] private ChunkTemplateBuffer templateBuffer;

        [Header("Псевдо-движение")]
        [SerializeField] private Transform referencePoint; // обычно Camera.main.transform
        [SerializeField] private float scrollSpeed = 5f;

        [Header("Буфер чанков")]
        [SerializeField] private int bufferAhead = 5;
        [SerializeField] private int bufferBehind = 2;

        private ITimeShifter _timeShifter;
        private ChunkManager _chunkManager;
        private LinkedList<ChunkInstance> _active = new LinkedList<ChunkInstance>();

        public override void Initialize()
        {
            // 1) Получаем сервисы
            _timeShifter = ServiceLocator.Get<ITimeShifter>();
            _chunkManager = ServiceLocator.Get<ChunkManager>();

            // 2) Генерируем линейный шаблон чанков
            templateBuffer.GenerateTemplate(daySettings);

            // 3) Спавним начальное окно чанков [0…bufferAhead]
            for (int i = 0; i <= bufferAhead; i++)
                SpawnAt(i);
        }

        public override void Tick(float deltaTime)
        {
            // 1) Сдвигаем «время» (если у вас временной шифтер крутит освещение и дату)
            _timeShifter.ShiftTime(deltaTime);

            // 2) Сдвигаем все чанки «назад» относительно камеры
            float shift = scrollSpeed * deltaTime;
            foreach (var inst in _active)
                inst.transform.Translate(-shift, 0, 0, Space.World);

            if (_active.Count == 0)
                return;

            // 3) Спавн нового чанка, когда последний «подкатился» к камере ближе, чем на длину одного чанка
            var last = _active.Last.Value;
            float spawnThreshold = referencePoint.position.x + daySettings.startHour; //пример: можно завести отдельную переменную spawnDistance
            if (last.transform.position.x < spawnThreshold)
                SpawnNext();

            // 4) Деспавн самого старого чанка, когда он ушёл далеко за камеру
            var first = _active.First.Value;
            float despawnThreshold = referencePoint.position.x - daySettings.startHour; //пример: despawnDistance
            if (first.transform.position.x < despawnThreshold)
                DespawnFirst();
        }

        public override void Shutdown()
        {
            foreach (var inst in _active)
                Destroy(inst.gameObject);
            _active.Clear();
        }

        private void SpawnNext()
        {
            int nextIndex = _active.Count == 0
                ? 0
                : Mathf.Min(templateBuffer.Template.Length - 1,
                            _active.Count);
            SpawnAt(nextIndex);
        }

        private void SpawnAt(int templateIndex)
        {
            var cfg = templateBuffer.Template[templateIndex];
            if (cfg == null) return;

            // 1) Инстанцируем префаб
            var go = Instantiate(cfg.primaryPrefab);
            var inst = go.GetComponent<ChunkInstance>();

            // 2) Регистрируем его у ChunkManager
            _chunkManager.RegisterChunkInstance(inst);

            // 3) Позиционируем: либо на камере, либо сразу за последним чанком
            if (_active.Count == 0)
            {
                go.transform.position = referencePoint.position;
            }
            else
            {
                var prev = _active.Last.Value;
                // Для простоты сдвигаем на ширину какого-то параметра;
                // лучше привязать в будущем entry/exit-точки.
                float offset = prev.GetComponent<Renderer>().bounds.size.x;
                go.transform.position = prev.transform.position + Vector3.right * offset;
            }

            _active.AddLast(inst);
        }

        private void DespawnFirst()
        {
            var inst = _active.First.Value;
            _active.RemoveFirst();
            Destroy(inst.gameObject);
        }
    }
}
