using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using PDFExtractor.Modules.Bookmarks.Views;
using PDFExtractor.Core.Constants;
using PDFExtractor.Core.Singletons;

namespace PDFExtractor.Modules.Bookmarks
{
    public class BookmarksModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var navigator = containerProvider.Resolve<INavigationAssist>();

            navigator.Add<BookmarksView>(RegionNames.SHELL_BOOKMARKS, SchemeNames.SPLIT);
            navigator.Add<SignatureRemovalView>(RegionNames.SHELL_BOOKMARKS, SchemeNames.SIGNATURE);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}