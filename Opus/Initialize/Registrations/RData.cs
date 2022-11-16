using Opus.Common.Services.Data;
using Opus.Common.Implementation.Data;
using Opus.Values;
using Prism.Ioc;
using System.IO;
using WF.LoggingLib;

namespace Opus.Initialize.Registrations
{
    internal static class RData
    {
        internal static void Register(IContainerRegistry registry, ILogbook logbook)
        {
            logbook.Write($"Registering data services...", LogLevel.Debug, callerName: "App");

            var provider = new DataProviderLiteDB(
                Path.Combine(FilePaths.CONFIG_DIRECTORY, "App" + FilePaths.CONFIG_EXTENSION),
                logbook
            );

            registry.RegisterInstance<IDataProvider>(provider);

            logbook.Write($"Data services registered.", LogLevel.Debug, callerName: "App");
        }
    }
}
