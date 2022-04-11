using AsyncAwaitBestPractices.MVVM;
using CX.LoggingLib;
using Opus.Core.Base;
using Opus.Services.Configuration;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.UI;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Opus.Modules.Options.ViewModels
{
    public class MergeOptionsViewModel : OptionsViewModelBase<MergeSettingsDialog>
    {
        public MergeOptionsViewModel(IConfiguration configuration, IDialogAssist dialogAssist, ILogbook logbook)
            : base(dialogAssist, configuration, logbook) { }

        protected override MergeSettingsDialog CreateDialog()
        {
            return new MergeSettingsDialog(Resources.Labels.General.Settings)
            {
                AddPageNumbers = configuration.MergeAddPageNumbers
            };
        }

        protected override void SaveSettings(MergeSettingsDialog dialog)
        {
            configuration.MergeAddPageNumbers = dialog.AddPageNumbers;
        }
    }
}
