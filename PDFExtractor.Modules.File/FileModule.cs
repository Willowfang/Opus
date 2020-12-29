using PDFExtractor.Modules.File.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using PDFExtractor.Core.Constants;

namespace PDFExtractor.Modules.File
{
    public class FileModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RequestNavigate(RegionNames.SHELL_FILE, nameof(FileNavigationView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<FileNavigationView>();
        }
    }
}