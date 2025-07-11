using UnityEngine;
using System.Collections.Generic;
using Otrabotka.Level.Configs;

namespace Otrabotka.Systems
{
    public class ChunkObjectPool : MonoBehaviour
    {
        [System.Serializable]
        public class PooledChunk
        {
            public ChunkConfig config;
            public int poolSize = 10;
        }

        [SerializeField] private List<PooledChunk> chunkPools;
        
        private Dictionary<ChunkConfig, Queue<ChunkInstance>> _pools;
        private Dictionary<ChunkConfig, List<ChunkInstance>> _activeInstances;

        private void Awake()
        {
            InitializePools();
        }

        private void InitializePools()
        {
            _pools = new Dictionary<ChunkConfig, Queue<ChunkInstance>>();
            _activeInstances = new Dictionary<ChunkConfig, List<ChunkInstance>>();

            foreach (var pooledChunk in chunkPools)
            {
                var queue = new Queue<ChunkInstance>();
                var activeList = new List<ChunkInstance>();

                // Предварительно создаем объекты в пуле
                for (int i = 0; i < pooledChunk.poolSize; i++)
                {
                    var instance = CreateChunkInstance(pooledChunk.config);
                    instance.gameObject.SetActive(false);
                    queue.Enqueue(instance);
                }

                _pools[pooledChunk.config] = queue;
                _activeInstances[pooledChunk.config] = activeList;
            }
        }

        public ChunkInstance GetChunk(ChunkConfig config)
        {
            if (!_pools.ContainsKey(config))
            {
                Debug.LogWarning($"Пул для {config.name} не найден, создаем новый");
                return CreateChunkInstance(config);
            }

            var pool = _pools[config];
            ChunkInstance instance;

            if (pool.Count > 0)
            {
                instance = pool.Dequeue();
                instance.gameObject.SetActive(true);
            }
            else
            {
                // Если пул пуст, создаем новый объект
                instance = CreateChunkInstance(config);
            }

            _activeInstances[config].Add(instance);
            return instance;
        }

        public void ReturnChunk(ChunkInstance instance)
        {
            if (instance == null || instance.Config == null) return;

            var config = instance.Config;
            if (!_pools.ContainsKey(config)) return;

            // Удаляем из активных
            _activeInstances[config].Remove(instance);

            // Сбрасываем состояние
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(transform);

            // Возвращаем в пул
            _pools[config].Enqueue(instance);
        }

        private ChunkInstance CreateChunkInstance(ChunkConfig config)
        {
            var go = Instantiate(config.primaryPrefab);
            var instance = go.AddComponent<ChunkInstance>();
            instance.Init(config, 0, config.secondaryPrefab);
            return instance;
        }

        public void ReturnAllActiveChunks()
        {
            foreach (var kvp in _activeInstances)
            {
                foreach (var instance in kvp.Value.ToArray())
                {
                    ReturnChunk(instance);
                }
            }
        }

        private void OnDestroy()
        {
            ReturnAllActiveChunks();
        }

        // Методы для отладки
        public int GetPoolSize(ChunkConfig config)
        {
            return _pools.ContainsKey(config) ? _pools[config].Count : 0;
        }

        public int GetActiveCount(ChunkConfig config)
        {
            return _activeInstances.ContainsKey(config) ? _activeInstances[config].Count : 0;
        }
    }
} 