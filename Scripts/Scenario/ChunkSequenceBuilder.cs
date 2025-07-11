using System;
using System.Collections.Generic;
using UnityEngine;
using Otrabotka.Level.Configs;
using Otrabotka.Core;
using Otrabotka.Scenario.Configs;

namespace Otrabotka.Scenario
{
    /// <summary>
    /// Строит последовательность чанков для уровня на основе настроек и времени прохождения.
    /// </summary>
    public static class ChunkSequenceBuilder
    {
        public static List<ChunkConfig> BuildSequence(ScenarioSettings settings, float timeTaken)
        {
            var sequence = new List<ChunkConfig>();

            // 1. Hospital biome
            int hospitalCount = Mathf.Clamp(settings.HospitalChunkCount, 0, settings.HospitalChunks.Count);
            sequence.AddRange(settings.HospitalChunks.GetRange(0, hospitalCount));

            // 2. Выбор сложных событий
            var rnd = new System.Random();
            var selectedComplex = new List<ComplexEventConfig>();
            foreach (var cfg in settings.ComplexEventConfigs)
            {
                if (rnd.NextDouble() < cfg.TriggerProbability)
                    selectedComplex.Add(cfg);
            }

            // 3. Выбор финального биома по времени
            FinalBiomeConfig finalConfig = null;
            foreach (var fb in settings.FinalBiomes)
            {
                if (timeTaken >= fb.MinTimeTaken && timeTaken <= fb.MaxTimeTaken)
                {
                    finalConfig = fb;
                    break;
                }
            }
            if (finalConfig == null && settings.FinalBiomes.Count > 0)
                finalConfig = settings.FinalBiomes[settings.FinalBiomes.Count - 1];

            // 4. Расчёт количества простых чанков
            int finalCount = finalConfig != null ? finalConfig.Sequence.Count : 0;
            int complexTotal = 0;
            foreach (var ev in selectedComplex)
                complexTotal += ev.Sequence.Count;

            int simpleTotal = settings.TotalChunks - hospitalCount - complexTotal - finalCount;
            if (simpleTotal < 0) simpleTotal = 0;

            int segments = selectedComplex.Count + 1;
            int baseSimple = segments > 0 ? simpleTotal / segments : 0;
            int remainder = segments > 0 ? simpleTotal % segments : 0;

            // 5. Добавляем простые чанки и сложные события по сегментам
            for (int i = 0; i < segments; i++)
            {
                int count = baseSimple + (i < remainder ? 1 : 0);
                for (int j = 0; j < count; j++)
                {
                    if (settings.SimpleChunkConfigs.Count > 0)
                    {
                        int idx = rnd.Next(settings.SimpleChunkConfigs.Count);
                        sequence.Add(settings.SimpleChunkConfigs[idx]);
                    }
                }
                if (i < selectedComplex.Count)
                    sequence.AddRange(selectedComplex[i].Sequence);
            }

            // 6. Добавляем финальный биом
            if (finalConfig != null)
                sequence.AddRange(finalConfig.Sequence);

            return sequence;
        }
    }
} 