using Opus.Actions.Implementation.WorkCopy;
using Opus.Actions.Services.WorkCopy;
using Prism.Ioc;
using WF.LoggingLib;

namespace Opus.Initialize.Registrations
{
    internal static class RWorkCopyActions
    {
        internal static void Register(IContainerRegistry registry, ILogbook logbook)
        {
            logbook.Write($"Registering work copy actions...", LogLevel.Debug, callerName: "App");

            registry.RegisterSingleton<IWorkCopyProperties, WorkCopyProperties>();
            registry.Register<IWorkCopyMethods, WorkCopyMethods>();
            registry.RegisterSingleton<IWorkCopyEventHandling, WorkCopyEventHandling>();

            logbook.Write($"Work copy actions registered.", LogLevel.Debug, callerName: "App");
        }
    }
}
