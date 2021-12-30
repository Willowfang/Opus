using AsyncAwaitBestPractices.MVVM;
using Opus.Core.Base;
using Opus.Services.Configuration;
using Opus.Services.Data;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.UI;
using Prism.Events;
using Prism.Regions;
using System.Threading.Tasks;

namespace Opus.Modules.Options.ViewModels
{
    public class SignatureOptionsViewModel : OptionsViewModelBase<SignatureSettingsDialog>
    {
        private ISignatureOptions options;

        public SignatureOptionsViewModel(IConfiguration configuration, ISignatureOptions options, IDialogAssist dialogAssist)
            : base(dialogAssist, configuration) 
        { 
            this.options = options;
        }

        protected override SignatureSettingsDialog CreateDialog()
        {
            return new SignatureSettingsDialog(Resources.Labels.General.Settings)
            {
                Suffix = options.Suffix
            };
        }

        protected override void SaveSettings(SignatureSettingsDialog dialog)
        {
            options.Suffix = dialog.Suffix;
        }
    }
}
