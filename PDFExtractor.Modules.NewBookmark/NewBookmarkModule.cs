using PDFExtractor.Core.Constants;
using PDFExtractor.Core.Events;
using PDFExtractor.Core.Singletons;
using PDFExtractor.Modules.NewBookmark.Views;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace PDFExtractor.Modules.NewBookmark
{
    public class NewBookmarkModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var navigation = containerProvider.Resolve<INavigationAssist>();

            navigation.Add<BookmarkNewView>(RegionNames.SHELL_NEW, SchemeNames.SPLIT);
            navigation.Add<IdentifierView>(RegionNames.SHELL_NEW, SchemeNames.SIGNATURE);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        { }
    }
}