using Opus.Common.Services.Commands.Base;
using System.Windows.Input;

namespace Opus.Common.Services.Commands
{
    /// <summary>
    /// Merge-specific commands.
    /// </summary>
    public interface IMergeCommands : IActionCommands
    {
        /// <summary>
        /// Command for editing a file entry.
        /// </summary>
        public ICommand EditCommand { get; }

        /// <summary>
        /// Command for deleting file entries.
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// Command for clearing the whole collection.
        /// </summary>
        public ICommand ClearCommand { get; }

        /// <summary>
        /// Command for executing merge.
        /// </summary>
        public ICommand MergeCommand { get; }
    }
}
