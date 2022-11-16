using Opus.Services.Commands.Base;
using System.Windows.Input;

namespace Opus.Services.Commands
{
    /// <summary>
    /// Commands related to extraction.
    /// </summary>
    public interface IExtractionActionCommands : IActionCommands
    {
        /// <summary>
        /// Command for adding new bookmarks.
        /// </summary>
        public ICommand AddCommand { get; }

        /// <summary>
        /// Command for bookmark selection changes.
        /// </summary>
        public ICommand SelectionCommand { get; }

        /// <summary>
        /// Command for viewing the selected file in an external program.
        /// </summary>
        public ICommand ViewFileCommand { get; }

        /// <summary>
        /// Command from deleting the file from the list
        /// </summary>
        public ICommand DeleteFileCommand { get; }
    }
}
