namespace Otrabotka.Configs
{
    using UnityEngine;
    using System.Collections.Generic;
    using Otrabotka.Level.Configs;

    [CreateAssetMenu(menuName = "Otrabotka/Configs/DayCycleSettings", fileName = "DayCycleSettings")]
    public class DayCycleSettings : ScriptableObject
    {
        [Tooltip("������ ������������� ����� �� �����")]
        public AnimationCurve intensityOverDay = AnimationCurve.Linear(0, 0, 1, 1);

        [Tooltip("��� ������ ��� (0�24)")]
        public float startHour = 6f;

        [Header("�������� ���")]
        public List<ChunkConfig> startChunks;

        [Header("������ ��� ������/�������� ������")]
        public float spawnDistance = 10f;
        public float despawnDistance = 15f;
    }
}
    