using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using PDFExtractor.Modules.Bookmarks.Views;
using PDFExtractor.Core.Constants;

namespace PDFExtractor.Modules.Bookmarks
{
    public class BookmarksModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.SHELL_BOOKMARKS, containerProvider.Resolve<BookmarksView>);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            
        }
    }
}