using Microsoft.Win32;
using Opus.Core.Base;
using Opus.Values;
using Opus.Events;
using Opus.Services.Input;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System.IO;

namespace Opus.Modules.File.ViewModels
{
    /// <summary>
    /// Viewmodel for selecting a single file.
    /// </summary>
    public class FileNavigationViewModel : ViewModelBase
    {
        #region DI services
        private IPathSelection input;
        private IEventAggregator eventAggregator;
        #endregion

        #region Fields and properties
        private string fileName;

        /// <summary>
        /// Name of the currently selected file.
        /// </summary>
        public string FileName
        {
            get { return Path.GetFileNameWithoutExtension(fileName); }
            set { SetProperty(ref fileName, value); }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new viewmodel for selecting a single file.
        /// </summary>
        /// <param name="eventAggregator">Service for publishing and subscribing to events between viewModels.</param>
        /// <param name="input">Service for user selection of an input path.</param>
        public FileNavigationViewModel(IEventAggregator eventAggregator, IPathSelection input)
        {
            FileName = Resources.Buttons.General.SelectFile;
            this.input = input;
            this.eventAggregator = eventAggregator;
        }
        #endregion

        #region Commands
        private DelegateCommand openFileCommand;

        /// <summary>
        /// Command for opening a file path.
        /// </summary>
        public DelegateCommand OpenFileCommand =>
            openFileCommand ?? (openFileCommand = new DelegateCommand(ExecuteOpenFile));

        /// <summary>
        /// Execution method for open file command, see <see cref="OpenFileCommand"/>.
        /// </summary>
        protected void ExecuteOpenFile()
        {
            string path = input.OpenFile(
                Resources.UserInput.Descriptions.SelectOpenFile,
                FileType.PDF
            );
            if (path == null)
                return;

            FileName = path;
            eventAggregator.GetEvent<FileSelectedEvent>().Publish(path);
        }
        #endregion
    }
}
