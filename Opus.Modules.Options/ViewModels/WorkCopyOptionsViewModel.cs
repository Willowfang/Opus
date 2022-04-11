using AsyncAwaitBestPractices.MVVM;
using CX.LoggingLib;
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
    public class WorkCopyOptionsViewModel : OptionsViewModelBase<WorkCopySettingsDialog>
    {

        public WorkCopyOptionsViewModel(IConfiguration configuration, IDialogAssist dialogAssist,
            ILogbook logbook)
            : base(dialogAssist, configuration, logbook) { }

        protected override WorkCopySettingsDialog CreateDialog()
        {
            string template = configuration.UnsignedTitleTemplate ?? Resources.DefaultValues.DefaultValues.UnsignedTemplate;
            return new WorkCopySettingsDialog(Resources.Labels.General.Settings)
            {
                TitleTemplate = template,
                FlattenRedactions = configuration.WorkCopyFlattenRedactions
            };
        }

        protected override void SaveSettings(WorkCopySettingsDialog dialog)
        {
            configuration.UnsignedTitleTemplate = dialog.TitleTemplate;
            configuration.WorkCopyFlattenRedactions = dialog.FlattenRedactions;
        }
    }
}
