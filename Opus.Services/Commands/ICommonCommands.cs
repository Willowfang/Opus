using System.Windows.Input;

namespace Opus.Services.Commands
{
    /// <summary>
    /// Commands for opening external services.
    /// </summary>
    public interface ICommonCommands
    {
        /// <summary>
        /// Open the User Manual.
        /// </summary>
        public ICommand OpenManualCommand { get; }

        /// <summary>
        /// Open application license information.
        /// </summary>
        public ICommand OpenLicensesCommand { get; }

        /// <summary>
        /// Open application source code for viewing. 
        /// </summary>
        public ICommand OpenSourceCodeCommand { get; }

        /// <summary>
        /// Change application language.
        /// </summary>
        public ICommand LanguageCommand { get; }

        /// <summary>
        /// Navigate the view to a different scheme.
        /// </summary>
        public ICommand NavigateCommand { get; }

        /// <summary>
        /// Exit the application.
        /// </summary>
        public ICommand ExitCommand { get; }

        /// <summary>
        /// Reset the current view scheme to its defaults.
        /// </summary>
        public ICommand ResetCommand { get; }

        /// <summary>
        /// Change the logging level.
        /// </summary>
        public ICommand LogLevelCommand { get; }
    }
}
