using System.Windows;
using Prism.Ioc;
using Prism.Unity;
using ExtLib;
using Prism.Modularity;

namespace PDFExtractor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IBookmark, Bookmarks.Bookmark>();
        }
        protected override Window CreateShell()
        {
            var w = Container.Resolve<MainWindow>();
            return w;
        }
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<Modules.File.FileModule>();
            moduleCatalog.AddModule<Modules.Bookmarks.BookmarksModule>();
            moduleCatalog.AddModule<Modules.NewBookmark.NewBookmarkModule>();
        }
    }
}
