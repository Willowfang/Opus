using Opus.Common.ViewModels;
using Opus.Events;
using Opus.Common.Services.Input;
using Prism.Commands;
using Prism.Events;
using System.IO;
using Opus.Modules.File.Base;
using WF.LoggingLib;

namespace Opus.Modules.File.ViewModels
{
    /// <summary>
    /// Viewmodel for selecting a single file.
    /// </summary>
    public class FileNavigationViewModel : FileViewModelBase<FileNavigationViewModel>
    {
        private string fileName;

        /// <summary>
        /// Name of the currently selected file.
        /// </summary>
        public string FileName
        {
            get { return Path.GetFileNameWithoutExtension(fileName); }
            set { SetProperty(ref fileName, value); }
        }

        /// <summary>
        /// Create a new viewmodel for single file selection.
        /// </summary>
        /// <param name="pathSelection">Path selection service.</param>
        /// <param name="eventAggregator">Event handling service.</param>
        /// <param name="logbook">Logging service.</param>
        public FileNavigationViewModel(
            IPathSelection pathSelection,
            IEventAggregator eventAggregator,
            ILogbook logbook) : base(pathSelection, eventAggregator, logbook)
        {
            FileName = Resources.Buttons.General.SelectFile;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void OpenExecute()
        {
            logbook.Write($"Selecting file path.", LogLevel.Debug);

            string path = pathSelection.OpenFile(
                Resources.UserInput.Descriptions.SelectOpenFile,
                FileType.PDF);

            if (path == null)
            {
                logbook.Write($"No path was selected.", LogLevel.Debug);

                return;
            }

            FileName = path;

            eventAggregator.GetEvent<FileSelectedEvent>().Publish(path);

            logbook.Write($"Path was selected and event sent.", LogLevel.Debug);
        }
    }
}
