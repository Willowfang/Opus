using Opus.Actions.Services.Base;
using Opus.Common.Wrappers;

namespace Opus.Actions.Services.Extract
{
    /// <summary>
    /// Methods for extraction support action.
    /// </summary>
    public interface IExtractionSupportMethods : IActionMethods
    {
        /// <summary>
        /// Save selected bookmarks into one or more files.
        /// </summary>
        /// <param name="destination">Destination as a file or folder path (depending on
        /// whether bookmarks are saved in a single or multiple files).</param>
        /// <param name="files"></param>
        /// <returns>An awaitable task.</returns>
        public Task Save(FileSystemInfo destination, IList<FileAndBookmarkWrapper> files);

        /// <summary>
        /// Update info on all collection items (e.g. <see cref="FileAndBookmarkWrapper.Index"/> property)
        /// and raise relevant property notifications.
        /// </summary>
        public void UpdateEntries(IExtractionSupportProperties properties);

        /// <summary>
        /// External addition execution method.
        /// </summary>
        public void ExecuteAddExternal();

        /// <summary>
        /// Execution method for edition.
        /// </summary>
        /// <returns></returns>
        public Task ExecuteEdit();

        /// <summary>
        /// Execution method for deletion.
        /// </summary>
        public void ExecuteDelete();

        /// <summary>
        /// Execution method for saving into a single file.
        /// </summary>
        /// <returns></returns>
        public Task ExecuteSaveFile();

        /// <summary>
        /// Execution method for separate saving.
        /// </summary>
        /// <returns></returns>
        public Task ExecuteSaveSeparate();
    }
}
