using UnityEngine;

namespace Otrabotka.Level.Configs
{
    [CreateAssetMenu(menuName = "Otrabotka/Level/ChunkConfig", fileName = "ChunkConfig")]
    public class ChunkConfig : ScriptableObject
    {
        [Header("Префабы чанка")]
        public GameObject primaryPrefab;
        public GameObject secondaryPrefab;

        [Header("Точка выравнивания")]
        [Tooltip("Имя дочернего объекта в префабе для точки входа")]
        public string entryPointName = "entryPoint";
        [Tooltip("Имя дочернего объекта в префабе для точки выхода")]
        public string exitPointName = "exitPoint";

        [Header("Смещение и поворот при спавне")]
        public Vector3 spawnOffset = Vector3.zero;
        public Vector3 spawnRotationEuler = Vector3.zero;

        [Header("Настройки графа")]
        public ChunkConfig[] allowedNext;
        [Range(0f, 1f)] public float weight = 1f;
        public bool isCritical = false;
    }
}
