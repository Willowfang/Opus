﻿using WF.LoggingLib;
using Opus.Services.Commands;

namespace Opus.Core.Base
{
    /// <summary>
    /// Abstract base class for ViewModels that represent options and settings for various actions. Contains
    /// info on a dialog that will be opened when options change is requested.
    /// </summary>
    public abstract class OptionsViewModelBase<VMType> : ViewModelBaseLogging<VMType>
    {
        public ISettingsCommands SettingsCommands { get; }

        /// <summary>
        /// Creates a a new Options ViewModel. Receives required DI services as parameters.
        /// </summary>
        /// <param name="logbook">Logging library service.</param>
        /// <param name="settingsCommands">Service for commands related to settings.</param>
        public OptionsViewModelBase(
            ILogbook logbook,
            ISettingsCommands settingsCommands
        ) : base(logbook)
        {
            SettingsCommands = settingsCommands;
        }
    }
}
