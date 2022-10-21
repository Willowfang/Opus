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
    /// <summary>
    /// Viewmodel for handling extraction options.
    /// </summary>
    public class ExtractOptionsViewModel : OptionsViewModelBase<ExtractSettingsDialog>
    {
        #region Constructor
        /// <summary>
        /// Create a new viewmodel for handling extraction options modifications.
        /// </summary>
        /// <param name="dialogAssist">Service for showing and otherwise handling dialogs.</param>
        /// <param name="configuration">Program-wide settings.</param>
        /// <param name="logbook">Logging service.</param>
        public ExtractOptionsViewModel(
            IDialogAssist dialogAssist,
            IConfiguration configuration,
            ILogbook logbook
        ) : base(dialogAssist, configuration, logbook) { }
        #endregion

        #region Overrides
        /// <summary>
        /// Create a dialog for modifying options.
        /// </summary>
        /// <returns>The created dialog.</returns>
        protected override ExtractSettingsDialog CreateDialog()
        {
            return new ExtractSettingsDialog(Resources.Labels.General.Settings)
            {
                Title = configuration.ExtractionTitle,
                AlwaysAsk = configuration.ExtractionTitleAsk,
                PdfA = configuration.ExtractionConvertPdfA,
                PdfADisabled = configuration.ExtractionPdfADisabled,
                Annotations = configuration.Annotations,
                CreateZip = configuration.ExtractionCreateZip,
                GroupByFiles = configuration.GroupByFiles
            };
        }

        /// <summary>
        /// Save modified options.
        /// </summary>
        /// <inheritdoc/>
        /// <param name="dialog"></param>
        protected override void SaveSettings(ExtractSettingsDialog dialog)
        {
            configuration.ExtractionTitle = dialog.Title;
            configuration.ExtractionTitleAsk = dialog.AlwaysAsk;
            configuration.ExtractionConvertPdfA = dialog.PdfA;
            configuration.Annotations = dialog.Annotations;
            configuration.ExtractionCreateZip = dialog.CreateZip;
            configuration.GroupByFiles = dialog.GroupByFiles;
        }
        #endregion
    }
}
