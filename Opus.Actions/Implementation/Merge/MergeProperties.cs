using Opus.Actions.Services.Merge;
using Opus.Common.Wrappers;
using Opus.Common.Collections;

namespace Opus.Actions.Implementation.Merge
{
    /// <summary>
    /// Implementation for <see cref="IMergeProperties"/>.
    /// </summary>
    public class MergeProperties : IMergeProperties
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ReorderCollection<FileStorage> Collection { get; }

        /// <summary>
        /// Create new instance of properties implementation.
        /// </summary>
        public MergeProperties()
        {
            // Initialize collection
            Collection = new ReorderCollection<FileStorage>()
            {
                CanReorder = true,
            };
        }
    }
}
