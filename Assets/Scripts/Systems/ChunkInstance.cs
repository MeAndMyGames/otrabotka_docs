using UnityEngine;
using Otrabotka.Level.Configs; // для ChunkConfig

namespace Otrabotka.Systems
{
    public class ChunkInstance : MonoBehaviour
    {
        public int EventId { get; private set; }
        public ChunkConfig Config { get; private set; }
        public GameObject PrimaryPrefab { get; private set; }
        public GameObject SecondaryPrefab { get; private set; }

        private GameObject _currentVisual;

        /// <summary>
        /// Инициализирует чанк: запоминает префабы и создаёт визуал.
        /// </summary>
        public void Init(ChunkConfig cfg, int eventId, GameObject secondaryPrefab)
        {
            Config = cfg;
            EventId = eventId;
            PrimaryPrefab = cfg.primaryPrefab;
            SecondaryPrefab = secondaryPrefab;

            // убираем старый визуал, если есть
            if (_currentVisual != null)
                Destroy(_currentVisual);

            // удалено: автоматическое создание PrimaryPrefab, визуал создается в ChunkSpawner
        }

        /// <summary>
        /// Меняет визуал на вторичный
        /// </summary>
        public void ApplySecondaryPrefab(GameObject secondaryPrefab)
        {
            if (_currentVisual != null)
                Destroy(_currentVisual);

            if (secondaryPrefab != null)
                _currentVisual = Instantiate(secondaryPrefab, transform);
        }
    }
}
