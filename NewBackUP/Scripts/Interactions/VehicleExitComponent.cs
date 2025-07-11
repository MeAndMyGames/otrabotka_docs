using UnityEngine;
using Otrabotka.Configs;

namespace Otrabotka.Interactions
{
    /// <summary>
    /// Hold a reference to the InteractionDefinition used for exiting the vehicle.
    /// </summary>
    public class VehicleExitComponent : MonoBehaviour
    {
        [Tooltip("Definition for exit interaction when leaving the vehicle")]
        [SerializeField] private InteractionDefinition exitDefinition;
        [Tooltip("Prefab to spawn for re-entering the vehicle (Interaction trigger)")]
        [SerializeField] private GameObject entryPrefab;

        public InteractionDefinition ExitDefinition => exitDefinition;
        public GameObject EntryPrefab => entryPrefab;
    }
} 