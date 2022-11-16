using AsyncAwaitBestPractices.MVVM;
using Opus.Events;
using Opus.Common.Services.Commands;
using Opus.Common.Services.Configuration;
using Opus.Common.Logging;
using Opus.Common.Dialogs;
using Opus.Common.Services.Dialogs;
using Opus.Values;
using Prism.Commands;
using Prism.Events;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using WF.LoggingLib;

namespace Opus.Commands.Implementation
{
    /// <summary>
    /// Commands for common functionalities (such as opening external websites and language changes).
    /// </summary>
    public class CommonCommands : LoggingCapable<CommonCommands>, ICommonCommands
    {
        private IConfiguration configuration;
        private IDialogAssist dialogAssist;
        private IEventAggregator eventAggregator;

        /// <summary>
        /// Create new implementation instance for general commands.
        /// </summary>
        /// <param name="logbook">Logging service.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="dialogAssist">Dialog service.</param>
        /// <param name="eventAggregator">Event service.</param>
        public CommonCommands(
            ILogbook logbook,
            IConfiguration configuration,
            IDialogAssist dialogAssist,
            IEventAggregator eventAggregator) : base(logbook)
        {
            this.configuration = configuration;
            this.dialogAssist = dialogAssist;
            this.eventAggregator = eventAggregator;
        }

        private DelegateCommand? openManualCommand;

        /// <summary>
        /// Command for opening the user manual.
        /// </summary>
        public ICommand OpenManualCommand =>
            openManualCommand
            ?? (openManualCommand = new DelegateCommand(ExecuteOpenManualCommand));

        /// <summary>
        /// Execution method for user manual opening command, see <see cref="OpenManualCommand"/>
        /// <para>
        /// Opens the User manual as a separate process. The manual is an internet link, so most probably it will
        /// be opened in the system default browser.
        /// </para>
        /// </summary>
        protected void ExecuteOpenManualCommand()
        {
            logbook.Write($"Opening user manual.", LogLevel.Information);

            // Open link in system default browser

            var p = new Process();
            p.StartInfo = new ProcessStartInfo(@Resources.Hyperlinks.Hyperlinks.UserManual)
            {
                UseShellExecute = true
            };
            p.Start();

            logbook.Write($"User manual opened externally.", LogLevel.Information);
        }

        private DelegateCommand? openLicensesCommand;

        /// <summary>
        /// Command for opening the licenses for viewing.
        /// </summary>
        public ICommand OpenLicensesCommand =>
            openLicensesCommand
            ?? (openLicensesCommand = new DelegateCommand(ExecuteOpenLicensesCommand));

        /// <summary>
        /// Execution method for open licences command, see <see cref="OpenLicensesCommand"/>
        /// <para>
        /// Opens the license page.
        /// </para>
        /// </summary>
        protected void ExecuteOpenLicensesCommand()
        {
            logbook.Write($"Opening license information.", LogLevel.Information);

            // Open the license page

            var p = new Process();
            p.StartInfo = new ProcessStartInfo(@Resources.Hyperlinks.Hyperlinks.Licenses)
            {
                UseShellExecute = true
            };
            p.Start();

            logbook.Write($"License information opened externally.", LogLevel.Information);
        }

        private DelegateCommand? openSourceCodeCommand;

        /// <summary>
        /// Command for opening the source code browser.
        /// </summary>
        public ICommand OpenSourceCodeCommand =>
            openSourceCodeCommand
            ?? (openSourceCodeCommand = new DelegateCommand(ExecuteOpenSourceCodeCommand));

        /// <summary>
        /// Execution method for source code opening command, see <see cref="OpenSourceCodeCommand"/>.
        /// <para>
        /// Source code is published to GitHub. This is, again, an internet link and will most likely be opened in the default browser.
        /// </para>
        /// </summary>
        protected void ExecuteOpenSourceCodeCommand()
        {
            logbook.Write($"Opening source code.", LogLevel.Information);

            // Open link in system default browser.

            var p = new Process();
            p.StartInfo = new ProcessStartInfo(@Resources.Hyperlinks.Hyperlinks.SourceCode)
            {
                UseShellExecute = true
            };
            p.Start();

            logbook.Write($"Source code opened externally.", LogLevel.Information);
        }

        private IAsyncCommand<string>? languageCommand;

        /// <summary>
        /// Command for changing UI language.
        /// </summary>
        public ICommand LanguageCommand =>
            languageCommand ??= new AsyncCommand<string>(ExecuteLanguageCommand);

        /// <summary>
        /// Execution method for UI language change command, see <see cref="LanguageCommand"/>.
        /// <para>
        /// Only changes language, if the selected language is not the same as the language already selected.
        /// </para>
        /// <para>
        /// Saves the language configuration for persistence and notifies the user of the need for restart.
        /// </para>
        /// </summary>
        /// <param name="language">New language choice as a two-letter code (e.g. "fi")</param>
        /// <returns>A void async task</returns>
        protected async Task ExecuteLanguageCommand(string? language)
        {
            logbook.Write($"Changing language to {language ?? "null"}.", LogLevel.Information);

            // Requested language already selected, just return.

            var lang = configuration.LanguageCode;
            if (language == lang)
            {
                logbook.Write($"Language already selected.", LogLevel.Information);

                return;
            }

            // Save language change to configuration and prompt for program restart.

            configuration.LanguageCode = language;

            logbook.Write($"Language changed.", LogLevel.Information);

            await dialogAssist.Show(
                new MessageDialog(
                    Resources.Labels.General.Notification,
                    Resources.Messages.MainWindow.ChangeLanguage
                )
            );
        }

        private DelegateCommand<string>? navigateCommand;

        /// <summary>
        /// Command for navigating regions to different views based on a scheme name (e.g. "extract").
        /// </summary>
        public ICommand NavigateCommand =>
            navigateCommand
            ?? (navigateCommand = new DelegateCommand<string>(ExecuteNavigateCommand));

        /// <summary>
        /// Execution method for navigation command, see <see cref="NavigateCommand"/>.
        /// <para>
        /// Publishes a ViewChange event that is listened to by <see cref="INavigationAssist"/>.
        /// </para>
        /// </summary>
        /// <param name="name">Name of the scheme to apply. Scheme names can be found at
        /// <see cref="SchemeNames"/>.</param>
        protected void ExecuteNavigateCommand(string name)
        {
            logbook.Write($"Navigating to scheme {name}.", LogLevel.Information);

            // Publish a ViewChangeEvent. This event is listened to by INavigationAssist, which
            // will handle the view changes according to the scheme name. ViewModels will be notified
            // of the change and if they implement INavigationTarget, will fire their OnArrival or
            // WhenLeaving handlers accordingly.

            eventAggregator.GetEvent<ViewChangeEvent>().Publish(name);
        }

        private DelegateCommand? exitCommand;

        /// <summary>
        /// Command for exiting the application.
        /// </summary>
        public ICommand ExitCommand =>
            exitCommand ??= new DelegateCommand(ExecuteExitCommand);

        /// <summary>
        /// Execution method for exit command, see <see cref="ExitCommand"/>.
        /// <para>
        /// Shuts the program down by calling shutdown on <see cref="Application.Current"/>.
        /// </para>
        /// </summary>
        protected void ExecuteExitCommand()
        {
            logbook.Write($"Exiting application.", LogLevel.Information);

            Application.Current.Shutdown();
        }

        private DelegateCommand? resetCommand;

        /// <summary>
        /// Command for resetting current scheme to defaults.
        /// </summary>
        public ICommand ResetCommand =>
            resetCommand ?? (resetCommand = new DelegateCommand(ExecuteResetCommand));

        /// <summary>
        /// Execution method for scheme resetting command, see <see cref="ResetCommand"/>
        /// <para>
        /// Publishes an <see cref="ActionResetEvent"/> that is subscribed to by viewmodels of currently viewed
        /// views.
        /// </para>
        /// </summary>
        protected void ExecuteResetCommand()
        {
            logbook.Write($"Resetting action state.", LogLevel.Information);

            // ViewModels of the current scheme have been subscribed to ActionResetEvent if they
            // implement INavigationTarget. Reset is handled by each ViewModel respectively with
            // its own implementation.

            eventAggregator.GetEvent<ActionResetEvent>().Publish();
        }

        private DelegateCommand<string>? logLevelCommand;

        public ICommand LogLevelCommand =>
            logLevelCommand ??= new DelegateCommand<string>(ExecuteLogLevelCommand);

        protected void ExecuteLogLevelCommand(string parameter)
        {
            logbook.Write($"Changing log level to {parameter}.", LogLevel.Information);

            LogLevel level = Enum.Parse<LogLevel>(parameter);

            logbook.BaseLogbook.ChangeLevel(level);

            configuration.LoggingLevel = (int)level;

            logbook.Write($"Log level changed.", LogLevel.Information);
        }
    }
}
