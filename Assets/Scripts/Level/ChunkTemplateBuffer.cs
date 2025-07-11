using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Otrabotka.Configs;
using Otrabotka.Level.Configs;
using Otrabotka.Scenario;
using Otrabotka.Core;  // импорт ScenarioSettings

namespace Otrabotka.Level
{
    /// <summary>
    /// Singleton buffer holding the linear chunk template for the current day session.
    /// Generates and stores a fixed sequence of ChunkConfig based on a seed.
    /// </summary>
    public class ChunkTemplateBuffer : MonoBehaviour
    {
        public static ChunkTemplateBuffer Instance { get; private set; }

        [Tooltip("Number of chunks to generate for the day")]
        [SerializeField] private int templateLength = 100;

        public int Seed { get; private set; }
        public List<ChunkConfig> Template { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Generates the daily chunk template based on a seed and available start chunks.
        /// </summary>
        public void GenerateTemplate(ScenarioSettings settings, float timeTaken)
        {
            // Строим последовательность чанков по сценарию
            Template = ChunkSequenceBuilder.BuildSequence(settings, timeTaken);
        }

        /// <summary>
        ///     . 
        ///     null  previousChunk.
        /// </summary>
        private ChunkConfig PickByWeight(List<ChunkConfig> list, System.Random rnd, ChunkConfig previousChunk)
        {
            if (list == null || list.Count == 0)
            {
                Debug.LogWarning($"ChunkTemplateBuffer: allowedNext   {previousChunk.name},   .");
                return previousChunk;
            }

            //   
            float total = list.Sum(cfg => cfg.weight);
            //    0  total
            float sample = (float)rnd.NextDouble() * total;

            float accum = 0f;
            foreach (var cfg in list)
            {
                accum += cfg.weight;
                if (sample <= accum)
                    return cfg;
            }
            //   
            return list[list.Count - 1];
        }
    }
}
