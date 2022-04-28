using DataAccess.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheForestDSM.Events
{
    public class ConfigurationSavedEvent : PubSubEvent<Configuration>
    {
    }
}
