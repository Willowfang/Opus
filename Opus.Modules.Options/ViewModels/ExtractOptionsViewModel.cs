using Opus.Common.ViewModels;
using Opus.Common.Dialogs;
using WF.LoggingLib;
using Opus.Common.Services.Commands;
using Opus.Modules.Options.Base;

namespace Opus.Modules.Options.ViewModels
{
    /// <summary>
    /// Viewmodel for handling extraction options.
    /// </summary>
    public class ExtractOptionsViewModel : OptionsViewModelBase<ExtractSettingsDialog>
    {
        /// <summary>
        /// Create a new viewmodel for handling extraction options modifications.
        /// </summary>
        /// <param name="logbook">Logging service.</param>
        /// <param name="settingsCommands">Service for commands related to settings.</param>
        public ExtractOptionsViewModel(
            ILogbook logbook,
            IExtractSettingsCommands settingsCommands
        ) : base(logbook, settingsCommands) { }
    }
}
