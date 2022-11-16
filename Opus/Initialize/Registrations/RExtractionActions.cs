using Opus.Actions.Implementation.Extract;
using Opus.Actions.Services.Extract;
using Prism.Ioc;
using WF.LoggingLib;

namespace Opus.Initialize.Registrations
{
    internal static class RExtractionActions
    {
        internal static void Register(IContainerRegistry registry, ILogbook logbook)
        {
            logbook.Write($"Registering extraction actions...", LogLevel.Debug, callerName: "App");

            registry.RegisterSingleton<IExtractionActionProperties, ExtractionActionProperties>();
            registry.Register<IExtractionActionMethods, ExtractionActionMethods>();
            registry.RegisterSingleton<IExtractionActionEventHandling, ExtractionActionEventHandling>();

            registry.RegisterSingleton<IExtractionSupportProperties, ExtractionSupportProperties>();
            registry.Register<IExtractionSupportMethods, ExtractionSupportMethods>();
            registry.RegisterSingleton<IExtractionSupportEventHandling,
                ExtractionSupportEventHandling>();

            logbook.Write($"Extraction actions registered.", LogLevel.Debug, callerName: "App");
        }
    }
}
