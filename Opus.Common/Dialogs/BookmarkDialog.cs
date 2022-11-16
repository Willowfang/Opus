using Opus.Common.Services.Dialogs;
using System.ComponentModel;

namespace Opus.Common.Dialogs
{
    /// <summary>
    /// A dialog for adding or editing a bookmark.
    /// </summary>
    public class BookmarkDialog : DialogBase, IDialog, IDataErrorInfo
    {
        private int startPage;

        /// <summary>
        /// Start page for the bookmark (also the page on which the bookmark resides in the document).
        /// </summary>
        public int StartPage
        {
            get { return startPage; }
            set
            {
                SetProperty(ref startPage, value);
                RaisePropertyChanged(nameof(EndPage));
            }
        }
        private int endPage;

        /// <summary>
        /// End page of the bookmark range.
        /// </summary>
        public int EndPage
        {
            get { return endPage; }
            set { SetProperty(ref endPage, value); }
        }
        private string title;

        /// <summary>
        /// Bookmark title (its name in the tree).
        /// </summary>
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        /// <summary>
        /// Create a new bookmark dialog.
        /// </summary>
        /// <param name="initialBookmarkTitle">Initial name for the bookmark.</param>
        /// <param name="dialogTitle">Title of the dialog.</param>
        public BookmarkDialog(string dialogTitle, string? initialBookmarkTitle = null) : base(dialogTitle)
        {
            title = initialBookmarkTitle ?? string.Empty;
        }

        /// <summary>
        /// Validation error, always return empty string.
        /// </summary>
        public string Error
        {
            get => string.Empty;
        }

        /// <summary>
        /// Validation for dialog.
        /// </summary>
        /// <param name="propertyName">Property to validate</param>
        /// <returns>Validation error message (or null, if valid)</returns>
        public string this[string propertyName]
        {
            get
            {
                if (propertyName == nameof(StartPage))
                {
                    if (StartPage < 1)
                    {
                        return Resources.Validation.General.PageZero;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else if (propertyName == nameof(EndPage))
                {
                    if (EndPage < 1)
                    {
                        return Resources.Validation.General.PageZero;
                    }
                    else if (EndPage < StartPage)
                    {
                        return Resources.Validation.General.PageZero;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else if (propertyName == nameof(Title))
                {
                    if (string.IsNullOrEmpty(Title))
                    {
                        return Resources.Validation.General.NameEmpty;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
