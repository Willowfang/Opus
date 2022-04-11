using AsyncAwaitBestPractices.MVVM;
using CX.LoggingLib;
using Opus.Services.Configuration;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Core.Base
{
    public abstract class OptionsViewModelBase<DialogType> : ViewModelBaseLogging<DialogType> where DialogType : IDialog
    {
        protected IDialogAssist dialogAssist;
        protected IConfiguration configuration;

        public OptionsViewModelBase(IDialogAssist dialogAssist, IConfiguration configuration,
            ILogbook logbook) : base(logbook)
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
                logbook.Write($"Cancellation requested at {nameof(IDialog)} '{dialog}'.", LogLevel.Information);
                return;
            }

            SaveSettings(dialog);
        }

        protected abstract DialogType CreateDialog();

        protected abstract void SaveSettings(DialogType dialog);
    }
}
