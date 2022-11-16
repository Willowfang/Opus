using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Initialize.Registrations
{
    internal static class RModules
    {
        internal static void Register(IModuleCatalog catalog)
        {
            catalog.AddModule<Modules.MainSection.MainSectionModule>();
            catalog.AddModule<Modules.File.FileModule>();
            catalog.AddModule<Modules.Action.ActionModule>();
            catalog.AddModule<Modules.Options.OptionsModule>();
        }
    }
}
