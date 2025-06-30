using System.Collections.Generic;
using UnityEngine;

namespace Otrabotka.Scenario.Providers
{
    [CreateAssetMenu(menuName = "Otrabotka/Static Sequence Config", fileName = "StaticSequenceConfig")]
    public class StaticSequenceConfig : ScriptableObject
    {
        public List<DayStaticSequence> Days = new List<DayStaticSequence>();
    }

    [System.Serializable]
    public class DayStaticSequence
    {
        public int DayNumber;
        public List<EventTemplate> Sequence = new List<EventTemplate>();
    }
}
