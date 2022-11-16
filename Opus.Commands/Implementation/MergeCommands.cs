using AsyncAwaitBestPractices.MVVM;
using Opus.Common.Logging;
using System.Windows.Input;
using WF.LoggingLib;
using Opus.Actions.Services.Merge;
using Prism.Commands;
using Opus.Common.Services.Commands;

namespace Opus.Commands.Implementation
{
    /// <summary>
    /// Commands for merging.
    /// </summary>
    public class MergeCommands : LoggingCapable<MergeCommands>, IMergeCommands
    {
        private readonly IMergeMethods methods;

        /// <summary>
        /// Create new implementation instance.
        /// </summary>
        /// <param name="methods">Merge methods service.</param>
        /// <param name="logbook">Logging service.</param>
        public MergeCommands(
            IMergeMethods methods,
            ILogbook logbook) : base(logbook)
        {
            this.methods = methods;
        }

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
            deleteCommand ??= new DelegateCommand(methods.ExecuteDelete);

        private DelegateCommand? clearCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand ClearCommand =>
            clearCommand ??= new DelegateCommand(methods.ExecuteClear);

        private IAsyncCommand? mergeCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand MergeCommand => mergeCommand ??= new AsyncCommand(methods.ExecuteMerge);
    }
}
