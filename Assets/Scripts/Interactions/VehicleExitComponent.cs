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

        public InteractionDefinition ExitDefinition => exitDefinition;
    }
} 