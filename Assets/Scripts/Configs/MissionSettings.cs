namespace Otrabotka.Configs
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Otrabotka/Configs/MissionSettings")]
    public class MissionSettings : ScriptableObject
    {
        [Tooltip("Часов на выполнение миссии")]
        public float missionTimeoutHours = 18f;
    }
}