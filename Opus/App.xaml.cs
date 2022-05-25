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
using CX.LoggingLib;
using LoggingLib.Defaults;
using Opus.Services.Implementation.Logging;
using Opus.Values;
using System.Reflection;
using System;
using System.Diagnostics;
using CX.ZipLib;
using CX.ZipLib.Framework;
using CX.TaskManagerLib;
using CX.TaskManagerLib.LiteDB;

namespace Opus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private string[] arguments;
        private bool updating;

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0)
                arguments = e.Args;
            else
                arguments = null;

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            (Container.Resolve<ILogbook>() as SeriLogbook).CloseAndFlush();
            base.OnExit(e);
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
            // Logging services
            if (arguments == null)
                containerRegistry.RegisterSingleton<ILogbook, SeriLogbook>();
            else
                containerRegistry.RegisterSingleton<ILogbook>(x => EmptyLogbook.Create());

            // Configuration services
            string configPath = Path.Combine(FilePaths.CONFIG_DIRECTORY, "Config" + FilePaths.CONFIG_EXTENSION);
            containerRegistry.RegisterSingleton<IConfiguration>(x => Configuration.Load(configPath, Container.Resolve<ILogbook>()));
            SetLanguage();
            containerRegistry.RegisterSingleton<ITaskConnection, TaskConnection>();

            // Services for manipulating data
            containerRegistry.Register<IAnnotationService, AnnotationService>();
            containerRegistry.Register<IPdfAConvertService, PdfAConverter>();
            containerRegistry.Register<IBookmarkService, BookmarkService>();
            containerRegistry.Register<IExtractionService, ExtractionService>();
            containerRegistry.Register<ISigningService, SigningService>();
            containerRegistry.Register<IMergingService, MergingService>();
            containerRegistry.Register<IWordConvertService, WordConvertService>();
            containerRegistry.Register<IZipService, ZipService>();

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
            containerRegistry.RegisterSingleton<ICompositionOptions, CompositionOptions>();
            containerRegistry.RegisterSingleton<ITaskManager, TaskManager>();

            containerRegistry.Register<IExtractionExecutor, ExtractionExecutor>();
            containerRegistry.Register<ISignatureExecutor, SignatureExecutor>();

            UpdateProfiles();

            containerRegistry.Register<IComposer, Composer>();

            // Context Menu
            containerRegistry.Register<IContextMenu, ContextMenuExecutor>();
            
            containerRegistry.RegisterSingleton<IUpdateExecutor, UpdateExecutor>();

            ILogbook logbook = Container.Resolve<ILogbook>();
            logbook.Write("Types registered.", LogLevel.Debug, callerName: "Application");
        }
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<Modules.MainSection.MainSectionModule>();
            moduleCatalog.AddModule<Modules.File.FileModule>();
            moduleCatalog.AddModule<Modules.Action.ActionModule>();
            moduleCatalog.AddModule<Modules.Options.OptionsModule>();

            ILogbook logbook = Container.Resolve<ILogbook>();
            logbook.Write("Modules configured.", LogLevel.Debug, callerName: "Application");
        }
        protected override Window CreateShell()
        {
            updating = Container.Resolve<IUpdateExecutor>().CheckForUpdates();

            return arguments == null ? Container.Resolve<MainWindowView>() : Container.Resolve<ContextMenuView>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (updating)
            {
                Update();
            }
            else if (MainWindow.DataContext is ContextMenuViewModel viewModel)
            {
                RunContext(viewModel);
            }
        }

        private async void RunContext(ContextMenuViewModel viewModel)
        {
            await viewModel.ContextMenu.Run(arguments);
            Current.Shutdown();
        }

        private async void Update()
        {
            bool initilized = await Container.Resolve<IUpdateExecutor>().InitializeUpdate();

            if (initilized)
            {
                Current.Shutdown();
            }
            else
            {
                if (MainWindow.DataContext is ContextMenuViewModel viewModel)
                {
                    RunContext(viewModel);
                }
            }
        }
    }
}
