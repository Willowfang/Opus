using Opus.Commands.Implementation;
using Opus.Common.Services.Commands;
using Prism.Ioc;
using WF.LoggingLib;

namespace Opus.Initialize.Registrations
{
    internal static class RCommands
    {
        internal static void Register(IContainerRegistry registry, ILogbook logbook)
        {
            logbook.Write($"Registering commands...", LogLevel.Debug, callerName: "App");

            registry.RegisterSingleton<ICommonCommands, CommonCommands>();
            registry.Register<IComposeSettingsCommands, CompositionSettingsCommands>();
            registry.Register<IExtractSettingsCommands, ExtractSettingsCommands>();
            registry.Register<IMergeSettingsCommands, MergeSettingsCommands>();
            registry.Register<IWorkCopySettingsCommands, WorkCopySettingsCommands>();

            registry.Register<IExtractionActionCommands, ExtractionActionCommands>();
            registry.Register<IExtractionSupportCommands, ExtractionSupportCommands>();

            registry.Register<IMergeCommands, MergeCommands>();

            registry.Register<IWorkCopyCommands, WorkCopyCommands>();

            registry.Register<ICompositionCommands, CompositionCommands>();

            logbook.Write($"Commands registered.", LogLevel.Debug, callerName: "App");
        }
    }
}
