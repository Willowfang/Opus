using Opus.Common.Services.Commands.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opus.Common.Services.Commands
{
    /// <summary>
    /// Commands for redaction.
    /// </summary>
    public interface IRedactCommands : IActionCommands
    {
        /// <summary>
        /// Command for deleting a file from the list
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// Command for redacting parts of files.
        /// </summary>
        public ICommand ExecuteRedactionsCommand { get; }

        /// <summary>
        /// Command for applying existing redactions in files.
        /// </summary>
        public ICommand ApplyRedactionsCommand { get; }
    }
}
