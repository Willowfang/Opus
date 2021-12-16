using Opus.Modules.File.Views;
using Prism.Ioc;
using Prism.Modularity;
using Opus.Core.Constants;
using Opus.Services.UI;

namespace Opus.Modules.File
{
    public class FileModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var navigator = containerProvider.Resolve<INavigationAssist>();

            navigator.Add<FileNavigationView>(RegionNames.MAINSECTION_THREE_FILE, SchemeNames.SPLIT);
            navigator.Add<FileMultipleView>(RegionNames.MAINSECTION_THREE_FILE, SchemeNames.SIGNATURE);
            navigator.Add<FileMultipleView>(RegionNames.MAINSECTION_THREE_FILE, SchemeNames.MERGE);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}