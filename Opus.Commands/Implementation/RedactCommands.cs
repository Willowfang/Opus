using AsyncAwaitBestPractices.MVVM;
using Opus.Actions.Services.Redact;
using Opus.Common.Logging;
using Opus.Common.Services.Commands;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WF.LoggingLib;

namespace Opus.Commands.Implementation
{
    /// <summary>
    /// Implementation for redaction commands.
    /// </summary>
    public class RedactCommands : LoggingCapable<RedactCommands>, IRedactCommands
    {
        private readonly IRedactMethods methods;

        /// <summary>
        /// Create new redaction commands implementation.
        /// </summary>
        /// <param name="methods">Redaction methods.</param>
        /// <param name="logbook">Logging service.</param>
        public RedactCommands(
            IRedactMethods methods,
            ILogbook logbook) : base(logbook)
        {
            this.methods = methods;
        }

        private DelegateCommand? deleteCommand;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand DeleteCommand => deleteCommand ??= new DelegateCommand(methods.ExecuteDelete);

        private IAsyncCommand? executeRedactionsCommand;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand ExecuteRedactionsCommand =>
            executeRedactionsCommand ??= new AsyncCommand(methods.ExecuteRedactions);

        private IAsyncCommand? applyRedactionsCommand;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand ApplyRedactionsCommand =>
            applyRedactionsCommand ??= new AsyncCommand(methods.ApplyRedactions);
    }
}
