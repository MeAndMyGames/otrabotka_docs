using UnityEngine;
using Otrabotka.Configs;

namespace Otrabotka.Interfaces
{
    public interface ITransportUser
    {
        bool IsInVehicle { get; }
        void EnterVehicle(GameObject vehiclePrefab, InteractionDefinition definition);
        void ExitVehicle();
    }
} 