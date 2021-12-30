using AsyncAwaitBestPractices.MVVM;
using Opus.Services.Configuration;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Core.Base
{
    public abstract class OptionsViewModelBase<DialogType> : ViewModelBase where DialogType : IDialog
    {
        protected IDialogAssist dialogAssist;
        protected IConfiguration configuration;

        public OptionsViewModelBase(IDialogAssist dialogAssist, IConfiguration configuration)
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

            if (dialog.IsCanceled) return;

            SaveSettings(dialog);
        }

        protected abstract DialogType CreateDialog();

        protected abstract void SaveSettings(DialogType dialog);
    }
}
