using WF.LoggingLib;
using Opus.Core.Base;
using Opus.Services.Configuration;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.UI;

namespace Opus.Modules.Options.ViewModels
{
    /// <summary>
    /// Viewmodel for modifying work copy options.
    /// </summary>
    public class WorkCopyOptionsViewModel : OptionsViewModelBase<WorkCopySettingsDialog>
    {
        #region Constructor
        /// <summary>
        /// Create a new view model for modifying work copy options.
        /// </summary>
        /// <param name="configuration">Program-wide configurations.</param>
        /// <param name="dialogAssist">Service for showing and otherwise handling dialogs.</param>
        /// <param name="logbook">Logging service.</param>
        public WorkCopyOptionsViewModel(
            IConfiguration configuration,
            IDialogAssist dialogAssist,
            ILogbook logbook
        ) : base(dialogAssist, configuration, logbook) { }
        #endregion

        #region Overrides
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns>Created dialog.</returns>
        protected override WorkCopySettingsDialog CreateDialog()
        {
            string template =
                configuration.UnsignedTitleTemplate
                ?? Resources.DefaultValues.DefaultValues.UnsignedTemplate;
            return new WorkCopySettingsDialog(Resources.Labels.General.Settings)
            {
                TitleTemplate = template,
                FlattenRedactions = configuration.WorkCopyFlattenRedactions
            };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="dialog">Dialog containing modified options.</param>
        protected override void SaveSettings(WorkCopySettingsDialog dialog)
        {
            configuration.UnsignedTitleTemplate = dialog.TitleTemplate;
            configuration.WorkCopyFlattenRedactions = dialog.FlattenRedactions;
        }
        #endregion
    }
}
