using Opus.Common.Services.ContextMenu;
using Prism.Ioc;
using WF.LoggingLib;

namespace Opus.Initialize.Registrations
{
    internal static class RContextMenu
    {
        internal static void Register(IContainerRegistry registry, ILogbook logbook)
        {
            logbook.Write($"Registering context menu...", LogLevel.Debug, callerName: "App");

            registry.Register<IContextMenu, ContextMenu>();

            logbook.Write($"Context menu registered.", LogLevel.Debug, callerName: "App");
        }
    }
}
