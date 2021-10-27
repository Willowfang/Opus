using PDFExtractor.Modules.File.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using PDFExtractor.Core.Constants;
using Prism.Events;
using PDFExtractor.Core.Singletons;

namespace PDFExtractor.Modules.File
{
    public class FileModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var navigator = containerProvider.Resolve<INavigationAssist>();

            navigator.Add<FileNavigationView>(RegionNames.SHELL_FILE, SchemeNames.SPLIT);
            navigator.Add<FileMultipleView>(RegionNames.SHELL_FILE, SchemeNames.SIGNATURE);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}