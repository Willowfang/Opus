using Opus.Actions.Services.Extract;
using Opus.Common.Wrappers;
using Opus.Common.Collections;
using Prism.Mvvm;

namespace Opus.Actions.Implementation.Extract
{
    /// <summary>
    /// Implementation for <see cref="IExtractionSupportProperties"/>
    /// </summary>
    public class ExtractionSupportProperties : BindableBase, IExtractionSupportProperties
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ReorderCollection<FileAndBookmarkWrapper> Bookmarks { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsSelectedActualBookmark
        {
            get =>
                Bookmarks.SelectedItem != null
                && Bookmarks.SelectedItem.Bookmark.Pages.Count > 0;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool CollectionHasActualBookmarks
        {
            get
            {
                bool value = false;
                foreach (FileAndBookmarkWrapper wrapper in Bookmarks)
                {
                    if (wrapper.Bookmark.Pages.Count > 0)
                    {
                        value = true;
                        break;
                    }
                }
                return value;
            }
        }

        /// <summary>
        /// Create a new implementation instance.
        /// </summary>
        public ExtractionSupportProperties()
        {
            Bookmarks = new ReorderCollection<FileAndBookmarkWrapper>();
            Bookmarks.CanReorder = true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="propName"></param>
        public void RaiseChanged(string propName)
        {
            RaisePropertyChanged(propName);
        }
    }
}
