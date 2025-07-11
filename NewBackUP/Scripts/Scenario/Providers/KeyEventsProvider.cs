using System.Collections.Generic;
using System.Linq;
using Otrabotka.Scenario.Providers;

namespace Otrabotka.Scenario.Providers
{
    public class KeyEventsProvider : IKeyEventsProvider
    {
        public List<EventTemplate> GetKeyEvents(int day) => new List<EventTemplate>();
    }
}