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
using CX.LoggingLib;

namespace Opus.Modules.Options.ViewModels
{
    public class ExtractOptionsViewModel : OptionsViewModelBase<ExtractSettingsDialog>
    {
        public ExtractOptionsViewModel(IDialogAssist dialogAssist, IConfiguration configuration, ILogbook logbook)
            : base(dialogAssist, configuration, logbook) { }

        protected override ExtractSettingsDialog CreateDialog()
        {
            return new ExtractSettingsDialog(Resources.Labels.General.Settings)
            {
                Title = configuration.ExtractionTitle,
                AlwaysAsk = configuration.ExtractionTitleAsk,
                PdfA = configuration.ExtractionConvertPdfA,
                PdfADisabled = configuration.ExtractionPdfADisabled,
                Annotations = configuration.Annotations
            };
        }

        protected override void SaveSettings(ExtractSettingsDialog dialog)
        {
            configuration.ExtractionTitle = dialog.Title;
            configuration.ExtractionTitleAsk = dialog.AlwaysAsk;
            configuration.ExtractionConvertPdfA = dialog.PdfA;
            configuration.Annotations = dialog.Annotations;
        }
    }
}
