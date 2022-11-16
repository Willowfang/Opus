using Opus.Common.Implementation.Input;
using Opus.Common.Services.Dialogs;
using Opus.Common.Services.Input;
using Opus.Common.Implementation.Dialogs;
using Prism.Ioc;
using Opus.Common.Implementation.Navigation;
using Opus.Common.Services.Navigation;
using WF.LoggingLib;

namespace Opus.Initialize.Registrations
{
    internal static class RUserInterface
    {
        internal static void Register(IContainerRegistry registry, ILogbook logbook)
        {
            logbook.Write($"Registering UI services...", LogLevel.Debug, callerName: "App");

            registry.RegisterManySingleton<NavigationAssist>(
                typeof(INavigationAssist),
                typeof(INavigationTargetRegistry)
            );
            registry.Register<IPathSelection, PathSelectionWin>();
            registry.RegisterSingleton<IDialogAssist, DialogAssist>();
            registry.RegisterSingleton<ISchemeInstructions, SchemeInstructions>();

            logbook.Write($"UI services registered.", LogLevel.Debug, callerName: "App");
        }
    }
}
