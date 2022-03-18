using System.Windows;
using Prism.Ioc;
using Prism.Unity;
using Prism.Modularity;
using Opus.Views;
using System.Globalization;
using System.Threading;
using Opus.Services.UI;
using Opus.Services.Implementation.UI;
using Opus.Services.Data;
using Opus.Services.Implementation.Data;
using Opus.Core.Constants;
using Opus.Services.Configuration;
using Opus.Services.Implementation.Configuration;
using CX.PdfLib.Services;
using CX.PdfLib.iText7;
using Opus.Services.Input;
using Opus.Services.Implementation.Input;
using System.IO;
using Opus.Services.Data.Composition;
using Opus.Services.Implementation.Data.Composition;
using Opus.ViewModels;
using PdfLib.PDFTools;
using Opus.Core.Executors;

namespace Opus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private string[] arguments;

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0)
                arguments = e.Args;
            else
                arguments = null;

            base.OnStartup(e);
        }

        /// <summary>
        /// Set application display language
        /// </summary>
        /// <param name="container">Temporary container for registered types</param>
        private void SetLanguage()
        {
            CultureInfo ci = new CultureInfo(Container.Resolve<IConfiguration>().LanguageCode);
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        private void UpdateProfiles()
        {
            ICompositionOptions options = Container.Resolve<ICompositionOptions>();

            bool errorFlag = false;

            if (Directory.Exists(FilePaths.PROFILE_DIRECTORY) == false) return;

            foreach (string filePath in Directory.GetFiles(FilePaths.PROFILE_DIRECTORY))
            {
                try
                {
                    ICompositionProfile profile = options.ImportProfile(filePath);
                    options.SaveProfile(profile);
                    File.Delete(filePath);
                }
                catch
                {
                    errorFlag = true;
                }
            }

            if (errorFlag)
            {
                MessageBox.Show(Opus.Resources.Messages.StartUp.ProfileUpdateFailed,
                        Opus.Resources.Labels.General.Error);
            }
        }

        // Overrides for Prism application methods
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Configuration services
            string configPath = Path.Combine(FilePaths.CONFIG_DIRECTORY, "Config" + FilePaths.CONFIG_EXTENSION);
            containerRegistry.RegisterSingleton<IConfiguration>(x => Configuration.Load(configPath));
            SetLanguage();

            // Services for manipulating data
            containerRegistry.Register<IPdfAConverter, PdfAConverter>();
            containerRegistry.Register<IBookmarker, Bookmarker>();
            containerRegistry.Register<IExtractor, Extractor>();
            containerRegistry.Register<ISigner, Signer>();
            containerRegistry.Register<IMerger, Merger>();
            containerRegistry.Register<IConverter, ConverterWord>();
            containerRegistry.Register<IManipulator, Manipulator>();

            // UI-related services
            containerRegistry.RegisterManySingleton<NavigationAssist>(
                typeof(INavigationAssist),
                typeof(INavigationTargetRegistry));
            containerRegistry.Register<IPathSelection, PathSelectionWin>();
            containerRegistry.RegisterSingleton<IDialogAssist, DialogAssist>();

            // Data services
            var provider = new DataProviderLiteDB(Path.Combine(FilePaths.CONFIG_DIRECTORY,
                "App" + FilePaths.CONFIG_EXTENSION));
            containerRegistry.RegisterInstance<IDataProvider>(provider);
            containerRegistry.RegisterSingleton<ISignatureOptions, SignatureOptions>();
            containerRegistry.RegisterSingleton<ICompositionOptions, CompositionOptions>();

            containerRegistry.Register<IExtractionExecutor, ExtractionExecutor>();

            UpdateProfiles();

            containerRegistry.Register<IComposerFactory, ComposerFactory>();

            // Context Menu
            containerRegistry.Register<IContextMenu, ContextMenuExecutor>();
        }
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<Modules.MainSection.MainSectionModule>();
            moduleCatalog.AddModule<Modules.File.FileModule>();
            moduleCatalog.AddModule<Modules.Action.ActionModule>();
            moduleCatalog.AddModule<Modules.Options.OptionsModule>();
        }
        protected override Window CreateShell()
        {
            return arguments == null ? Container.Resolve<MainWindowView>() : Container.Resolve<ContextMenuView>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (MainWindow.DataContext is ContextMenuViewModel viewModel)
            {
                RunContext(viewModel);
            }
        }

        private async void RunContext(ContextMenuViewModel viewModel)
        {
            await viewModel.ContextMenu.Run(arguments);
            Current.Shutdown();
        }
    }
}
