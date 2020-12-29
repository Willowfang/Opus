using PDFExtractor.Core.Constants;
using PDFExtractor.Modules.NewBookmark.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace PDFExtractor.Modules.NewBookmark
{
    public class NewBookmarkModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.SHELL_NEW, containerProvider.Resolve<BookmarkNewView>);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}