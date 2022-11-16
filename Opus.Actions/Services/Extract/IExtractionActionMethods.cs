using Opus.Actions.Services.Base;
using System.Windows.Controls;

namespace Opus.Actions.Services.Extract
{
    /// <summary>
    /// Methods for extraction action.
    /// </summary>
    public interface IExtractionActionMethods : IActionMethods
    {
        /// <summary>
        /// Add new files to extractables.
        /// </summary>
        /// <param name="filePaths">Paths of files to add.</param>
        /// <returns>An awaitable task.</returns>
        public Task AddNewFiles(string[] filePaths);

        /// <summary>
        /// Deselect a bookmark and its children.
        /// </summary>
        /// <param name="id">Id of the bookmark to deselect.</param>
        public void DeselectBookmark(Guid id);

        /// <summary>
        /// Execution method for bookmark addition.
        /// </summary>
        /// <returns></returns>
        public Task ExecuteAdd();

        /// <summary>
        /// Execution method for selection change.
        /// </summary>
        /// <param name="parameter">Event arguments of changed selection.</param>
        public void ExecuteSelection(SelectionChangedEventArgs parameter);

        /// <summary>
        /// Execution method for file viewing.
        /// </summary>
        public void ExecuteViewFile();

        /// <summary>
        /// Execution method for file deletion.
        /// </summary>
        public void ExecuteDeleteFile();
    }
}
