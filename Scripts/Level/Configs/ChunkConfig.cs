using UnityEngine;

namespace Otrabotka.Level.Configs
{
    [CreateAssetMenu(menuName = "Otrabotka/Level/ChunkConfig", fileName = "ChunkConfig")]
    public class ChunkConfig : ScriptableObject
    {
        [Header("������� �����")]
        public GameObject primaryPrefab;
        public GameObject secondaryPrefab;

        [Header("����� ������������")]
        [Tooltip("��� ��������� ������� � ������� ��� ����� �����")]
        public string entryPointName = "entryPoint";
        [Tooltip("��� ��������� ������� � ������� ��� ����� ������")]
        public string exitPointName = "exitPoint";

        [Header("�������� � ������� ��� ������")]
        public Vector3 spawnOffset = Vector3.zero;
        public Vector3 spawnRotationEuler = Vector3.zero;

        [Header("��������� �����")]
        public ChunkConfig[] allowedNext;
        [Range(0f, 1f)] public float weight = 1f;
        public bool isCritical = false;
    }
}
