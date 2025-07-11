using System.Collections.Generic;

namespace Otrabotka.Scenario.Providers
{
    public interface IStaticSequenceProvider
    {
        List<EventTemplate> GetStaticSequence(int day);
    }

    public interface IKeyEventsProvider
    {
        List<EventTemplate> GetKeyEvents(int day);
    }

    public interface IRandomEventProvider
    {
        EventTemplate GetRandomTemplate();
    }
}
