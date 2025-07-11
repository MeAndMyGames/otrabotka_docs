using UnityEngine;
using System.Collections.Generic;
using Otrabotka.Core;

namespace Otrabotka.Systems
{
    [DisallowMultipleComponent]
    public class ChunkManager : MonoBehaviour
    {
        // Словарь: eventId → ChunkInstance
        private readonly Dictionary<int, ChunkInstance> _chunksByEvent = new Dictionary<int, ChunkInstance>();

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        // Регистрируем чанк сразу после Instantiate + Init
        public void RegisterChunkInstance(ChunkInstance chunk)
        {
            if (chunk == null) return;
            _chunksByEvent[chunk.EventId] = chunk;
        }

        // При провале события вызываем замену
        public void ReplaceChunkForEvent(int eventId)
        {
            if (!_chunksByEvent.TryGetValue(eventId, out var chunk))
            {
                Debug.LogWarning($"ChunkManager: нет чанка для события {eventId}");
                return;
            }

            if (chunk.SecondaryPrefab == null)
            {
                Debug.Log($"ChunkManager: у чанка для event {eventId} нет SecondaryPrefab");
                return;
            }

            chunk.ApplySecondaryPrefab(chunk.SecondaryPrefab);
            Debug.Log($"ChunkManager: применил SecondaryPrefab для события {eventId}");
        }
    }
}
