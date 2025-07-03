namespace Otrabotka.Configs
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Otrabotka/Configs/DayCycleSettings")]
    public class DayCycleSettings : ScriptableObject
    {
        [Tooltip("Кривая интенсивности света за сутки")]
        public AnimationCurve intensityOverDay = AnimationCurve.Linear(0, 0, 1, 1);

        [Tooltip("Час старта дня (0–24)")]
        public float startHour = 6f;
    }
}