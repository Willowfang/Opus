using Opus.Actions.Services.Base;
using Opus.Common.Wrappers;
using Opus.Common.Collections;

namespace Opus.Actions.Services.Merge
{
    /// <summary>
    /// Properties for merge action.
    /// </summary>
    public interface IMergeProperties : IActionProperties
    {
        /// <summary>
        /// Collection of files to merge. May be reordered and the level (indentation) of individual file
        /// may be changed.
        /// </summary>
        public ReorderCollection<FileStorage> Collection { get; }
    }
}
