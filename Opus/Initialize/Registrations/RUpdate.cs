using Opus.Actions.Implementation.Update;
using Opus.Actions.Services.Update;
using Prism.Ioc;
using WF.LoggingLib;

namespace Opus.Initialize.Registrations
{
    internal static class RUpdate
    {
        internal static void Register(IContainerRegistry registry, ILogbook logbook)
        {
            logbook.Write($"Registering update services...", LogLevel.Debug, callerName: "App");

            registry.RegisterSingleton<IUpdateProperties, UpdateProperties>();
            registry.Register<IUpdateMethods, UpdateMethods>();

            logbook.Write($"Update services registered.", LogLevel.Debug, callerName: "App");
        }
    }
}
