using System.Collections.Generic;
using UnityEngine;
using Otrabotka.Level.Configs;

namespace Otrabotka.Scenario.Configs
{
    [CreateAssetMenu(menuName = "Otrabotka/Configs/ComplexEventConfig", fileName = "ComplexEventConfig")]
    public class ComplexEventConfig : ScriptableObject
    {
        [Tooltip("Последовательность чанков для сложного события")]
        public List<ChunkConfig> Sequence;

        [Tooltip("Вероятность срабатывания этого события (0-1)")]
        [Range(0f, 1f)]
        public float TriggerProbability = 1f;
    }
} 