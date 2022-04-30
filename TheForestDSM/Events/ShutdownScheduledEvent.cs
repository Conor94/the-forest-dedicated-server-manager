using DataAccess.Models;
using Prism.Events;

namespace TheForestDSM.Events
{
    public class ShutdownScheduledEvent : PubSubEvent<ShutdownServiceData>
    {
    }
}
