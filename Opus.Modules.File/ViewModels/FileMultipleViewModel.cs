using Opus.Common.ViewModels;
using Opus.Events;
using Opus.Common.Services.Input;
using Prism.Commands;
using Prism.Events;
using Opus.Modules.File.Base;
using WF.LoggingLib;
using System.Windows.Automation;

namespace Opus.Modules.File.ViewModels
{
    /// <summary>
    /// View model for selecting multiple files.
    /// </summary>
    public class FileMultipleViewModel : FileViewModelBase<FileMultipleViewModel>
    {
        /// <summary>
        /// Create new viewmodel for selecting multiple files.
        /// </summary>
        /// <param name="pathSelection">Path selection service.</param>
        /// <param name="eventAggregator">Event handling service.</param>
        /// <param name="logbook">Logging service.</param>
        public FileMultipleViewModel(
            IPathSelection pathSelection,
            IEventAggregator eventAggregator,
            ILogbook logbook) : base(pathSelection, eventAggregator, logbook) { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void OpenExecute()
        {
            logbook.Write($"Selecting one or more files.", LogLevel.Debug);

            string[] path = pathSelection.OpenFiles(
                Resources.UserInput.Descriptions.SelectOpenFiles,
                FileType.PDF);

            if (path.Length == 0)
            {
                logbook.Write($"No paths were selected.", LogLevel.Debug);
                
                return;
            }

            eventAggregator.GetEvent<FilesAddedEvent>().Publish(path);

            logbook.Write($"Paths selected and event sent.", LogLevel.Debug);
        }
    }
}
