using WF.PdfLib.Services.Data;
using Prism.Mvvm;

namespace Opus.Common.Wrappers
{
    /// <summary>
    /// Storage class for displaying and selecting bookmarks.
    /// </summary>
    public class BookmarkStorage : BindableBase
    {
        private bool isSelected;

        /// <summary>
        /// Bookmark contained in storage.
        /// </summary>
        public ILeveledBookmark Value { get; }

        /// <summary>
        /// Is the current bookmark selected.
        /// </summary>
        public bool IsSelected
        {
            get => isSelected;
            set { SetProperty(ref isSelected, value); }
        }

        /// <summary>
        /// Pages between start page and end page as a string.
        /// </summary>
        public string Range { get; }

        /// <summary>
        /// Create a new storage class for a bookmark.
        /// </summary>
        /// <param name="value">Bookmark to store.</param>
        public BookmarkStorage(ILeveledBookmark value)
        {
            Value = value;
            Range = value.StartPage + "-" + value.EndPage;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool Equals(object? obj) => 
            obj is BookmarkStorage other && Equals(other);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Equals(BookmarkStorage other)
        {
            return CheckEquality(this, other);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override int GetHashCode()
        {
            return Value.Pages.GetHashCode();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public static bool operator ==(BookmarkStorage a, BookmarkStorage b)
        {
            return CheckEquality(a, b);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public static bool operator !=(BookmarkStorage a, BookmarkStorage b)
        {
            return !CheckEquality(a, b);
        }

        private static bool CheckEquality(BookmarkStorage a, BookmarkStorage b)
        {
            if (a is null && b is null)
                return true;
            if (a is null || b is null)
                return false;
            return a.Value == b.Value;
        }
    }
}
