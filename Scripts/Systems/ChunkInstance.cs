using UnityEngine;

namespace Otrabotka.Systems
{
    public class ChunkInstance : MonoBehaviour
    {
        [Tooltip("ID связанного события")]
        public int EventId;

        [Tooltip("Префаб, используемый до события")]
        public GameObject PrimaryPrefab;

        [Tooltip("Префаб, заменяющий PrimaryPrefab после провала")]
        public GameObject SecondaryPrefab;

        // Флаг — есть ли у нас 'после'-стейт
        public bool HasSecondaryState => SecondaryPrefab != null;

        // Хранит текущий визуальный объект чанка
        private GameObject _currentVisual;

        private void Awake()
        {
            // При старте присвоим currentVisual первичный префаб
            if (PrimaryPrefab != null)
            {
                _currentVisual = Instantiate(PrimaryPrefab, transform);
            }
        }

        /// <summary>
        /// Удаляет текущее визуальное представление и ставит SecondaryPrefab.
        /// </summary>
        public void ApplySecondaryPrefab()
        {
            if (_currentVisual != null)
                Destroy(_currentVisual);

            _currentVisual = Instantiate(SecondaryPrefab, transform);
        }
    }
}
