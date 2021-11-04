using System.Windows;
using Prism.Ioc;
using Prism.Unity;
using Prism.Modularity;
using Opus.Views;
using System.Globalization;
using System.Threading;
using PDFLib.Services;
using PDFLib.Implementation;
using Opus.ContextMenu;
using Opus.Services.UI;
using Opus.Core.ServiceImplementations.UI;
using Opus.Services.Data;
using Opus.Core.ServiceImplementations.Data;
using Opus.Core.Constants;
using Opus.Services.Configuration;
using Opus.Core.ServiceImplementations.Configuration;
using Unity.Injection;

namespace Opus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            IDataProvider Provider = DataProviderLiteDB.GetService();
            IDataProvider ProviderLang = DataProviderLanguage.GetService();
            IConfiguration.App AppConfig = AppConfiguration.GetService(ProviderLang);
            CultureInfo ci = new CultureInfo(AppConfig.GetLanguage());
            Thread.CurrentThread.CurrentUICulture = ci;

            if (e.Args.Length > 0)
            {
                if (e.Args[0] == "-remove") RemoveSignature.GetService(Signature.GetService(), 
                    SignConfiguration.GetService(Provider, AppConfig)).RunCommand(e.Args[1]);

                if (e.Args[0] == "-splitall") ExtractAll.GetService(Extraction.GetService()).RunCommand(e.Args[1]);

                if (e.Args[0] == "-splitapp") ExtractAppendices.GetService(Extraction.GetService()).RunCommand(e.Args[1]);

                if (e.Args[0] == "-splitalldir") DirExtractAll.GetService(Extraction.GetService()).RunCommand(e.Args[1]);

                if (e.Args[0] == "-splitappdir") DirExtractAppendices.GetService(Extraction.GetService()).RunCommand(e.Args[1]);

                Current.Shutdown();
            }
            else
            {
                base.OnStartup(e);
            }
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Model services
            containerRegistry.Register<IBookmark, Bookmark>();
            containerRegistry.Register<IFilePDF, FilePDF>();

            // Services for manipulating data
            containerRegistry.Register<IBookmarkOperator, BookmarkOperator>();
            containerRegistry.Register<IExtraction, Extraction>();
            containerRegistry.Register<ISignature, Signature>();

            // UI-related services
            containerRegistry.RegisterSingleton<INavigationAssist, NavigationAssist>();
            containerRegistry.RegisterSingleton<IDialogAssist, DialogAssist>();

            // Data services
            containerRegistry.RegisterSingleton<IDataProvider, DataProviderLiteDB>();
            containerRegistry.RegisterSingleton(typeof(IDataProvider), typeof(DataProviderLanguage),
                ServiceNames.LANGUAGEPROVIDER);

            // Configuration services
            containerRegistry.RegisterSingleton<IConfiguration.App>(x =>
                AppConfiguration.GetImplementation(Container.Resolve<IDataProvider>(ServiceNames.LANGUAGEPROVIDER)));
            containerRegistry.RegisterSingleton<IConfiguration.Sign, SignConfiguration>();
        }
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<Modules.MainSection.MainSectionModule>();
            moduleCatalog.AddModule<Modules.File.FileModule>();
            moduleCatalog.AddModule<Modules.Action.ActionModule>();
            moduleCatalog.AddModule<Modules.Options.OptionsModule>();
            moduleCatalog.AddModule<Modules.Dialog.DialogModule>();
        }
        protected override Window CreateShell()
        {
            var w = Container.Resolve<MainWindowView>();
            return w;
        }
    }
}
