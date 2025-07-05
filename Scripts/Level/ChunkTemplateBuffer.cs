using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Otrabotka.Configs;
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
        public void GenerateTemplate(DayCycleSettings daySettings)
        {
            // Create a reproducible random sequence
            Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            var rnd = new System.Random(Seed);

            Template = new List<ChunkConfig>(templateLength);

            // pick a random start chunk
            var startList = daySettings.startChunks;
            if (startList == null || startList.Count == 0)
            {
                Debug.LogError("DayCycleSettings.startChunks пуст! √енераци€ невозможна.");
                return;
            }
            Template.Add(startList[rnd.Next(startList.Count)]);

            // build the rest of the sequence by walking the allowedNext graph
            for (int i = 1; i < templateLength; i++)
            {
                var prev = Template[i - 1];
                var candidates = prev.allowedNext?.ToList();
                Template.Add(PickByWeight(candidates, rnd, prev));
            }
        }

        /// <summary>
        /// ¬ыбирает случайный элемент по весам. 
        /// ≈сли список пуст или null Ч возвращает previousChunk.
        /// </summary>
        private ChunkConfig PickByWeight(List<ChunkConfig> list, System.Random rnd, ChunkConfig previousChunk)
        {
            if (list == null || list.Count == 0)
            {
                Debug.LogWarning($"ChunkTemplateBuffer: allowedNext пуст дл€ {previousChunk.name}, возвращаем его самого.");
                return previousChunk;
            }

            // вычисл€ем сумму весов
            float total = list.Sum(cfg => cfg.weight);
            // случайное значение от 0 до total
            float sample = (float)rnd.NextDouble() * total;

            float accum = 0f;
            foreach (var cfg in list)
            {
                accum += cfg.weight;
                if (sample <= accum)
                    return cfg;
            }
            // на вс€кий случай
            return list[list.Count - 1];
        }
    }
}
