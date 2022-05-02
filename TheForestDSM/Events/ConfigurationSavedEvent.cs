using DataAccess.Models;
using Prism.Events;

namespace TheForestDSM.Events
{
    public class ConfigurationSavedEvent : PubSubEvent<Configuration>
    {
    }
}
