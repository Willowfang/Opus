using AsyncAwaitBestPractices.MVVM;
using Opus.Actions.Services.Extract;
using Opus.Common.Services.Commands;
using Opus.Common.Logging;
using Prism.Commands;
using System.Windows.Input;
using WF.LoggingLib;

namespace Opus.Commands.Implementation
{
    /// <summary>
    /// Implementation for extraction support commands.
    /// </summary>
    public class ExtractionSupportCommands : LoggingCapable<ExtractionSupportCommands>,
        IExtractionSupportCommands
    {
        private readonly IExtractionSupportMethods methods;

        /// <summary>
        /// Create a new instance of implementing class.
        /// </summary>
        /// <param name="methods">Extraction support methods service.</param>
        /// <param name="logbook">Logging service.</param>
        public ExtractionSupportCommands(
            IExtractionSupportMethods methods,
            ILogbook logbook) : base(logbook)
        {
            this.methods = methods;
        }

        private DelegateCommand? addExternalCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand AddExternalCommand =>
            addExternalCommand ??= new DelegateCommand(methods.ExecuteAddExternal);

        private IAsyncCommand? editCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand EditCommand => editCommand ??= new AsyncCommand(methods.ExecuteEdit);

        private DelegateCommand? deleteCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand DeleteCommand =>
            deleteCommand ?? (deleteCommand = new DelegateCommand(methods.ExecuteDelete));

        private IAsyncCommand? saveFileCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand SaveFileCommand =>
            saveFileCommand ?? (saveFileCommand = new AsyncCommand(methods.ExecuteSaveFile));

        private IAsyncCommand? saveSeparateCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand SaveSeparateCommand =>
            saveSeparateCommand
            ?? (saveSeparateCommand = new AsyncCommand(methods.ExecuteSaveSeparate));
    }
}
