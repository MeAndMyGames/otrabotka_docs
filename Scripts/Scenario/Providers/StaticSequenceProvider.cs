using System.Collections.Generic;
using System.Linq;
using Otrabotka.Scenario.Providers;

namespace Otrabotka.Scenario.Providers
{
    public class StaticSequenceProvider : IStaticSequenceProvider
    {
        private readonly StaticSequenceConfig _config;

        public StaticSequenceProvider(StaticSequenceConfig config)
        {
            _config = config;
        }

        public List<EventTemplate> GetStaticSequence(int day)
        {
            var seq = _config.Days.FirstOrDefault(d => d.DayNumber == day);
            return seq != null
                ? new List<EventTemplate>(seq.Sequence)
                : new List<EventTemplate>();
        }
    }
}
