using WF.LoggingLib;
using Opus.Common.ViewModels;
using Opus.Common.Dialogs;
using Opus.Common.Services.Commands;

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
        /// <param name="logbook">Logging service.</param>
        /// <param name="settingsCommands">Service for commands related to settings.</param>
        public WorkCopyOptionsViewModel(
            ILogbook logbook,
            IWorkCopySettingsCommands settingsCommands
        ) : base(logbook, settingsCommands) { }
        #endregion
    }
}
