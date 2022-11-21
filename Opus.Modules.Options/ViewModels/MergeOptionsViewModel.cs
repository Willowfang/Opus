using WF.LoggingLib;
using Opus.Common.ViewModels;
using Opus.Common.Dialogs;
using Opus.Common.Services.Commands;
using Opus.Modules.Options.Base;

namespace Opus.Modules.Options.ViewModels
{
    /// <summary>
    /// View model for modifying merge options.
    /// </summary>
    public class MergeOptionsViewModel : OptionsViewModelBase<MergeSettingsDialog>
    {
        /// <summary>
        /// Create a new merge options viewmodel.
        /// </summary>
        /// <param name="logbook">Logging service.</param>
        /// <param name="settingsCommands">Service for commands related to settings.</param>
        public MergeOptionsViewModel(
            ILogbook logbook,
            IMergeSettingsCommands settingsCommands
        ) : base(logbook, settingsCommands) { }
    }
}
