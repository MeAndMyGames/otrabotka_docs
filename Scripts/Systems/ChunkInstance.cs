// Assets/Scripts/Systems/ChunkInstance.cs
using UnityEngine;
using Otrabotka.Level.Configs;

namespace Otrabotka.Systems
{
    [DisallowMultipleComponent]
    public class ChunkInstance : MonoBehaviour
    {
        public int EventId { get; private set; }
        public int TemplateIndex { get; private set; }
        public ChunkConfig Config { get; private set; }
        public bool HasSecondaryState => Config.secondaryPrefab != null;

        private Transform _entryPoint;
        private Transform _exitPoint;
        private GameObject _currentVisual;

        public void Init(ChunkConfig config, int eventId, int templateIndex)
        {
            Config = config;
            EventId = eventId;
            TemplateIndex = templateIndex;

            // ищем точки внутри иерархии
            _entryPoint = transform.Find("entryPoint");
            _exitPoint = transform.Find("exitPoint");

            // создаём primary-визуал
            if (_currentVisual != null)
                Destroy(_currentVisual);

            if (Config.primaryPrefab != null)
                _currentVisual = Instantiate(Config.primaryPrefab, transform);
        }

        public Vector3 GetEntryWorldPosition() =>
            _entryPoint != null ? _entryPoint.position : transform.position;

        public Vector3 GetExitWorldPosition() =>
            _exitPoint != null ? _exitPoint.position : transform.position;

        public Quaternion GetExitWorldRotation() =>
            _exitPoint != null ? _exitPoint.rotation : transform.rotation;

        public void ApplySecondaryPrefab()
        {
            if (!HasSecondaryState) return;

            if (_currentVisual != null)
                Destroy(_currentVisual);

            _currentVisual = Instantiate(Config.secondaryPrefab, transform);
        }
    }
}
