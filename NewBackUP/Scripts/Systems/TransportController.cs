using UnityEngine;
using Otrabotka.Configs;
using Otrabotka.Interfaces;
using Otrabotka.Level;
using System.Collections.Generic;

namespace Otrabotka.Systems
{
    public class TransportController : MonoBehaviour, ITransportUser
    {
        public static TransportController Instance { get; private set; }

        public bool IsInVehicle { get; private set; }
        private GameObject currentVehicle;
        // Компонент для выхода/входа из транспорта на текущем транспортном средстве
        private Otrabotka.Interactions.VehicleExitComponent exitComponent;
        // Текущая конфигурация взаимодействия для выхода из транспорта
        private InteractionDefinition currentDefinition;
        private Transform spriteHolder;

        // Информация об оставленных транспортных средствах по индексу чанка
        public class ExitInfo { public InteractionDefinition definition; public Vector3 localPosition; }
        // Информация о точках повторной посадки (entry) по индексу чанка
        public class EntryInfo { public GameObject prefab; public Vector3 localPosition; }
        private readonly Dictionary<int, ExitInfo> exitMap = new Dictionary<int, ExitInfo>();
        private readonly Dictionary<int, EntryInfo> entryMap = new Dictionary<int, EntryInfo>();

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

        public void EnterVehicle(GameObject vehiclePrefab, InteractionDefinition definition)
        {
            if (IsInVehicle) return;
            var playerRoot = GameObject.FindGameObjectWithTag("Player");
            if (playerRoot == null) return;
            var holder = playerRoot.transform.Find("SpriteHolder");
            if (holder != null)
            {
                foreach (var r in holder.GetComponentsInChildren<Renderer>(true))
                    r.enabled = false;
            }
            // Instantiate and ensure vehicle is fully active (root and all children)
            currentVehicle = Instantiate(vehiclePrefab, playerRoot.transform.position, playerRoot.transform.rotation);
            // Activate root
            if (!currentVehicle.activeSelf)
                currentVehicle.SetActive(true);
            // Activate all child game objects, in case any are disabled by default
            var allTransforms = currentVehicle.GetComponentsInChildren<Transform>(true);
            foreach (var tr in allTransforms)
                tr.gameObject.SetActive(true);
            currentVehicle.tag = playerRoot.tag;
            currentVehicle.layer = playerRoot.layer;
            spriteHolder = holder;
            // Сохраняем текущее определение выхода из компонента на машине (если есть)
            exitComponent = currentVehicle.GetComponent<Otrabotka.Interactions.VehicleExitComponent>();
            if (exitComponent != null && exitComponent.ExitDefinition != null)
                currentDefinition = exitComponent.ExitDefinition;
            else
                currentDefinition = definition;
            // Запоминаем entryPrefab для последующей посадки
            if (exitComponent != null && exitComponent.EntryPrefab != null)
                // entryMap заполнится при выходе
                ;
            IsInVehicle = true;
            Debug.LogWarning("[TransportController] EnterVehicle");
        }

        public void ExitVehicle()
        {
            if (!IsInVehicle) return;
            // Определяем позицию выхода
            Vector3 spawnPos = currentVehicle != null ? currentVehicle.transform.position : Vector3.zero;
            // Ищем ближайший чанк
            GameObject chunksContainer = GameObject.Find("ChunksContainer");
            Transform parentChunk = null;
            float bestDist = float.MaxValue;
            if (chunksContainer != null)
            {
                foreach (Transform chunk in chunksContainer.transform)
                {
                    float dist = Mathf.Abs(chunk.position.x - spawnPos.x);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        parentChunk = chunk;
                    }
                }
            }
            // Сохраняем и инстанцируем exitEventPrefab как потомка чанка
            if (parentChunk != null && currentDefinition != null && currentDefinition.exitEventPrefab != null)
            {
                Vector3 localPos = parentChunk.InverseTransformPoint(spawnPos);
                var exitGO = Instantiate(currentDefinition.exitEventPrefab, parentChunk);
                exitGO.transform.localPosition = localPos;
                exitGO.SetActive(true);
                foreach (var tr in exitGO.GetComponentsInChildren<Transform>(true)) tr.gameObject.SetActive(true);
                // Запоминаем exit для чанка
                int chunkIndex = ParseChunkIndex(parentChunk.name);
                exitMap[chunkIndex] = new ExitInfo { definition = currentDefinition, localPosition = localPos };
            }
            // Spawn entry interaction (точка посадки) используя сохранённый компонент
            if (parentChunk != null && exitComponent != null && exitComponent.EntryPrefab != null)
            {
                int chunkIndex = ParseChunkIndex(parentChunk.name);
                Vector3 localPos = parentChunk.InverseTransformPoint(spawnPos);
                entryMap[chunkIndex] = new EntryInfo { prefab = exitComponent.EntryPrefab, localPosition = localPos };
                var entryGO = Instantiate(exitComponent.EntryPrefab, parentChunk);
                entryGO.transform.localPosition = localPos;
                entryGO.SetActive(true);
                foreach (var tr in entryGO.GetComponentsInChildren<Transform>(true)) tr.gameObject.SetActive(true);
            }
            // Destroy vehicle instance
            if (currentVehicle != null)
                Destroy(currentVehicle);
            if (spriteHolder != null)
            {
                foreach (var r in spriteHolder.GetComponentsInChildren<Renderer>(true))
                    r.enabled = true;
            }
            // Сброс состояния
            IsInVehicle = false;
            currentDefinition = null;
            Debug.LogWarning("[TransportController] ExitVehicle");
        }

        private void Update()
        {
            if (IsInVehicle && Input.GetKeyDown(KeyCode.E))
                ExitVehicle();
        }

        /// <summary>
        /// Проверяет, есть ли сохранённый выход для указанного чанка.
        /// </summary>
        public bool HasExitForChunk(int chunkIndex) => exitMap.ContainsKey(chunkIndex);
        public ExitInfo GetExitInfo(int chunkIndex) => exitMap[chunkIndex];
        public bool HasEntryForChunk(int chunkIndex) => entryMap.ContainsKey(chunkIndex);
        public EntryInfo GetEntryInfo(int chunkIndex) => entryMap[chunkIndex];

        // Парсит индекс чанка из имени контейнера формата "Chunk[8]"
        private int ParseChunkIndex(string chunkName)
        {
            int start = chunkName.IndexOf('[');
            int end = chunkName.IndexOf(']');
            if (start >= 0 && end > start)
            {
                string num = chunkName.Substring(start + 1, end - start - 1);
                if (int.TryParse(num, out int idx))
                    return idx;
            }
            return -1;
        }
    }
} 