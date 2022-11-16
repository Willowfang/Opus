using WF.LoggingLib;
using Opus.Common.ViewModels;
using Opus.Common.Dialogs;
using Opus.Common.Services.Commands;

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
        /// <param name="logbook">Logging service.</param>
        /// <param name="settingsCommands">Service for commands related to settings.</param>
        public ComposeOptionsViewModel(
            ILogbook logbook,
            IComposeSettingsCommands settingsCommands
        ) : base(logbook, settingsCommands) { }
        #endregion
    }
}
