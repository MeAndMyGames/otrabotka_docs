namespace Otrabotka.Configs
{
    using UnityEngine;
    using System.Collections.Generic;
    using Otrabotka.Level.Configs;

    [CreateAssetMenu(menuName = "Otrabotka/Configs/DayCycleSettings", fileName = "DayCycleSettings")]
    public class DayCycleSettings : ScriptableObject
    {
        [Tooltip("Кривая интенсивности света за сутки")]
        public AnimationCurve intensityOverDay = AnimationCurve.Linear(0, 0, 1, 1);

        [Tooltip("Час старта дня (0–24)")]
        public float startHour = 6f;

        [Header("Настройки генератора чанков")]
        [Tooltip("Список стартовых чанков для генерации уровня в этот день")]
        public List<ChunkConfig> startChunks;
    }
}
