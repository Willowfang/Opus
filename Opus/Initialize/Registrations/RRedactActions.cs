using Opus.Actions.Implementation.Merge;
using Opus.Actions.Implementation.Redact;
using Opus.Actions.Services.Merge;
using Opus.Actions.Services.Redact;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.LoggingLib;

namespace Opus.Initialize.Registrations
{
    internal static class RRedactActions
    {
        internal static void Register(IContainerRegistry registry, ILogbook logbook)
        {
            logbook.Write($"Registering redact actions...", LogLevel.Debug, callerName: "App");

            registry.RegisterSingleton<IRedactProperties, RedactProperties>();
            registry.Register<IRedactMethods, RedactMethods>();
            registry.RegisterSingleton<IRedactEventHandling, RedactEventHandling>();

            logbook.Write($"Redact actions registered.", LogLevel.Debug, callerName: "App");
        }
    }
}
