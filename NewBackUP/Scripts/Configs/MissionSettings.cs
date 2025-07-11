namespace Otrabotka.Configs
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Otrabotka/Configs/MissionSettings")]
    public class MissionSettings : ScriptableObject
    {
        [Tooltip("����� �� ���������� ������")]
        public float missionTimeoutHours = 18f;
    }
}