using Opus.Core.Base;
using Opus.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Opus.Events.Data;
using Opus.Services.UI;
using Opus.Services.Implementation.UI.Dialogs;
using AsyncAwaitBestPractices.MVVM;
using System.Threading.Tasks;
using Opus.Services.Configuration;

namespace Opus.Modules.Options.ViewModels
{
    public class ExtractOptionsViewModel : OptionsViewModelBase<ExtractSettingsDialog>
    {
        public ExtractOptionsViewModel(IDialogAssist dialogAssist, IConfiguration configuration)
            : base(dialogAssist, configuration) { }

        protected override ExtractSettingsDialog CreateDialog()
        {
            return new ExtractSettingsDialog(Resources.Labels.General.Settings)
            {
                Prefix = configuration.ExtractionPrefix,
                Suffix = configuration.ExtractionSuffix
            };
        }

        protected override void SaveSettings(ExtractSettingsDialog dialog)
        {
            configuration.ExtractionPrefix = dialog.Prefix;
            configuration.ExtractionSuffix = dialog.Suffix;
        }
    }
}
