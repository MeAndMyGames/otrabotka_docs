using UnityEngine;
using System.Collections.Generic;
using Otrabotka.Core;        // ServiceLocator, ManagerBase
using Otrabotka.Configs;     // DayCycleSettings
using Otrabotka.Interfaces;  // ITimeShifter
using Otrabotka.Systems;     // ChunkManager, ChunkInstance
using Otrabotka.Level;       // ChunkTemplateBuffer

namespace Otrabotka.Level
{
    [DisallowMultipleComponent]
    public class ChunkSpawner : ManagerBase
    {
        [Header("Источник данных")]
        [SerializeField] private DayCycleSettings daySettings;
        [SerializeField] private ChunkTemplateBuffer templateBuffer;

        [Header("Контейнер чанков")]
        [SerializeField] private Transform chunksParent;

        [Header("Псевдо-движение")]
        [SerializeField] private Transform referencePoint; // например, Test_ReferencePoint.CharacterTarget
        [SerializeField] private float scrollSpeed = 5f;

        [Header("Позиционирование чанков")]
        [Tooltip("Сдвиг от точки ReferencePoint или от предыдущего чанка")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(10f, 0f, 0f);
        [Tooltip("Поворот чанка при инстанциировании")]
        [SerializeField] private Vector3 spawnRotationEuler = Vector3.zero;

        [Header("Буфер чанков")]
        [SerializeField] private int bufferAhead = 5;
        [SerializeField] private int bufferBehind = 2;

        private ITimeShifter _timeShifter;
        private ChunkManager _chunkManager;
        private List<ChunkInstance> _active = new List<ChunkInstance>();
        private int _templateIndex;

        public override void Initialize()
        {
            // 1) Получаем сервисы (теперь, после правки в ChunkManager и MissionTimer, они есть)
            _timeShifter = ServiceLocator.Get<ITimeShifter>();
            _chunkManager = ServiceLocator.Get<ChunkManager>();

            // 2) Генерируем шаблон один раз за «день»
            templateBuffer.GenerateTemplate(daySettings);

            // 3) Спавним первую волну чанков: [0] и далее bufferAhead штук
            _templateIndex = 0;
            SpawnAt(_templateIndex);
            for (int i = 1; i <= bufferAhead; i++)
                SpawnNext();
        }

        public override void Tick(float deltaTime)
        {
            // 1) Крутилка «мира» (освещение, таймеры и т.п.)
            _timeShifter.ShiftTime(deltaTime);

            // 2) Сдвигаем все активные чанки «назад» относительно камеры
            float shift = scrollSpeed * deltaTime;
            foreach (var inst in _active)
                inst.transform.Translate(-shift, 0, 0, Space.World);

            if (_active.Count == 0)
                return;

            // 3) Спавним новый чанк, когда последний подошёл слишком близко
            var last = _active[_active.Count - 1];
            if (last.transform.position.x < referencePoint.position.x + spawnOffset.x)
                SpawnNext();

            // 4) Удаляем самый старый чанк, когда он ушёл далеко за камерой
            //    (можно держать bufferBehind штук позади, если bufferBehind>0)
            while (_active.Count > bufferAhead + bufferBehind + 1)
                DespawnFirst();

            // Дополнительно: если первый чанк прошёл за порог X - можно тоже сразу удалять
            var first = _active[0];
            if (first.transform.position.x < referencePoint.position.x - spawnOffset.x)
                DespawnFirst();
        }

        public override void Shutdown()
        {
            // Чистим всё
            foreach (var inst in _active)
                Destroy(inst.gameObject);
            _active.Clear();
        }

        private void SpawnNext()
        {
            if (_templateIndex >= templateBuffer.Template.Length - 1)
                return;
            _templateIndex++;
            SpawnAt(_templateIndex);
        }

        private void SpawnAt(int templateIndex)
        {
            var cfg = templateBuffer.Template[templateIndex];
            if (cfg == null) return;

            // 1) Инстанцируем префаб чанка
            var go = Instantiate(
                cfg.primaryPrefab,
                Vector3.zero,
                Quaternion.Euler(spawnRotationEuler),
                chunksParent
            );

            // 2) Позиционируем:
            if (_active.Count == 0)
            {
                // первый чанк от ReferencePoint
                go.transform.position = referencePoint.position + spawnOffset;
            }
            else
            {
                // далее — относительно последнего
                var prev = _active[_active.Count - 1];
                go.transform.position = prev.transform.position + spawnOffset;
            }

            // 3) Регистрируем и добавляем в наш список
            var inst = go.GetComponent<ChunkInstance>();
            if (inst != null)
            {
                _chunkManager.RegisterChunkInstance(inst);
                _active.Add(inst);
            }
            else
            {
                Debug.LogError($"ChunkSpawner: у префаба {go.name} нет компонента ChunkInstance!");
                Destroy(go);
            }
        }

        private void DespawnFirst()
        {
            if (_active.Count == 0) return;
            var inst = _active[0];
            _active.RemoveAt(0);
            Destroy(inst.gameObject);
        }
    }
}
