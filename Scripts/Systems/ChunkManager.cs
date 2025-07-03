using UnityEngine;
using System.Collections.Generic;



namespace Otrabotka.Systems
{
    [DisallowMultipleComponent]
    public class ChunkManager : MonoBehaviour
    {
        // Словарь: событие → инстанс чанка
        private readonly Dictionary<int, ChunkInstance> _chunksByEvent = new Dictionary<int, ChunkInstance>();

        /// <summary>
        /// Регистрирует вновь созданный чанк и привязывает его к конкретному eventId.
        /// Вызывать сразу после Instantiate(prefab).
        /// </summary>
        public void RegisterChunkInstance(ChunkInstance chunk)
        {
            if (chunk == null) return;
            _chunksByEvent[chunk.EventId] = chunk;
        }

        /// <summary>
        /// Вызывается при провале события: ищет нужный чанк и заменяет
        /// в нём префаб на Secondary (если он задан).
        /// </summary>
        public void ReplaceChunkForEvent(int eventId)
        {
            if (!_chunksByEvent.TryGetValue(eventId, out var chunk))
            {
                Debug.LogWarning($"ChunkManager: нет зарегистрированного чанка для события {eventId}");
                return;
            }

            if (!chunk.HasSecondaryState)
            {
                Debug.Log($"ChunkManager: у события {eventId} нет вторичного состояния, пропускаем");
                return;
            }

            chunk.ApplySecondaryPrefab();
            Debug.Log($"ChunkManager: для event {eventId} применён SecondaryPrefab");
        }
    }
}
