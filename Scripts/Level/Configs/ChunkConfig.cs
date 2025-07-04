using System.Collections.Generic;
using UnityEngine;

namespace Otrabotka.Level.Configs
{
    [CreateAssetMenu(menuName = "Otrabotka/Level/ChunkConfig", fileName = "ChunkConfig")]
    public class ChunkConfig : ScriptableObject
    {
        [Header("Prefabs")]
        public GameObject primaryPrefab;
        public GameObject secondaryPrefab;

        [Header("Graph")]
        public List<ChunkConfig> allowedNext;
        [Range(0f, 1f)] public float weight = 1f;
        public bool isCritical = false;

        [Header("Alignment Points (named in prefab)")]
        public string entryPointName = "entryPoint";
        public string exitPointName = "exitPoint";

        [Header("Spawn Settings")]
        [Tooltip("ƒополнительное смещение от точки выхода предыдущего чанка (миры, м)")]
        public Vector3 spawnOffset = Vector3.zero;
        [Tooltip("ƒополнительный поворот (Euler, градусы) после выравнивани€")]
        public Vector3 spawnRotationEuler = Vector3.zero;
    }
}
