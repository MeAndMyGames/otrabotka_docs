using System;
using Otrabotka.Configs;
using UnityEngine;
using Otrabotka.Level.Configs;

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
        public ChunkConfig[] Template { get; private set; }

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
        public void GenerateTemplate(DayCycleSettings daySettings)
        {
            // Create a reproducible random sequence
            Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            var rnd = new System.Random(Seed);

            Template = new ChunkConfig[templateLength];
            // pick a random start chunk
            var startList = daySettings.startChunks;
            Template[0] = startList[rnd.Next(startList.Count)];

            // build the rest of the sequence by walking the allowedNext graph
            for (int i = 1; i < templateLength; i++)
            {
                var prev = Template[i - 1];
                var candidates = prev.allowedNext;
                Template[i] = PickByWeight(candidates, rnd);
            }
        }

        private ChunkConfig PickByWeight(System.Collections.Generic.List<ChunkConfig> list, System.Random rnd)
        {
            float total = 0f;
            foreach (var cfg in list)
                total += cfg.weight;

            float sample = (float)rnd.NextDouble() * total;
            float accum = 0f;
            foreach (var cfg in list)
            {
                accum += cfg.weight;
                if (sample <= accum)
                    return cfg;
            }
            // fallback
            return list[list.Count - 1];
        }
    }
}
