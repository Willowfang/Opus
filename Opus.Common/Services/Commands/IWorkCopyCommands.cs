using Opus.Common.Services.Commands.Base;
using System.Windows.Input;

namespace Opus.Common.Services.Commands
{
    /// <summary>
    /// Commands related to work copy action.
    /// </summary>
    public interface IWorkCopyCommands : IActionCommands
    {
        /// <summary>
        /// Command for deleting a file entry from the list.
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// Command for clearing the whole list.
        /// </summary>
        public ICommand ClearCommand { get; }

        /// <summary>
        /// Command for creating work copies.
        /// </summary>
        public ICommand CreateWorkCopyCommand { get; }
    }
}
