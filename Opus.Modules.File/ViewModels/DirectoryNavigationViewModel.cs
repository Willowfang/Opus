using Opus.Core.Base;
using Opus.Events;
using Opus.Services.Input;
using Prism.Commands;
using Prism.Events;
using System.IO;

namespace Opus.Modules.File.ViewModels
{
    /// <summary>
    /// Viewmodel for choosing directory paths.
    /// </summary>
    public class DirectoryNavigationViewModel : ViewModelBase
    {
        #region DI services
        private IPathSelection input;
        private IEventAggregator eventAggregator;
        #endregion

        #region Fields and properties
        private DirectoryInfo directory;

        /// <summary>
        /// Name of the currently selected directory.
        /// </summary>
        public string DirectoryName
        {
            get
            {
                if (directory == null)
                    return Resources.Buttons.General.SelectFolder;

                return directory.Name;
            }
            set => SetProperty(ref directory, new DirectoryInfo(value));
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new directory navigation viewModel.
        /// </summary>
        /// <param name="input">Service for obtaining user path input.</param>
        /// <param name="eventAggregator">Service for publishing and subscribing to events between viewModels.</param>
        public DirectoryNavigationViewModel(IPathSelection input, IEventAggregator eventAggregator)
        {
            // Assign DI services

            this.input = input;
            this.eventAggregator = eventAggregator;
        }
        #endregion

        #region Commands
        private DelegateCommand openDirectoryCommand;

        /// <summary>
        /// Command for finding and opening a directory.
        /// </summary>
        public DelegateCommand OpenDirectoryCommand =>
            openDirectoryCommand ??= new DelegateCommand(ExecuteOpenDirectory);

        /// <summary>
        /// Execution method for directory open command, see <see cref="OpenDirectoryCommand"/>.
        /// </summary>
        protected void ExecuteOpenDirectory()
        {
            string path = input.OpenDirectory(Resources.UserInput.Descriptions.SelectOpenFolder);
            if (path == null)
                return;
            eventAggregator.GetEvent<DirectorySelectedEvent>().Publish(path);
        }
        #endregion
    }
}
