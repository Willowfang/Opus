using System.Windows;
using Prism.Ioc;
using Prism.Unity;
using Prism.Modularity;
using Opus.Views;
using System.Globalization;
using System.Threading;
using Opus.Common.Services.Configuration;
using Opus.Common.Services.Data.Composition;
using Opus.ViewModels;
using WF.LoggingLib;
using Opus.Common.Logging;
using Opus.Initialize;
using Opus.Initialize.Registrations;
using Opus.Actions.Services.Update;

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
            ILogbook logbook = Container.Resolve<ILogbook>();

            logbook.Write("Setting application language.", LogLevel.Debug, callerName: "App");

            CultureInfo ci = new CultureInfo(Container.Resolve<IConfiguration>().LanguageCode);
            Thread.CurrentThread.CurrentUICulture = ci;

            logbook.Write($"Application language set to {ci.DisplayName}.", LogLevel.Debug, callerName: "App");
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
            ILogbook logbook = Container.Resolve<ILogbook>();

            logbook.Write("Checking for updates to profiles.", LogLevel.Debug, callerName: "App");

            ICompositionOptions options = Container.Resolve<ICompositionOptions>();

            ProfileUpdater updater = new ProfileUpdater(options);

            updater.CheckNewProfilesAndUpdate();

            logbook.Write("Profile update check completed.", LogLevel.Debug, callerName: "App");
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
            RLogging.Register(containerRegistry, arguments);

            // Configuration services. Load from file or create new configuration if none is found
            // from files.
            RConfiguration.Register(containerRegistry, Container);

            // Set logging level according to configuration and write first logbook entry.
            ILogbook logbook = SetLogLevelAndTest();

            // Set the program language according to loaded settings.
            SetLanguage();

            // Services for manipulating data. These services and their implementation
            // contains the bulk of the business logic the application uses. Many of these services
            // are contained in the CX.PdfLib -libraries. If you are looking for particulars of
            // pdf manipulation functions, they are probably included in one of these.
            RExternalServices.Register(containerRegistry, logbook);

            // UI-related services. Navigation assistant and registry takes care
            // of navigation of the view according to selected scheme name.
            // Path selection services let the user select a specific file or folder
            // path (the implementation of said service was dependent on native libraries)
            // and dialogAssist takes care of displaying dialogs to the user in the selected
            // order.
            RUserInterface.Register(containerRegistry, logbook);

            // Data services. This service saves data to a database. In this case, a NoSQL-database
            // called LiteDB. Database is currently basically only used to store file composition
            // profiles. The database is stored locally along with the configuration file, inside the
            // application directory.
            RData.Register(containerRegistry, logbook);

            // Composition options contain many different options so they are stored
            // separate from the application-wide configuration file.
            ROptions.Register(containerRegistry, logbook);

            // Update composition profiles once relevant services have been registered
            // in the container.
            UpdateProfiles();

            // Register update checkin service for checking program updates.
            RUpdate.Register(containerRegistry, logbook);

            // Register actions. This includes properties, methods and event handling.

            RExtractionActions.Register(containerRegistry, logbook);

            RMergeActions.Register(containerRegistry, logbook);

            RWorkCopyActions.Register(containerRegistry, logbook);

            RCompositionActions.Register(containerRegistry, logbook);

            // Register commands.

            RCommands.Register(containerRegistry, logbook);

            // Context Menu. This service takes care of the windows context menu functionalities
            // and is dependent on many of the previous services.

            RContextMenu.Register(containerRegistry, logbook);

            logbook.Write(
                "All services registered.",
                LogLevel.Debug,
                callerName: "App",
                callerMemberName: "RegisterTypes");
        }

        private ILogbook SetLogLevelAndTest()
        {
            ILogbook logbook = Container.Resolve<ILogbook>();

            IConfiguration configuration = Container.Resolve<IConfiguration>();

            logbook.ChangeLevel((LogLevel)configuration.LoggingLevel);

            logbook.Write(
                "Hello from logging! I have been registered. Starting registration for other services.",
                LogLevel.Debug,
                callerName: "App",
                callerMemberName: "SetLogLevelAndTest");

            return logbook;
        }

        /// <summary>
        /// Register application modules for the catalog. This includes action modules, file selection, options and
        /// main section, which organizes all the other modules.
        /// </summary>
        /// <param name="moduleCatalog">The catalog to register the modules in.</param>
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            ILogbook logbook = Container.Resolve<ILogbook>();

            logbook.Write("Starting module configuration.", LogLevel.Debug, callerName: "App");

            RModules.Register(moduleCatalog);

            logbook.Write("Modules configured.", LogLevel.Debug, callerName: "App");
        }

        /// <summary>
        /// Create the main shell window to hold the contents of the modules.
        /// </summary>
        /// <returns>Returns the instance of the main shell window.</returns>
        protected override Window CreateShell()
        {
            ILogbook logbook = Container.Resolve<ILogbook>();

            logbook.Write("Checking for application updates.", LogLevel.Debug, callerName: "App");

            updating = Container.Resolve<IUpdateMethods>().CheckForUpdates();

            logbook.Write("Application updates checked.", LogLevel.Debug, callerName: "App");

            logbook.Write("Creating shell.", LogLevel.Debug, callerName: "App");

            Window shell = arguments == null
                ? Container.Resolve<MainWindowView>()
                : Container.Resolve<ContextMenuView>();

            logbook.Write("Shell created.", LogLevel.Debug, callerName: "App");

            return shell;
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
            ILogbook logbook = Container.Resolve<ILogbook>();

            logbook.Write("Running context menu action.", LogLevel.Debug, callerName: "App");

            await viewModel.ContextMenu.Run(arguments);
            Current.Shutdown();
        }

        /// <summary>
        /// Update the program and restart it.
        /// </summary>
        protected async void Update()
        {
            ILogbook logbook = Container.Resolve<ILogbook>();

            logbook.Write("Update found. Starting update process.", LogLevel.Debug, callerName: "App");

            bool initialized = await Container.Resolve<IUpdateMethods>().InitializeUpdate();

            if (initialized)
            {
                logbook.Write($"Shutting down application for updates...", LogLevel.Debug, callerName: "App");

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
