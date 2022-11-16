using Opus.Actions.Services.Base;
using WF.PdfLib.Services.Data;

namespace Opus.Actions.Services.Merge
{
    /// <summary>
    /// Methods for merge action.
    /// </summary>
    public interface IMergeMethods : IActionMethods
    {
        /// <summary>
        /// Convert file entries in the <see cref="IMergeProperties.Collection"/> into
        /// mergeInputs for merging.
        /// </summary>
        /// <returns>List of all inputs for merging.</returns>
        public IList<IMergeInput> GetMergeInputs(IMergeProperties properties);

        /// <summary>
        /// Execution method for edit.
        /// </summary>
        /// <returns>An awaitable task.</returns>
        public Task ExecuteEdit();

        /// <summary>
        /// Execution method for deletion.
        /// <para>
        /// Delete an entry from the collection.
        /// </para>
        /// </summary>
        public void ExecuteDelete();

        /// <summary>
        /// Execution method for clear.
        /// <para>
        /// Clear the whole collection of all entries.
        /// </para>
        /// </summary>
        public void ExecuteClear();

        /// <summary>
        /// Execution method for merging files.
        /// <para>
        /// Merges the files and optionally marks them with page numbers.
        /// </para>
        /// </summary>
        /// <returns>An awaitable task.</returns>
        public Task ExecuteMerge();
    }
}
