using AsyncAwaitBestPractices.MVVM;
using WF.LoggingLib;
using System.Windows.Input;
using Opus.Common.Logging;
using Prism.Commands;
using System.Windows.Controls;
using Opus.Actions.Services.Extract;
using Opus.Common.Services.Commands;

namespace Opus.Commands.Implementation
{
    /// <summary>
    /// Commands for extraction-related user actions.
    /// </summary>
    public class ExtractionActionCommands : LoggingCapable<ExtractionActionCommands>, IExtractionActionCommands
    {
        private IExtractionActionMethods methods;

        /// <summary>
        /// Create a new implementation instance for commands related to extraction.
        /// </summary>
        /// <param name="methods">Service for methods related to extraction.</param>
        /// <param name="logbook">Logging service.</param>
        public ExtractionActionCommands(
            IExtractionActionMethods methods,
            ILogbook logbook) : base(logbook)
        {
            this.methods = methods;
        }

        private IAsyncCommand? addCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand AddCommand =>
            addCommand ?? (addCommand = new AsyncCommand(methods.ExecuteAdd));


        private DelegateCommand<SelectionChangedEventArgs>? selectionCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand SelectionCommand =>
            selectionCommand
            ?? (selectionCommand = new DelegateCommand<SelectionChangedEventArgs>(
                    methods.ExecuteSelection));

        private DelegateCommand? viewFileCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand ViewFileCommand =>
            viewFileCommand ?? (viewFileCommand = new DelegateCommand(methods.ExecuteViewFile));

        private DelegateCommand? deleteFileCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand DeleteFileCommand =>
            deleteFileCommand
            ?? (deleteFileCommand = new DelegateCommand(methods.ExecuteDeleteFile));
    }
}
