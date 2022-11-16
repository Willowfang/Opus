using AsyncAwaitBestPractices.MVVM;
using Opus.Actions.Services.WorkCopy;
using Opus.Common.Services.Commands;
using Opus.Common.Logging;
using Prism.Commands;
using System.Windows.Input;
using WF.LoggingLib;

namespace Opus.Commands.Implementation
{
    public class WorkCopyCommands : LoggingCapable<WorkCopyCommands>, IWorkCopyCommands
    {
        private readonly IWorkCopyMethods methods;

        /// <summary>
        /// Create new implementation instance.
        /// </summary>
        /// <param name="methods">Work copy methods service.</param>
        /// <param name="logbook">Logging service.</param>
        public WorkCopyCommands(
            IWorkCopyMethods methods,
            ILogbook logbook) : base(logbook)
        {
            this.methods = methods;
        }

        private DelegateCommand? deleteCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand DeleteCommand =>
            deleteCommand ?? (deleteCommand = new DelegateCommand(methods.ExecuteDelete));

        private DelegateCommand? clearCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand ClearCommand =>
            clearCommand ?? (clearCommand = new DelegateCommand(methods.ExecuteClear));

        private IAsyncCommand? createWorkCopyCommand;

        /// <summary>
        /// Command for creating work copies.
        /// </summary>
        public ICommand CreateWorkCopyCommand =>
            createWorkCopyCommand
            ?? (createWorkCopyCommand = new AsyncCommand(methods.ExecuteCreateWorkCopy));
    }
}
