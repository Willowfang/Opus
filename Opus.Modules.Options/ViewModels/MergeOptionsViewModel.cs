using WF.LoggingLib;
using Opus.Core.Base;
using Opus.Services.Configuration;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.UI;

namespace Opus.Modules.Options.ViewModels
{
    /// <summary>
    /// View model for modifying merge options.
    /// </summary>
    public class MergeOptionsViewModel : OptionsViewModelBase<MergeSettingsDialog>
    {
        #region Constructor
        /// <summary>
        /// Create a new merge options viewmodel.
        /// </summary>
        /// <param name="configuration">Program-wide configurations.</param>
        /// <param name="dialogAssist">Service for showing and otherwise handling dialogs.</param>
        /// <param name="logbook">Logging service.</param>
        public MergeOptionsViewModel(
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
        protected override MergeSettingsDialog CreateDialog()
        {
            return new MergeSettingsDialog(Resources.Labels.General.Settings)
            {
                AddPageNumbers = configuration.MergeAddPageNumbers
            };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="dialog">Dialog that contains the modified settings.</param>
        protected override void SaveSettings(MergeSettingsDialog dialog)
        {
            configuration.MergeAddPageNumbers = dialog.AddPageNumbers;
        }
        #endregion
    }
}
