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

        [Header("Сценарий дня")]
        public List<ChunkConfig> startChunks;

        [Header("Пороги для спавна/деспавна чанков")]
        public float spawnDistance = 10f;
        public float despawnDistance = 15f;
    }
}
    