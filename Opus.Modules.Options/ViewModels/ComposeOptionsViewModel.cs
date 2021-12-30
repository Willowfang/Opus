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
    public class ComposeOptionsViewModel : OptionsViewModelBase<CompositionSettingsDialog>
    {
        public ComposeOptionsViewModel(IDialogAssist dialogAssist, IConfiguration configuration)
            : base(dialogAssist, configuration) { }

        protected override CompositionSettingsDialog CreateDialog()
        {
            return new CompositionSettingsDialog(Resources.Labels.General.Settings)
            {
                SearchSubDirectories = configuration.CompositionSearchSubDirectories
            };
        }

        protected override void SaveSettings(CompositionSettingsDialog dialog)
        {
            configuration.CompositionSearchSubDirectories = dialog.SearchSubDirectories;
        }
    }
}
