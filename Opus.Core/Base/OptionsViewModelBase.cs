using AsyncAwaitBestPractices.MVVM;
using CX.LoggingLib;
using Opus.Services.Configuration;
using Opus.Services.UI;
using System.Threading.Tasks;

namespace Opus.Core.Base
{
    /// <summary>
    /// Abstract base class for ViewModels that represent options and settings for various actions. Contains
    /// info on a dialog that will be opened, when options change is requested.
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
        public IAsyncCommand SettingsCommand =>
            settingsCommand ??= new AsyncCommand(ExecuteSettingsCommand);

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

        protected abstract DialogType CreateDialog();

        protected abstract void SaveSettings(DialogType dialog);
    }
}
