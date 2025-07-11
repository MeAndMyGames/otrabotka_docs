using System.Collections.Generic;
using UnityEngine;
using Otrabotka.Level.Configs;

namespace Otrabotka.Scenario.Configs
{
    [CreateAssetMenu(menuName = "Otrabotka/Configs/FinalBiomeConfig", fileName = "FinalBiomeConfig")]
    public class FinalBiomeConfig : ScriptableObject
    {
        [Tooltip("Последовательность чанков для финального биома")]
        public List<ChunkConfig> Sequence;

        [Tooltip("Минимальное время прохождения дня (в часах)")]
        public float MinTimeTaken;

        [Tooltip("Максимальное время прохождения дня (в часах)")]
        public float MaxTimeTaken;
    }
} 