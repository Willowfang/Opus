using Opus.Common.ViewModels;
using Opus.Values;
using Prism.Events;
using Opus.Common.Services.Dialogs;
using Opus.Events;
using WF.LoggingLib;
using System.Collections.ObjectModel;
using Opus.Common.Services.Commands;
using Opus.Common.Services.Configuration;
using Opus.Common.Languages;

namespace Opus.ViewModels
{
    /// <summary>
    /// Main window ViewModel. Main Window contains regions for displaying content and handles general,
    /// program-wide functions.
    /// </summary>
    public class MainWindowViewModel : ViewModelBaseLogging<MainWindowViewModel>
    {
        /// <summary>
        /// Service for handling common application commands.
        /// </summary>
        public ICommonCommands CommonCommands { get; set; }
        /// <summary>
        /// Dialog services reference for showing various dialogs.
        /// </summary>
        /// <remarks>
        /// MainWindowView is bound to this object, hence its publicity. The view is
        /// bound to the currently shown dialog.
        /// </remarks>
        public IDialogAssist Dialog { get; set; }

        /// <summary>
        /// Application configuration.
        /// </summary>

        public IConfiguration Configuration { get; }

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
        /// <param name="eventAggregator">Service for sending events between view models (and other entities).</param>
        /// <param name="commonCommands">Service for common application commands.</param>
        /// <param name="dialog">Dialog services.</param>
        /// <param name="logbook">Logging services.</param>
        /// <param name="configuration">Application configuration.</param>
        public MainWindowViewModel(
            IEventAggregator eventAggregator,
            ILogbook logbook,
            ICommonCommands commonCommands,
            IDialogAssist dialog,
            IConfiguration configuration
        ) : base(logbook)
        {

            this.logbook.Write($"Initializing {nameof(MainWindowViewModel)}.", LogLevel.Debug);

            CommonCommands = commonCommands;
            Dialog = dialog;
            this.Configuration = configuration;

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

            eventAggregator.GetEvent<ViewChangeEvent>().Subscribe(ChangeTitle);

            logbook.Write($"{nameof(MainWindowViewModel)} initialized.", LogLevel.Debug);
        }
        #endregion

        private void ChangeTitle(string name)
        {
            logbook.Write($"Title change called with name: {name}.", LogLevel.Debug);

            if (name == SchemeNames.EXTRACT)
                Title = Resources.Labels.MainWindow.Titles.Extract.ToUpper();
            if (name == SchemeNames.WORKCOPY)
                Title = Resources.Labels.MainWindow.Titles.Workcopy.ToUpper();
            if (name == SchemeNames.MERGE)
                Title = Resources.Labels.MainWindow.Titles.Merge.ToUpper();
            if (name == SchemeNames.COMPOSE)
                Title = Resources.Labels.MainWindow.Titles.Compose.ToUpper();
            if (name == SchemeNames.REDACT)
                Title = Resources.Labels.MainWindow.Titles.Redact.ToUpper();
        }
    }
}
