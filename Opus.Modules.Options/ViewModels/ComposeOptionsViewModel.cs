using WF.LoggingLib;
using Opus.Core.Base;
using Opus.Services.Configuration;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.UI;

namespace Opus.Modules.Options.ViewModels
{
    /// <summary>
    /// A view model for composition options.
    /// </summary>
    public class ComposeOptionsViewModel : OptionsViewModelBase<CompositionSettingsDialog>
    {
        #region Constructor
        /// <summary>
        /// Create a new viewmodel for changing composition options.
        /// </summary>
        /// <param name="dialogAssist">Service for showing and otherwise handling dialogs.</param>
        /// <param name="configuration">Program-wide configurations.</param>
        /// <param name="logbook">Logging service.</param>
        public ComposeOptionsViewModel(
            IDialogAssist dialogAssist,
            IConfiguration configuration,
            ILogbook logbook
        ) : base(dialogAssist, configuration, logbook) { }
        #endregion

        #region Overrides
        /// <summary>
        /// Create the options dialog.
        /// <para>
        /// Dialog is <see cref="CompositionSettingsDialog"/>.
        /// </para>
        /// </summary>
        /// <returns>The created dialog.</returns>
        protected override CompositionSettingsDialog CreateDialog()
        {
            return new CompositionSettingsDialog(Resources.Labels.General.Settings)
            {
                SearchSubDirectories = configuration.CompositionSearchSubDirectories,
                DeleteConverted = configuration.CompositionDeleteConverted
            };
        }

        /// <summary>
        /// Save the modified options.
        /// </summary>
        /// <param name="dialog">Dialog where the options are held.</param>
        protected override void SaveSettings(CompositionSettingsDialog dialog)
        {
            configuration.CompositionSearchSubDirectories = dialog.SearchSubDirectories;
            configuration.CompositionDeleteConverted = dialog.DeleteConverted;
        }
        #endregion
    }
}
