using Opus.Common.ViewModels;
using Opus.Events;
using Opus.Common.Services.Input;
using Prism.Commands;
using Prism.Events;
using System.IO;
using WF.LoggingLib;
using Opus.Modules.File.Base;

namespace Opus.Modules.File.ViewModels
{
    /// <summary>
    /// Viewmodel for choosing directory paths.
    /// </summary>
    public class DirectoryNavigationViewModel : FileViewModelBase<DirectoryNavigationViewModel>
    {
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

        /// <summary>
        /// Create new viewmodel for handling directory selection.
        /// </summary>
        /// <param name="pathSelection">Path selection service.</param>
        /// <param name="eventAggregator">Event handling service.</param>
        /// <param name="logbook">Logging service.</param>
        public DirectoryNavigationViewModel(
            IPathSelection pathSelection,
            IEventAggregator eventAggregator,
            ILogbook logbook) : base(pathSelection, eventAggregator, logbook) { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void OpenExecute()
        {
            logbook.Write($"Selecting a directory.", LogLevel.Debug);

            string path = pathSelection.OpenDirectory(Resources.UserInput.Descriptions.SelectOpenFolder);

            if (path == null)
            {
                logbook.Write($"No directory selected.", LogLevel.Debug);
                return;
            }

            eventAggregator.GetEvent<DirectorySelectedEvent>().Publish(path);

            logbook.Write($"Directory selected and event sent.", LogLevel.Debug);
        }
    }
}
