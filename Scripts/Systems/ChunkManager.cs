using UnityEngine;
using System.Collections.Generic;

namespace Otrabotka.Systems
{
    [DisallowMultipleComponent]
    public class ChunkManager : MonoBehaviour
    {
        // Словарь: eventId → ChunkInstance
        private readonly Dictionary<int, ChunkInstance> _chunksByEvent = new Dictionary<int, ChunkInstance>();

        /// <summary>
        /// Регистрирует чанк сразу после Instantiate.
        /// </summary>
        public void RegisterChunkInstance(ChunkInstance chunk)
        {
            if (chunk == null) return;
            _chunksByEvent[chunk.EventId] = chunk;
        }

        /// <summary>
        /// Меняет prefab на SecondaryPrefab у чанка с заданным eventId.
        /// </summary>
        public void ReplaceChunkForEvent(int eventId)
        {
            if (!_chunksByEvent.TryGetValue(eventId, out var chunk))
            {
                Debug.LogWarning($"ChunkManager: не найден чанк для события {eventId}");
                return;
            }

            if (!chunk.HasSecondaryState)
            {
                Debug.Log($"ChunkManager: у чанка {eventId} нет secondary prefab");
                return;
            }

            chunk.ApplySecondaryPrefab();
            Debug.Log($"ChunkManager: применён secondary prefab для event {eventId}");
        }
    }
}
