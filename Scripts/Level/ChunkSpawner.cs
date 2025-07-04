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
        [Header("�������� �������� � ������")]
        [SerializeField] private DayCycleSettings daySettings;
        [SerializeField] private ChunkTemplateBuffer templateBuffer;

        [Header("������-��������")]
        [SerializeField] private Transform referencePoint; // ������ Camera.main.transform
        [SerializeField] private float scrollSpeed = 5f;

        [Header("����� ������")]
        [SerializeField] private int bufferAhead = 5;
        [SerializeField] private int bufferBehind = 2;

        private ITimeShifter _timeShifter;
        private ChunkManager _chunkManager;
        private LinkedList<ChunkInstance> _active = new LinkedList<ChunkInstance>();

        public override void Initialize()
        {
            // 1) �������� �������
            _timeShifter = ServiceLocator.Get<ITimeShifter>();
            _chunkManager = ServiceLocator.Get<ChunkManager>();

            // 2) ���������� �������� ������ ������
            templateBuffer.GenerateTemplate(daySettings);

            // 3) ������� ��������� ���� ������ [0�bufferAhead]
            for (int i = 0; i <= bufferAhead; i++)
                SpawnAt(i);
        }

        public override void Tick(float deltaTime)
        {
            // 1) �������� ������� (���� � ��� ��������� ������ ������ ��������� � ����)
            _timeShifter.ShiftTime(deltaTime);

            // 2) �������� ��� ����� ������ ������������ ������
            float shift = scrollSpeed * deltaTime;
            foreach (var inst in _active)
                inst.transform.Translate(-shift, 0, 0, Space.World);

            if (_active.Count == 0)
                return;

            // 3) ����� ������ �����, ����� ��������� ������������ � ������ �����, ��� �� ����� ������ �����
            var last = _active.Last.Value;
            float spawnThreshold = referencePoint.position.x + daySettings.startHour; //������: ����� ������� ��������� ���������� spawnDistance
            if (last.transform.position.x < spawnThreshold)
                SpawnNext();

            // 4) ������� ������ ������� �����, ����� �� ���� ������ �� ������
            var first = _active.First.Value;
            float despawnThreshold = referencePoint.position.x - daySettings.startHour; //������: despawnDistance
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

            // 1) ������������ ������
            var go = Instantiate(cfg.primaryPrefab);
            var inst = go.GetComponent<ChunkInstance>();

            // 2) ������������ ��� � ChunkManager
            _chunkManager.RegisterChunkInstance(inst);

            // 3) �������������: ���� �� ������, ���� ����� �� ��������� ������
            if (_active.Count == 0)
            {
                go.transform.position = referencePoint.position;
            }
            else
            {
                var prev = _active.Last.Value;
                // ��� �������� �������� �� ������ ������-�� ���������;
                // ����� ��������� � ������� entry/exit-�����.
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
