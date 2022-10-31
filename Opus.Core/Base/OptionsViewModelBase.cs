using AsyncAwaitBestPractices.MVVM;
using WF.LoggingLib;
using Opus.Services.Configuration;
using Opus.Services.UI;
using System.Threading.Tasks;

namespace Opus.Core.Base
{
    /// <summary>
    /// Abstract base class for ViewModels that represent options and settings for various actions. Contains
    /// info on a dialog that will be opened when options change is requested.
    /// </summary>
    /// <typeparam name="DialogType">The type of the dialog to open.</typeparam>
    public abstract class OptionsViewModelBase<DialogType> : ViewModelBaseLogging<DialogType>
        where DialogType : IDialog
    {
        protected IDialogAssist dialogAssist;
        protected IConfiguration configuration;

        /// <summary>
        /// Creates a a new Options ViewModel. Receives required DI services as parameters.
        /// </summary>
        /// <param name="dialogAssist">Service for storing, changing, displaying and otherwise handling dialogs.</param>
        /// <param name="configuration">Program configurations as an accessible class.</param>
        /// <param name="logbook">Logging library service.</param>
        public OptionsViewModelBase(
            IDialogAssist dialogAssist,
            IConfiguration configuration,
            ILogbook logbook
        ) : base(logbook)
        {
            this.dialogAssist = dialogAssist;
            this.configuration = configuration;
        }

        private IAsyncCommand settingsCommand;

        /// <summary>
        /// Command for opening the settings dialog.
        /// </summary>
        public IAsyncCommand SettingsCommand =>
            settingsCommand ??= new AsyncCommand(ExecuteSettingsCommand);

        /// <summary>
        /// Execution method for settings opening command.
        /// <para>
        /// Creates the dialog using <see cref="CreateDialog"/> and displays it to the user.
        /// Saves the settings using <see cref="SaveSettings(DialogType)"/> if the dialog has not been canceled.
        /// </para>
        /// </summary>
        /// <returns></returns>
        protected virtual async Task ExecuteSettingsCommand()
        {
            DialogType dialog = CreateDialog();

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled)
            {
                logbook.Write(
                    $"Cancellation requested at {nameof(IDialog)} '{dialog}'.",
                    LogLevel.Information
                );
                return;
            }

            SaveSettings(dialog);
        }

        /// <summary>
        /// Dialog creation method. Must be overridden for each implementation of this
        /// abstract class.
        /// </summary>
        /// <returns></returns>
        protected abstract DialogType CreateDialog();

        /// <summary>
        /// Method for saving the selected settings. Must be overridden for each implementation
        /// of this abstract class.
        /// </summary>
        /// <param name="dialog">The type of the dialog used for asking the new settings from the user.</param>
        protected abstract void SaveSettings(DialogType dialog);
    }
}
