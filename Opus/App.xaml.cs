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
using CX.ZipLib;
using CX.ZipLib.Framework;

namespace Opus
{
    /// <summary>
    /// Main application class and starting point for Opus.
    /// <para>
    /// Contains application startup logic, defines modules,
    /// shell, DI service implementations and language. Checks
    /// for updates.
    /// </para>
    /// </summary>
    public partial class App : PrismApplication
    {
        private string[] arguments;
        private bool updating;

        /// <summary>
        /// Functions to perform when starting the application.
        /// <para>
        /// Store the given arguments for internal use (if any were given) and
        /// then proceed in regular startup order. <see cref="OnInitialized"/> has been overridden
        /// and will later be called. Several other startup-related methods have also been overridden.
        /// </para>
        /// </summary>
        /// <param name="e">Arguments for program startup. Contains console startup arguments.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0)
                arguments = e.Args;
            else
                arguments = null;

            base.OnStartup(e);
        }

        /// <summary>
        /// Actions to do when closing the application.
        /// <para>
        /// Logging service implementation will be flushed and closed before exiting the application (as per
        /// recommendations of <see cref="Serilog"/>). Normal <see cref="OnExit(ExitEventArgs)"/> routines will be
        /// continued after this detour.
        /// </para>
        /// </summary>
        /// <param name="e">Application exit event arguments.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            (Container.Resolve<ILogbook>() as SeriLogbook).CloseAndFlush();
            base.OnExit(e);
        }

        /// <summary>
        /// Set application display language.
        /// <para>
        /// Selected language code will be retrieved from application configuration. If no language code has been
        /// saved by the user, the code will correspond to the culture of the current thread.
        /// </para>
        /// </summary>
        protected void SetLanguage()
        {
            CultureInfo ci = new CultureInfo(Container.Resolve<IConfiguration>().LanguageCode);
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        /// <summary>
        /// Update profiles for composition of documents.
        /// <para>If new profiles (added with updates or otherwise into the
        /// profile directory) are found, they are imported into the options. If a previous profile with the same
        /// exists, it will be overridden. The new profile file will be deleted after import.
        /// </para>
        /// </summary>
        protected void UpdateProfiles()
        {
            ICompositionOptions options = Container.Resolve<ICompositionOptions>();

            bool errorFlag = false;

            if (Directory.Exists(FilePaths.PROFILE_DIRECTORY) == false)
                return;

            // Find new profile files in the profiles directory.
            foreach (string filePath in Directory.GetFiles(FilePaths.PROFILE_DIRECTORY))
            {
                try
                {
                    // Import each profile and override and old one, if such a profile exist.
                    ICompositionProfile profile = options.ImportProfile(filePath);
                    options.SaveProfile(profile);
                    File.Delete(filePath);
                }
                catch
                {
                    errorFlag = true;
                }
            }

            // If there was an error when importing profiles, notify the user.
            if (errorFlag)
            {
                MessageBox.Show(
                    Opus.Resources.Messages.StartUp.ProfileUpdateFailed,
                    Opus.Resources.Labels.General.Error
                );
            }
        }

        /// <summary>
        /// Register types in the DI container (Unity, through Prism abstraction).
        /// </summary>
        /// <param name="containerRegistry">Registry to register the types in.</param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Logging services. These are contained in the CX.LoggingLibrary.
            // If the arguments given for program start were not null, then this instance
            // is a context menu or command line instance. If that is the case, do not
            // save log files.
            if (arguments == null)
                containerRegistry.RegisterSingleton<ILogbook, SeriLogbook>();
            else
                containerRegistry.RegisterSingleton<ILogbook>(x => EmptyLogbook.Create());

            // Configuration services. Load from file or create new configuration if none is found
            // from files.
            string configPath = Path.Combine(
                FilePaths.CONFIG_DIRECTORY,
                "Config" + FilePaths.CONFIG_EXTENSION
            );
            containerRegistry.RegisterSingleton<IConfiguration>(
                x => Configuration.Load(configPath, Container.Resolve<ILogbook>())
            );

            // Set the program language according to loaded settings.
            SetLanguage();

            // Services for manipulating data. These services and their implementation
            // contains the bulk of the business logic the application uses. Many of these services
            // are contained in the CX.PdfLib -libraries. If you are looking for particulars of
            // pdf manipulation functions, they are probably included in one of these.
            containerRegistry.Register<IAnnotationService, AnnotationService>();
            containerRegistry.Register<IPdfAConvertService, PdfAConverter>();
            containerRegistry.Register<IBookmarkService, BookmarkService>();
            containerRegistry.Register<IExtractionService, ExtractionService>();
            containerRegistry.Register<ISigningService, SigningService>();
            containerRegistry.Register<IMergingService, MergingService>();
            containerRegistry.Register<IWordConvertService, WordConvertService>();
            containerRegistry.Register<IZipService, ZipService>();

            // UI-related services. Navigation assistant and registry takes care
            // of navigation of the view according to selected scheme name.
            // Path selection services let the user select a specific file or folder
            // path (the implementation of said service was dependent on native libraries)
            // and dialogAssist takes care of displaying dialogs to the user in the selected
            // order.
            containerRegistry.RegisterManySingleton<NavigationAssist>(
                typeof(INavigationAssist),
                typeof(INavigationTargetRegistry)
            );
            containerRegistry.Register<IPathSelection, PathSelectionWin>();
            containerRegistry.RegisterSingleton<IDialogAssist, DialogAssist>();
            containerRegistry.RegisterSingleton<ISchemeInstructions, SchemeInstructions>();

            // Data services. This service saves data to a database. In this case, a NoSQL-database
            // called LiteDB. Database is currently basically only used to store file composition
            // profiles. The database is stored locally along with the configuration file, inside the
            // application directory.
            var provider = new DataProviderLiteDB(
                Path.Combine(FilePaths.CONFIG_DIRECTORY, "App" + FilePaths.CONFIG_EXTENSION)
            );
            containerRegistry.RegisterInstance<IDataProvider>(provider);

            // Composition options contain many different options so they are stored
            // separate from the application-wide configuration file.
            containerRegistry.RegisterSingleton<ICompositionOptions, CompositionOptions>();

            // These are services that hold together individual, smaller services related
            // to file parts extraction and signature removal.
            containerRegistry.Register<IExtractionExecutor, ExtractionExecutor>();
            containerRegistry.Register<ISignatureExecutor, SignatureExecutor>();

            // Update composition profiles once relevant services have been registered
            // in the container.
            UpdateProfiles();

            // Register the composing service after the profiles have been updated.
            containerRegistry.Register<IComposer, Composer>();

            // Context Menu. This service takes care of the windows context menu functionalities
            // and is dependant on many of the previous services.
            containerRegistry.Register<IContextMenu, ContextMenuExecutor>();

            // Register update checkin service for checking program updates.
            containerRegistry.RegisterSingleton<IUpdateExecutor, UpdateExecutor>();

            // Register logging services and write the first logbook entry.
            ILogbook logbook = Container.Resolve<ILogbook>();
            logbook.Write(
                "Hello from logging! I have been registered.",
                LogLevel.Debug,
                callerName: "Application"
            );
        }

        /// <summary>
        /// Register application modules for the catalog. This includes action modules, file selection, options and
        /// main section, which organizes all the other modules.
        /// </summary>
        /// <param name="moduleCatalog">The catalog to register the modules in.</param>
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<Modules.MainSection.MainSectionModule>();
            moduleCatalog.AddModule<Modules.File.FileModule>();
            moduleCatalog.AddModule<Modules.Action.ActionModule>();
            moduleCatalog.AddModule<Modules.Options.OptionsModule>();

            ILogbook logbook = Container.Resolve<ILogbook>();
            logbook.Write("Modules configured.", LogLevel.Debug, callerName: "Application");
        }

        /// <summary>
        /// Create the main shell window to hold the contents of the modules.
        /// </summary>
        /// <returns>Returns the instance of the main shell window.</returns>
        protected override Window CreateShell()
        {
            updating = Container.Resolve<IUpdateExecutor>().CheckForUpdates();

            return arguments == null
                ? Container.Resolve<MainWindowView>()
                : Container.Resolve<ContextMenuView>();
        }

        /// <summary>
        /// Override of the default OnInitialized. Run the base method and after that,
        /// check for updates and update if necessary. Run in regular or Context menu -mode,
        /// depending on the environment.
        /// </summary>
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

        /// <summary>
        /// Run context menu action given with arguments.
        /// </summary>
        /// <param name="viewModel">Instance of the context menu viewModel to run
        /// the action on.</param>
        protected async void RunContext(ContextMenuViewModel viewModel)
        {
            await viewModel.ContextMenu.Run(arguments);
            Current.Shutdown();
        }

        /// <summary>
        /// Update the program and restart it.
        /// </summary>
        protected async void Update()
        {
            bool initialized = await Container.Resolve<IUpdateExecutor>().InitializeUpdate();

            if (initialized)
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
