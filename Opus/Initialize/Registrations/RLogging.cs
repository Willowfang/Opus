using Opus.Common.Logging;
using Prism.Ioc;
using WF.LoggingLib.Defaults;
using WF.LoggingLib;

namespace Opus.Initialize.Registrations
{
    internal static class RLogging
    {
        internal static void Register(IContainerRegistry registry, string[] arguments)
        {
            if (arguments == null)
                registry.RegisterSingleton<ILogbook, SeriLogbook>();
            else
                registry.RegisterSingleton<ILogbook>(() => EmptyLogbook.Create());
        }
    }
}
