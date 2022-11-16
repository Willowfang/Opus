using Opus.Services.Commands.Base;
using System.Windows.Input;

namespace Opus.Services.Commands
{
    /// <summary>
    /// Commands for extraction support.
    /// </summary>
    public interface IExtractionSupportCommands : IActionCommands
    {
        /// <summary>
        /// Command for adding a placeholder for external files (for numbering purposes)
        /// </summary>
        public ICommand AddExternalCommand { get; }

        /// <summary>
        /// Command for editing a selected bookmark (other than a placeholder)
        /// </summary>
        public ICommand EditCommand { get; }

        /// <summary>
        /// Command for deleting the selected bookmark.
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// Command for saving into a single file.
        /// </summary>
        public ICommand SaveFileCommand { get; }

        /// <summary>
        /// Command for saving bookmarks in separate files.
        /// </summary>
        public ICommand SaveSeparateCommand { get; }
    }
}
