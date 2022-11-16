using Opus.Actions.Implementation.Compose;
using Opus.Actions.Services.Compose;
using Prism.Ioc;
using WF.LoggingLib;

namespace Opus.Initialize.Registrations
{
    internal static class RCompositionActions
    {
        internal static void Register(IContainerRegistry registry, ILogbook logbook)
        {
            logbook.Write($"Registering composition actions...", LogLevel.Debug, callerName: "App");

            registry.RegisterSingleton<ICompositionProperties, CompositionProperties>();
            registry.Register<ICompositionMethods, CompositionMethods>();
            registry.RegisterSingleton<ICompositionEventHandling, CompositionEventHandling>();

            logbook.Write($"Composition actions registered.", LogLevel.Debug, callerName: "App");
        }
    }
}
