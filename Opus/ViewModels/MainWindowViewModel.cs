using System;
using System.Diagnostics;
using System.IO;
using Opus.Core.Base;
using Opus.Values;
using Opus.Services.Configuration;
using Prism.Commands;
using Prism.Events;
using Opus.Services.UI;
using Opus.Events;
using Opus.Services.Implementation.UI.Dialogs;
using AsyncAwaitBestPractices.MVVM;
using System.Threading.Tasks;
using System.Windows;
using CX.LoggingLib;
using Opus.Core.Wrappers;
using System.Collections.ObjectModel;

namespace Opus.ViewModels
{
    /// <summary>
    /// Main window ViewModel. Main Window contains regions for displaying content and handles general,
    /// program-wide functions.
    /// </summary>
    public class MainWindowViewModel : ViewModelBaseLogging<MainWindowViewModel>
    {
        #region DI services
        private readonly IConfiguration configuration;
        private readonly IEventAggregator eventAggregator;

        /// <summary>
        /// NOTE! MainWindowView is bound to this service (by getting the active dialog from here)
        /// </summary>
        public IDialogAssist Dialog { get; set; }
        #endregion

        #region Properties and fields
        private string title;

        /// <summary>
        /// Header title content.
        /// </summary>
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        /// <summary>
        /// A collection of known languages for UI language selection.
        /// </summary>
        public ObservableCollection<LanguageOption> Languages { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// ViewModel responsible for the main window. Handles navigation functions
        /// and program-wide responsibilities.
        /// </summary>
        /// <param name="eventAggregator"></param>
        /// <param name="config"></param>
        /// <param name="dialogAssist"></param>
        /// <param name="logbook"></param>
        public MainWindowViewModel(
            IEventAggregator eventAggregator,
            IConfiguration configuration,
            IDialogAssist dialogAssist,
            ILogbook logbook
        ) : base(logbook)
        {
            // Assign DI services

            this.configuration = configuration;
            this.eventAggregator = eventAggregator;
            Dialog = dialogAssist;

            // Initialize supported languages

            Languages = new ObservableCollection<LanguageOption>();

            foreach (string code in SupportedTypes.CULTURES)
            {
                Languages.Add(
                    new LanguageOption(
                        code,
                        Resources.Labels.MainWindow.Languages.ResourceManager.GetString(code)
                    )
                );
            }
        }
        #endregion

        #region Commands
        private DelegateCommand openManualCommand;

        /// <summary>
        /// Command for opening the user manual.
        /// </summary>
        public DelegateCommand OpenManualCommand =>
            openManualCommand
            ?? (openManualCommand = new DelegateCommand(ExecuteOpenManualCommand));

        /// <summary>
        /// Execution method for user manual opening command, see <see cref="OpenManualCommand"/>
        /// </summary>
        void ExecuteOpenManualCommand()
        {
            // Open link in system default browser

            var p = new Process();
            p.StartInfo = new ProcessStartInfo(@Resources.Hyperlinks.Hyperlinks.UserManual)
            {
                UseShellExecute = true
            };
            p.Start();
        }

        private DelegateCommand openLicensesCommand;

        /// <summary>
        /// Command for opening the licenses for viewing.
        /// </summary>
        public DelegateCommand OpenLicensesCommand =>
            openLicensesCommand
            ?? (openLicensesCommand = new DelegateCommand(ExecuteOpenLicensesCommand));

        /// <summary>
        /// Execution method for open licences command, see <see cref="OpenLicensesCommand"/>
        /// </summary>
        void ExecuteOpenLicensesCommand()
        {
            // Open the included licenses file in the system default program

            var p = new Process();
            p.StartInfo = new ProcessStartInfo(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "TextFiles",
                    Resources.Hyperlinks.Hyperlinks.Licenses
                )
            )
            {
                UseShellExecute = true
            };
            p.Start();
        }

        private DelegateCommand openSourceCodeCommand;

        /// <summary>
        /// Command for opening the source code browser.
        /// </summary>
        public DelegateCommand OpenSourceCodeCommand =>
            openSourceCodeCommand
            ?? (openSourceCodeCommand = new DelegateCommand(ExecuteOpenSourceCodeCommand));

        /// <summary>
        /// Execution method for source code opening command, see <see cref="OpenSourceCodeCommand"/>
        /// </summary>
        void ExecuteOpenSourceCodeCommand()
        {
            // Open link in system default browser.

            var p = new Process();
            p.StartInfo = new ProcessStartInfo(@Resources.Hyperlinks.Hyperlinks.SourceCode)
            {
                UseShellExecute = true
            };
            p.Start();
        }

        private IAsyncCommand<string> languageCommand;

        /// <summary>
        /// Command for changing UI language.
        /// </summary>
        public IAsyncCommand<string> LanguageCommand =>
            languageCommand ??= new AsyncCommand<string>(ExecuteLanguageCommand);

        /// <summary>
        /// Execution method for UI language change command, see <see cref="LanguageCommand"/>
        /// </summary>
        /// <param name="language">New language choice as a two-letter code (e.g. "fi")</param>
        /// <returns></returns>
        private async Task ExecuteLanguageCommand(string language)
        {
            logbook.Write($"Change of language to {language} requested.", LogLevel.Information);

            // Requested language already selected, just return.

            var lang = configuration.LanguageCode;
            if (language == lang)
                return;

            // Save language change to configuration and prompt for program restart.

            configuration.LanguageCode = language;
            await Dialog.Show(
                new MessageDialog(
                    Resources.Labels.General.Notification,
                    Resources.Messages.MainWindow.ChangeLanguage
                )
            );
        }

        private DelegateCommand<string> navigateCommand;

        /// <summary>
        /// Command for navigating regions to different views based on a scheme name (e.g. "extract").
        /// </summary>
        public DelegateCommand<string> NavigateCommand =>
            navigateCommand
            ?? (navigateCommand = new DelegateCommand<string>(ExecuteNavigateCommand));

        /// <summary>
        /// Execution method for navigation command, see <see cref="NavigateCommand"/>.
        /// </summary>
        /// <param name="name">Name of the scheme to apply. Scheme names can be found at
        /// <see cref="SchemeNames"/>.</param>
        void ExecuteNavigateCommand(string name)
        {
            logbook.Write($"Navigation requested for scheme {name}.", LogLevel.Information);

            // Publish a ViewChangeEvent. This event is listened to by INavigationAssist, which
            // will handle the view changes according to the scheme name. ViewModels will be notified
            // of the change and if they implement INavigationTarget, will fire their OnArrival or
            // WhenLeaving handlers accordingly.

            eventAggregator.GetEvent<ViewChangeEvent>().Publish(name);

            // Change the header title to reflect the scheme.

            if (name == SchemeNames.EXTRACT)
                Title = Resources.Labels.MainWindow.Titles.Extract.ToUpper();
            if (name == SchemeNames.WORKCOPY)
                Title = Resources.Labels.MainWindow.Titles.Workcopy.ToUpper();
            if (name == SchemeNames.MERGE)
                Title = Resources.Labels.MainWindow.Titles.Merge.ToUpper();
            if (name == SchemeNames.COMPOSE)
                Title = Resources.Labels.MainWindow.Titles.Compose.ToUpper();
        }

        private DelegateCommand exitCommand;

        /// <summary>
        /// Command for exiting the application.
        /// </summary>
        public DelegateCommand ExitCommand =>
            exitCommand ??= new DelegateCommand(ExecuteExitCommand);

        /// <summary>
        /// Execution method for exit command, see <see cref="ExitCommand"/>.
        /// </summary>
        private void ExecuteExitCommand()
        {
            Application.Current.Shutdown();
        }

        private DelegateCommand resetCommand;

        /// <summary>
        /// Command for resetting current scheme to defaults.
        /// </summary>
        public DelegateCommand ResetCommand =>
            resetCommand ?? (resetCommand = new DelegateCommand(ExecuteResetCommand));

        /// <summary>
        /// Execution method for scheme resetting command, see <see cref="ResetCommand"/>.
        /// </summary>
        private void ExecuteResetCommand()
        {
            // ViewModels of the current scheme have been subscribed to ActionResetEvent if they
            // implement INavigationTarget. Reset is handled by each ViewModel respectively with
            // its own implementation.

            eventAggregator.GetEvent<ActionResetEvent>().Publish();
        }
        #endregion
    }
}
