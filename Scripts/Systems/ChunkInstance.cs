using UnityEngine;

namespace Otrabotka.Systems
{
    public class ChunkInstance : MonoBehaviour
    {
        [Tooltip("ID ���������� �������")]
        public int EventId;

        [Tooltip("������, ������������ �� �������")]
        public GameObject PrimaryPrefab;

        [Tooltip("������, ���������� PrimaryPrefab ����� �������")]
        public GameObject SecondaryPrefab;

        // ���� � ���� �� � ��� '�����'-�����
        public bool HasSecondaryState => SecondaryPrefab != null;

        // ������ ������� ���������� ������ �����
        private GameObject _currentVisual;

        private void Awake()
        {
            // ��� ������ �������� currentVisual ��������� ������
            if (PrimaryPrefab != null)
            {
                _currentVisual = Instantiate(PrimaryPrefab, transform);
            }
        }

        /// <summary>
        /// ������� ������� ���������� ������������� � ������ SecondaryPrefab.
        /// </summary>
        public void ApplySecondaryPrefab()
        {
            if (_currentVisual != null)
                Destroy(_currentVisual);

            _currentVisual = Instantiate(SecondaryPrefab, transform);
        }
    }
}
