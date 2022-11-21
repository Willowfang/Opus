using Opus.Common.Services.Input;
using Opus.Common.ViewModels;
using Prism.Commands;
using Prism.Events;
using System.Windows.Input;
using WF.LoggingLib;

namespace Opus.Modules.File.Base
{
    /// <summary>
    /// A baseclass for file view model.
    /// </summary>
    /// <typeparam name="VMType">Type of the inheriting view model.</typeparam>
    public abstract class FileViewModelBase<VMType> : ViewModelBaseLogging<VMType>
    {
        /// <summary>
        /// Path selection service.
        /// </summary>
        protected IPathSelection pathSelection;

        /// <summary>
        /// Event handling service.
        /// </summary>
        protected IEventAggregator eventAggregator;

        /// <summary>
        /// Create new base class instance.
        /// </summary>
        /// <param name="pathSelection">Path selection service.</param>
        /// <param name="eventAggregator">Event handling service.</param>
        /// <param name="logbook">Logging service.</param>
        public FileViewModelBase(
            IPathSelection pathSelection,
            IEventAggregator eventAggregator,
            ILogbook logbook) : base(logbook)
        {
            this.pathSelection = pathSelection;
            this.eventAggregator = eventAggregator;
        }

        private DelegateCommand openCommand;

        /// <summary>
        /// Command for opening the files and folders. Method for opening
        /// is defined in <see cref="OpenExecute"/>.
        /// </summary>
        public ICommand OpenCommand =>
            openCommand ??= new DelegateCommand(OpenExecute);

        /// <summary>
        /// The method for opening files or folders.
        /// </summary>
        protected abstract void OpenExecute();
    }
}
