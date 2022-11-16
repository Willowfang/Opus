using Opus.Actions.Services.Base;
using Opus.Common.Wrappers;
using Opus.Common.Collections;

namespace Opus.Actions.Services.Extract
{
    /// <summary>
    /// Properties for extraction support section.
    /// </summary>
    public interface IExtractionSupportProperties : IActionProperties
    {
        /// <summary>
        /// Collection for the bookmarks to extract.
        /// </summary>
        /// <remarks>See <see cref="ReorderCollection{T}"/> for more information on properties and methods
        /// on organizing the bookmarks before extraction.</remarks>
        public ReorderCollection<FileAndBookmarkWrapper> Bookmarks { get; }

        /// <summary>
        /// Check whether currently selected item is an actual bookmark from a document (rather than
        /// a placeholder or null).
        /// </summary>
        public bool IsSelectedActualBookmark { get; }

        /// <summary>
        /// Check whether the collection contains any items and whether any of those items are actual bookmarks
        /// from a document (rather than just placeholders).
        /// </summary>
        public bool CollectionHasActualBookmarks { get; }

        /// <summary>
        /// Raise the property changed event of a given property.
        /// </summary>
        /// <param name="propName">Name of the property to raise event for.</param>
        public void RaiseChanged(string propName);
    }
}
