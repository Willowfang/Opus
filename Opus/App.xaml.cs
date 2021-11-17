using System.Windows;
using Prism.Ioc;
using Prism.Unity;
using Prism.Modularity;
using Opus.Views;
using System.Globalization;
using System.Threading;
using Opus.ContextMenu;
using Opus.Services.UI;
using Opus.Core.ServiceImplementations.UI;
using Opus.Services.Data;
using Opus.Core.ServiceImplementations.Data;
using Opus.Core.Constants;
using Opus.Services.Configuration;
using Opus.Core.ServiceImplementations.Configuration;
using Unity.Injection;
using CX.PdfLib.Services;
using CX.PdfLib.Implementation;

namespace Opus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Create a temporary container and register types.
            // Temporary container is used in selecting language
            // and performing I/O operations without initializing UI.
            var tempContainer = CreateContainerExtension();
            RegisterTypes(tempContainer);
            SetLanguage(tempContainer);

            // If started with arguments, display no UI,
            // run command and exit.
            if (e.Args.Length > 0)
            {
                StartUpNoUI(tempContainer, e.Args);
                Current.Shutdown();
            }
            // Otherwise, initialize Prism and show UI
            else
            {
                base.OnStartup(e);
            }
        }

        /// <summary>
        /// Perform the command line command provided in arguments
        /// </summary>
        /// <param name="container">Temporary container for registered types</param>
        /// <param name="arguments">Given arguments</param>
        private void StartUpNoUI(IContainerExtension container, string[] arguments)
        {
            if (arguments[0] == "-remove") container.Register<IContextMenuCommand, RemoveSignature>();
            if (arguments[0] == "-split") container.Register<IContextMenuCommand, ExtractDocument>();
            if (arguments[0] == "-splitdir") container.Register<IContextMenuCommand, ExtractDirectory>();

            if (container.IsRegistered(typeof(IContextMenuCommand)))
                container.Resolve<IContextMenuCommand>().RunCommand(arguments);
        }

        /// <summary>
        /// Set application display language
        /// </summary>
        /// <param name="container">Temporary container for registered types</param>
        private void SetLanguage(IContainerExtension container)
        {
            CultureInfo ci = new CultureInfo(container.Resolve<IConfiguration.App>().GetLanguage());
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Services for manipulating data
            containerRegistry.Register<IBookmarker, Bookmarker>();
            containerRegistry.Register<IExtractor, Extractor>();
            containerRegistry.Register<ISigner, Signer>();
            containerRegistry.Register<IMerger, Merger>();
            containerRegistry.Register<IManipulator, Manipulator>();

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
