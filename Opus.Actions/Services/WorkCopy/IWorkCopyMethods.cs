using Opus.Actions.Services.Base;
using Opus.Common.Wrappers;

namespace Opus.Actions.Services.WorkCopy
{
    /// <summary>
    /// Methods for work copy action.
    /// </summary>
    public interface IWorkCopyMethods : IActionMethods 
    {
        /// <summary>
        /// Remove digital signatures from a pdf-files.
        /// </summary>
        /// <param name="files">Files to remove signatures from.</param>
        /// <param name="destination">Directory, where products should be saved.</param>
        /// <returns></returns>
        public Task<IList<FileInfo>> Remove(IEnumerable<FileStorage> files, DirectoryInfo destination);

        /// <summary>
        /// Execution method for delete.
        /// <para>
        /// Remove an entry from the list.
        /// </para>
        /// </summary>
        public void ExecuteDelete();

        /// <summary>
        /// Execution method for clear.
        /// <para>
        /// Clear the whole list of file entries.
        /// </para>
        /// </summary>
        public void ExecuteClear();

        /// <summary>
        /// Execution method for work copy creation.
        /// <para>
        /// Executes the actual action.
        /// </para>
        /// </summary>
        public Task ExecuteCreateWorkCopy();
    }
}
