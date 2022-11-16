using Opus.Common.Services.Data.Composition;
using Opus.Common.Implementation.Data.Composition;
using Prism.Ioc;
using WF.LoggingLib;

namespace Opus.Initialize.Registrations
{
    internal static class ROptions
    {
        internal static void Register(IContainerRegistry registry, ILogbook logbook)
        {
            logbook.Write($"Registering options services...", LogLevel.Debug, callerName: "App");

            registry.RegisterSingleton<ICompositionOptions, CompositionOptions>();

            logbook.Write($"Options services registered.", LogLevel.Debug, callerName: "App");
        }
    }
}
