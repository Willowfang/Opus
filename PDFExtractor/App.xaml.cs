using System.Windows;
using Prism.Ioc;
using Prism.Unity;
using ExtLib;
using Prism.Modularity;
using PDFExtractor.Views;
using PDFExtractor.Core.Singletons;
using Prism.Events;
using PDFExtractor.Core.Events;
using PDFExtractor.Core.Constants;
using System.Globalization;
using System.Threading;

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
            containerRegistry.Register<IFilePDF, FilePDF>();
            containerRegistry.RegisterSingleton<ICommonValues, CommonValues>();
            containerRegistry.RegisterSingleton<INavigationAssist, NavigationAssist>();
            containerRegistry.RegisterSingleton<IDialogAssist, DialogAssist>();
        }
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<Modules.File.FileModule>();
            moduleCatalog.AddModule<Modules.Bookmarks.BookmarksModule>();
            moduleCatalog.AddModule<Modules.NewBookmark.NewBookmarkModule>();
            moduleCatalog.AddModule<Modules.Dialog.DialogModule>();
        }
        protected override Window CreateShell()
        {
            // CultureInfo info = new CultureInfo("fi-FI");
            // Thread.CurrentThread.CurrentCulture = info;
            // Thread.CurrentThread.CurrentUICulture = info;

            var w = Container.Resolve<MainWindowView>();
            return w;
        }
    }
}
