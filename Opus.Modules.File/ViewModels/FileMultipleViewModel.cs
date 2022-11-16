using Opus.Common.ViewModels;
using Opus.Events;
using Opus.Common.Services.Input;
using Prism.Commands;
using Prism.Events;

namespace Opus.Modules.File.ViewModels
{
    /// <summary>
    /// View model for selecting multiple files.
    /// </summary>
    public class FileMultipleViewModel : ViewModelBase
    {
        #region DI Services
        private IPathSelection input;
        private IEventAggregator eventAggregator;
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new multiple file selection viewModel.
        /// </summary>
        /// <param name="eventAggregator">Service for publishing and subscribing to events between viewModels.</param>
        /// <param name="input">Service for user input path selection.</param>
        public FileMultipleViewModel(IEventAggregator eventAggregator, IPathSelection input)
        {
            this.input = input;
            this.eventAggregator = eventAggregator;
        }
        #endregion

        #region Commands
        private DelegateCommand addFilesCommand;

        /// <summary>
        /// Command for adding new files.
        /// </summary>
        public DelegateCommand AddFilesCommand =>
            addFilesCommand ?? (addFilesCommand = new DelegateCommand(ExecuteAddFiles));

        /// <summary>
        /// Execution method for files addition command, see <see cref="AddFilesCommand"/>.
        /// </summary>
        protected void ExecuteAddFiles()
        {
            string[] path = input.OpenFiles(
                Resources.UserInput.Descriptions.SelectOpenFiles,
                FileType.PDF
            );
            if (path.Length == 0)
                return;

            eventAggregator.GetEvent<FilesAddedEvent>().Publish(path);
        }
        #endregion
    }
}
