// Assets/Scripts/Level/Configs/ChunkConfig.cs
using System.Collections.Generic;
using UnityEngine;

namespace Otrabotka.Level.Configs
{
    [CreateAssetMenu(menuName = "Otrabotka/Level/ChunkConfig", fileName = "ChunkConfig")]
    public class ChunkConfig : ScriptableObject
    {
        public string chunkID;
        public GameObject primaryPrefab;
        public GameObject secondaryPrefab;
        public List<ChunkConfig> allowedNext;
        [Range(0f, 1f)] public float weight = 1f;
        public bool isCritical = false;

        // Точки входа/выхода для выравнивания соседних чанков
        public Transform entryPoint;
        public Transform exitPoint;
    }
}
