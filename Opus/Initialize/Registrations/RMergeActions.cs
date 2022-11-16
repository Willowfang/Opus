using Opus.Actions.Implementation.Merge;
using Opus.Actions.Services.Merge;
using Prism.Ioc;
using WF.LoggingLib;

namespace Opus.Initialize.Registrations
{
    internal static class RMergeActions
    {
        internal static void Register(IContainerRegistry registry, ILogbook logbook)
        {
            logbook.Write($"Registering merge actions...", LogLevel.Debug, callerName: "App");

            registry.RegisterSingleton<IMergeProperties, MergeProperties>();
            registry.Register<IMergeMethods, MergeMethods>();
            registry.RegisterSingleton<IMergeEventHandling, MergeEventHandling>();

            logbook.Write($"Merge actions registered.", LogLevel.Debug, callerName: "App");
        }
    }
}
