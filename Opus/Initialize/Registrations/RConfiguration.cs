using Opus.Common.Services.Configuration;
using Opus.Common.Implementation.Configuration;
using Opus.Values;
using Prism.Ioc;
using System.IO;
using WF.LoggingLib;

namespace Opus.Initialize.Registrations
{
    internal static class RConfiguration
    {
        internal static void Register(IContainerRegistry registry, IContainerProvider container)
        {
            string configPath = Path.Combine(
                FilePaths.CONFIG_DIRECTORY,
                "Config" + FilePaths.CONFIG_EXTENSION
            );
            registry.RegisterSingleton<IConfiguration>(
                x => Configuration.Load(configPath, container.Resolve<ILogbook>())
            );
        }
    }
}
