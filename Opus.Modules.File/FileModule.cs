using Opus.Modules.File.Views;
using Prism.Ioc;
using Prism.Modularity;
using Opus.Values;
using Opus.Services.UI;

namespace Opus.Modules.File
{
    public class FileModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var navigator = containerProvider.Resolve<INavigationAssist>();

            navigator.Add<FileMultipleView>(RegionNames.MAINSECTION_FOUR_FILE, SchemeNames.SPLIT);
            navigator.Add<FileMultipleView>(RegionNames.MAINSECTION_THREE_FILE, SchemeNames.WORKCOPY);
            navigator.Add<FileMultipleView>(RegionNames.MAINSECTION_THREE_FILE, SchemeNames.MERGE);
            navigator.Add<DirectoryNavigationView>(RegionNames.MAINSECTION_THREE_FILE, SchemeNames.COMPOSE);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}