using Opus.Common.Dialogs;
using Opus.Common.Services.Commands;
using Opus.Modules.Options.Base;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using WF.LoggingLib;

namespace Opus.Modules.Options.ViewModels
{
    /// <summary>
    /// View model for modifying redaction options.
    /// </summary>
    public class RedactOptionsViewModel : OptionsViewModelBase<RedactSettingsDialog>
    {
        /// <summary>
        /// Create a new viewmodel for redaction options.
        /// </summary>
        /// <param name="logbook">Logging service.</param>
        /// <param name="commands">Commands for these options.</param>
        public RedactOptionsViewModel(
            ILogbook logbook,
            IRedactSettingsCommands commands)
            : base(logbook, commands) { }
    }
}
